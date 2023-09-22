using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Twats
{
    // Model matcthing the table twats in mysql.
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int twatId { get; set; }
    public string? userImg { get; set; }
    public required string username { get; set; }
    public required DateTime timeOfPost { get; set; }
    public required string content { get; set; }
}


