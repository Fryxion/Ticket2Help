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
            _currentUser = currentUser;
            _controller = controller;
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
                    TotalTicketsText.Text = "Total: " + _allTickets.Count.ToString();
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

        #endregion

        #region Filtros e Pesquisa

        /// <summary>
        /// Filtro aplicado à coleção de tickets
        /// </summary>
        private void TicketsViewSource_Filter(object sender, FilterEventArgs e)
        {
            if (e.Item is not Ticket ticket)
            {
                e.Accepted = false;
                return;
            }

            // Filtro por texto
            if (!string.IsNullOrWhiteSpace(_searchText))
            {
                var search = _searchText.ToLower();
                if (!(ticket.ToString().ToLower().Contains(search)
                    || ticket.GetInformacaoEspecifica().ToLower().Contains(search)))
                {
                    e.Accepted = false;
                    return;
                }
            }

            // Filtro por estado
            if (StatusFilterComboBox.SelectedItem is EstadoTicket estadoSelecionado &&
                ticket.EstadoTicket != estadoSelecionado)
            {
                e.Accepted = false;
                return;
            }

            // Filtro por tipo
            if (TypeFilterComboBox.SelectedItem is TipoTicket tipoSelecionado &&
                ticket.TipoTicket != tipoSelecionado)
            {
                e.Accepted = false;
                return;
            }

            // Filtro por data (exemplo simples: mês atual)
            if (DateFilterComboBox.SelectedIndex == 1 && ticket.DataCriacao.Month != DateTime.Now.Month)
            {
                e.Accepted = false;
            }
        }

        /// <summary>
        /// Aplica todos os filtros ativos
        /// </summary>
        private void ApplyFilters()
        {
            _ticketsViewSource.View.Refresh();
        }

        #endregion

        #region UI - Detalhes, Atualização, Mensagens

        /// <summary>
        /// Atualiza o painel de detalhes quando seleciona outro ticket
        /// </summary>
        private void UpdateTicketDetails()
        {
            if (_selectedTicket == null)
            {
                TicketDetailsPanel.Visibility = Visibility.Collapsed;
                return;
            }
            

            TicketDetailsPanel.Visibility = Visibility.Visible;
            DetailsContentPanel.Visibility = Visibility.Visible;
            NoSelectionMessage.Visibility = Visibility.Collapsed;
            var ticket = _allTickets.FirstOrDefault(t => t.TicketId == _selectedTicket.TicketId);

            if (ticket != null)
            {
                DetailIdText.Text = ticket.TicketId.ToString();
                DetailTypeText.Text = ticket.TipoTicket.ToString();
                DetailStatusText.Text = ticket.EstadoTicket.ToString();
                DetailCreatedText.Text = ticket.DataCriacao.ToString();

            }


            DetailSpecificInfoText.Text = _selectedTicket.GetInformacaoEspecifica();
        }

        /// <summary>
        /// Mostra mensagem no status bar
        /// </summary>
        private void ShowStatus(string msg)
        {
            StatusText.Text = msg;
        }

        /// <summary>
        /// Mostra uma MessageBox customizada
        /// </summary>
        private void ShowMessage(string msg, string title = "Info", MessageBoxImage icon = MessageBoxImage.Information)
        {
            MessageBox.Show(this, msg, title, MessageBoxButton.OK, icon);
        }

        #endregion

        #region Comandos UI (Botões, DataGrid, etc)

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadTicketsAsync();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = new MainWindow(_currentUser, _controller);
            mainWindow.Show();
            this.Close();
        }

        private void CreateTicketButton_Click(object sender, RoutedEventArgs e)
        {
            // Abre janela de criação (deves ter uma CreateTicketWindow implementada)
            //var createWindow = new CreateTicketWindow(_currentUser, _controller);
            //if (createWindow.ShowDialog() == true)
            //{
            //   LoadTicketsAsync(); // Recarrega tickets após criação
            //}
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchText = SearchTextBox.Text;
        }

        private void StatusFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void TypeFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void DateFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        // Para botões (Click)
        private void ViewFullDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Lógica para abrir detalhes completos do ticket selecionado
        }

        private void PrintTicketButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Lógica para imprimir ou exportar o ticket
        }

        private void ClearFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            StatusFilterComboBox.SelectedIndex = -1;
            TypeFilterComboBox.SelectedIndex = -1;
            DateFilterComboBox.SelectedIndex = -1;
            ApplyFilters();
        }

        // Para SelectionChanged em ComboBox/DataGrid
        private void TypeFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void DateFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        // Para seleção no DataGrid
        private void TicketsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TicketsDataGrid.SelectedItem is Ticket selected)
            {
                SelectedTicket = selected;
            }
        }


        private void TicketsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SelectedTicket != null)
            {
                // Mostra detalhes completos ou permite ações conforme perfil
                //var details = new TicketDetailsWindow(SelectedTicket, _controller, _currentUser);
                //details.ShowDialog();
            }
        }

        private void ViewTicketsWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                LoadTicketsAsync();
                e.Handled = true;
            }
        }

        #endregion

        #region Utilidades

        private async Task EnrichTicketData(Ticket ticket)
        {
            // Caso queiras enriquecer o ticket com info adicional (ex: nome técnico)
            // Podes implementar fetch extra aqui, se precisares
            await Task.CompletedTask;
        }

        private void UpdateStatistics()
        {
            // Se quiseres mostrar estatísticas em labels da janela (total, resolvidos, etc.)
            // Exemplo:
            // TotalTicketsTextBlock.Text = _allTickets.Count.ToString();
            // ResolvedTicketsTextBlock.Text = _allTickets.Count(t => t.EstadoAtendimento == EstadoAtendimento.Resolvido).ToString();
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion
    }
}
