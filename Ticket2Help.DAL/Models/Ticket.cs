
namespace Ticket2Help.DAL.Models
{
    public abstract class Ticket
    {
        public int Id { get; set; }               // Ticket ID
        public DateTime CreationDate { get; set; } = DateTime.Now; // Ticket creation date
        public int EmployeeId { get; set; }       // ID of the employee who created the ticket
        public TicketStatus Status { get; set; } = TicketStatus.Pending; // Ticket status (e.g., Pending, In Progress, Attended)
        public DateTime? AttendanceDate { get; set; } // Date when the ticket was attended
        public AttendanceStatus? AttendanceStatus { get; set; } // Ticket attendance status (e.g., Open, Resolved)

        public abstract string GetSummary();  // Method to get a summary of the ticket
    }

    // Enum for ticket status
    public enum TicketStatus
    {
        Pending,   // Ticket is not attended yet
        InProgress, // Ticket is being worked on
        Attended   // Ticket has been resolved
    }

    // Enum for ticket attendance status
    public enum AttendanceStatus
    {
        Open,       // Ticket is open
        Resolved,   // Ticket is resolved
        Unresolved  // Ticket was not resolved
    }
}