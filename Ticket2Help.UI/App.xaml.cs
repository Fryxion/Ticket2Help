using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Windows;
using Ticket2Help.BLL.Services;
using Ticket2Help.DAL.Database;
using Ticket2Help.DAL.Repositories;
using Ticket2Help.UI.ViewModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Ticket2Help.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            var serviceProvider = new ServiceCollection()
                .AddDbContext<TicketDbContext>(options => options.UseSqlServer("Server=localhost\\SQLEXPRESS;Database=Ticket2HelpDb;User Id=sa;Password=123;TrustServerCertificate=True;"))
                .AddSingleton<TicketService>()
                .AddSingleton<TicketViewModel>()
                .AddSingleton<TicketRepository>()
                .BuildServiceProvider();

            var telaTicket = serviceProvider.GetRequiredService<TelaTicket>();
            telaTicket.Show();
        }
    }

}
