namespace RainTracker.Models
{
    // <summary>
    /// Response model for rain data
    /// </summary>
    public class RainDataResponse
    {
        /// <summary>
        /// Array of rain data records
        /// </summary>
        public IEnumerable<RainDataItem> Data { get; set; } = new List<RainDataItem>();
    }

    // <summary>
    /// Individual rain data item
    /// </summary>
    public class RainDataItem
    {
        /// <summary>
        /// Timestamp when the rain data was recorded (ISO 8601 format)
        /// </summary>
        /// <example>2024-11-05T19:51:33.294Z</example>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Whether it rained or not
        /// </summary>
        /// <example>true</example>
        public bool Rain { get; set; }
    }
}
