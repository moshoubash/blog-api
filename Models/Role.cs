using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DotnetAPI.Models;

public class Role
{
    public const int AuthorId = 1;
    public const string Author = "Author";

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [JsonIgnore]
    public ICollection<User> Users { get; set; } = new List<User>();
}
