# Dynamics 365 Data Clean Up Activity

A robust C# Windows Forms application for deleting Dynamics 365 CRM records in batches based on CSV instructions.

## Features

- ✅ CSV-based batch deletion instructions
- ✅ Configurable pagination (100 records per page)
- ✅ Batch delete operations using ExecuteMultiple (100 records per batch)
- ✅ **Hourly progress reports** with detailed statistics
- ✅ **Real-time progress tracking** for each entity
- ✅ Comprehensive logging with file and console output
- ✅ Retry logic for transient CRM errors (configurable, default: 3 retries)
- ✅ Date range filtering (CreatedOn field)
- ✅ Sequential processing based on sequence number
- ✅ Detailed exception handling and stack traces
- ✅ Connection string masking in logs
- ✅ Performance throttling with configurable delays
- ✅ Execution statistics and summary reporting
- ✅ **Custom SQL and FetchXML query support**
- ✅ Parallel batch processing for faster deletion

## Requirements

- .NET 6.0 or higher
- Dynamics 365 / Dataverse instance
- Valid CRM connection credentials
- Appropriate permissions to delete records in target entities

## Installation

1. Clone or download this repository
2. Open the solution in Visual Studio 2022 or later
3. Restore NuGet packages:
   ```
   dotnet restore
   ```
4. Build the solution:
   ```
   dotnet build --configuration Release
   ```

## Configuration

Edit `App.config` to customize behavior:

```xml
<appSettings>
  <add key="RetryCount" value="3"/>
  <add key="DelayBetweenPages" value="200"/>
  <add key="PageSize" value="100"/>
  <add key="BatchSize" value="100"/>
</appSettings>
```

### Configuration Parameters

| Parameter | Description | Default |
|-----------|-------------|---------|
| `RetryCount` | Number of retry attempts for transient errors | 3 |
| `DelayBetweenPages` | Delay in milliseconds between page requests | 200 |
| `PageSize` | Number of records to fetch per page | 100 |
| `BatchSize` | Number of records to delete per batch | 100 |

## Usage

### Running the Application

1. **Launch** the application by double-clicking `DataCleanUpActivity.exe`
2. **Browse** for your CSV file using the "Browse..." button or enter the path manually
3. **Enter** your Azure AD authentication details:
   - Tenant ID (Azure AD tenant GUID)
   - Client ID (registered app client GUID)
   - Client Secret (app client secret value)
   - Environment URL (Dynamics 365 URL)
4. **Click** "Start Process" to begin the deletion operation
5. **Monitor** the progress in the real-time log window

The application provides:
- ✅ GUI input fields for CSV path and authentication credentials
- ✅ Browse button for easy CSV file selection
- ✅ Separate fields for Tenant ID, Client ID, Client Secret, and Environment URL
- ✅ Real-time color-coded log output
- ✅ Progress indicator during execution
- ✅ Cancel button to stop the process
- ✅ Save log functionality
- ✅ Confirmation dialogs for safety

### Authentication Setup

The application uses **Azure AD App Registration with Client Secret** authentication.

#### Required Information:

| Field | Description | Example |
|-------|-------------|---------|
| **Tenant ID** | Your Azure AD tenant ID (GUID) | `aca3c8d6-aa71-4e1a-a10e-03572fc58c0b` |
| **Client ID** | Your registered app client ID (GUID) | `5131d635-511a-415f-a0e5-38f9efe17cd8` |
| **Client Secret** | Your application client secret value | `xxx~xxxxxxxxx+jyCdf8LY03zTCYNz1bbFhTLZHnE=` |
| **Environment URL** | Your Dynamics 365 environment URL | `https://io-sanofi-apac-uat.crm5.dynamics.com` |

#### How to Get These Values:

1. **Azure Portal** → **Azure Active Directory** → **App Registrations**
2. Create or select your app registration
3. Copy the **Application (client) ID** → This is your **Client ID**
4. Copy the **Directory (tenant) ID** → This is your **Tenant ID**
5. Go to **Certificates & secrets** → Create a new client secret → Copy the **value**
6. Go to **API permissions** → Add **Dynamics CRM** → **user_impersonation** permission
7. Grant admin consent for the permissions
8. Your **Environment URL** is your Dynamics 365 organization URL

## CSV Format

The CSV file must contain the following columns:

| Column | Type | Description |
|--------|------|-------------|
| `sequence` | Integer | Order of execution (ascending) |
| `entityname` | String | Logical entity name in CRM |
| `start date` | Date | CreatedOn start date (inclusive) |
| `end date` | Date | CreatedOn end date (inclusive) |

### CSV Example

```csv
sequence,entityname,start date,end date
1,contact,2020-01-01,2020-12-31
2,account,2020-01-01,2020-06-30
3,lead,2019-01-01,2019-12-31
4,opportunity,2018-01-01,2018-12-31
```

### Supported Date Formats

- `yyyy-MM-dd` (ISO format, recommended)
- `MM/dd/yyyy` (US format)
- `dd/MM/yyyy` (European format)

## Processing Logic

For each CSV row (processed in sequence order):

1. **Connect** to Dynamics 365 using the connection string
2. **Fetch** records where:
   - `createdon >= start date`
   - `createdon <= end date`
3. **Paginate** with 100 records per page
4. **Delete** in batches of 100 using ExecuteMultiple
5. **Continue** fetching next pages until all qualifying records are deleted
6. **Track** and log:
   - Records fetched per page
   - Records deleted per batch
   - Total deleted per sequence
   - Execution time per sequence

## Logging

### Log File Location

Logs are saved to: `<ApplicationDirectory>\Logs\DeletionLog_YYYYMMDD_HHMMSS.log`

### Log Contents

- Start and end timestamps
- Input parameters (connection string masked)
- Total CSV rows loaded
- Per-sequence execution details:
  - Page number
  - Records fetched
  - Records deleted
  - Errors and warnings
- Full exception stack traces
- Completion status and summary

### Sample Log Output

```
[2025-12-03 10:15:30.123] [INFO   ] ================================================================================
[2025-12-03 10:15:30.124] [INFO   ] Data Clean Up Activity - Log Started at 2025-12-03 10:15:30
[2025-12-03 10:15:30.125] [INFO   ] ================================================================================
[2025-12-03 10:15:30.126] [INFO   ] 
╔═══════════════════════════════════════════════════════════════════════════╗
║                                                                           ║
║                    DYNAMICS 365 DATA CLEAN UP ACTIVITY                    ║
║                                                                           ║
║                    Batch Record Deletion Tool v1.0                        ║
║                                                                           ║
╚═══════════════════════════════════════════════════════════════════════════╝

[2025-12-03 10:15:30.130] [INFO   ] 
[2025-12-03 10:15:30.131] [INFO   ] --- INPUT PARAMETERS ---
[2025-12-03 10:15:30.132] [INFO   ] CSV File Path: C:\Data\deletions.csv
[2025-12-03 10:15:30.133] [INFO   ] Connection String: AuthType=OAuth;Username=user@contoso.com;Password=***MASKED***;Url=https://contoso.crm.dynamics.com
[2025-12-03 10:15:30.140] [INFO   ] 
[2025-12-03 10:15:30.141] [INFO   ] --- CSV PARSING ---
[2025-12-03 10:15:30.142] [INFO   ] Parsing CSV file: C:\Data\deletions.csv
[2025-12-03 10:15:30.156] [INFO   ] Successfully parsed 4 rows from CSV
[2025-12-03 10:15:30.157] [INFO   ] Validating deletion instructions...
[2025-12-03 10:15:30.158] [INFO   ] Validation complete. 4 valid instructions out of 4 total rows
[2025-12-03 10:15:30.160] [INFO   ] 
[2025-12-03 10:15:30.161] [INFO   ] --- CRM CONNECTION ---
[2025-12-03 10:15:30.162] [INFO   ] Connecting to Dynamics 365 CRM...
[2025-12-03 10:15:32.500] [SUCCESS] Successfully connected to CRM: Contoso Organization
[2025-12-03 10:15:32.501] [INFO   ] Organization: contoso
[2025-12-03 10:15:32.502] [INFO   ] CRM Version: 9.2.23114.00203
[2025-12-03 10:15:32.510] [INFO   ] 
[2025-12-03 10:15:32.511] [INFO   ] --- SEQUENCE 1 ---
[2025-12-03 10:15:32.512] [INFO   ] Sequence: 1, Entity: contact, Period: 2020-01-01 to 2020-12-31
[2025-12-03 10:15:32.513] [INFO   ] Fetching records from entity 'contact'...
[2025-12-03 10:15:33.200] [INFO   ] Page 1: Fetched 100 records (Total: 100)
[2025-12-03 10:15:33.450] [INFO   ] Page 2: Fetched 50 records (Total: 150)
[2025-12-03 10:15:33.451] [INFO   ] Total records to delete: 150
[2025-12-03 10:15:33.452] [INFO   ] Starting batch deletion...
[2025-12-03 10:15:33.453] [INFO   ] Batch 1: Deleting 100 records...
[2025-12-03 10:15:35.100] [SUCCESS] Batch 1: Successfully deleted 100 records
[2025-12-03 10:15:35.101] [INFO   ] Batch 2: Deleting 50 records...
[2025-12-03 10:15:36.200] [SUCCESS] Batch 2: Successfully deleted 50 records
[2025-12-03 10:15:36.201] [SUCCESS] Sequence 1 completed successfully
[2025-12-03 10:15:36.202] [INFO   ] Duration: 3.69 seconds
[2025-12-03 10:15:36.203] [INFO   ] Total records deleted: 150
[2025-12-03 10:15:40.500] [INFO   ] 
[2025-12-03 10:15:40.501] [INFO   ] ================================================================================
[2025-12-03 10:15:40.502] [INFO   ] EXECUTION SUMMARY
[2025-12-03 10:15:40.503] [INFO   ] ================================================================================
[2025-12-03 10:15:40.504] [INFO   ] Start Time:              2025-12-03 10:15:30
[2025-12-03 10:15:40.505] [INFO   ] End Time:                2025-12-03 10:15:40
[2025-12-03 10:15:40.506] [INFO   ] Total Duration:          0h 0m 10s
[2025-12-03 10:15:40.507] [INFO   ] Total Rows Processed:    4
[2025-12-03 10:15:40.508] [INFO   ] Total Records Deleted:   523
[2025-12-03 10:15:40.509] [SUCCESS] Status:                  COMPLETED SUCCESSFULLY
[2025-12-03 10:15:40.510] [INFO   ] ================================================================================
[2025-12-03 10:15:40.511] [INFO   ] Log file saved to: C:\Code\DataCleanUpActivity\bin\Release\net6.0\Logs\DeletionLog_20251203_101530.log
```

## Error Handling

The application handles the following error scenarios:

### CSV Errors
- File not found
- Invalid CSV format
- Missing or invalid columns
- Invalid date formats
- Date range validation failures

### CRM Errors
- Authentication failures
- Connection timeout
- Invalid entity names
- Network errors
- Service unavailable (503)
- Throttling (429)

### Transient Errors (with Retry)
- Network timeouts
- CRM throttling
- Service unavailable
- Connection drops

### Application Errors
- IO exceptions
- Null reference exceptions
- Configuration errors

**Note:** The job continues processing the next sequence even if one fails (unless initialization fails).

## Exit Codes

- `0` - Success
- `1` - Error occurred

## Performance Considerations

### API Limits
- Respects Dynamics 365 API limits
- Configurable delay between page requests (default: 200ms)
- Automatic retry for throttling errors

### Optimization Tips
1. Process during off-peak hours
2. Increase `DelayBetweenPages` if experiencing throttling
3. Monitor logs for performance bottlenecks
4. Consider breaking large deletions into multiple CSV files

## Extensibility

The code is designed to support future enhancements:

### Planned Features
- ✨ Additional filter criteria (beyond CreatedOn)
- ✨ Update/deactivate operations (instead of delete)
- ✨ Task Scheduler integration
- ✨ Azure Automation support
- ✨ JSON-based input format
- ✨ Parallel processing of sequences
- ✨ Email notifications
- ✨ Web dashboard for monitoring

### Extending the Code

#### Adding Custom Filters
Modify `CrmHelper.FetchRecords()` to add additional `FilterExpression` conditions.

#### Supporting Other Operations
Create new methods in `CrmHelper` (e.g., `UpdateRecordsInBatch()`, `DeactivateRecordsInBatch()`).

#### Custom Logging
Implement `ILogger` interface and inject into services.

## Project Structure

```
DataCleanUpActivity/
│
├── Models/
│   └── DeletionInstruction.cs       # CSV row model
│
├── Services/
│   ├── Logger.cs                    # Logging service
│   ├── CsvParser.cs                 # CSV parsing and validation
│   ├── CrmHelper.cs                 # CRM operations
│   └── DeletionEngine.cs            # Orchestration logic
│
├── Program.cs                        # Main entry point
├── App.config                        # Application configuration
├── DataCleanUpActivity.csproj       # Project file
├── sample_deletions.csv             # Sample CSV file
└── README.md                         # This file
```

## Dependencies

- **CsvHelper** (v30.0.1) - CSV parsing
- **Microsoft.PowerPlatform.Dataverse.Client** (v1.1.14) - CRM connectivity
- **Microsoft.Crm.Sdk.Proxy** (v9.0.2.56) - CRM SDK

## Troubleshooting

### Connection Issues
- Verify connection string format
- Check network connectivity to CRM
- Ensure credentials are valid
- Verify AppId and RedirectUri (for OAuth)

### Authentication Failures
- Confirm user has necessary permissions
- Check if MFA is enabled (use app password)
- Verify Azure AD app registration settings

### Throttling Errors
- Increase `DelayBetweenPages` in App.config
- Reduce `PageSize` or `BatchSize`
- Process during off-peak hours

### CSV Parsing Errors
- Verify CSV has header row
- Check date formats
- Ensure no special characters in entity names
- Validate sequence numbers are positive integers

## Security Best Practices

1. **Never commit connection strings** to source control
2. **Use environment variables** or Azure Key Vault for credentials
3. **Mask sensitive data** in logs (automatically done by Logger)
4. **Restrict file system access** to Logs directory
5. **Use service accounts** with minimal required permissions
6. **Enable audit logging** in Dynamics 365

## Support and Contribution

For issues, questions, or contributions, please refer to the project repository.

## License

This project is provided as-is for internal use. Modify as needed for your organization.

## Version History

### v1.0 (2025-12-03)
- Initial release
- CSV-based batch deletion
- Comprehensive logging
- Retry logic and error handling
- Configurable pagination and batching

---

**⚠️ WARNING:** This tool permanently deletes data. Always test in a non-production environment first and ensure you have proper backups.
