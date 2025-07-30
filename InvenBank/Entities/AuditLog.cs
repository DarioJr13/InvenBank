using System.ComponentModel.DataAnnotations;

namespace InvenBank.API.Entities
{
    public class AuditLog
    {
        public long Id { get; set; }

        [Required, MaxLength(100)]
        public string TableName { get; set; } = string.Empty;

        [Required, MaxLength(10)]
        public string Operation { get; set; } = string.Empty; // INSERT, UPDATE, DELETE

        public int RecordId { get; set; }
        public int? UserId { get; set; }
        public string? OldValues { get; set; } // JSON
        public string? NewValues { get; set; } // JSON
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual User? User { get; set; }
    }
}
