using Microsoft.EntityFrameworkCore;
using Ticket2Help.BLL.Models;
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

        // Método para adicionar um ticket
        public void AdicionarTicket(Ticket ticket)
        {
            _context.Tickets.Add(ticket);
            _context.SaveChanges();
        }

        // Método para obter tickets de um colaborador
        public IQueryable<Ticket> ObterTicketsPorColaborador(int codigoColaborador)
        {
            return _context.Tickets.Where(t => t.CodigoColaborador == codigoColaborador);
        }

        // Método para marcar ticket como atendido
        public void AtenderTicket(Ticket ticket)
        {
            _context.Tickets.Update(ticket);
            _context.SaveChanges();
        }
    }
}