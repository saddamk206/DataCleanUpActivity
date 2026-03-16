# Quick Start Guide

## Getting Started in 5 Minutes

### Step 1: Build the Application
```powershell
cd C:\Code\DataCleanUpActivity
dotnet restore
dotnet build --configuration Release
```

### Step 2: Navigate to Output
```powershell
cd bin\Release\net6.0-windows
```

### Step 3: Run the Application
```powershell
.\DataCleanUpActivity.exe
```

### Step 4: Use the GUI

1. **Browse for CSV File**
   - Click the "Browse..." button
   - Select your CSV file containing deletion instructions
   - Or manually enter the path: `C:\Data\deletions.csv`

2. **Enter Authentication Details**
   - **Tenant ID**: Your Azure AD tenant ID (GUID)
   - **Client ID**: Your registered app client ID (GUID)
   - **Client Secret**: Your application client secret
   - **Environment URL**: Your Dynamics 365 environment URL

3. **Start the Process**
   - Click the green "Start Process" button
   - Confirm the deletion warning
   - Watch the real-time log output

4. **Monitor Progress**
   - View color-coded logs in real-time
   - Check the status label for current state
   - Use Cancel button if needed

5. **Review Results**
   - Completion dialog shows statistics
   - Log file automatically saved to `Logs\` folder
   - Use "Save Log" button to export log

## CSV File Format

Create a CSV file with this format:

```csv
sequence,entityname,start date,end date
1,contact,2020-01-01,2020-12-31
2,account,2020-01-01,2020-06-30
3,lead,2019-01-01,2019-12-31
```

A sample file `sample_deletions.csv` is included in the project root.

## Connection String Examples

### OAuth (Username/Password)
```
AuthType=OAuth;Username=user@contoso.com;Password=YourPassword;Url=https://contoso.crm.dynamics.com;AppId=51f81489-12ee-4a9e-aaae-a2591f45987d;RedirectUri=app://58145B91-0C36-4500-8554-080854F2AC97;LoginPrompt=Auto
```

### Client Secret (Service Principal)
```
AuthType=ClientSecret;Url=https://contoso.crm.dynamics.com;ClientId=YOUR_CLIENT_ID;ClientSecret=YOUR_CLIENT_SECRET
```

## Application Window

```
┌──────────────────────────────────────────────────────┐
│  Dynamics 365 Data Clean Up Activity                 │
├──────────────────────────────────────────────────────┤
│  CSV File Path: [________________] [Browse...]       │
│  Tenant ID: [__________]  Client ID: [__________]    │
│  Client Secret: [______________________________]     │
│  Environment URL: [______________________________]   │
│                                                       │
│  [Start Process]  [Cancel]                           │
│  Status: Ready                                        │
│                                                       │
│  Execution Log:                                       │
│  ┌────────────────────────────────────────────────┐ │
│  │ [INFO] Application started...                  │ │
│  │ [SUCCESS] Connected to CRM...                  │ │
│  │ [INFO] Processing sequence 1...                │ │
│  └────────────────────────────────────────────────┘ │
│  [Clear Log]  [Save Log]                             │
└──────────────────────────────────────────────────────┘
```

## Features Highlight

✅ **No Command Line Required** - Pure Windows GUI application
✅ **File Browser** - Easy CSV file selection
✅ **ClientSecret Authentication** - Secure Azure AD app authentication
✅ **Real-time Logs** - Color-coded output during execution
✅ **Progress Tracking** - Visual progress bar and status updates
✅ **Cancellable** - Stop the process at any time
✅ **Auto-save Logs** - All executions logged to file
✅ **Safety Confirmations** - Warnings before destructive operations

## Configuration (Optional)

Edit `App.config` in the application directory to customize:

```xml
<appSettings>
  <add key="RetryCount" value="3"/>
  <add key="DelayBetweenPages" value="200"/>
  <add key="PageSize" value="100"/>
  <add key="BatchSize" value="100"/>
</appSettings>
```

## Log Files

Logs are automatically saved to:
```
<ApplicationDirectory>\Logs\DeletionLog_YYYYMMDD_HHMMSS.log
```

You can also save the current log using the "Save Log" button in the UI.

## Troubleshooting

**Application won't start:**
- Install .NET 6.0 Desktop Runtime from Microsoft
- Check Windows version (Windows 10/11 required)

**Can't connect to CRM:**
- Verify Tenant ID, Client ID, and Client Secret are correct
- Check network connectivity to Dynamics 365
- Ensure app registration has proper API permissions
- Verify Environment URL is correct

**CSV parsing errors:**
- Verify CSV has header row
- Check date format (yyyy-MM-dd recommended)
- Ensure no special characters in entity names

## Next Steps

1. ✅ Build and run the application
2. ✅ Test with sample CSV in non-production environment
3. ✅ Verify connection to your Dynamics 365 instance
4. ✅ Review logs after test run
5. ✅ Run production deletions during off-peak hours

## Support

For detailed documentation, see:
- `README.md` - Complete feature documentation
- `UI_GUIDE.md` - Detailed UI component guide
- `sample_deletions.csv` - Example CSV format

---

**⚠️ IMPORTANT:** This tool permanently deletes data. Always:
- Test in non-production environment first
- Ensure you have proper backups
- Review CSV instructions carefully
- Monitor the logs during execution
