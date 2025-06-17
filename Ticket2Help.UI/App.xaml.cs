using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Ticket2Help.BLL.Configuration;
using Ticket2Help.BLL.Models;
using Ticket2Help.BLL.Services;

namespace Ticket2Help.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            try
            {
                // Inicializar o sistema Ticket2Help
                await Ticket2HelpSystem.StartAsync();

                // Verificar se sistema está saudável
                if (!Ticket2HelpSystem.IsSystemHealthy)
                {
                    MessageBox.Show("Erro ao inicializar o sistema!", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    Shutdown();
                    return;
                }

                // Criar e mostrar janela principal
                //var mainWindow = new LoginWindow();
                //mainWindow.Show();

                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro fatal: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Libertar recursos do sistema
            Ticket2HelpSystem.Shutdown();
            base.OnExit(e);
        }
    }

}
