using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Ticket2Help.BLL.Controllers;
using Ticket2Help.BLL.Models;
using Ticket2Help.UI.Windows;

namespace Ticket2Help.UI
{
    /// <summary>
    /// Janela principal simples e acessível do sistema Ticket2Help
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly BLL.Models.User _currentUser;
        private readonly TicketController _controller;
        private DispatcherTimer _clockTimer;

        public MainWindow(BLL.Models.User user, TicketController controller)
        {
            _currentUser = user ?? throw new ArgumentNullException(nameof(user));
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));

            InitializeComponent();
            SetupAccessibility();
            SetupUserInterface();
            SetupClock();
            LoadUserDataAsync();
        }

        /// <summary>
        /// Configura recursos de acessibilidade
        /// </summary>
        private void SetupAccessibility()
        {
            // Configurar ordem de tabulação
            ViewTicketsButton.TabIndex = 1;
            CreateTicketButton.TabIndex = 2;
            AttendTicketsButton.TabIndex = 3;
            ViewReportsButton.TabIndex = 4;
            ProfileButton.TabIndex = 5;
            LogoutButton.TabIndex = 6;

            // Focar no primeiro botão disponível quando janela carrega
            Loaded += (s, e) => ViewTicketsButton.Focus();

            // Configurar teclas de atalho
            KeyDown += MainWindow_KeyDown;
        }

        /// <summary>
        /// Configura a interface baseada no tipo de utilizador
        /// </summary>
        private void SetupUserInterface()
        {
            // Atualizar título da janela
            Title = $"Ticket2Help - {_currentUser.Nome}";

            // Mostrar informações do utilizador
            UserWelcome.Text = $"Bem-vindo, {_currentUser.Nome}!";
            UserNameText.Text = _currentUser.Nome;
            UserTypeText.Text = _currentUser.GetDescricaoTipo();
            UserEmailText.Text = _currentUser.Email;
            LoginTimeText.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

            // Mostrar menus baseados no tipo de utilizador
            if (_currentUser.PodeAtenderTickets())
            {
                TechnicianActions.Visibility = Visibility.Visible;
                StatsCard.Visibility = Visibility.Visible;
            }

        }

        /// <summary>
        /// Configura o relógio em tempo real
        /// </summary>
        private void SetupClock()
        {
            _clockTimer = new DispatcherTimer();
            _clockTimer.Interval = TimeSpan.FromSeconds(1);
            _clockTimer.Tick += (s, e) => CurrentTimeText.Text = DateTime.Now.ToString("HH:mm:ss");
            _clockTimer.Start();

            // Mostrar hora inicial
            CurrentTimeText.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        /// <summary>
        /// Carrega dados do utilizador de forma assíncrona
        /// </summary>
        private async void LoadUserDataAsync()
        {
            if (_currentUser.PodeAtenderTickets())
            {
                await LoadQuickStatsAsync();
            }
        }

        /// <summary>
        /// Carrega estatísticas rápidas
        /// </summary>
        private async Task LoadQuickStatsAsync()
        {
            try
            {
                ShowLoading("Carregando estatísticas...");

                // Obter estatísticas básicas
                var endDate = DateTime.Now;
                var startDate = endDate.AddDays(-30); // Últimos 30 dias

                var summary = await _controller.GetDashboardSummaryAsync(startDate, endDate);

                // Atualizar interface
                TotalTicketsText.Text = summary.Data.TotalTickets.ToString();
                PendingTicketsText.Text = (summary.Data.TotalTickets - summary.Data.TicketsAtendidos).ToString();
                ResolvedTicketsText.Text = summary.Data.TicketsResolvidos.ToString();

                HideLoading();
            }
            catch (Exception ex)
            {
                HideLoading();
                ShowMessage($"Erro ao carregar estatísticas: {ex.Message}", "Aviso", MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Manipula teclas de atalho
        /// </summary>
        private void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case System.Windows.Input.Key.F1:
                    HelpButton_Click(null, null);
                    break;
                case System.Windows.Input.Key.F5:
                    RefreshDataAsync();
                    break;
                case System.Windows.Input.Key.Escape:
                    ViewTicketsButton.Focus();
                    break;
            }
        }

        /// <summary>
        /// Atualiza dados da interface
        /// </summary>
        private async void RefreshDataAsync()
        {
            if (_currentUser.PodeAtenderTickets())
            {
                await LoadQuickStatsAsync();
            }
        }

        #region Event Handlers dos Botões

        /// <summary>
        /// Ver tickets do utilizador
        /// </summary>
        private async void ViewTicketsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowLoading("Carregando seus tickets...");

                // Simular carregamento
                await Task.Delay(1000);

                HideLoading();
                //ShowMessage("Funcionalidade 'Ver Meus Tickets' em desenvolvimento.", "Informação", MessageBoxImage.Information);
                var mainWindow = new ViewTicketsWindow(_currentUser, _controller);
                mainWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                HideLoading();
                ShowMessage($"Erro: {ex.Message}", "Erro", MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Criar novo ticket
        /// </summary>
        private async void CreateTicketButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowLoading("Preparando formulário...");

                // Simular carregamento
                await Task.Delay(1000);

                HideLoading();
                var createTicketWindow = new CreateTicketWindow(_currentUser, _controller);
                createTicketWindow.Owner = this;

                var result = createTicketWindow.ShowDialog();

                // Se ticket foi criado com sucesso
                if (result == true && createTicketWindow.TicketCreated)
                {
                    var ticketId = createTicketWindow.CreatedTicketId;

                    ShowMessage(
                        $"✅ Ticket #{ticketId} criado com sucesso!\n\n" +
                        "Seu ticket foi adicionado à fila de atendimento.\n" +
                        "Você pode acompanhar o status na área 'Meus Tickets'.",
                        "Ticket Criado",
                        MessageBoxImage.Information);

                    // Atualizar estatísticas se for técnico/admin
                    if (_currentUser.PodeAtenderTickets())
                    {
                        await LoadQuickStatsAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                HideLoading();
                ShowMessage($"Erro: {ex.Message}", "Erro", MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Atender tickets (Técnico/Admin)
        /// </summary>
        private async void AttendTicketsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowLoading("Carregando tickets pendentes...");

                // Simular carregamento
                await Task.Delay(1000);

                HideLoading();

                // Criar e mostrar a janela
                var janelaAtendimento = new AtenderTicketsWindow(_currentUser, _controller);

                // Configurar como janela modal (opcional)
                // janelaAtendimento.Owner = this; // se for chamado de uma janela pai

                // Mostrar a janela
                janelaAtendimento.ShowDialog(); // Modal
                                                // ou
                                                // janelaAtendimento.Show(); // Não modal
            }
            catch (Exception ex)
            {
                HideLoading();
                ShowMessage($"Erro: {ex.Message}", "Erro", MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Ver relatórios (Técnico/Admin)
        /// </summary>
        private async void ViewReportsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowLoading("Gerando relatórios...");

                // Simular carregamento
                await Task.Delay(1000);

                HideLoading();

                var relatoriosWindow = new RelatoriosWindow(_currentUser, _controller);
                relatoriosWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                HideLoading();
                ShowMessage($"Erro: {ex.Message}", "Erro", MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Gerir utilizadores (Admin)
        /// </summary>
        private async void ManageUsersButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowLoading("Carregando utilizadores...");

                // Simular carregamento
                await Task.Delay(1000);

                HideLoading();
                ShowMessage("Funcionalidade 'Gerir Utilizadores' em desenvolvimento.", "Informação", MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                HideLoading();
                ShowMessage($"Erro: {ex.Message}", "Erro", MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Configurações do sistema (Admin)
        /// </summary>
        private async void SystemSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowLoading("Carregando configurações...");

                // Simular carregamento
                await Task.Delay(1000);

                HideLoading();
                ShowMessage("Funcionalidade 'Configurações' em desenvolvimento.", "Informação", MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                HideLoading();
                ShowMessage($"Erro: {ex.Message}", "Erro", MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Ver/editar perfil
        /// </summary>
        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            ShowMessage($"Perfil de {_currentUser.Nome}\n\n" +
                       $"Tipo: {_currentUser.GetDescricaoTipo()}\n" +
                       $"Email: {_currentUser.Email}\n" +
                       $"Ativo: {(_currentUser.Ativo ? "Sim" : "Não")}\n" +
                       $"Criado: {_currentUser.DataCriacao:dd/MM/yyyy}",
                       "Informações do Perfil", MessageBoxImage.Information);
        }

        /// <summary>
        /// Ajuda
        /// </summary>
        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            string helpText = "Ticket2Help - Ajuda\n\n" +
                             "Teclas de Atalho:\n" +
                             "• F1: Esta ajuda\n" +
                             "• F5: Atualizar dados\n" +
                             "• Tab: Navegar entre botões\n" +
                             "• Esc: Voltar ao primeiro botão\n\n" +
                             "Contacto:\n" +
                             "📧 suporte@ticket2help.com\n" +
                             "📞 +351 123 456 789";

            ShowMessage(helpText, "Ajuda", MessageBoxImage.Information);
        }

        /// <summary>
        /// Terminar sessão
        /// </summary>
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Tem a certeza que deseja terminar a sessão?",
                "Confirmar Logout",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Parar o timer do relógio
                _clockTimer?.Stop();

                // Abrir janela de login
                var loginWindow = new LoginWindow();
                loginWindow.Show();

                // Fechar janela atual
                this.Close();
            }
        }

        #endregion

        #region Métodos Auxiliares

        /// <summary>
        /// Mostra overlay de loading
        /// </summary>
        private void ShowLoading(string message = "Carregando...")
        {
            LoadingText.Text = message;
            LoadingOverlay.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Esconde overlay de loading
        /// </summary>
        private void HideLoading()
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Mostra mensagem para o utilizador
        /// </summary>
        private void ShowMessage(string message, string title = "Informação", MessageBoxImage icon = MessageBoxImage.Information)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, icon);
        }

        /// <summary>
        /// Cleanup quando a janela fecha
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            // Parar timer
            _clockTimer?.Stop();

            base.OnClosed(e);
        }

        #endregion
    }
}