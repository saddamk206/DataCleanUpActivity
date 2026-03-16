using System.Text;

namespace DataCleanUpActivity.Services
{
    /// <summary>
    /// Handles logging to both file and GUI with different severity levels
    /// </summary>
    public class Logger : IDisposable
    {
        private readonly string _logFilePath;
        private readonly StreamWriter _logWriter;
        private readonly object _lockObject = new object();
        private readonly DateTime _startTime;
        private readonly MainForm? _mainForm;

        public Logger(MainForm? mainForm = null, string? logDirectory = null, string? csvFileName = null)
        {
            _mainForm = mainForm;
            _startTime = DateTime.Now;
            
            // Use custom log directory or default to Logs folder in application directory
            string logsDirectory;
            if (!string.IsNullOrWhiteSpace(logDirectory))
            {
                logsDirectory = logDirectory;
            }
            else
            {
                logsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            }
            
            Directory.CreateDirectory(logsDirectory);

            // Create log file name based on CSV file name if provided
            string timestamp = _startTime.ToString("yyyyMMdd_HHmmss");
            string logFileName;
            
            if (!string.IsNullOrWhiteSpace(csvFileName))
            {
                // Extract file name without extension from CSV file path
                string csvFileNameOnly = Path.GetFileNameWithoutExtension(csvFileName);
                logFileName = $"{csvFileNameOnly}_log_{timestamp}.log";
            }
            else
            {
                logFileName = $"DeletionLog_{timestamp}.log";
            }
            
            _logFilePath = Path.Combine(logsDirectory, logFileName);

            _logWriter = new StreamWriter(_logFilePath, append: true, Encoding.UTF8)
            {
                AutoFlush = true
            };

            LogInfo("=".PadRight(80, '='));
            LogInfo($"Data Clean Up Activity - Log Started at {_startTime:yyyy-MM-dd HH:mm:ss}");
            LogInfo("=".PadRight(80, '='));
        }

        /// <summary>
        /// Logs an informational message
        /// </summary>
        public void LogInfo(string message)
        {
            Log("INFO", message, ConsoleColor.White);
        }

        /// <summary>
        /// Logs a warning message
        /// </summary>
        public void LogWarning(string message)
        {
            Log("WARN", message, ConsoleColor.Yellow);
        }

        /// <summary>
        /// Logs an error message with optional exception
        /// </summary>
        public void LogError(string message, Exception? ex = null)
        {
            Log("ERROR", message, ConsoleColor.Red);
            
            if (ex != null)
            {
                Log("ERROR", $"Exception Type: {ex.GetType().Name}", ConsoleColor.Red);
                Log("ERROR", $"Exception Message: {ex.Message}", ConsoleColor.Red);
                Log("ERROR", $"Stack Trace: {ex.StackTrace}", ConsoleColor.Red);

                if (ex.InnerException != null)
                {
                    Log("ERROR", $"Inner Exception: {ex.InnerException.Message}", ConsoleColor.Red);
                    Log("ERROR", $"Inner Stack Trace: {ex.InnerException.StackTrace}", ConsoleColor.Red);
                }
            }
        }

        /// <summary>
        /// Logs a success message
        /// </summary>
        public void LogSuccess(string message)
        {
            Log("SUCCESS", message, ConsoleColor.Green);
        }

        /// <summary>
        /// Logs a debug message
        /// </summary>
        public void LogDebug(string message)
        {
            Log("DEBUG", message, ConsoleColor.Gray);
        }

        /// <summary>
        /// Core logging method
        /// </summary>
        private void Log(string level, string message, ConsoleColor color)
        {
            lock (_lockObject)
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                string logMessage = $"[{timestamp}] [{level.PadRight(7)}] {message}";

                // Write to file
                _logWriter.WriteLine(logMessage);

                // Write to GUI if available
                if (_mainForm != null)
                {
                    System.Drawing.Color guiColor = ConsoleColorToDrawingColor(color);
                    _mainForm.LogToUI(logMessage, guiColor);
                }
                else
                {
                    // Write to console with color (fallback for console mode)
                    var originalColor = Console.ForegroundColor;
                    Console.ForegroundColor = color;
                    Console.WriteLine(logMessage);
                    Console.ForegroundColor = originalColor;
                }
            }
        }

        /// <summary>
        /// Converts ConsoleColor to System.Drawing.Color for GUI
        /// </summary>
        private System.Drawing.Color ConsoleColorToDrawingColor(ConsoleColor consoleColor)
        {
            return consoleColor switch
            {
                ConsoleColor.Red => System.Drawing.Color.Red,
                ConsoleColor.Green => System.Drawing.Color.LightGreen,
                ConsoleColor.Yellow => System.Drawing.Color.Yellow,
                ConsoleColor.Gray => System.Drawing.Color.Gray,
                ConsoleColor.White => System.Drawing.Color.White,
                _ => System.Drawing.Color.LightGray
            };
        }

        /// <summary>
        /// Logs a section header
        /// </summary>
        public void LogSection(string sectionName)
        {
            LogInfo("");
            LogInfo($"--- {sectionName} ---");
        }

        /// <summary>
        /// Logs execution summary statistics
        /// </summary>
        public void LogSummary(int totalRowsProcessed, int totalRecordsDeleted, bool success, int failedSequences = 0)
        {
            var endTime = DateTime.Now;
            var duration = endTime - _startTime;

            LogInfo("");
            LogInfo("=".PadRight(80, '='));
            LogInfo("EXECUTION SUMMARY");
            LogInfo("=".PadRight(80, '='));
            LogInfo($"Start Time:              {_startTime:yyyy-MM-dd HH:mm:ss}");
            LogInfo($"End Time:                {endTime:yyyy-MM-dd HH:mm:ss}");
            LogInfo($"Total Duration:          {duration.Hours}h {duration.Minutes}m {duration.Seconds}s");
            LogInfo($"Total Rows Processed:    {totalRowsProcessed}");
            LogInfo($"Total Records Deleted:   {totalRecordsDeleted}");
            
            if (failedSequences > 0)
            {
                LogWarning($"Failed Sequences:        {failedSequences}");
            }

            if (success)
            {
                LogSuccess($"Status:                  COMPLETED SUCCESSFULLY");
            }
            else
            {
                LogError($"Status:                  COMPLETED WITH ERRORS");
            }

            LogInfo("=".PadRight(80, '='));
            LogInfo($"Log file saved to: {_logFilePath}");
        }

        /// <summary>
        /// Logs the masked connection string for security
        /// </summary>
        public void LogConnectionString(string connectionString)
        {
            string maskedConnection = MaskConnectionString(connectionString);
            LogInfo($"Connection String: {maskedConnection}");
        }

        /// <summary>
        /// Masks sensitive information in connection string
        /// </summary>
        private string MaskConnectionString(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                return "[EMPTY]";

            // Mask password and client secret
            var masked = connectionString;
            
            // Mask Password
            if (masked.Contains("Password=", StringComparison.OrdinalIgnoreCase))
            {
                var startIdx = masked.IndexOf("Password=", StringComparison.OrdinalIgnoreCase);
                var endIdx = masked.IndexOf(";", startIdx);
                if (endIdx == -1) endIdx = masked.Length;
                
                var passwordPart = masked.Substring(startIdx, endIdx - startIdx);
                masked = masked.Replace(passwordPart, "Password=***MASKED***");
            }

            // Mask ClientSecret
            if (masked.Contains("ClientSecret=", StringComparison.OrdinalIgnoreCase))
            {
                var startIdx = masked.IndexOf("ClientSecret=", StringComparison.OrdinalIgnoreCase);
                var endIdx = masked.IndexOf(";", startIdx);
                if (endIdx == -1) endIdx = masked.Length;
                
                var secretPart = masked.Substring(startIdx, endIdx - startIdx);
                masked = masked.Replace(secretPart, "ClientSecret=***MASKED***");
            }

            return masked;
        }

        public void Dispose()
        {
            _logWriter?.Dispose();
        }
    }
}
