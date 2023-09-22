namespace TwatterDemo.Models
{
    // Model for user input, UserDTO is shorthand User Data transfer Object
    public class UserDTO
    {
        public required string userName { get; set; }
        public required string password { get; set; }
    }
}
