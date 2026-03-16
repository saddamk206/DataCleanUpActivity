using Microsoft.Data.SqlClient;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System.Net;
using Azure.Identity;
using Azure.Core;

namespace DataCleanUpActivity.Services
{
    /// <summary>
    /// Handles all Dynamics 365 CRM operations including connection, fetch, and delete
    /// </summary>
    public class CrmHelper : IDisposable
    {
        private readonly Logger _logger;
        private ServiceClient? _serviceClient;
        private readonly string _connectionString;
       // private readonly string? _sqlConnectionString;
        private  string? _connectionStringSQL;
        private readonly string? _tenantId;
        private readonly string? _clientId;
        private readonly string? _clientSecret;
        private readonly string? _environmentUrl;
        private readonly int _retryCount;
        private readonly int _pageSize;
        private readonly int _batchSize;
        private readonly int _parallelThreads;
        private readonly bool _useSqlForFetch;

        public CrmHelper(string connectionString, Logger logger, int retryCount = 5, int pageSize = 5000, int batchSize = 100,
            string? tenantId = null, string? clientId = null, string? clientSecret = null, string? environmentUrl = null, int parallelThreads = 4, bool useSqlForFetch = true)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
           // _sqlConnectionString = sqlConnectionString;
            _tenantId = tenantId;
            _clientId = clientId;
            _clientSecret = clientSecret;
            _environmentUrl = environmentUrl;
            _useSqlForFetch = useSqlForFetch;
            _retryCount = retryCount;
            _pageSize = pageSize;
            _batchSize = batchSize;
            _parallelThreads = parallelThreads;
            
            // Increase default HTTP timeout and configure for better stability
            System.Net.ServicePointManager.MaxServicePointIdleTime = 600000; // 10 minutes
            System.Net.ServicePointManager.Expect100Continue = false;
            System.Net.ServicePointManager.DefaultConnectionLimit = 10;
        }

        /// <summary>
        /// Tests SQL connection before CRM connection
        /// </summary>
        public void TestSqlConnection()
        {
            _logger.LogInfo("Testing SQL connection...");

            try
            {
                // SqlConnection connection;

                // Use Azure AD authentication
                if (!string.IsNullOrWhiteSpace(_tenantId) && !string.IsNullOrWhiteSpace(_clientId) && !string.IsNullOrWhiteSpace(_clientSecret) && !string.IsNullOrWhiteSpace(_environmentUrl))
                {
                    _logger.LogInfo("Using Azure AD authentication (ClientId/Secret) for SQL connection...");
                    System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                    string connectionString =
                        $"Server={new Uri(_environmentUrl).Host},1433;" +  // Added port 1433
                        "Authentication=Active Directory Service Principal;" +
                        $"User Id={_clientId};" +
                        $"Password={_clientSecret};" +
                        "Database=orgdb;" +  // REQUIRED for Dataverse
                        "Encrypt=True;" +
                        "TrustServerCertificate=False;" +
                        "Connection Timeout=30;";

                    _logger.LogInfo("Testing Dataverse SQL Connection...");

                    using (var connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        _logger.LogSuccess($"Successfully connected to SQL Server: {connection.Database}");
                        _logger.LogInfo($"Server Version: {connection.ServerVersion}");

                    }                  
                }
              
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to establish SQL connection", ex);
                throw;
            }
        }

        public void TestConnection2()
        {


            try
            {

            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine($"❌ SQL Error #{sqlEx.Number}: {sqlEx.Message}");
                //  HandleSqlError(sqlEx);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
                Console.WriteLine($"Exception Type: {ex.GetType().FullName}");
            }
        }



        /// <summary>
        /// Establishes connection to Dynamics 365 CRM
        /// </summary>
        public void Connect()
        {
            _logger.LogInfo("Connecting to Dynamics 365 CRM...");

            try
            {
                // Set extended timeouts for large batch operations
                ServiceClient.MaxConnectionTimeout = TimeSpan.FromMinutes(5);
                
                // Add timeout to connection string
                string connectionStringWithTimeout = _connectionString;
                if (!_connectionString.Contains("Timeout="))
                {
                    connectionStringWithTimeout += ";Timeout=00:10:00"; // 10 minute timeout
                }
                
                _serviceClient = new ServiceClient(connectionStringWithTimeout);
                
                // Optimize for better performance
                if (_serviceClient != null)
                {
                    _serviceClient.DisableCrossThreadSafeties = true;
                }

                if (_serviceClient == null || !_serviceClient.IsReady)
                {
                    var errorMessage = _serviceClient?.LastError ?? "Unknown connection error";
                    throw new Exception($"Failed to connect to CRM: {errorMessage}");
                }

                _logger.LogSuccess($"Successfully connected to CRM: {_serviceClient.ConnectedOrgFriendlyName}");
                _logger.LogInfo($"Organization: {_serviceClient.ConnectedOrgUniqueName}");
                _logger.LogInfo($"CRM Version: {_serviceClient.ConnectedOrgVersion}");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to establish CRM connection", ex);
                throw;
            }
        }

        /// <summary>
        /// Fetches records for a given entity within the date range
        /// </summary>
        public List<EntityReference> FetchRecords(string entityName, DateTime? startDate, DateTime? endDate, int pageNumber, string? pagingCookie = null)
        {
            if (_serviceClient == null || !_serviceClient.IsReady)
            {
                throw new InvalidOperationException("CRM connection is not established");
            }

            var records = new List<EntityReference>();

            try
            {
                // Build query with date filters
                var query = new QueryExpression(entityName)
                {
                    ColumnSet = new ColumnSet(false), // Only fetch IDs for performance
                    Criteria = new FilterExpression(LogicalOperator.And)
                };

                // Add date range filters
                // Add date filters only if dates are provided
                if (startDate.HasValue && endDate.HasValue)
                {
                    query.Criteria.AddCondition("createdon", ConditionOperator.GreaterEqual, startDate.Value);
                    query.Criteria.AddCondition("createdon", ConditionOperator.LessEqual, endDate.Value);
                }

                // Configure pagination
                query.PageInfo = new PagingInfo
                {
                    Count = _pageSize,
                    PageNumber = pageNumber,
                    PagingCookie = pagingCookie
                };

                _logger.LogDebug($"Fetching page {pageNumber} for entity '{entityName}'...");

                // Execute query with retry logic
                EntityCollection result = ExecuteWithRetry(() =>
                {
                    return _serviceClient.RetrieveMultiple(query);
                });

                if (result != null && result.Entities != null)
                {
                    foreach (var entity in result.Entities)
                    {
                        records.Add(entity.ToEntityReference());
                    }

                    _logger.LogInfo($"Page {pageNumber}: Fetched {records.Count} records from '{entityName}'");
                }

                return records;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching records from '{entityName}' (Page {pageNumber})", ex);
                throw;
            }
        }

        /// <summary>
        /// Fetches all records for a given entity within the date range - Uses SQL or API based on configuration
        /// </summary>
        public List<EntityReference> FetchAllRecords(string entityName, string EntityIdColumn, DateTime? startDate, DateTime? endDate, string? additionalQuery = null, Models.QueryType queryType = Models.QueryType.Standard)
        {
            // Handle custom query types first
            if (queryType == Models.QueryType.SQL && !string.IsNullOrWhiteSpace(additionalQuery))
            {
                _logger.LogInfo($"Using custom SQL query for '{entityName}'");
                return FetchAllRecordsFromSql(entityName, EntityIdColumn, startDate, endDate, additionalQuery, true);
            }
            else if (queryType == Models.QueryType.FetchXML && !string.IsNullOrWhiteSpace(additionalQuery))
            {
                _logger.LogInfo($"Using custom FetchXML query for '{entityName}'");
                return FetchAllRecordsFromFetchXml(entityName, additionalQuery);
            }
            // Standard queries with optional filters
            else if (_useSqlForFetch)
            {
                _logger.LogInfo($"Using SQL for fetching records from '{entityName}'");
                return FetchAllRecordsFromSql(entityName, EntityIdColumn, startDate, endDate, additionalQuery, false);
            }
            else
            {
                _logger.LogInfo($"Using Dynamics API for fetching records from '{entityName}'");
                return FetchAllRecordsFromDynamicsApi(entityName, startDate, endDate, additionalQuery);
            }
        }

        /// <summary>
        /// Adds dbo schema prefix to Dynamics table names in SQL queries if not already present
        /// </summary>
        private string EnsureSchemaQualification(string sqlQuery)
        {
            // This is a simple implementation that adds dbo. prefix to common patterns
            // For more complex queries, consider using a SQL parser
            
            // Pattern: FROM tablename -> FROM dbo.tablename (if not already qualified)
            sqlQuery = System.Text.RegularExpressions.Regex.Replace(
                sqlQuery, 
                @"\bFROM\s+(?!dbo\.)([a-zA-Z_][a-zA-Z0-9_]*)\b",
                "FROM dbo.$1",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            
            // Pattern: JOIN tablename -> JOIN dbo.tablename (if not already qualified)
            sqlQuery = System.Text.RegularExpressions.Regex.Replace(
                sqlQuery,
                @"\bJOIN\s+(?!dbo\.)([a-zA-Z_][a-zA-Z0-9_]*)\b",
                "JOIN dbo.$1",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            
            return sqlQuery;
        }

        /// <summary>
        /// Fetches records from Dynamics SQL database with Azure AD authentication
        /// </summary>
        private List<EntityReference> FetchAllRecordsFromSql(string entityName, string entityIdColumn, DateTime? startDate, DateTime? endDate, string? additionalQuery = null, bool isFullCustomQuery = false)
        {
            var allRecords = new List<EntityReference>();

            var dateRangeLog = startDate.HasValue && endDate.HasValue 
                ? $"between {startDate.Value:yyyy-MM-dd} and {endDate.Value:yyyy-MM-dd}" 
                : "with custom query";
            _logger.LogInfo($"Starting SQL fetch from '{entityName}' {dateRangeLog}");
            if (!string.IsNullOrWhiteSpace(additionalQuery))
            {
                _logger.LogInfo($"Additional query filter: {additionalQuery}");
            }

            try
            {
              //  SqlConnection connection;

                // Use Azure AD authentication if credentials are provided
                if (!string.IsNullOrWhiteSpace(_tenantId) && !string.IsNullOrWhiteSpace(_clientId) && !string.IsNullOrWhiteSpace(_clientSecret))
                {
                    _logger.LogInfo("Using Azure AD authentication (ClientId/Secret) for SQL connection...");

                  //  _logger.LogInfo("Using Azure AD authentication (ClientId/Secret) for SQL connection...");
                    _connectionStringSQL =
                      $"Server={new Uri(_environmentUrl!).Host},1433;" +  // Added port 1433
                      "Authentication=Active Directory Service Principal;" +
                      $"User Id={_clientId};" +
                      $"Password={_clientSecret};" +
                      "Database=orgdb;" +  // REQUIRED for Dataverse
                      "Encrypt=True;" +
                      "TrustServerCertificate=False;" +
                      "Connection Timeout=30;";

                            }
                else
                {
                    _logger.LogInfo("Using SQL connection string authentication (fallback)...");
                  //  connection = new SqlConnection(_sqlConnectionString);
                }

                using (var connection = new SqlConnection(_connectionStringSQL))
                {
                    string query = string.Empty;
                    bool useParameterizedDates = true;

                    connection.Open();

                    // Check if this is a full custom SQL query
                    if (isFullCustomQuery && !string.IsNullOrWhiteSpace(additionalQuery))
                    {
                        // Full custom SQL query provided - ensure schema qualification
                        query = EnsureSchemaQualification(additionalQuery);
                        useParameterizedDates = false; // Custom queries handle their own parameters
                        _logger.LogInfo("Using full custom SQL query from additionalquery field");
                    }
                    else if (!string.IsNullOrWhiteSpace(additionalQuery) && 
                        additionalQuery.Trim().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
                    {
                        // Legacy support: detect SELECT in additionalQuery for backward compatibility
                        query = EnsureSchemaQualification(additionalQuery);
                        useParameterizedDates = false;
                        _logger.LogInfo("Using full custom SQL query (auto-detected from SELECT keyword)");
                    }
                    else
                    {
                        // Build standard query with optional WHERE clause additions
                        string tableName = $"{entityName}";
                        string whereClause = "";
                        
                        // Add date filter only if dates are provided
                        if (startDate.HasValue && endDate.HasValue)
                        {
                            whereClause = "WHERE CreatedOn >= @StartDate AND CreatedOn < @EndDate";
                            
                            if (!string.IsNullOrWhiteSpace(additionalQuery))
                            {
                                // Append additional query (e.g., "AND owningbusinessunit = 'guid'")
                                whereClause += $" AND {additionalQuery}";
                            }
                        }
                        else if (!string.IsNullOrWhiteSpace(additionalQuery))
                        {
                            // Only additionalQuery, no date filter
                            whereClause = $"WHERE {additionalQuery}";
                        }

                        if (string.IsNullOrWhiteSpace(entityIdColumn))
                        {
                            query = $@"
                        SELECT {entityName}id
                        FROM {tableName} WITH (NOLOCK)
                        {whereClause}";
                        }
                        else
                        {
                            query = $@"
                        SELECT {entityIdColumn}
                        FROM {tableName} WITH (NOLOCK)
                        {whereClause}";
                        }
                    }

                    using (var command = new SqlCommand(query, connection))
                    {
                        // Only add date parameters if using standard query and dates are provided
                        if (useParameterizedDates && startDate.HasValue && endDate.HasValue)
                        {
                            command.Parameters.AddWithValue("@StartDate", startDate.Value);
                            command.Parameters.AddWithValue("@EndDate", endDate.Value);
                        }
                        
                        command.CommandTimeout = 300; // 5 minutes timeout

                        _logger.LogInfo($"Executing SQL query: {query}");

                        using (var reader = command.ExecuteReader())
                        {
                            int count = 0;
                            while (reader.Read())
                            {
                                Guid recordId = reader.GetGuid(0);
                                allRecords.Add(new EntityReference(entityName, recordId));
                                count++;

                                // Log progress every 1000 records
                                if (count % 1000 == 0)
                                {
                                    _logger.LogInfo($"Fetched {count} records so far...");
                                }
                            }
                        }
                    }
                }

                _logger.LogSuccess($"SQL fetch completed: Total {allRecords.Count} records found for table : {entityName}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching records from SQL", ex);
                throw;
            }

            return allRecords;
        }

        /// <summary>
        /// Fetches records using standard Dataverse API with optional additional query filters
        /// </summary>
        private List<EntityReference> FetchAllRecordsFromDynamicsApi(string entityName, DateTime? startDate, DateTime? endDate, string? additionalQuery = null)
        {
            var allRecords = new List<EntityReference>();
            int pageNumber = 1;
            string? pagingCookie = null;
            bool moreRecords = true;

            var dateRangeLog = startDate.HasValue && endDate.HasValue 
                ? $"between {startDate.Value:yyyy-MM-dd} and {endDate.Value:yyyy-MM-dd}" 
                : "with custom query";
            _logger.LogInfo($"Starting API fetch from '{entityName}' {dateRangeLog}");
            if (!string.IsNullOrWhiteSpace(additionalQuery))
            {
                _logger.LogInfo($"Additional query filter: {additionalQuery}");
            }

            while (moreRecords)
            {
                try
                {
                    var query = new QueryExpression(entityName)
                    {
                        ColumnSet = new ColumnSet(false),
                        Criteria = new FilterExpression(LogicalOperator.And)
                    };

                    // Add date filters only if dates are provided
                    if (startDate.HasValue && endDate.HasValue)
                    {
                        query.Criteria.AddCondition("createdon", ConditionOperator.GreaterEqual, startDate.Value);
                        query.Criteria.AddCondition("createdon", ConditionOperator.LessEqual, endDate.Value);
                    }

                    // Add additional query filters if provided
                    // Note: additionalQuery parsing would need to be implemented based on your specific needs
                    // For now, this is a placeholder for the basic implementation

                    query.PageInfo = new PagingInfo
                    {
                        Count = _pageSize,
                        PageNumber = pageNumber,
                        PagingCookie = pagingCookie
                    };

                    EntityCollection result = ExecuteWithRetry(() =>
                    {
                        return _serviceClient!.RetrieveMultiple(query);
                    });

                    if (result != null && result.Entities != null)
                    {
                        foreach (var entity in result.Entities)
                        {
                            allRecords.Add(entity.ToEntityReference());
                        }

                        _logger.LogInfo($"Page {pageNumber}: Fetched {result.Entities.Count} records (Total so far: {allRecords.Count})");

                        moreRecords = result.MoreRecords;
                        if (moreRecords)
                        {
                            pageNumber++;
                            pagingCookie = result.PagingCookie;
                        }
                    }
                    else
                    {
                        moreRecords = false;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error during pagination on page {pageNumber}", ex);
                    throw;
                }
            }

            _logger.LogSuccess($"API fetch completed: Total {allRecords.Count} records found for table: {entityName}");
            return allRecords;
        }

        /// <summary>
        /// Fetches records using FetchXML query
        /// </summary>
        private List<EntityReference> FetchAllRecordsFromFetchXml(string entityName, string fetchXml)
        {
            if (_serviceClient == null || !_serviceClient.IsReady)
            {
                throw new InvalidOperationException("CRM connection is not established");
            }

            var allRecords = new List<EntityReference>();
            
            _logger.LogInfo($"Starting FetchXML query for entity '{entityName}'");
            _logger.LogDebug($"FetchXML: {fetchXml}");

            try
            {
                int pageNumber = 1;
                string? pagingCookie = null;
                bool moreRecords = true;

                // Parse and modify FetchXML to add paging if not already present
                string pagedFetchXml = fetchXml;

                while (moreRecords)
                {
                    // Add paging to FetchXML
                    pagedFetchXml = CreatePagedFetchXml(fetchXml, pageNumber, _pageSize, pagingCookie);

                    _logger.LogDebug($"Fetching page {pageNumber}...");

                    // Execute FetchXML query
                    var fetchExpression = new FetchExpression(pagedFetchXml);
                    EntityCollection result = ExecuteWithRetry(() =>
                    {
                        return _serviceClient.RetrieveMultiple(fetchExpression);
                    });

                    if (result != null && result.Entities != null)
                    {
                        foreach (var entity in result.Entities)
                        {
                            allRecords.Add(entity.ToEntityReference());
                        }

                        _logger.LogInfo($"Page {pageNumber}: Fetched {result.Entities.Count} records (Total so far: {allRecords.Count})");

                        moreRecords = result.MoreRecords;
                        if (moreRecords)
                        {
                            pageNumber++;
                            pagingCookie = result.PagingCookie;
                        }
                    }
                    else
                    {
                        moreRecords = false;
                    }
                }

                _logger.LogSuccess($"FetchXML query completed: Total {allRecords.Count} records found");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error executing FetchXML query", ex);
                throw;
            }

            return allRecords;
        }

        /// <summary>
        /// Creates paged FetchXML from base FetchXML
        /// </summary>
        private string CreatePagedFetchXml(string fetchXml, int pageNumber, int pageSize, string? pagingCookie)
        {
            // Remove existing page/count attributes if present
            var xml = System.Xml.Linq.XDocument.Parse(fetchXml);
            var fetchElement = xml.Root;

            if (fetchElement != null)
            {
                // Set or update count and page
                fetchElement.SetAttributeValue("count", pageSize);
                fetchElement.SetAttributeValue("page", pageNumber);

                // Add paging cookie if provided
                if (!string.IsNullOrEmpty(pagingCookie))
                {
                    fetchElement.SetAttributeValue("paging-cookie", pagingCookie);
                }
            }

            return xml.ToString();
        }

        /// <summary>
        /// Fetches records using standard Dataverse API
        /// </summary>
        private List<EntityReference> FetchAllRecordsFromApi(string entityName, DateTime startDate, DateTime endDate)
        {
            var allRecords = new List<EntityReference>();
            int pageNumber = 1;
            string? pagingCookie = null;
            bool moreRecords = true;

            _logger.LogInfo($"Starting API fetch from '{entityName}' between {startDate:yyyy-MM-dd} and {endDate:yyyy-MM-dd}");

            while (moreRecords)
            {
                try
                {
                    var query = new QueryExpression(entityName)
                    {
                        ColumnSet = new ColumnSet(false),
                        Criteria = new FilterExpression(LogicalOperator.And)
                    };

                    query.Criteria.AddCondition("createdon", ConditionOperator.GreaterEqual, startDate);
                    query.Criteria.AddCondition("createdon", ConditionOperator.LessEqual, endDate);

                    query.PageInfo = new PagingInfo
                    {
                        Count = _pageSize,
                        PageNumber = pageNumber,
                        PagingCookie = pagingCookie
                    };

                    EntityCollection result = ExecuteWithRetry(() =>
                    {
                        return _serviceClient!.RetrieveMultiple(query);
                    });

                    if (result != null && result.Entities != null)
                    {
                        foreach (var entity in result.Entities)
                        {
                            allRecords.Add(entity.ToEntityReference());
                        }

                        _logger.LogInfo($"Page {pageNumber}: Fetched {result.Entities.Count} records (Total so far: {allRecords.Count})");

                        moreRecords = result.MoreRecords;
                        if (moreRecords)
                        {
                            pageNumber++;
                            pagingCookie = result.PagingCookie;
                        }
                    }
                    else
                    {
                        moreRecords = false;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error during pagination on page {pageNumber}", ex);
                    throw;
                }
            }

            _logger.LogSuccess($"API fetch completed: Total {allRecords.Count} records found");
            return allRecords;
        }

        /// <summary>
        /// Deletes records in batches using ExecuteMultiple with parallel processing
        /// </summary>
        public int DeleteRecordsInBatch(List<EntityReference> records)
        {
            if (_serviceClient == null || !_serviceClient.IsReady)
            {
                throw new InvalidOperationException("CRM connection is not established");
            }

            if (records == null || records.Count == 0)
            {
                return 0;
            }

            // Split records into batches
            var batches = new List<(int batchNumber, List<EntityReference> records)>();
            int batchNumber = 1;
            
            for (int i = 0; i < records.Count; i += _batchSize)
            {
                var batch = records.Skip(i).Take(_batchSize).ToList();
                batches.Add((batchNumber, batch));
                batchNumber++;
            }

            _logger.LogInfo($"Processing {batches.Count} batches in parallel with max degree of parallelism: {_parallelThreads}");
            _logger.LogWarning($"Note: Using {_parallelThreads} parallel threads for stable processing");

            // Process batches in parallel with configurable limit
            int totalDeleted = 0;
            int failedBatches = 0;
            object lockObject = new object();

            Parallel.ForEach(batches, 
                new ParallelOptions { MaxDegreeOfParallelism = _parallelThreads },
                batch =>
                {
                    try
                    {
                        _logger.LogInfo($"Batch {batch.batchNumber}: Deleting {batch.records.Count} records...");

                        int deleted = DeleteBatch(batch.records);
                        
                        lock (lockObject)
                        {
                            totalDeleted += deleted;
                        }

                        _logger.LogSuccess($"Batch {batch.batchNumber}: Successfully deleted {deleted} records");
                        
                        // Add small delay between batches to prevent overwhelming server
                        Thread.Sleep(2000); // 2 second delay
                    }
                    catch (Exception ex)
                    {
                        lock (lockObject)
                        {
                            failedBatches++;
                        }
                        _logger.LogError($"Error deleting batch {batch.batchNumber}", ex);
                        _logger.LogWarning($"Failed batches so far: {failedBatches}. Continuing with remaining batches...");
                        // Don't throw - continue with other batches
                    }
                });

            if (failedBatches > 0)
            {
                _logger.LogWarning($"Completed with {failedBatches} failed batch(es) out of {batches.Count} total batches");
            }

            return totalDeleted;
        }

        /// <summary>
        /// Deletes a single batch of records using ExecuteMultiple
        /// </summary>
        private int DeleteBatch(List<EntityReference> batch)
        {
            var multipleRequest = new ExecuteMultipleRequest
            {
                Settings = new ExecuteMultipleSettings
                {
                    ContinueOnError = true,
                    ReturnResponses = true  // Changed to true to capture detailed errors
                },
                Requests = new OrganizationRequestCollection()
            };

            foreach (var record in batch)
            {
                var deleteRequest = new DeleteRequest { Target = record };
                
                // Bypass custom plugin execution and workflows for better performance
                // Note: Requires "prvBypassCustomPlugins" and "prvBypassCustomBusinessLogic" privileges
                deleteRequest.Parameters.Add("BypassCustomPluginExecution", true);
                deleteRequest.Parameters.Add("BypassBusinessLogicExecution", true);
                
                multipleRequest.Requests.Add(deleteRequest);
            }

            ExecuteMultipleResponse multipleResponse = ExecuteWithRetry(() =>
            {
                return  (ExecuteMultipleResponse)_serviceClient!.Execute(multipleRequest);
            });

            // Check for any faults in the response and log to separate error file
            int failureCount = 0;
            
            foreach (var responseItem in multipleResponse.Responses)
            {
                if (responseItem.Fault != null)
                {
                    failureCount++;
                    
                    // Get the failed record details
                    var failedRequest = multipleRequest.Requests[responseItem.RequestIndex] as DeleteRequest;
                    if (failedRequest != null)
                    {
                        var entityRef = failedRequest.Target;
                        LogDeletionError(entityRef.LogicalName, entityRef.Id, responseItem.Fault);
                    }
                }
            }
            
            int successCount = batch.Count - failureCount;

            if (failureCount > 0)
            {
                _logger.LogWarning($"Batch completed with {failureCount} failures out of {batch.Count} records");
                _logger.LogWarning($"Check deletion_errors.log for detailed error information");
            }

            return successCount;
        }

        // Static lock object for thread-safe file access
        private static readonly object _errorLogLock = new object();

        /// <summary>
        /// Logs deletion errors to a separate error log file (thread-safe)
        /// </summary>
        private void LogDeletionError(string entityName, Guid recordId, OrganizationServiceFault fault)
        {
            try
            {
                string errorLogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "deletion_errors.log");
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                
                string errorEntry = $"[{timestamp}] Entity: {entityName} | RecordId: {recordId} | ErrorCode: {fault.ErrorCode} | Message: {fault.Message}";
                
                if (!string.IsNullOrEmpty(fault.TraceText))
                {
                    errorEntry += $" | Trace: {fault.TraceText}";
                }
                
                errorEntry += Environment.NewLine;
                
                // Thread-safe file writing
                lock (_errorLogLock)
                {
                    File.AppendAllText(errorLogPath, errorEntry);
                }
                
                // Also log to main logger
                _logger.LogError($"Deletion failed - Entity: {entityName}, ID: {recordId}, Error: {fault.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to write to deletion error log: {ex.Message}");
            }
        }

        /// <summary>
        /// Executes an operation with retry logic for transient errors and throttling
        /// </summary>
        private T ExecuteWithRetry<T>(Func<T> operation)
        {
            int attempt = 0;
            Exception? lastException = null;

            while (attempt < _retryCount)
            {
                try
                {
                    return operation();
                }
                catch (Exception ex)
                {
                    attempt++;
                    lastException = ex;

                    if (IsTransientError(ex) && attempt < _retryCount)
                    {
                        int delayMs = 0;
                        
                        // Check for throttling errors - need longer delays
                        if (IsThrottlingError(ex))
                        {
                            // Exponential backoff for throttling: 5s, 15s, 30s
                            delayMs = attempt == 1 ? 5000 : attempt == 2 ? 15000 : 30000;
                            _logger.LogWarning($"API THROTTLING detected. Waiting {delayMs/1000}s before retry {attempt} of {_retryCount}");
                            _logger.LogWarning($"Throttling details: {ex.Message}");
                        }
                        else
                        {
                            // Normal transient errors - minimal delay
                            delayMs = 500;
                            _logger.LogWarning($"Transient error detected. Retry attempt {attempt} of {_retryCount} after {delayMs}ms");
                            _logger.LogDebug($"Error: {ex.Message}");
                        }
                        
                        Thread.Sleep(delayMs);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (lastException != null)
            {
                _logger.LogError($"Operation failed after {attempt} attempts", lastException);
                throw lastException;
            }
            
            throw new Exception("Operation failed with unknown error");
        }

        /// <summary>
        /// Determines if an error is a throttling error requiring longer backoff
        /// </summary>
        private bool IsThrottlingError(Exception ex)
        {
            string message = ex.Message.ToLower();
            
            return message.Contains("combined execution time") ||
                   message.Contains("exceeded limit") ||
                   message.Contains("throttl") ||
                   message.Contains("too many requests") ||
                   message.Contains("429") ||
                   message.Contains("rate limit");
        }

        /// <summary>
        /// Determines if an error is transient and can be retried
        /// </summary>
        private bool IsTransientError(Exception ex)
        {
            // Check for common transient error patterns
            string message = ex.Message.ToLower();
            string exceptionType = ex.GetType().Name.ToLower();

            return message.Contains("timeout") ||
                   message.Contains("throttl") ||
                   message.Contains("service unavailable") ||
                   message.Contains("too many requests") ||
                   message.Contains("network") ||
                   message.Contains("connection") ||
                   message.Contains("exceeded limit") ||
                   message.Contains("error occurred while sending") ||
                   exceptionType.Contains("communication") ||
                   exceptionType.Contains("timeout") ||
                   ex is System.Net.WebException ||
                   ex is System.Net.Http.HttpRequestException;
        }

        /// <summary>
        /// Tests the connection to CRM
        /// </summary>
        public bool TestConnection()
        {
            try
            {
                if (_serviceClient == null || !_serviceClient.IsReady)
                {
                    return false;
                }

                // Try to execute a simple WhoAmI request
                var response = _serviceClient.Execute(new Microsoft.Crm.Sdk.Messages.WhoAmIRequest());
                return response != null;
            }
            catch (Exception ex)
            {
                _logger.LogError("Connection test failed", ex);
                return false;
            }
        }

        public void Dispose()
        {
            if (_serviceClient != null)
            {
                _logger.LogInfo("Disposing CRM connection...");
                _serviceClient.Dispose();
                _serviceClient = null;
            }
        }
    }
}
