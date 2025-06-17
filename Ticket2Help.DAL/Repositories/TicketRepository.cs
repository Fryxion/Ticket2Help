
using Ticket2Help.DAL.Models;
using Ticket2Help.DAL.Database;
using System.Linq;

namespace Ticket2Help.DAL.Repositories
{
    public class TicketRepository
    {
        private readonly TicketDbContext _context;

        public TicketRepository(TicketDbContext context)
        {
            _context = context;
        }

        public void AddTicket(Ticket ticket)
        {
            _context.Tickets.Add(ticket);
            _context.SaveChanges();
        }

        public IQueryable<Ticket> GetTicketsByEmployee(int employeeId)
        {
            return _context.Tickets.Where(t => t.EmployeeId == employeeId);
        }

        public void UpdateTicket(Ticket ticket)
        {
            _context.Tickets.Update(ticket);
            _context.SaveChanges();
        }

        public Ticket GetTicketById(int id)
        {
            return _context.Tickets.FirstOrDefault(t => t.Id == id);
        }
    }
}
