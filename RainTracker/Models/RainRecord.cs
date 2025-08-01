using System.ComponentModel.DataAnnotations;

namespace RainTracker.Models
{
    public class RainRecord
    {
        [Key]
        public string Id { get; set; }

        public bool Rain { get; set; }

        public DateTime Timestamp { get; set; }

        public string UserId { get; set; } = string.Empty;
    }

    public class RainRecordDto
    {
        public bool Rain { get; set; }
    }
}
