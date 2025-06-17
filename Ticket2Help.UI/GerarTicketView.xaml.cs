using Ticket2Help.DAL.Repositories;
using Ticket2Help.DAL.Models;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore.Design;

namespace Ticket2Help.UI.Views
{
    public partial class GerarTicketView : Window
    {
        private readonly TicketRepository _ticketRepository;

        public GerarTicketView(TicketRepository ticketRepository)
        {
            InitializeComponent();
            _ticketRepository = ticketRepository;
        }

        private void BtnSubmeter_Click(object sender, RoutedEventArgs e)
        {
            string tipo = ((ComboBoxItem)cbTipoTicket.SelectedItem).Content.ToString();
            int.TryParse(txtCodigoColaborador.Text, out int codigo);
            string descricao = txtDescricao.Text;

            Ticket ticket = tipo == "Hardware" ?
                new HardwareTicket { Id = codigo, Malfunction = descricao } :
                new SoftwareTicket { Id = codigo, NecessityDescription = descricao };

            _ticketRepository.AddTicket(ticket);
            MessageBox.Show("Ticket enviado com sucesso!");
            this.Close();
        }
    }
}
