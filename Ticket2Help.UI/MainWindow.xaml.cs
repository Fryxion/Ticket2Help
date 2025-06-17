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
using Ticket2Help.BLL.Controllers;
using Ticket2Help.BLL.Services;


namespace Ticket2Help.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly TicketController _controller;

        public MainWindow()
        {
            InitializeComponent();
            // O controller será injetado via DataContext
            _controller = DataContext as TicketController;

            // Subscrever eventos
            _controller.TicketCreated += OnTicketCreated;
            _controller.TicketAttended += OnTicketAttended;
        }

        private async void CreateHardwareTicket_Click(object sender, RoutedEventArgs e)
        {
            var result = await _controller.CreateHardwareTicketAsync(
                userId: 1,
                equipment: "Computador Dell",
                malfunction: "Não liga"
            );

            if (result.IsSuccess)
            {
                MessageBox.Show("Ticket criado com sucesso!");
                // Atualizar UI
            }
            else
            {
                MessageBox.Show($"Erro: {result.ErrorMessage}");
            }
        }

        private async void AttendNextTicket_Click(object sender, RoutedEventArgs e)
        {
            var nextTicket = await _controller.GetNextTicketForAttendanceAsync();
            if (nextTicket.IsSuccess && nextTicket.Data != null)
            {
                var attendResult = await _controller.AttendTicketAsync(
                    nextTicket.Data.Id,
                    technicianId: 2
                );

                if (attendResult.IsSuccess)
                {
                    MessageBox.Show("Ticket atendido!");
                }
            }
        }

    }