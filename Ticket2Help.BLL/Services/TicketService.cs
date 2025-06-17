using Ticket2Help.DAL.Models;
using Ticket2Help.DAL.Repositories;

namespace Ticket2Help.BLL.Services
{
    public class TicketService
    {
        private readonly TicketRepository _ticketRepository;

        public TicketService(TicketRepository ticketRepository)
        {
            _ticketRepository = ticketRepository;
        }

        public void CreateTicket(int employeeId, string ticketType, string description, string detail)
        {
            Ticket ticket = ticketType.ToLower() == "hardware"
                ? new HardwareTicket
                {
                    EmployeeId = employeeId,
                    Equipment = description,
                    Malfunction = detail
                }
                : new SoftwareTicket
                {
                    EmployeeId = employeeId,
                    SoftwareName = description,
                    NecessityDescription = detail
                };

            _ticketRepository.AddTicket(ticket);
        }

        public void UpdateTicketStatus(int ticketId, TicketStatus status, AttendanceStatus attendanceStatus)
        {
            var ticket = _ticketRepository.GetTicketById(ticketId);
            if (ticket != null)
            {
                ticket.Status = status;
                ticket.AttendanceStatus = attendanceStatus;
                ticket.AttendanceDate = DateTime.Now;
                _ticketRepository.UpdateTicket(ticket);
            }
        }

        // Add the method to get tickets by employee code
        public IQueryable<Ticket> GetTicketsByEmployee(int employeeCode)
        {
            return _ticketRepository.GetTicketsByEmployee(employeeCode);
        }
    }
}
