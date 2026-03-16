# Windows Application UI Guide

## Application Window

The Dynamics 365 Data Clean Up Activity application features a user-friendly Windows Forms interface with the following components:

### Main Window Layout

```
┌─────────────────────────────────────────────────────────────────────────┐
│  Dynamics 365 Data Clean Up Activity                                    │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                           │
│  [Input Parameters]                                                       │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │  CSV File Path: *                                                  │  │
│  │  [____________________________] [Browse...]                         │  │
│  │                                                                     │  │
│  │  Connection String: *                                               │  │
│  │  [___________________________________________________________]      │  │
│  │  [___________________________________________________________]      │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                           │
│  [Start Process]  [Cancel]  [████████████████████] (Progress Bar)       │
│  Status: Ready                                                           │
│                                                                           │
│  [Execution Log]                                                         │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │  Execution Log:                                                    │  │
│  │  ┌─────────────────────────────────────────────────────────────┐  │  │
│  │  │ [2025-12-03 10:15:30] [INFO] Application started...          │  │  │
│  │  │ [2025-12-03 10:15:31] [INFO] Ready to process...             │  │  │
│  │  │ [2025-12-03 10:15:32] [SUCCESS] Connected to CRM...          │  │  │
│  │  │                                                                │  │  │
│  │  │                                                                │  │  │
│  │  └─────────────────────────────────────────────────────────────┘  │  │
│  │  [Clear Log]  [Save Log]                                           │  │
│  └───────────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────────┘
```

## UI Components

### 1. Input Parameters Section
**CSV File Path:**
- Text box for entering the path to your CSV file
- Browse button to open file dialog for easy selection
- Supports drag-and-drop (future enhancement)

**Tenant ID:**
- Text box for Azure AD tenant ID
- GUID format (e.g., aca3c8d6-aa71-4e1a-a10e-03572fc58c0b)

**Client ID:**
- Text box for registered app client ID
- GUID format (e.g., 5131d635-511a-415f-a0e5-38f9efe17cd8)

**Client Secret:**
- Secure text box (masked with asterisks)
- Your application client secret value

**Environment URL:**
- Text box for Dynamics 365 environment URL
- Format: https://[org].crm[N].dynamics.com

### 2. Control Buttons

**Start Process (Green):**
- Initiates the deletion process
- Shows confirmation dialog before starting
- Disabled during execution

**Cancel (Red):**
- Stops the current operation
- Enabled only during execution
- Shows confirmation dialog before cancelling

### 3. Progress Bar
- Marquee-style animation during execution
- Hidden when not processing
- Provides visual feedback of active operation

### 4. Status Label
- Displays current status:
  - "Ready" (Cyan) - Waiting for input
  - "Processing..." (Blue) - Operation in progress
  - "Completed Successfully" (Green) - Success
  - "Completed with Errors" (Orange) - Partial success
  - "Failed" (Red) - Operation failed
  - "Cancelled" (Orange) - User cancelled

### 5. Execution Log Window
- Black background with color-coded text
- Real-time log updates
- Color scheme:
  - **White** - Informational messages
  - **Green** - Success messages
  - **Yellow** - Warnings
  - **Red** - Errors
  - **Cyan** - Section headers
  - **Gray** - Debug messages

**Log Buttons:**
- **Clear Log** - Clears the log window
- **Save Log** - Opens save dialog to export log to file

## Features

### Safety Features
✅ Confirmation dialog before starting deletion
✅ Confirmation dialog before cancelling
✅ Confirmation dialog when closing during execution
✅ Input validation before starting
✅ Connection string masking in saved logs

### Real-time Feedback
✅ Live log output with color coding
✅ Progress bar animation
✅ Status updates
✅ Completion dialogs with statistics

### User Experience
✅ Fixed-size window (no maximize)
✅ Centered on screen
✅ Tab order for easy navigation
✅ Keyboard shortcuts (Enter to start)
✅ File browser integration
✅ Auto-scroll log window

## Workflow

1. **Launch Application**
   - Window opens centered on screen
   - Status shows "Ready"
   - Sample connection string format displayed

2. **Select CSV File**
   - Click "Browse..." button
   - File dialog filters to show only .csv files
   - Selected path populates in text box
   - Log shows confirmation message

3. **Enter Connection String**
   - Paste or type connection string
   - Text is masked for security
   - Can be multi-line for long strings

4. **Start Process**
   - Click "Start Process" button
   - Confirmation dialog appears
   - Click "Yes" to proceed

5. **Monitor Execution**
   - Progress bar animates
   - Real-time logs appear
   - Status updates show progress
   - Cancel button becomes available

6. **View Results**
   - Completion dialog shows statistics
   - Status updates with final result
   - Log shows complete execution details
   - Option to save log file

7. **Save Log (Optional)**
   - Click "Save Log" button
   - Choose location and filename
   - Log exported to text file

## Keyboard Shortcuts

- **Tab** - Navigate between fields
- **Enter** (on Start button) - Begin processing
- **Esc** (during processing) - Cancel operation
- **Ctrl+L** - Clear log (when not processing)
- **Ctrl+S** - Save log

## Error Handling

The application handles errors gracefully with:
- Message boxes for critical errors
- Detailed stack traces in log window
- Option to continue after non-critical errors
- Automatic log file generation for troubleshooting

## Tips

💡 **Test First:** Always test with a small CSV in a non-production environment
💡 **Save Logs:** Use "Save Log" before closing the application
💡 **Monitor Progress:** Watch the log window for any warnings
💡 **Connection String:** Keep your connection string saved securely outside the app
💡 **Backup:** Ensure you have backups before running deletions

## Troubleshooting

**Problem:** Browse button doesn't open file dialog
- **Solution:** Check file permissions on your system

**Problem:** Authentication fails
- **Solution:** Verify Tenant ID, Client ID, and Client Secret are correct
- **Solution:** Ensure the app registration has appropriate permissions

**Problem:** Log window doesn't scroll
- **Solution:** Logs auto-scroll, but you can manually scroll if paused

**Problem:** Application freezes during execution
- **Solution:** Use Cancel button - process runs on background thread

**Problem:** Can't see client secret (all asterisks)
- **Solution:** This is intentional for security - copy from Azure portal
