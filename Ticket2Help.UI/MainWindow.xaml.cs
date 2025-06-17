using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.EntityFrameworkCore.Design;

namespace Ticket2Help.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnGerarTicket_Click(object sender, RoutedEventArgs e)
        {
            //var gerarTicket = new Views.GerarTicketView();
            //gerarTicket.Show();
        }

        private void BtnListarTickets_Click(object sender, RoutedEventArgs e)
        {
            //var listar = new Views.ListarTicketsView();
            //listar.Show();
        }

        private void BtnTecnico_Click(object sender, RoutedEventArgs e)
        {
            //var tecnico = new Views.AtenderTicketView();
            //tecnico.Show();
        }
    }





    }