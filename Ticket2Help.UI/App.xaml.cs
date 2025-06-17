using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Windows;
using Ticket2Help.BLL.Services;
using Ticket2Help.UI.ViewModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Ticket2Help.DAL.Database;
using Microsoft.Extensions.Configuration;
using Ticket2Help.BLL.Configuration;
using Ticket2Help.BLL.Controllers;

namespace Ticket2Help.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IServiceProvider _serviceProvider;

        protected override async void OnStartup(StartupEventArgs e)
        {
            // Configurar dependency injection
            var services = new ServiceCollection();
            var configuration = BuildConfiguration();

            services.ConfigureServices(configuration);

            _serviceProvider = services.BuildServiceProvider();

            // Inicializar sistema
            await SystemBootstrap.InitializeApplicationAsync(_serviceProvider);

            // Criar janela principal
            var mainWindow = new MainWindow();
            mainWindow.DataContext = _serviceProvider.GetService<TicketController>();
            mainWindow.Show();

            base.OnStartup(e);
        }

        private IConfiguration BuildConfiguration()
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .Build();
        }
    }

}
