using DataCleanUpActivity.Services;
using System.Configuration;

namespace DataCleanUpActivity
{
    public partial class MainForm : Form
    {
        private Logger? _logger;
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _isProcessing = false;
        private bool _useSqlDataSource = true; // Default to SQL

        public MainForm()
        {
            InitializeComponent();
            this.Load += MainForm_Load;
        }

        private void MainForm_Load(object? sender, EventArgs e)
        {
            // Initialize with sample values
         //txtTenantId.Text = "aca3c8d6-aa71-4e1a-a10e-03572fc58c0b";
         //txtClientId.Text = "5131d635-511a-415f-a0e5-38f9efe17cd8";
         //txtClientSecret.Text = "guQc4fiRoiwi9i4+jyCdf8LY03zTCYNz1bbFhTLZHnE=";
         //txtEnvironmentUrl.Text = "https://io-sanofi-apac-uat.crm5.dynamics.com";

            txtTenantId.Text = "8941539b-c8a9-4f72-bed7-8f2781243cbd";
            txtClientId.Text = "a1cb8f75-5887-446a-8428-287df6d57448";
            txtClientSecret.Text = "j_i99OO61-SmD~lT~.IIn3~MXHdxct37pv";
            txtEnvironmentUrl.Text = "https://sanofionecrmsit.crm.dynamics.cn";

            // txtSqlConnectionString.Text = "Server=io-sanofi-apac-uat.crm5.dynamics.com;Database=io-sanofi-india-uat0;Encrypt=true;TrustServerCertificate=false;";
            //Url=https://sanofionecrm.crm.dynamics.cn/api/data/v9.2;TenantId=8941539b-c8a9-4f72-bed7-8f2781243cbd;ClientId=a1cb8f75-5887-446a-8428-287df6d57448;ClientSecret=j_i99OO61-SmD~lT~.IIn3~MXHdxct37pv

            // Set default data source
            radioSql.Checked = true;
            _useSqlDataSource = true;

            // Wire up event handlers
            radioSql.CheckedChanged += radioDataSource_CheckedChanged;
            radioDynamics.CheckedChanged += radioDataSource_CheckedChanged;

            LogToUI("Application started. Please provide CSV file path and authentication credentials.", Color.Cyan);
            LogToUI("Data Source: SQL (default) - Switch to Dynamics API if needed.", Color.Yellow);
            LogToUI("Ready to process deletion instructions.", Color.White);
        }

        private void btnBrowseCsv_Click(object? sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                txtCsvPath.Text = openFileDialog.FileName;
                LogToUI($"Selected CSV file: {openFileDialog.FileName}", Color.LightGreen);
            }
        }

        private void btnBrowseLogDirectory_Click(object? sender, EventArgs e)
        {
            using var folderDialog = new FolderBrowserDialog();
            folderDialog.Description = "Select Log Directory";
            folderDialog.ShowNewFolderButton = true;

            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                txtLogDirectory.Text = folderDialog.SelectedPath;
                LogToUI($"Selected log directory: {folderDialog.SelectedPath}", Color.LightGreen);
            }
        }

        private async void btnStart_Click(object? sender, EventArgs e)
        {
            if (_isProcessing)
            {
                MessageBox.Show("A process is already running. Please wait for it to complete or cancel it.",
                    "Process Running", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validate inputs
            if (string.IsNullOrWhiteSpace(txtCsvPath.Text))
            {
                MessageBox.Show("Please select a CSV file.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!File.Exists(txtCsvPath.Text))
            {
                MessageBox.Show("The selected CSV file does not exist.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtTenantId.Text))
            {
                MessageBox.Show("Please enter a Tenant ID.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtClientId.Text))
            {
                MessageBox.Show("Please enter a Client ID.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtClientSecret.Text))
            {
                MessageBox.Show("Please enter a Client Secret.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtEnvironmentUrl.Text))
            {
                MessageBox.Show("Please enter an Environment URL.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Validate batch size
            if (string.IsNullOrWhiteSpace(txtBatchSize.Text) || !int.TryParse(txtBatchSize.Text, out int batchSize) || batchSize < 1 || batchSize > 1000)
            {
                MessageBox.Show("Please enter a valid Batch Size (1-1000).", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Validate parallel threads
            if (string.IsNullOrWhiteSpace(txtParallelThreads.Text) || !int.TryParse(txtParallelThreads.Text, out int parallelThreads) || parallelThreads < 1 || parallelThreads > 16)
            {
                MessageBox.Show("Please enter a valid Parallel Threads count (1-16).", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Confirm before starting
            var result = MessageBox.Show(
                "WARNING: This will permanently delete records from Dynamics 365.\n\n" +
                "Are you sure you want to proceed?",
                "Confirm Deletion",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
            {
                return;
            }

            // Start processing
            await StartProcessingAsync();
        }

        private async Task StartProcessingAsync()
        {
            _isProcessing = true;
            _cancellationTokenSource = new CancellationTokenSource();

            // Update UI
            btnStart.Enabled = false;
            btnCancel.Enabled = true;
            progressBar.Visible = true;
            progressBar.Style = ProgressBarStyle.Marquee;
            lblStatus.Text = "Status: Processing...";
            lblStatus.ForeColor = Color.Blue;
            txtCsvPath.Enabled = false;
            txtLogDirectory.Enabled = false;
            txtTenantId.Enabled = false;
            txtClientId.Enabled = false;
            txtClientSecret.Enabled = false;
            txtEnvironmentUrl.Enabled = false;
            txtBatchSize.Enabled = false;
            txtParallelThreads.Enabled = false;
            btnBrowseCsv.Enabled = false;
            btnBrowseLogDirectory.Enabled = false;
            radioSql.Enabled = false;
            radioDynamics.Enabled = false;

            try
            {
                await Task.Run(() => ExecuteDeletionProcess(_cancellationTokenSource.Token));
            }
            catch (OperationCanceledException)
            {
                LogToUI("Process cancelled by user.", Color.Yellow);
                lblStatus.Text = "Status: Cancelled";
                lblStatus.ForeColor = Color.Orange;
            }
            catch (Exception ex)
            {
                LogToUI($"Error: {ex.Message}", Color.Red);
                lblStatus.Text = "Status: Failed";
                lblStatus.ForeColor = Color.Red;
                MessageBox.Show($"An error occurred: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Reset UI
                _isProcessing = false;
                this.Invoke(() =>
                {
                    btnStart.Enabled = true;
                    btnCancel.Enabled = false;
                    progressBar.Visible = false;
                    txtCsvPath.Enabled = true;
                    txtLogDirectory.Enabled = true;
                    txtTenantId.Enabled = true;
                    txtClientId.Enabled = true;
                    txtClientSecret.Enabled = true;
                    txtEnvironmentUrl.Enabled = true;
                    txtBatchSize.Enabled = true;
                    txtParallelThreads.Enabled = true;
                    btnBrowseCsv.Enabled = true;
                    btnBrowseLogDirectory.Enabled = true;
                    radioSql.Enabled = true;
                    radioDynamics.Enabled = true;
                });
            }
        }

        private void ExecuteDeletionProcess(CancellationToken cancellationToken)
        {
            try
            {
                // Get input values (need to invoke on UI thread)
                string csvFilePath = string.Empty;
                string logDirectory = string.Empty;
                string tenantId = string.Empty;
                string clientId = string.Empty;
                string clientSecret = string.Empty;
                string environmentUrl = string.Empty;
                // string? sqlConnectionString = null;
                bool useSqlDataSource = false;

                this.Invoke(() =>
                {
                    csvFilePath = txtCsvPath.Text;
                    logDirectory = txtLogDirectory.Text;
                    tenantId = txtTenantId.Text;
                    clientId = txtClientId.Text;
                    clientSecret = txtClientSecret.Text;
                    environmentUrl = txtEnvironmentUrl.Text;
                    useSqlDataSource = radioSql.Checked;
                    // sqlConnectionString = string.IsNullOrWhiteSpace(txtSqlConnectionString.Text) ? null : txtSqlConnectionString.Text;
                });

                // Initialize logger with GUI output, custom log directory and CSV file name
                _logger = new Logger(this, string.IsNullOrWhiteSpace(logDirectory) ? null : logDirectory, csvFilePath);

                LogToUI("=".PadRight(80, '='), Color.Cyan);
                LogToUI("DYNAMICS 365 DATA CLEAN UP ACTIVITY - STARTED", Color.Cyan);
                LogToUI("=".PadRight(80, '='), Color.Cyan);

                // Build connection string using ClientSecret authentication
                // Format for Microsoft.PowerPlatform.Dataverse.Client
                string connectionString = $"AuthType=ClientSecret;Url={environmentUrl};ClientId={clientId};ClientSecret={clientSecret};";

                // Log input parameters
                _logger.LogSection("INPUT PARAMETERS");
                _logger.LogInfo($"CSV File Path: {csvFilePath}");
                if (!string.IsNullOrWhiteSpace(logDirectory))
                {
                    _logger.LogInfo($"Log Directory: {logDirectory}");
                }
                _logger.LogInfo($"Tenant ID: {tenantId}");
                _logger.LogInfo($"Client ID: {clientId}");
                _logger.LogInfo($"Environment URL: {environmentUrl}");
                _logger.LogInfo($"Data Source: {(useSqlDataSource ? "SQL (Direct)" : "Dynamics API (Standard)")}");
                // _logger.LogInfo($"Fetch Method: {(sqlConnectionString != null ? "SQL (Direct)" : "API (Standard)")}");
                _logger.LogConnectionString(connectionString);

                //if (sqlConnectionString != null)
                //{
                //    _logger.LogInfo("SQL Connection String provided - will use SQL for fetching records");
                //}

                cancellationToken.ThrowIfCancellationRequested();

                // Load configuration
                var config = LoadConfiguration();

                // Parse CSV
                _logger.LogSection("CSV PARSING");
                var csvParser = new CsvParser(_logger);
                var rawInstructions = csvParser.ParseCsv(csvFilePath);
                var validInstructions = csvParser.ValidateAndOrder(rawInstructions);

                if (validInstructions.Count == 0)
                {
                    _logger.LogError("No valid instructions found in CSV. Exiting.");
                    return;
                }

                _logger.LogInfo($"Total CSV rows loaded: {rawInstructions.Count}");
                _logger.LogInfo($"Valid instructions: {validInstructions.Count}");

                cancellationToken.ThrowIfCancellationRequested();

                // Get batch size and parallel threads from UI
                int batchSize = int.Parse(txtBatchSize.Text);
                int parallelThreads = int.Parse(txtParallelThreads.Text);

                _logger.LogInfo($"Batch Size: {batchSize}");
                _logger.LogInfo($"Parallel Threads: {parallelThreads}");

                // Initialize connections
                using var crmHelper = new CrmHelper(
                    connectionString,
                    _logger,
                    config.RetryCount,
                    config.PageSize,
                    batchSize,
                    txtTenantId.Text.Trim(),
                    txtClientId.Text.Trim(),
                    txtClientSecret.Text.Trim(),
                    txtEnvironmentUrl.Text.Trim(),
                    parallelThreads,
                    useSqlDataSource  // Pass the data source selection
                );


                // Test SQL connection only if SQL data source is selected
                if (useSqlDataSource)
                {
                    _logger.LogSection("SQL CONNECTION");
                    crmHelper.TestSqlConnection();
                }

                cancellationToken.ThrowIfCancellationRequested();

                // Then connect to Dynamics CRM
                _logger.LogSection("CRM CONNECTION");
                crmHelper.Connect();

                cancellationToken.ThrowIfCancellationRequested();

                // Initialize deletion engine
                var deletionEngine = new DeletionEngine(crmHelper, _logger, config.DelayBetweenPages);

                // Validate prerequisites
                if (!deletionEngine.ValidatePrerequisites())
                {
                    _logger.LogError("Prerequisite validation failed. Exiting.");
                    return;
                }

                cancellationToken.ThrowIfCancellationRequested();

                // Execute deletion process
                bool success = deletionEngine.Execute(validInstructions);

                // Get statistics
                var stats = deletionEngine.GetStatistics();

                // Log summary
                _logger.LogSummary(
                    stats.processed,
                    stats.deleted,
                    success,
                    stats.failed);

                // Update UI status
                this.Invoke(() =>
                {
                    if (success)
                    {
                        lblStatus.Text = $"Status: Completed Successfully ({stats.deleted} records deleted)";
                        lblStatus.ForeColor = Color.Green;
                        MessageBox.Show(
                            $"Process completed successfully!\n\n" +
                            $"Rows Processed: {stats.processed}\n" +
                            $"Records Deleted: {stats.deleted}",
                            "Success",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                    else
                    {
                        lblStatus.Text = $"Status: Completed with Errors";
                        lblStatus.ForeColor = Color.Orange;
                        MessageBox.Show(
                            $"Process completed with errors.\n\n" +
                            $"Rows Processed: {stats.processed}\n" +
                            $"Records Deleted: {stats.deleted}\n" +
                            $"Failed Sequences: {stats.failed}",
                            "Warning",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                    }
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError("CRITICAL ERROR - Application failed", ex);
                _logger?.LogSummary(0, 0, false);
                throw;
            }
            finally
            {
                _logger?.Dispose();
            }
        }

        private AppConfiguration LoadConfiguration()
        {
            var config = new AppConfiguration
            {
                RetryCount = GetConfigValue("RetryCount", 5),
                DelayBetweenPages = GetConfigValue("DelayBetweenPages", 0),
                PageSize = GetConfigValue("PageSize", 100),
                BatchSize = GetConfigValue("BatchSize", 100)
            };

            _logger?.LogInfo($"Retry Count: {config.RetryCount}");
            _logger?.LogInfo($"Delay Between Pages: {config.DelayBetweenPages}ms");
            _logger?.LogInfo($"Page Size: {config.PageSize}");
            _logger?.LogInfo($"Batch Size: {config.BatchSize}");

            return config;
        }

        private int GetConfigValue(string key, int defaultValue)
        {
            try
            {
                var value = ConfigurationManager.AppSettings[key];
                if (!string.IsNullOrWhiteSpace(value) && int.TryParse(value, out int result))
                {
                    return result;
                }
            }
            catch
            {
                // Use default value
            }

            return defaultValue;
        }

        private void btnCancel_Click(object? sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to cancel the current process?",
                "Confirm Cancel",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _cancellationTokenSource?.Cancel();
                btnCancel.Enabled = false;
            }
        }

        private void btnClearLog_Click(object? sender, EventArgs e)
        {
            txtLog.Clear();
            LogToUI("Log cleared.", Color.Gray);
        }

        private void btnSaveLog_Click(object? sender, EventArgs e)
        {
            saveFileDialog.FileName = $"DeletionLog_{DateTime.Now:yyyyMMdd_HHmmss}.log";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    File.WriteAllText(saveFileDialog.FileName, txtLog.Text);
                    MessageBox.Show($"Log saved successfully to:\n{saveFileDialog.FileName}",
                        "Log Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to save log: {ex.Message}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public void LogToUI(string message, Color color)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(() => LogToUI(message, color));
                return;
            }

            txtLog.SelectionStart = txtLog.TextLength;
            txtLog.SelectionLength = 0;
            txtLog.SelectionColor = color;
            txtLog.AppendText(message + Environment.NewLine);
            txtLog.SelectionColor = txtLog.ForeColor;
            txtLog.ScrollToCaret();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_isProcessing)
            {
                var result = MessageBox.Show(
                    "A process is currently running. Are you sure you want to exit?",
                    "Confirm Exit",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }

                _cancellationTokenSource?.Cancel();
            }

            base.OnFormClosing(e);
        }

        private void progressBar_Click(object sender, EventArgs e)
        {

        }

        private void radioDataSource_CheckedChanged(object? sender, EventArgs e)
        {
            if (radioSql.Checked)
            {
                _useSqlDataSource = true;
                LogToUI("Data Source changed to: SQL (Direct) - Faster performance", Color.Cyan);
            }
            else if (radioDynamics.Checked)
            {
                _useSqlDataSource = false;
                LogToUI("Data Source changed to: Dynamics API (Standard) - May be slower", Color.Yellow);
            }
        }
    }

    internal class AppConfiguration
    {
        public int RetryCount { get; set; }
        public int DelayBetweenPages { get; set; }
        public int PageSize { get; set; }
        public int BatchSize { get; set; }
    }
}
