using Ticket2Help.BLL.Services;
using Ticket2Help.DAL.Models;
using System.Collections.ObjectModel;

namespace Ticket2Help.UI.ViewModel
{
    public class TicketViewModel
    {
        private readonly TicketService _ticketService;
        public ObservableCollection<Ticket> Tickets { get; set; }

        public TicketViewModel(TicketService ticketService)
        {
            _ticketService = ticketService;
            Tickets = new ObservableCollection<Ticket>();
        }

        // Função para criar um novo ticket
        public void CreateTicket(int employeeCode, string ticketType, string description, string details)
        {
            _ticketService.CreateTicket(employeeCode, ticketType, description, details);
        }

        // Função para carregar tickets de um colaborador
        public void LoadTickets(int employeeCode)
        {
            var tickets = _ticketService.GetTicketsByEmployee(employeeCode);
            Tickets.Clear();
            foreach (var ticket in tickets)
            {
                Tickets.Add(ticket);
            }
        }

       /*
        // Função para atender um ticket
        public void AnswerTicket(int ticketId, TicketStatus status, AttendanceStatus attendanceStatus)
        {
            _ticketService.AnswerTicket(ticketId, status, attendanceStatus);
        }
       */
    }
}
