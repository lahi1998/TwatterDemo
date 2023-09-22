using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TwatterDemo.Models
{
    public class User
    {
        // Model matcthing the table users in mysql.
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int userID { get; set; }

        public string userName { get; set; } = string.Empty;

        public string passwordHashed { get; set; } = string.Empty;

        public string userImg { get; set; } = string.Empty;

        public string creationDate { get; set; } =string.Empty;
    }
}
