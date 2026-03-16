namespace DataCleanUpActivity
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblTitle = new Label();
            lblCsvPath = new Label();
            txtCsvPath = new TextBox();
            btnBrowseCsv = new Button();
            lblLogDirectory = new Label();
            txtLogDirectory = new TextBox();
            btnBrowseLogDirectory = new Button();
            lblTenantId = new Label();
            txtTenantId = new TextBox();
            lblClientId = new Label();
            txtClientId = new TextBox();
            lblClientSecret = new Label();
            txtClientSecret = new TextBox();
            lblEnvironmentUrl = new Label();
            txtEnvironmentUrl = new TextBox();
            lblBatchSize = new Label();
            txtBatchSize = new TextBox();
            lblParallelThreads = new Label();
            txtParallelThreads = new TextBox();
            btnStart = new Button();
            btnCancel = new Button();
            lblLog = new Label();
            txtLog = new RichTextBox();
            progressBar = new ProgressBar();
            lblStatus = new Label();
            groupBoxInputs = new GroupBox();
            groupBoxDataSource = new GroupBox();
            radioSql = new RadioButton();
            radioDynamics = new RadioButton();
            groupBoxLog = new GroupBox();
            btnSaveLog = new Button();
            btnClearLog = new Button();
            openFileDialog = new OpenFileDialog();
            saveFileDialog = new SaveFileDialog();
            groupBoxInputs.SuspendLayout();
            groupBoxDataSource.SuspendLayout();
            groupBoxLog.SuspendLayout();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold, GraphicsUnit.Point);
            lblTitle.ForeColor = Color.DarkBlue;
            lblTitle.Location = new Point(17, 15);
            lblTitle.Margin = new Padding(4, 0, 4, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(504, 38);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Dynamics 365 Data Clean Up Activity";
            // 
            // lblCsvPath
            // 
            lblCsvPath.AutoSize = true;
            lblCsvPath.Location = new Point(21, 50);
            lblCsvPath.Margin = new Padding(4, 0, 4, 0);
            lblCsvPath.Name = "lblCsvPath";
            lblCsvPath.Size = new Size(131, 25);
            lblCsvPath.TabIndex = 1;
            lblCsvPath.Text = "CSV File Path: *";
            // 
            // txtCsvPath
            // 
            txtCsvPath.Location = new Point(21, 80);
            txtCsvPath.Margin = new Padding(4, 5, 4, 5);
            txtCsvPath.Name = "txtCsvPath";
            txtCsvPath.Size = new Size(927, 31);
            txtCsvPath.TabIndex = 2;
            // 
            // btnBrowseCsv
            // 
            btnBrowseCsv.Location = new Point(959, 78);
            btnBrowseCsv.Margin = new Padding(4, 5, 4, 5);
            btnBrowseCsv.Name = "btnBrowseCsv";
            btnBrowseCsv.Size = new Size(143, 42);
            btnBrowseCsv.TabIndex = 3;
            btnBrowseCsv.Text = "Browse...";
            btnBrowseCsv.UseVisualStyleBackColor = true;
            btnBrowseCsv.Click += btnBrowseCsv_Click;
            // 
            // lblLogDirectory
            // 
            lblLogDirectory.AutoSize = true;
            lblLogDirectory.Location = new Point(21, 130);
            lblLogDirectory.Margin = new Padding(4, 0, 4, 0);
            lblLogDirectory.Name = "lblLogDirectory";
            lblLogDirectory.Size = new Size(197, 25);
            lblLogDirectory.TabIndex = 20;
            lblLogDirectory.Text = "Log Directory (Optional):";
            // 
            // txtLogDirectory
            // 
            txtLogDirectory.Location = new Point(21, 160);
            txtLogDirectory.Margin = new Padding(4, 5, 4, 5);
            txtLogDirectory.Name = "txtLogDirectory";
            txtLogDirectory.Size = new Size(927, 31);
            txtLogDirectory.TabIndex = 21;
            // 
            // btnBrowseLogDirectory
            // 
            btnBrowseLogDirectory.Location = new Point(959, 158);
            btnBrowseLogDirectory.Margin = new Padding(4, 5, 4, 5);
            btnBrowseLogDirectory.Name = "btnBrowseLogDirectory";
            btnBrowseLogDirectory.Size = new Size(143, 42);
            btnBrowseLogDirectory.TabIndex = 22;
            btnBrowseLogDirectory.Text = "Browse...";
            btnBrowseLogDirectory.UseVisualStyleBackColor = true;
            btnBrowseLogDirectory.Click += btnBrowseLogDirectory_Click;
            // 
            // lblTenantId
            // 
            lblTenantId.AutoSize = true;
            lblTenantId.Location = new Point(21, 222);
            lblTenantId.Margin = new Padding(4, 0, 4, 0);
            lblTenantId.Name = "lblTenantId";
            lblTenantId.Size = new Size(103, 25);
            lblTenantId.TabIndex = 4;
            lblTenantId.Text = "Tenant ID: *";
            // 
            // txtTenantId
            // 
            txtTenantId.Location = new Point(21, 252);
            txtTenantId.Margin = new Padding(4, 5, 4, 5);
            txtTenantId.Name = "txtTenantId";
            txtTenantId.Size = new Size(520, 31);
            txtTenantId.TabIndex = 5;
            // 
            // lblClientId
            // 
            lblClientId.AutoSize = true;
            lblClientId.Location = new Point(580, 222);
            lblClientId.Margin = new Padding(4, 0, 4, 0);
            lblClientId.Name = "lblClientId";
            lblClientId.Size = new Size(96, 25);
            lblClientId.TabIndex = 6;
            lblClientId.Text = "Client ID: *";
            // 
            // txtClientId
            // 
            txtClientId.Location = new Point(580, 252);
            txtClientId.Margin = new Padding(4, 5, 4, 5);
            txtClientId.Name = "txtClientId";
            txtClientId.Size = new Size(520, 31);
            txtClientId.TabIndex = 7;
            // 
            // lblClientSecret
            // 
            lblClientSecret.AutoSize = true;
            lblClientSecret.Location = new Point(21, 305);
            lblClientSecret.Margin = new Padding(4, 0, 4, 0);
            lblClientSecret.Name = "lblClientSecret";
            lblClientSecret.Size = new Size(126, 25);
            lblClientSecret.TabIndex = 8;
            lblClientSecret.Text = "Client Secret: *";
            // 
            // txtClientSecret
            // 
            txtClientSecret.Location = new Point(21, 335);
            txtClientSecret.Margin = new Padding(4, 5, 4, 5);
            txtClientSecret.Name = "txtClientSecret";
            txtClientSecret.PasswordChar = '*';
            txtClientSecret.Size = new Size(520, 31);
            txtClientSecret.TabIndex = 9;
            // 
            // lblEnvironmentUrl
            // 
            lblEnvironmentUrl.AutoSize = true;
            lblEnvironmentUrl.Location = new Point(580, 305);
            lblEnvironmentUrl.Margin = new Padding(4, 0, 4, 0);
            lblEnvironmentUrl.Name = "lblEnvironmentUrl";
            lblEnvironmentUrl.Size = new Size(165, 25);
            lblEnvironmentUrl.TabIndex = 10;
            lblEnvironmentUrl.Text = "Environment URL: *";
            // 
            // txtEnvironmentUrl
            // 
            txtEnvironmentUrl.Location = new Point(580, 335);
            txtEnvironmentUrl.Margin = new Padding(4, 5, 4, 5);
            txtEnvironmentUrl.Name = "txtEnvironmentUrl";
            txtEnvironmentUrl.Size = new Size(520, 31);
            txtEnvironmentUrl.TabIndex = 11;
            // 
            // lblBatchSize
            // 
            lblBatchSize.AutoSize = true;
            lblBatchSize.Location = new Point(21, 380);
            lblBatchSize.Margin = new Padding(4, 0, 4, 0);
            lblBatchSize.Name = "lblBatchSize";
            lblBatchSize.Size = new Size(93, 25);
            lblBatchSize.TabIndex = 12;
            lblBatchSize.Text = "Batch Size:";
            // 
            // txtBatchSize
            // 
            txtBatchSize.Location = new Point(140, 377);
            txtBatchSize.Margin = new Padding(4, 5, 4, 5);
            txtBatchSize.Name = "txtBatchSize";
            txtBatchSize.Size = new Size(120, 31);
            txtBatchSize.TabIndex = 13;
            txtBatchSize.Text = "100";
            // 
            // lblParallelThreads
            // 
            lblParallelThreads.AutoSize = true;
            lblParallelThreads.Location = new Point(300, 380);
            lblParallelThreads.Margin = new Padding(4, 0, 4, 0);
            lblParallelThreads.Name = "lblParallelThreads";
            lblParallelThreads.Size = new Size(143, 25);
            lblParallelThreads.TabIndex = 14;
            lblParallelThreads.Text = "Parallel Threads:";
            // 
            // txtParallelThreads
            // 
            txtParallelThreads.Location = new Point(455, 377);
            txtParallelThreads.Margin = new Padding(4, 5, 4, 5);
            txtParallelThreads.Name = "txtParallelThreads";
            txtParallelThreads.Size = new Size(86, 31);
            txtParallelThreads.TabIndex = 15;
            txtParallelThreads.Text = "4";
            // 
            // btnStart
            // 
            btnStart.BackColor = Color.Green;
            btnStart.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
            btnStart.ForeColor = Color.White;
            btnStart.Location = new Point(39, 565);
            btnStart.Margin = new Padding(4, 5, 4, 5);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(171, 48);
            btnStart.TabIndex = 16;
            btnStart.Text = "Start Process";
            btnStart.UseVisualStyleBackColor = false;
            btnStart.Click += btnStart_Click;
            // 
            // btnCancel
            // 
            btnCancel.BackColor = Color.Red;
            btnCancel.Enabled = false;
            btnCancel.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
            btnCancel.ForeColor = Color.White;
            btnCancel.Location = new Point(219, 565);
            btnCancel.Margin = new Padding(4, 5, 4, 5);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(171, 48);
            btnCancel.TabIndex = 17;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = false;
            btnCancel.Click += btnCancel_Click;
            // 
            // lblLog
            // 
            lblLog.AutoSize = true;
            lblLog.Location = new Point(9, 32);
            lblLog.Margin = new Padding(4, 0, 4, 0);
            lblLog.Name = "lblLog";
            lblLog.Size = new Size(126, 25);
            lblLog.TabIndex = 8;
            lblLog.Text = "Execution Log:";
            // 
            // txtLog
            // 
            txtLog.BackColor = Color.Black;
            txtLog.Font = new Font("Consolas", 8F, FontStyle.Regular, GraphicsUnit.Point);
            txtLog.ForeColor = Color.White;
            txtLog.Location = new Point(9, 50);
            txtLog.Margin = new Padding(4, 5, 4, 5);
            txtLog.Name = "txtLog";
            txtLog.ReadOnly = true;
            txtLog.Size = new Size(1091, 350);
            txtLog.TabIndex = 9;
            txtLog.Text = "";
            txtLog.WordWrap = false;
            // 
            // progressBar
            // 
            progressBar.Location = new Point(399, 433);
            progressBar.Margin = new Padding(4, 5, 4, 5);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(720, 42);
            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.TabIndex = 16;
            progressBar.Visible = false;
            progressBar.Click += progressBar_Click;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 9F, FontStyle.Italic, GraphicsUnit.Point);
            lblStatus.Location = new Point(39, 500);
            lblStatus.Margin = new Padding(4, 0, 4, 0);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(117, 25);
            lblStatus.TabIndex = 17;
            lblStatus.Text = "Status: Ready";
            // 
            // groupBoxInputs
            // 
            groupBoxInputs.Controls.Add(groupBoxDataSource);
            groupBoxInputs.Controls.Add(lblCsvPath);
            groupBoxInputs.Controls.Add(txtCsvPath);
            groupBoxInputs.Controls.Add(btnBrowseCsv);
            groupBoxInputs.Controls.Add(lblLogDirectory);
            groupBoxInputs.Controls.Add(txtLogDirectory);
            groupBoxInputs.Controls.Add(btnBrowseLogDirectory);
            groupBoxInputs.Controls.Add(lblTenantId);
            groupBoxInputs.Controls.Add(txtTenantId);
            groupBoxInputs.Controls.Add(lblClientId);
            groupBoxInputs.Controls.Add(txtClientId);
            groupBoxInputs.Controls.Add(lblClientSecret);
            groupBoxInputs.Controls.Add(txtClientSecret);
            groupBoxInputs.Controls.Add(lblEnvironmentUrl);
            groupBoxInputs.Controls.Add(txtEnvironmentUrl);
            groupBoxInputs.Controls.Add(lblBatchSize);
            groupBoxInputs.Controls.Add(txtBatchSize);
            groupBoxInputs.Controls.Add(lblParallelThreads);
            groupBoxInputs.Controls.Add(txtParallelThreads);
            groupBoxInputs.Location = new Point(17, 65);
            groupBoxInputs.Margin = new Padding(4, 5, 4, 5);
            groupBoxInputs.Name = "groupBoxInputs";
            groupBoxInputs.Padding = new Padding(4, 5, 4, 5);
            groupBoxInputs.Size = new Size(1121, 485);
            groupBoxInputs.TabIndex = 18;
            groupBoxInputs.TabStop = false;
            groupBoxInputs.Text = "Input Parameters";
            // 
            // groupBoxDataSource
            // 
            groupBoxDataSource.Controls.Add(radioSql);
            groupBoxDataSource.Controls.Add(radioDynamics);
            groupBoxDataSource.Location = new Point(580, 375);
            groupBoxDataSource.Margin = new Padding(4, 5, 4, 5);
            groupBoxDataSource.Name = "groupBoxDataSource";
            groupBoxDataSource.Padding = new Padding(4, 5, 4, 5);
            groupBoxDataSource.Size = new Size(520, 95);
            groupBoxDataSource.TabIndex = 20;
            groupBoxDataSource.TabStop = false;
            groupBoxDataSource.Text = "Data Source";
            // 
            // radioSql
            // 
            radioSql.AutoSize = true;
            radioSql.Checked = true;
            radioSql.Location = new Point(20, 35);
            radioSql.Margin = new Padding(4, 5, 4, 5);
            radioSql.Name = "radioSql";
            radioSql.Size = new Size(159, 29);
            radioSql.TabIndex = 0;
            radioSql.TabStop = true;
            radioSql.Text = "SQL (Faster)";
            radioSql.UseVisualStyleBackColor = true;
            // 
            // radioDynamics
            // 
            radioDynamics.AutoSize = true;
            radioDynamics.Location = new Point(220, 35);
            radioDynamics.Margin = new Padding(4, 5, 4, 5);
            radioDynamics.Name = "radioDynamics";
            radioDynamics.Size = new Size(198, 29);
            radioDynamics.TabIndex = 1;
            radioDynamics.Text = "Dynamics (API)";
            radioDynamics.UseVisualStyleBackColor = true;
            // 
            // progressBar
            // 
            progressBar.Location = new Point(400, 575);
            progressBar.Margin = new Padding(4, 5, 4, 5);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(720, 32);
            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.TabIndex = 16;
            progressBar.Visible = false;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 9F, FontStyle.Italic, GraphicsUnit.Point);
            lblStatus.Location = new Point(39, 623);
            lblStatus.Margin = new Padding(4, 0, 4, 0);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(119, 25);
            lblStatus.TabIndex = 17;
            lblStatus.Text = "Status: Ready";
            // 
            // groupBoxLog
            // 
            groupBoxLog.Controls.Add(btnSaveLog);
            groupBoxLog.Controls.Add(btnClearLog);
            groupBoxLog.Controls.Add(lblLog);
            groupBoxLog.Controls.Add(txtLog);
            groupBoxLog.Location = new Point(17, 660);
            groupBoxLog.Margin = new Padding(4, 5, 4, 5);
            groupBoxLog.Name = "groupBoxLog";
            groupBoxLog.Padding = new Padding(4, 5, 4, 5);
            groupBoxLog.Size = new Size(1121, 320);
            groupBoxLog.TabIndex = 19;
            groupBoxLog.TabStop = false;
            groupBoxLog.Text = "Execution Log";
            // 
            // txtLog
            // 
            txtLog.Margin = new Padding(4, 5, 4, 5);
            txtLog.Name = "txtLog";
            txtLog.ReadOnly = true;
            txtLog.Size = new Size(1093, 210);
            txtLog.TabIndex = 9;
            txtLog.Text = "";
            txtLog.WordWrap = false;
            // 
            // btnSaveLog
            // 
            btnSaveLog.Location = new Point(146, 270);
            btnSaveLog.Margin = new Padding(4, 5, 4, 5);
            btnSaveLog.Name = "btnSaveLog";
            btnSaveLog.Size = new Size(129, 38);
            btnSaveLog.TabIndex = 11;
            btnSaveLog.Text = "Save Log";
            btnSaveLog.UseVisualStyleBackColor = true;
            btnSaveLog.Click += btnSaveLog_Click;
            // 
            // btnClearLog
            // 
            btnClearLog.Location = new Point(9, 270);
            btnClearLog.Margin = new Padding(4, 5, 4, 5);
            btnClearLog.Name = "btnClearLog";
            btnClearLog.Size = new Size(129, 38);
            btnClearLog.TabIndex = 10;
            btnClearLog.Text = "Clear Log";
            btnClearLog.UseVisualStyleBackColor = true;
            btnClearLog.Click += btnClearLog_Click;
            // 
            // openFileDialog
            // 
            openFileDialog.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
            openFileDialog.Title = "Select CSV File";
            // 
            // saveFileDialog
            // 
            saveFileDialog.DefaultExt = "log";
            saveFileDialog.Filter = "Log Files (*.log)|*.log|Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            saveFileDialog.Title = "Save Log File";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1156, 1000);
            Controls.Add(groupBoxLog);
            Controls.Add(groupBoxInputs);
            Controls.Add(lblStatus);
            Controls.Add(progressBar);
            Controls.Add(btnCancel);
            Controls.Add(btnStart);
            Controls.Add(lblTitle);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Margin = new Padding(4, 5, 4, 5);
            MaximizeBox = false;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Dynamics 365 Data Clean Up Activity";
            groupBoxInputs.ResumeLayout(false);
            groupBoxInputs.PerformLayout();
            groupBoxDataSource.ResumeLayout(false);
            groupBoxDataSource.PerformLayout();
            groupBoxLog.ResumeLayout(false);
            groupBoxLog.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblCsvPath;
        private System.Windows.Forms.TextBox txtCsvPath;
        private System.Windows.Forms.Button btnBrowseCsv;
        private System.Windows.Forms.Label lblLogDirectory;
        private System.Windows.Forms.TextBox txtLogDirectory;
        private System.Windows.Forms.Button btnBrowseLogDirectory;
        private System.Windows.Forms.Label lblTenantId;
        private System.Windows.Forms.TextBox txtTenantId;
        private System.Windows.Forms.Label lblClientId;
        private System.Windows.Forms.TextBox txtClientId;
        private System.Windows.Forms.Label lblClientSecret;
        private System.Windows.Forms.TextBox txtClientSecret;
        private System.Windows.Forms.Label lblEnvironmentUrl;
        private System.Windows.Forms.TextBox txtEnvironmentUrl;
        private System.Windows.Forms.Label lblBatchSize;
        private System.Windows.Forms.TextBox txtBatchSize;
        private System.Windows.Forms.Label lblParallelThreads;
        private System.Windows.Forms.TextBox txtParallelThreads;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblLog;
        private System.Windows.Forms.RichTextBox txtLog;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.GroupBox groupBoxInputs;
        private System.Windows.Forms.GroupBox groupBoxDataSource;
        private System.Windows.Forms.RadioButton radioSql;
        private System.Windows.Forms.RadioButton radioDynamics;
        private System.Windows.Forms.GroupBox groupBoxLog;
        private System.Windows.Forms.Button btnClearLog;
        private System.Windows.Forms.Button btnSaveLog;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
    }
}
