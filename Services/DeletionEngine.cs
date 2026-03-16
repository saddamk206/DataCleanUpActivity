using DataCleanUpActivity.Models;
using Microsoft.Xrm.Sdk;

namespace DataCleanUpActivity.Services
{
    /// <summary>
    /// Orchestrates the deletion process for all instructions
    /// </summary>
    public class DeletionEngine
    {
        private readonly Logger _logger;
        private readonly CrmHelper _crmHelper;
        private readonly int _delayBetweenPages;

        public int TotalRecordsDeleted { get; private set; }
        public int TotalRowsProcessed { get; private set; }
        public int FailedSequences { get; private set; }

        public DeletionEngine(CrmHelper crmHelper, Logger logger, int delayBetweenPages = 200)
        {
            _crmHelper = crmHelper ?? throw new ArgumentNullException(nameof(crmHelper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _delayBetweenPages = delayBetweenPages;
        }

        /// <summary>
        /// Executes the deletion process for all instructions
        /// </summary>
        public bool Execute(List<DeletionInstruction> instructions)
        {
            if (instructions == null || instructions.Count == 0)
            {
                _logger.LogWarning("No instructions to process");
                return false;
            }

            _logger.LogSection("STARTING DELETION PROCESS");
            _logger.LogInfo($"Total instructions to process: {instructions.Count}");

            bool overallSuccess = true;

            foreach (var instruction in instructions)
            {
                try
                {
                    bool success = ProcessInstruction(instruction);

                    if (!success)
                    {
                        overallSuccess = false;
                        FailedSequences++;
                    }

                    TotalRowsProcessed++;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Critical error processing sequence {instruction.Sequence}", ex);
                    overallSuccess = false;
                    FailedSequences++;
                    TotalRowsProcessed++;

                    // Continue with next instruction
                    _logger.LogWarning("Continuing with next instruction...");
                }
            }

            return overallSuccess;
        }

        /// <summary>
        /// Processes a single deletion instruction
        /// </summary>
        private bool ProcessInstruction(DeletionInstruction instruction)
        {
            _logger.LogSection($"SEQUENCE {instruction.Sequence}");
            _logger.LogInfo(instruction.ToString());

            try
            {
                var startTime = DateTime.Now;

                // Fetch all records
                _logger.LogInfo($"Fetching records from entity '{instruction.EntityName}'...");
                var records = FetchAllRecordsSimple(instruction);

                if (records.Count == 0)
                {
                    _logger.LogInfo("No records found matching the criteria");
                    return true;
                }

                _logger.LogInfo($"Total records to delete: {records.Count}");

                // Delete records in batches
                _logger.LogInfo("Starting batch deletion...");
                int deletedCount = _crmHelper.DeleteRecordsInBatch(records);

                TotalRecordsDeleted += deletedCount;

                var endTime = DateTime.Now;
                var duration = endTime - startTime;

                _logger.LogSuccess($"Sequence {instruction.Sequence} completed successfully");
                _logger.LogInfo($"Duration: {duration.TotalSeconds:F2} seconds");
                _logger.LogInfo($"Total records deleted: {deletedCount}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to process sequence {instruction.Sequence}", ex);
                return false;
            }
        }

        /// <summary>
        /// Fetches records with pagination and delay between pages
        /// </summary>
        private List<EntityReference> FetchRecordsWithPagination(DeletionInstruction instruction)
        {
            var allRecords = new List<EntityReference>();
            int pageNumber = 1;
            string? pagingCookie = null;
            bool moreRecords = true;

            while (moreRecords)
            {
                try
                {
                    // Add delay between page requests (except first page)
                    if (pageNumber > 1 && _delayBetweenPages > 0)
                    {
                        _logger.LogDebug($"Waiting {_delayBetweenPages}ms before next page request...");
                        Thread.Sleep(_delayBetweenPages);
                    }

                    // Fetch page
                    var pageRecords = _crmHelper.FetchRecords(
                        instruction.EntityName,
                        instruction.StartDate,
                        instruction.EndDate,
                        pageNumber,
                        pagingCookie);

                    if (pageRecords.Count > 0)
                    {
                        allRecords.AddRange(pageRecords);
                        _logger.LogInfo($"Page {pageNumber}: Fetched {pageRecords.Count} records (Total: {allRecords.Count})");
                    }

                    // Check if there are more records (if we got a full page, there might be more)
                    moreRecords = pageRecords.Count == 100; // Assuming pageSize is 100

                    if (moreRecords)
                    {
                        pageNumber++;
                        // For QueryExpression, we need to handle paging differently
                        // The pagingCookie would be set in the FetchRecords method
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error fetching page {pageNumber}", ex);
                    throw;
                }
            }

            return allRecords;
        }

        /// <summary>
        /// Alternative method that uses the CrmHelper's FetchAllRecords
        /// </summary>
        private List<EntityReference> FetchAllRecordsSimple(DeletionInstruction instruction)
        {
            return _crmHelper.FetchAllRecords(
                instruction.EntityName,
                instruction.EntityIdColumn,
                instruction.StartDate,
                instruction.EndDate,
                instruction.AdditionalQuery,
                instruction.QueryType);
        }

        /// <summary>
        /// Validates that all prerequisites are met before execution
        /// </summary>
        public bool ValidatePrerequisites()
        {
            _logger.LogInfo("Validating prerequisites...");

            try
            {
                // Test CRM connection
                if (!_crmHelper.TestConnection())
                {
                    _logger.LogError("CRM connection test failed");
                    return false;
                }

                _logger.LogSuccess("All prerequisites validated successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Prerequisite validation failed", ex);
                return false;
            }
        }

        /// <summary>
        /// Gets execution statistics
        /// </summary>
        public (int processed, int deleted, int failed) GetStatistics()
        {
            return (TotalRowsProcessed, TotalRecordsDeleted, FailedSequences);
        }
    }
}
