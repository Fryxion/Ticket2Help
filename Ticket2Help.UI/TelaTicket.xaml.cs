using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Ticket2Help.BLL.Services;
using Ticket2Help.DAL.Database;
using Ticket2Help.DAL.Repositories;
using Ticket2Help.UI.ViewModel;

namespace Ticket2Help.UI
{
    /// <summary>
    /// Lógica interna para TelaTicket.xaml
    /// </summary>
    public partial class TelaTicket : Window
    {
        private readonly TicketViewModel _viewModel;

        public TelaTicket()
        {
            InitializeComponent();
            //_viewModel = new TicketViewModel(new TicketService(new TicketRepository(new TicketDbContext())));
            DataContext = _viewModel;
        }

        private void CriarTicket_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.CreateTicket(1, "hardware", "Problema no computador", "Apareceu tela azul");
            _viewModel.LoadTickets(1); // Atualiza a lista de tickets
        }
    }
}
