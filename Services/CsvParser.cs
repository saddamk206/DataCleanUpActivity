using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using DataCleanUpActivity.Models;
using System.Globalization;

namespace DataCleanUpActivity.Services
{
    /// <summary>
    /// Handles parsing and validation of CSV deletion instructions
    /// </summary>
    public class CsvParser
    {
        private readonly Logger _logger;

        public CsvParser(Logger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Parses the CSV file and returns ordered deletion instructions
        /// </summary>
        public List<DeletionInstruction> ParseCsv(string filePath)
        {
            _logger.LogInfo($"Parsing CSV file: {filePath}");

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"CSV file not found: {filePath}");
            }

            var instructions = new List<DeletionInstruction>();

            try
            {
                using var reader = new StreamReader(filePath);
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    MissingFieldFound = null,
                    BadDataFound = null,
                    TrimOptions = TrimOptions.Trim,
                    HeaderValidated = null  // Ignore missing headers for optional fields
                };

                using var csv = new CsvReader(reader, config);

                csv.Context.RegisterClassMap<DeletionInstructionMap>();
                instructions = csv.GetRecords<DeletionInstruction>().ToList();

                _logger.LogInfo($"Successfully parsed {instructions.Count} rows from CSV");
            }
            catch (CsvHelperException ex)
            {
                _logger.LogError($"CSV parsing error: {ex.Message}", ex);
                throw new Exception($"Failed to parse CSV file: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error while parsing CSV: {ex.Message}", ex);
                throw;
            }

            return instructions;
        }

        /// <summary>
        /// Validates all instructions and returns valid ones sorted by sequence
        /// </summary>
        public List<DeletionInstruction> ValidateAndOrder(List<DeletionInstruction> instructions)
        {
            _logger.LogInfo("Validating deletion instructions...");

            var validInstructions = new List<DeletionInstruction>();
            var hasErrors = false;

            for (int i = 0; i < instructions.Count; i++)
            {
                var instruction = instructions[i];
                var errors = instruction.Validate();

                if (errors.Any())
                {
                    hasErrors = true;
                    _logger.LogWarning($"Row {i + 1} validation failed:");
                    foreach (var error in errors)
                    {
                        _logger.LogWarning($"  - {error}");
                    }
                }
                else
                {
                    validInstructions.Add(instruction);
                }
            }

            if (hasErrors && validInstructions.Count == 0)
            {
                throw new Exception("All CSV rows failed validation. Cannot proceed.");
            }

            // Sort by sequence in ascending order
            validInstructions = validInstructions.OrderBy(x => x.Sequence).ToList();

            _logger.LogInfo($"Validation complete. {validInstructions.Count} valid instructions out of {instructions.Count} total rows");

            return validInstructions;
        }
    }

    /// <summary>
    /// Custom mapping for CSV columns to handle different date formats
    /// </summary>
    public sealed class DeletionInstructionMap : ClassMap<DeletionInstruction>
    {
        public DeletionInstructionMap()
        {
            Map(m => m.Sequence).Name("sequence");
            Map(m => m.EntityName).Name("entityname");
            Map(m => m.EntityIdColumn).Name("entityidcolumn");
            Map(m => m.StartDate).Name("start date").TypeConverterOption.Format("dd-MM-yyyy", "yyyy-MM-dd", "MM/dd/yyyy", "dd/MM/yyyy", "MM-dd-yyyy").Optional();
            Map(m => m.EndDate).Name("end date").TypeConverterOption.Format("dd-MM-yyyy", "yyyy-MM-dd", "MM/dd/yyyy", "dd/MM/yyyy", "MM-dd-yyyy").Optional();
            Map(m => m.QueryTypeString).Name("querytype").Optional();
            Map(m => m.AdditionalQuery).Name("additionalquery").Optional();
        }
    }
}
