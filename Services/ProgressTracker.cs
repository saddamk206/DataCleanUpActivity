using DataCleanUpActivity.Models;

namespace DataCleanUpActivity.Services
{
    /// <summary>
    /// Tracks progress of entity deletion operations
    /// </summary>
    public class ProgressTracker
    {
        public class EntityProgress
        {
            public int Sequence { get; set; }
            public string EntityName { get; set; } = string.Empty;
            public string Status { get; set; } = "Pending"; // Pending, Fetching, Deleting, Completed, Failed
            public int TotalRecordsFound { get; set; }
            public int RecordsDeleted { get; set; }
            public int RecordsPending => TotalRecordsFound - RecordsDeleted;
            public DateTime? StartTime { get; set; }
            public DateTime? EndTime { get; set; }
            public TimeSpan? Duration => EndTime.HasValue && StartTime.HasValue ? EndTime.Value - StartTime.Value : null;
            public double ProgressPercentage => TotalRecordsFound > 0 ? (double)RecordsDeleted / TotalRecordsFound * 100 : 0;
        }

        private readonly Dictionary<int, EntityProgress> _entityProgress = new Dictionary<int, EntityProgress>();
        private readonly Logger _logger;
        private System.Timers.Timer? _reportTimer;
        private DateTime _processStartTime;
        private readonly string _reportDirectory;

        public ProgressTracker(Logger logger, string reportDirectory)
        {
            _logger = logger;
            _reportDirectory = reportDirectory;
            _processStartTime = DateTime.Now;

            // Ensure report directory exists
            if (!Directory.Exists(_reportDirectory))
            {
                Directory.CreateDirectory(_reportDirectory);
            }
        }

        /// <summary>
        /// Initialize tracking for all instructions
        /// </summary>
        public void Initialize(List<DeletionInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                _entityProgress[instruction.Sequence] = new EntityProgress
                {
                    Sequence = instruction.Sequence,
                    EntityName = instruction.EntityName,
                    Status = "Pending"
                };
            }
        }

        /// <summary>
        /// Start hourly report timer
        /// </summary>
        public void StartHourlyReporting()
        {
            _reportTimer = new System.Timers.Timer(3600000); // 1 hour = 3,600,000 ms
            _reportTimer.Elapsed += (sender, e) => GenerateProgressReport(false);
            _reportTimer.AutoReset = true;
            _reportTimer.Start();

            _logger.LogInfo("Hourly progress reporting started");
        }

        /// <summary>
        /// Stop hourly reporting
        /// </summary>
        public void StopHourlyReporting()
        {
            if (_reportTimer != null)
            {
                _reportTimer.Stop();
                _reportTimer.Dispose();
                _reportTimer = null;
            }
        }

        /// <summary>
        /// Update status when fetching starts
        /// </summary>
        public void StartFetching(int sequence)
        {
            if (_entityProgress.TryGetValue(sequence, out var progress))
            {
                progress.Status = "Fetching";
                progress.StartTime = DateTime.Now;
            }
        }

        /// <summary>
        /// Update when fetching completes
        /// </summary>
        public void CompleteFetching(int sequence, int totalRecords)
        {
            if (_entityProgress.TryGetValue(sequence, out var progress))
            {
                progress.TotalRecordsFound = totalRecords;
                progress.Status = totalRecords > 0 ? "Deleting" : "Completed";
                if (totalRecords == 0)
                {
                    progress.EndTime = DateTime.Now;
                }
            }
        }

        /// <summary>
        /// Update deletion progress
        /// </summary>
        public void UpdateDeletionProgress(int sequence, int recordsDeleted)
        {
            if (_entityProgress.TryGetValue(sequence, out var progress))
            {
                progress.RecordsDeleted = recordsDeleted;
                progress.Status = "Deleting";
            }
        }

        /// <summary>
        /// Mark entity as completed
        /// </summary>
        public void CompleteEntity(int sequence, int totalDeleted)
        {
            if (_entityProgress.TryGetValue(sequence, out var progress))
            {
                progress.RecordsDeleted = totalDeleted;
                progress.Status = "Completed";
                progress.EndTime = DateTime.Now;
            }
        }

        /// <summary>
        /// Mark entity as failed
        /// </summary>
        public void FailEntity(int sequence)
        {
            if (_entityProgress.TryGetValue(sequence, out var progress))
            {
                progress.Status = "Failed";
                progress.EndTime = DateTime.Now;
            }
        }

        /// <summary>
        /// Get current progress for an entity
        /// </summary>
        public EntityProgress? GetProgress(int sequence)
        {
            return _entityProgress.TryGetValue(sequence, out var progress) ? progress : null;
        }

        /// <summary>
        /// Generate progress report
        /// </summary>
        public void GenerateProgressReport(bool isFinal = false)
        {
            var reportType = isFinal ? "FINAL" : "HOURLY";
            var timestamp = DateTime.Now;
            var totalDuration = timestamp - _processStartTime;

            var report = new System.Text.StringBuilder();
            report.AppendLine("========================================");
            report.AppendLine($"{reportType} DELETION PROGRESS REPORT");
            report.AppendLine("========================================");
            report.AppendLine($"Report Generated: {timestamp:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine($"Process Started: {_processStartTime:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine($"Total Duration: {totalDuration.Hours}h {totalDuration.Minutes}m {totalDuration.Seconds}s");
            report.AppendLine();

            // Summary statistics
            var completed = _entityProgress.Values.Count(p => p.Status == "Completed");
            var inProgress = _entityProgress.Values.Count(p => p.Status == "Deleting" || p.Status == "Fetching");
            var pending = _entityProgress.Values.Count(p => p.Status == "Pending");
            var failed = _entityProgress.Values.Count(p => p.Status == "Failed");
            var totalRecordsFound = _entityProgress.Values.Sum(p => p.TotalRecordsFound);
            var totalRecordsDeleted = _entityProgress.Values.Sum(p => p.RecordsDeleted);
            var totalPending = totalRecordsFound - totalRecordsDeleted;

            report.AppendLine("SUMMARY:");
            report.AppendLine($"  Total Entities: {_entityProgress.Count}");
            report.AppendLine($"  Completed: {completed}");
            report.AppendLine($"  In Progress: {inProgress}");
            report.AppendLine($"  Pending: {pending}");
            report.AppendLine($"  Failed: {failed}");
            report.AppendLine();
            report.AppendLine($"  Total Records Found: {totalRecordsFound:N0}");
            report.AppendLine($"  Total Records Deleted: {totalRecordsDeleted:N0}");
            report.AppendLine($"  Total Records Pending: {totalPending:N0}");
            report.AppendLine($"  Overall Progress: {(totalRecordsFound > 0 ? (double)totalRecordsDeleted / totalRecordsFound * 100 : 0):F2}%");
            report.AppendLine();

            // Detailed entity progress
            report.AppendLine("DETAILED PROGRESS BY ENTITY:");
            report.AppendLine(new string('-', 120));
            report.AppendLine($"{"Seq",-5} {"Entity Name",-30} {"Status",-12} {"Found",-12} {"Deleted",-12} {"Pending",-12} {"Progress",-10} {"Duration"}");
            report.AppendLine(new string('-', 120));

            foreach (var progress in _entityProgress.Values.OrderBy(p => p.Sequence))
            {
                var duration = progress.Duration.HasValue 
                    ? $"{progress.Duration.Value.Hours}h {progress.Duration.Value.Minutes}m {progress.Duration.Value.Seconds}s"
                    : (progress.StartTime.HasValue ? $"{(DateTime.Now - progress.StartTime.Value).Hours}h {(DateTime.Now - progress.StartTime.Value).Minutes}m" : "-");

                report.AppendLine(
                    $"{progress.Sequence,-5} " +
                    $"{progress.EntityName,-30} " +
                    $"{progress.Status,-12} " +
                    $"{progress.TotalRecordsFound,-12:N0} " +
                    $"{progress.RecordsDeleted,-12:N0} " +
                    $"{progress.RecordsPending,-12:N0} " +
                    $"{progress.ProgressPercentage,-10:F2}% " +
                    $"{duration}");
            }

            report.AppendLine(new string('-', 120));
            report.AppendLine();

            // Log to console
            _logger.LogSection(report.ToString());

            // Save to file
            var fileName = isFinal 
                ? $"DeletionReport_Final_{_processStartTime:yyyyMMdd_HHmmss}.txt"
                : $"DeletionReport_{timestamp:yyyyMMdd_HHmmss}.txt";
            var filePath = Path.Combine(_reportDirectory, fileName);

            try
            {
                File.WriteAllText(filePath, report.ToString());
                _logger.LogSuccess($"Progress report saved to: {filePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to save progress report to file: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get overall statistics
        /// </summary>
        public (int total, int completed, int inProgress, int pending, int failed, int totalRecordsDeleted, int totalRecordsPending) GetStatistics()
        {
            var completed = _entityProgress.Values.Count(p => p.Status == "Completed");
            var inProgress = _entityProgress.Values.Count(p => p.Status == "Deleting" || p.Status == "Fetching");
            var pending = _entityProgress.Values.Count(p => p.Status == "Pending");
            var failed = _entityProgress.Values.Count(p => p.Status == "Failed");
            var totalRecordsDeleted = _entityProgress.Values.Sum(p => p.RecordsDeleted);
            var totalRecordsPending = _entityProgress.Values.Sum(p => p.RecordsPending);

            return (_entityProgress.Count, completed, inProgress, pending, failed, totalRecordsDeleted, totalRecordsPending);
        }
    }
}
