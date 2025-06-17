using System;
using Ticket2Help.DAL.Database;
using Ticket2Help.DAL.Models;


namespace Ticket2Help.DAL.Repositories
{
    public class UserRepository
    {
        private readonly TicketDbContext _context;

        public UserRepository(TicketDbContext context)
        {
            _context = context;
        }

        public User GetUserByEmail(string email)
        {
            return _context.Users.SingleOrDefault(u => u.Email == email);
        }

        public void AddUser(User usuario)
        {
            _context.Users.Add(usuario);
            _context.SaveChanges();
        }
    }
}
