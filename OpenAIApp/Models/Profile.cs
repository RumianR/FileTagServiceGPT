using Postgrest.Attributes;
using Postgrest.Models;

namespace OpenAIApp.Models
{
    [Table("profiles")]
    public class Profile : BaseModel
    {
        [PrimaryKey("id")]
        public Guid Id { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; } // Nullable

        [Column("username")]
        public string Username { get; set; } // Nullable

        [Column("full_name")]
        public string FullName { get; set; } // Nullable

        [Column("avatar_url")]
        public string AvatarUrl { get; set; } // Nullable
    }
}
