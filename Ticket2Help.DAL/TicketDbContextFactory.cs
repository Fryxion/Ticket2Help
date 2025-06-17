using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Ticket2Help.DAL.Database;

namespace Ticket2Help.DAL.Database
{
    public class TicketDbContextFactory : IDesignTimeDbContextFactory<TicketDbContext>
    {
        public TicketDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TicketDbContext>();
            optionsBuilder.UseSqlServer("Server=localhost;Database=Ticket2HelpDb;Trusted_Connection=True;");

            return new TicketDbContext(optionsBuilder.Options);
        }
    }
}