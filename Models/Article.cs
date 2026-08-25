using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace DotnetAPI.Models;

public class Article
{
    [Key]
    [DatabaseGenerated( DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public string Title { get; set; } = "no value";
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    [ForeignKey("User")]
    public int UserId { get; set; }

    public User? User { get; set; }
}