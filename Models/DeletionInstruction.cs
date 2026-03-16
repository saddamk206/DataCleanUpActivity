using CsvHelper.Configuration.Attributes;

namespace DataCleanUpActivity.Models
{
    /// <summary>
    /// Query type for fetching records
    /// </summary>
    public enum QueryType
    {
        /// <summary>Standard date-based query</summary>
        Standard,
        /// <summary>Custom SQL query</summary>
        SQL,
        /// <summary>Custom FetchXML query</summary>
        FetchXML
    }

    /// <summary>
    /// Represents a single row from the deletion instructions CSV file
    /// </summary>
    public class DeletionInstruction
    {
        [Name("sequence")]
        [Index(0)]
        public int Sequence { get; set; }

        [Name("entityname")]
        [Index(1)]
        public string EntityName { get; set; } = string.Empty;
         [Name("entityidcolumn")]
        [Index(4)]
        public string EntityIdColumn { get; set; } = string.Empty;

        [Name("start date")]
        [Index(2)]
        public DateTime? StartDate { get; set; }

        [Name("end date")]
        [Index(3)]
        public DateTime? EndDate { get; set; }

        [Name("querytype")]
        [Index(5)]
        [Optional]
        public string? QueryTypeString { get; set; }

        [Name("additionalquery")]
        [Index(6)]
        [Optional]
        public string? AdditionalQuery { get; set; }

        /// <summary>
        /// Gets the parsed query type
        /// </summary>
        [Ignore]
        public QueryType QueryType
        {
            get
            {
                if (string.IsNullOrWhiteSpace(QueryTypeString))
                    return QueryType.Standard;

                return QueryTypeString.Trim().ToUpperInvariant() switch
                {
                    "SQL" => QueryType.SQL,
                    "FETCHXML" => QueryType.FetchXML,
                    "STANDARD" => QueryType.Standard,
                    _ => QueryType.Standard
                };
            }
        }

        /// <summary>
        /// Validates the instruction data
        /// </summary>
        public List<string> Validate()
        {
            var errors = new List<string>();

            if (Sequence <= 0)
            {
                errors.Add("Sequence must be greater than 0");
            }

            if (string.IsNullOrWhiteSpace(EntityName))
            {
                errors.Add("EntityName cannot be empty");
            }

            // Only validate dates for Standard query type
            if (QueryType == QueryType.Standard)
            {
                if (!StartDate.HasValue || StartDate.Value == default)
                {
                    errors.Add("Start date is required for Standard query type");
                }

                if (!EndDate.HasValue || EndDate.Value == default)
                {
                    errors.Add("End date is required for Standard query type");
                }

                if (StartDate.HasValue && EndDate.HasValue && StartDate.Value > EndDate.Value)
                {
                    errors.Add("Start date cannot be after end date");
                }
            }
            else if (QueryType == QueryType.SQL || QueryType == QueryType.FetchXML)
            {
                // For SQL and FetchXML queries, AdditionalQuery is required
                if (string.IsNullOrWhiteSpace(AdditionalQuery))
                {
                    errors.Add($"AdditionalQuery is required for {QueryType} query type");
                }
            }

            return errors;
        }

        public override string ToString()
        {
            var dateRange = StartDate.HasValue && EndDate.HasValue 
                ? $"{StartDate.Value:yyyy-MM-dd} to {EndDate.Value:yyyy-MM-dd}" 
                : "N/A";
            return $"Sequence: {Sequence}, Entity: {EntityName}, Period: {dateRange}, QueryType: {QueryType}";
        }
    }
}
