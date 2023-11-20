using Postgrest.Attributes;
using Postgrest.Models;

namespace OpenAIApp.Models
{
    [Table("files")]
    public class FileModel : BaseModel
    {
        [PrimaryKey("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("name")]
        public string Name { get; set; }

        [Column("thumbnail_url")]
        public string ThumbnailUrl { get; set; }

        [Column("url")]
        public string Url { get; set; }

        [Column("mime_type")]
        public string MimeType { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("user_id")]
        public Guid? UserId { get; set; } = Guid.NewGuid();

        [Column("state")]
        public int State { get; set; }

        [Column("pages")]
        public int Pages { get; set; }

        [Column("size")]
        public long Size { get; set; }
    }
}
