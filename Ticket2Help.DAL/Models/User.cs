
namespace Ticket2Help.DAL.Models
{
    public class User
    {
        public int Id { get; set; }               // Unique user identifier
        public string Name { get; set; }          // User's full name
        public string Email { get; set; }         // User's email (unique)
        public string Password { get; set; }      // User's password
        public UserType Type { get; set; }        // User type (Technician or Regular)
    }

    // Enum for user type (Technician or Regular)
    public enum UserType
    {
        Technician,  // Technician user (can attend to tickets)
        Regular      // Regular user (can create and view their own tickets)
    }
}


