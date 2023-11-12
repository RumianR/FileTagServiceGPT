using Postgrest.Attributes;
using Postgrest.Models;

namespace OpenAIApp.Models
{
    [Table("file_tag")]
    public class FileTag : BaseModel
    {
        [PrimaryKey("id")]
        public long Id { get; set; }

        [Column("file_id")]
        public Guid FileId { get; set; }

        [Column("tag_id")]
        public Guid? TagId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
