using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace DotnetAPI.Models;

public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required] public string Name { get; set; } = "No value";

    public DateTime CreatedAt { get; set; } = DateTime.Now; 
    
    public ICollection<Article> Articles { get; set; } = new List<Article>();
    
    public int RoleId { get; set; }
    public Role? Role { get; set; }
}