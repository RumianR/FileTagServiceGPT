using Postgrest.Attributes;
using Postgrest.Models;

namespace OpenAIApp.Models
{
    [Table("file_tag")]
    public class FileTag : BaseModel
    {

        [Column("file_id")]
        public Guid FileId { get; set; }

        [Column("tag_id")]
        public Guid? TagId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
