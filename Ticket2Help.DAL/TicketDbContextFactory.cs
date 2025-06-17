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
            optionsBuilder.UseSqlServer("Server=localhost\\SQLEXPRESS;Database=Ticket2HelpDb;User Id=sa;Password=123;TrustServerCertificate=True;");

            return new TicketDbContext(optionsBuilder.Options);
        }
    }
}