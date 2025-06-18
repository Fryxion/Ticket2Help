using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Ticket2Help.BLL.Controllers;
using Ticket2Help.BLL.Models;

namespace Ticket2Help.UI
{
    /// <summary>
    /// Janela para visualização de tickets do utilizador
    /// Permite filtrar, pesquisar e ver detalhes dos tickets
    /// </summary>
    public partial class ViewTicketsWindow : Window, INotifyPropertyChanged
    {
        #region Propriedades e Campos

        private readonly User _currentUser;
        private readonly TicketController _controller;
        private readonly ObservableCollection<Ticket> _allTickets;
        private readonly CollectionViewSource _ticketsViewSource;

        private string _searchText = string.Empty;
        private Ticket _selectedTicket;
        private bool _isLoading = false;

        /// <summary>
        /// Texto de pesquisa
        /// </summary>
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                ApplyFilters();
            }
        }

        /// <summary>
        /// Ticket selecionado
        /// </summary>
        public Ticket SelectedTicket
        {
            get => _selectedTicket;
            set
            {
                _selectedTicket = value;
                OnPropertyChanged(nameof(SelectedTicket));
                UpdateTicketDetails();
            }
        }

        /// <summary>
        /// Coleção filtrada de tickets
        /// </summary>
        public ICollectionView FilteredTickets => _ticketsViewSource.View;

        /// <summary>
        /// Indica se está carregando
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
                LoadingOverlay.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        #endregion

        #region Construtor

        /// <summary>
        /// Construtor da janela
        /// </summary>
        public ViewTicketsWindow(User currentUser, TicketController controller)
        {
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
            _allTickets = new ObservableCollection<Ticket>();
            _ticketsViewSource = new CollectionViewSource { Source = _allTickets };

            InitializeComponent();
            SetupWindow();
            SetupDataBinding();
            SetupAccessibility();
            LoadTicketsAsync();
        }

        #endregion

        #region Configuração Inicial

        /// <summary>
        /// Configura a janela
        /// </summary>
        private void SetupWindow()
        {
            DataContext = this;
            Title = $"Meus Tickets - {_currentUser.Nome} - Ticket2Help";

            // Configurar título baseado no tipo de utilizador
            if (_currentUser.PodeAtenderTickets())
            {
                TitleText.Text = "Gestão de Tickets";
                SubtitleText.Text = "Visualize todos os tickets do sistema";
            }
            else
            {
                TitleText.Text = "Meus Tickets";
                SubtitleText.Text = "Visualize e acompanhe seus tickets de suporte";
            }
        }

        /// <summary>
        /// Configura databinding
        /// </summary>
        private void SetupDataBinding()
        {
            TicketsDataGrid.ItemsSource = FilteredTickets;

            // Configurar filtro personalizado
            _ticketsViewSource.Filter += TicketsViewSource_Filter;
        }

        /// <summary>
        /// Configura acessibilidade
        /// </summary>
        private void SetupAccessibility()
        {
            // Teclas de atalho
            KeyDown += ViewTicketsWindow_KeyDown;

            // Ordem de tabulação
            RefreshButton.TabIndex = 1;
            CreateTicketButton.TabIndex = 2;
            SearchTextBox.TabIndex = 3;
            StatusFilterComboBox.TabIndex = 4;
            TypeFilterComboBox.TabIndex = 5;
            DateFilterComboBox.TabIndex = 6;
            TicketsDataGrid.TabIndex = 7;
            CloseButton.TabIndex = 8;

            // Duplo-clique no DataGrid
            TicketsDataGrid.MouseDoubleClick += TicketsDataGrid_MouseDoubleClick;

            // Focar na pesquisa quando a janela abre
            Loaded += (s, e) => SearchTextBox.Focus();
        }

        #endregion

        #region Carregamento de Dados

        /// <summary>
        /// Carrega os tickets do utilizador
        /// </summary>
        private async void LoadTicketsAsync()
        {
            try
            {
                IsLoading = true;
                ShowStatus("Carregando tickets...");

                // Determinar que tickets carregar baseado no tipo de utilizador
                var result = _currentUser.PodeAtenderTickets()
                    ? await _controller.GetPendingTicketsAsync()  // Técnicos veem todos os tickets pendentes
                    : await _controller.GetUserTicketsAsync(_currentUser.UserId); // Utilizadores veem apenas os seus

                if (result.IsSuccess)
                {
                    _allTickets.Clear();
                    foreach (var ticket in result.Data.OrderByDescending(t => t.DataCriacao))
                    {
                        // Enriquecer ticket com dados adicionais se necessário
                        await EnrichTicketData(ticket);
                        _allTickets.Add(ticket);
                    }

                    UpdateStatistics();
                    ShowStatus($"Carregados {_allTickets.Count} tickets");
                }
                else
                {
                    ShowMessage($"Erro ao carregar tickets: {result.ErrorMessage}",
                               "Erro", MessageBoxImage.Error);
                    ShowStatus("Erro ao carregar tickets");
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro inesperado: {ex.Message}", "Erro", MessageBoxImage.Error);
                ShowStatus("Erro inesperado");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Enriquece dados do ticket (nome do técnico, etc.)
        /// </summary>
        private async Task EnrichTicketData(Ticket ticket)
        {
            try
            {
                await Task.Delay(1);
            }
            catch (Exception ex)
            {
                // Log do erro, mas não interromper o carregamento
                Console.WriteLine($"Erro ao enriquecer dados do ticket {ticket?.TicketId}: {ex.Message}");
            }
        }

        #endregion

        #region Filtros e Pesquisa

        /// <summary>
        /// Filtro personalizado para a coleção de tickets
        /// </summary>
        private void TicketsViewSource_Filter(object sender, FilterEventArgs e)
        {
            if (e.Item is Ticket ticket)
            {
                e.Accepted = PassesAllFilters(ticket);
            }
        }

        /// <summary>
        /// Verifica se o ticket passa por todos os filtros aplicados
        /// </summary>
        private bool PassesAllFilters(Ticket ticket)
        {
            // Filtro de pesquisa
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchLower = SearchText.ToLower();
                var passesSearch = ticket.TicketId.ToString().Contains(searchLower) ||
                                  ticket.TipoTicket.ToString().ToLower().Contains(searchLower) ||
                                  ticket.GetInformacaoEspecifica().ToLower().Contains(searchLower);

                if (!passesSearch) return false;
            }

            // Filtro de estado
            if (StatusFilterComboBox.SelectedIndex > 0)
            {
                var selectedStatus = ((ComboBoxItem)StatusFilterComboBox.SelectedItem).Content.ToString();
                var ticketStatus = ticket.EstadoTicket.ToString();

                var statusMatch = selectedStatus switch
                {
                    "Por Atender" => ticketStatus == "PorAtender",
                    "Em Atendimento" => ticketStatus == "EmAtendimento",
                    "Atendido" => ticketStatus == "Atendido",
                    _ => true
                };

                if (!statusMatch) return false;
            }

            // Filtro de tipo
            if (TypeFilterComboBox.SelectedIndex > 0)
            {
                var selectedType = ((ComboBoxItem)TypeFilterComboBox.SelectedItem).Content.ToString();
                if (ticket.TipoTicket.ToString() != selectedType)
                    return false;
            }

            // Filtro de data
            if (DateFilterComboBox.SelectedIndex > 0)
            {
                var selectedPeriod = ((ComboBoxItem)DateFilterComboBox.SelectedItem).Content.ToString();
                var cutoffDate = selectedPeriod switch
                {
                    "Últimos 7 dias" => DateTime.Now.AddDays(-7),
                    "Últimos 30 dias" => DateTime.Now.AddDays(-30),
                    "Últimos 90 dias" => DateTime.Now.AddDays(-90),
                    _ => DateTime.MinValue
                };

                if (ticket.DataCriacao < cutoffDate)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Aplica todos os filtros
        /// </summary>
        private void ApplyFilters()
        {
            FilteredTickets.Refresh();
            UpdateStatistics();
        }

        /// <summary>
        /// Atualiza as estatísticas mostradas
        /// </summary>
        private void UpdateStatistics()
        {
            var totalTickets = _allTickets.Count;
            var filteredCount = FilteredTickets.Cast<object>().Count();

            TotalTicketsText.Text = $"Total: {totalTickets}";
            FilteredTicketsText.Text = $"Exibindo: {filteredCount}";
        }

        #endregion

        #region Event Handlers - Filtros

        /// <summary>
        /// Manipula mudança no texto de pesquisa
        /// </summary>
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // A propriedade SearchText já tem binding e chama ApplyFilters()
        }

        /// <summary>
        /// Manipula mudança no filtro de estado
        /// </summary>
        private void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded) ApplyFilters();
        }

        /// <summary>
        /// Manipula mudança no filtro de tipo
        /// </summary>
        private void TypeFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded) ApplyFilters();
        }

        /// <summary>
        /// Manipula mudança no filtro de data
        /// </summary>
        private void DateFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded) ApplyFilters();
        }

        /// <summary>
        /// Limpa todos os filtros
        /// </summary>
        private void ClearFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            StatusFilterComboBox.SelectedIndex = 0;
            TypeFilterComboBox.SelectedIndex = 0;
            DateFilterComboBox.SelectedIndex = 0;

            ApplyFilters();
            ShowStatus("Filtros limpos");
        }

        #endregion

        #region Event Handlers - DataGrid

        /// <summary>
        /// Manipula seleção de ticket no DataGrid
        /// </summary>
        private void TicketsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedTicket = TicketsDataGrid.SelectedItem as Ticket;
        }

        /// <summary>
        /// Manipula duplo-clique no DataGrid
        /// </summary>
        private void TicketsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SelectedTicket != null)
            {
                ViewFullDetailsButton_Click(sender, new RoutedEventArgs());
            }
        }

        #endregion

        #region Event Handlers - Botões

        /// <summary>
        /// Atualiza a lista de tickets
        /// </summary>
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadTicketsAsync();
        }

        /// <summary>
        /// Abre janela de criação de ticket
        /// </summary>
        private void CreateTicketButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var createWindow = new CreateTicketWindow(_currentUser, _controller);
                createWindow.Owner = this;

                if (createWindow.ShowDialog() == true && createWindow.TicketCreated)
                {
                    ShowMessage(
                        $"✅ Ticket #{createWindow.CreatedTicketId} criado com sucesso!\n\n" +
                        "A lista será atualizada automaticamente.",
                        "Ticket Criado",
                        MessageBoxImage.Information);

                    // Recarregar lista após criação
                    LoadTicketsAsync();
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao abrir criação de ticket: {ex.Message}",
                           "Erro", MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Ver detalhes completos do ticket
        /// </summary>
        private void ViewFullDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedTicket == null)
            {
                ShowMessage("Selecione um ticket para ver os detalhes.",
                           "Aviso", MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Aqui você pode abrir uma janela de detalhes completos
                var detailsMessage = GetFullTicketDetails(SelectedTicket);
                ShowMessage(detailsMessage, $"Detalhes do Ticket #{SelectedTicket.TicketId}",
                           MessageBoxImage.Information);

                // Exemplo de como implementar com janela própria:
                // var detailsWindow = new TicketDetailsWindow(SelectedTicket, _controller);
                // detailsWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao exibir detalhes: {ex.Message}",
                           "Erro", MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Imprimir ticket
        /// </summary>
        private void PrintTicketButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedTicket == null)
            {
                ShowMessage("Selecione um ticket para imprimir.",
                           "Aviso", MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Implementar funcionalidade de impressão
                ShowMessage("Funcionalidade de impressão será implementada em breve.",
                           "Informação", MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao imprimir: {ex.Message}",
                           "Erro", MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Fechar janela
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = new MainWindow(_currentUser, _controller);
            mainWindow.Show();
            this.Close();
        }

        #endregion

        #region Event Handlers - Teclado

        /// <summary>
        /// Manipula teclas de atalho
        /// </summary>
        private void ViewTicketsWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F5:
                    RefreshButton_Click(sender, new RoutedEventArgs());
                    e.Handled = true;
                    break;

                case Key.Escape:
                    if (SelectedTicket != null)
                    {
                        TicketsDataGrid.SelectedItem = null;
                    }
                    else
                    {
                        Close();
                    }
                    e.Handled = true;
                    break;

                case Key.Enter:
                    if (SelectedTicket != null)
                    {
                        ViewFullDetailsButton_Click(sender, new RoutedEventArgs());
                    }
                    e.Handled = true;
                    break;

                case Key.F3:
                    SearchTextBox.Focus();
                    SearchTextBox.SelectAll();
                    e.Handled = true;
                    break;
            }
        }

        #endregion

        #region Atualização de Detalhes

        /// <summary>
        /// Atualiza o painel de detalhes do ticket selecionado
        /// </summary>
        private void UpdateTicketDetails()
        {
            if (SelectedTicket == null)
            {
                NoSelectionMessage.Visibility = Visibility.Visible;
                DetailsContentPanel.Visibility = Visibility.Collapsed;
                return;
            }

            NoSelectionMessage.Visibility = Visibility.Collapsed;
            DetailsContentPanel.Visibility = Visibility.Visible;

            // Informações básicas
            DetailIdText.Text = SelectedTicket.TicketId.ToString();
            DetailTypeText.Text = SelectedTicket.TipoTicket.ToString();
            DetailStatusText.Text = SelectedTicket.EstadoTicket.ToString();
            DetailCreatedText.Text = SelectedTicket.DataCriacao.ToString("dd/MM/yyyy HH:mm");

            // Cronologia
            DetailAttendedText.Text = SelectedTicket.DataAtendimento?.ToString("dd/MM/yyyy HH:mm") ?? "Não atendido";
            DetailTechnicianText.Text = string.IsNullOrWhiteSpace(SelectedTicket.TecnicoId)
                ? "Não atribuído"
                : SelectedTicket.TecnicoId; // Aqui você pode carregar o nome do técnico

            // Detalhes específicos
            DetailSpecificInfoText.Text = SelectedTicket.GetInformacaoEspecifica();

            ShowStatus($"Exibindo detalhes do ticket #{SelectedTicket.TicketId}");
        }

        #endregion

        #region Métodos Auxiliares

        /// <summary>
        /// Obtém detalhes completos do ticket formatados
        /// </summary>
        private string GetFullTicketDetails(Ticket ticket)
        {
            var details = $"TICKET #{ticket.TicketId}\n";
            details += new string('=', 40) + "\n\n";

            details += $"Tipo: {ticket.TipoTicket}\n";
            details += $"Estado: {ticket.EstadoTicket}\n";
            details += $"Criado em: {ticket.DataCriacao:dd/MM/yyyy HH:mm}\n";

            if (ticket.DataAtendimento.HasValue)
            {
                details += $"Atendido em: {ticket.DataAtendimento:dd/MM/yyyy HH:mm}\n";
            }

            if (!string.IsNullOrWhiteSpace(ticket.TecnicoId))
            {
                details += $"Técnico: {ticket.TecnicoId}\n";
            }

            details += "\nDetalhes Específicos:\n";
            details += new string('-', 20) + "\n";
            details += ticket.GetInformacaoEspecifica();

            if (ticket.EstadoAtendimento.HasValue)
            {
                details += $"\n\nEstado do Atendimento: {ticket.EstadoAtendimento}";
            }

            return details;
        }

        /// <summary>
        /// Mostra status na barra inferior
        /// </summary>
        private void ShowStatus(string message)
        {
            StatusText.Text = message;
        }

        /// <summary>
        /// Mostra mensagem para o utilizador
        /// </summary>
        private void ShowMessage(string message, string title = "Informação",
                                MessageBoxImage icon = MessageBoxImage.Information)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, icon);
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    /// <summary>
    /// Extensões para a classe Ticket para exibição na UI
    /// </summary>
    public static class TicketExtensions
    {
        /// <summary>
        /// Obtém um resumo da descrição para exibição na lista
        /// </summary>
        public static string GetResumoDescricao(this Ticket ticket)
        {
            var info = ticket.GetInformacaoEspecifica();
            if (string.IsNullOrWhiteSpace(info))
                return "Sem descrição";

            // Pegar apenas a primeira linha ou os primeiros 100 caracteres
            var lines = info.Split('\n');
            var firstLine = lines[0];

            if (firstLine.Length > 100)
                return firstLine.Substring(0, 100) + "...";

            return firstLine;
        }
    }
}