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
using Ticket2Help.BLL.Models;

namespace Ticket2Help.UI
{
    /// <summary>
    /// Lógica interna para GerarTicketView.xaml
    /// </summary>
    public partial class GerarTicketView : Window
    {
        public GerarTicketView()
        {
            InitializeComponent();
        }

        private void BtnSubmeter_Click(object sender, RoutedEventArgs e)
        {
            string tipo = ((ComboBoxItem)cbTipoTicket.SelectedItem).Content.ToString();
            int.TryParse(txtCodigoColaborador.Text, out int codigo);
            string descricao = txtDescricao.Text;

            if (tipo == "Hardware")
            {
                var ticket = new HardwareTicket
                {
                    CodigoColaborador = codigo,
                    Equipamento = "A definir",
                    Avaria = descricao
                };

                // Salvar na base de dados (futuramente via DAL)
                MessageBox.Show("Ticket de Hardware criado com sucesso.");
            }
            else
            {
                var ticket = new SoftwareTicket
                {
                    CodigoColaborador = codigo,
                    NomeSoftware = "A definir",
                    DescricaoNecessidade = descricao
                };

                MessageBox.Show("Ticket de Software criado com sucesso.");
            }

            this.Close();
        }


    }
}
