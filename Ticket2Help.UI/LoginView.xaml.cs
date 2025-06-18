using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Ticket2Help.BLL.Controllers;
using Ticket2Help.BLL.Models;
using Ticket2Help.BLL.Configuration;
using System.Windows.Automation;
using System.Windows.Controls;

namespace Ticket2Help.UI
{
    /// <summary>
    /// Janela de login simples e acessível
    /// </summary>
    public partial class LoginWindow : Window
    {
        private TicketController _controller;
        private bool _isLoggingIn = false;

        public LoginWindow()
        {
            InitializeComponent();
            InitializeAsync();
            SetupAccessibility();
        }

        /// <summary>
        /// Configura recursos de acessibilidade
        /// </summary>
        private void SetupAccessibility()
        {
            // Configurar ordem de tabulação
            UsernameTextBox.TabIndex = 1;
            PasswordBox.TabIndex = 2;
            LoginButton.TabIndex = 3;

            // Configurar teclas de atalho
            UsernameTextBox.KeyDown += OnKeyDown;
            PasswordBox.KeyDown += OnKeyDown;

            // Focar no primeiro campo
            Loaded += (s, e) => UsernameTextBox.Focus();

            // Configurar eventos para limpeza de erro
            UsernameTextBox.TextChanged += (s, e) => ClearError();
            PasswordBox.PasswordChanged += (s, e) => ClearError();
        }

        /// <summary>
        /// Inicializa o sistema de forma assíncrona
        /// </summary>
        private async void InitializeAsync()
        {
            try
            {
                await InitializeSystemAsync();
            }
            catch (Exception ex)
            {
                ShowError($"Erro ao inicializar o sistema: {ex.Message}");
                LoginButton.IsEnabled = false;
            }
        }

        /// <summary>
        /// Inicializa o sistema backend
        /// </summary>
        private async Task InitializeSystemAsync()
        {
            await Ticket2HelpSystem.StartAsync();
            _controller = Ticket2HelpSystem.Controller;

            // Criar utilizadores padrão se necessário
            //var userService = Ticket2HelpSystem.UserService;
            //await userService.CreateDefaultUsersAsync();
        }

        /// <summary>
        /// Manipula teclas especiais
        /// </summary>
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    if (sender == UsernameTextBox)
                    {
                        PasswordBox.Focus();
                    }
                    else if (sender == PasswordBox && !_isLoggingIn)
                    {
                        LoginButton_Click(null, null);
                    }
                    break;

                case Key.Escape:
                    ClearError();
                    break;
            }
        }

        /// <summary>
        /// Evento do botão de login
        /// </summary>
        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isLoggingIn) return;

            await PerformLoginAsync();
        }

        /// <summary>
        /// Executa o processo de login
        /// </summary>
        private async Task PerformLoginAsync()
        {
            string username = UsernameTextBox.Text?.Trim();
            string password = PasswordBox.Password;

            // Validação simples
            if (string.IsNullOrEmpty(username))
            {
                ShowError("Por favor, introduza o nome de utilizador.");
                UsernameTextBox.Focus();
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                ShowError("Por favor, introduza a password.");
                PasswordBox.Focus();
                return;
            }

            _isLoggingIn = true;

            try
            {
                var result = await _controller.AuthenticateUserAsync(username, password);

                if (result.IsSuccess)
                {
                    // Login bem-sucedido
                    var user = result.Data;
                    OpenMainWindow(user);
                }
                else
                {
                    // Erro de autenticação
                    PasswordBox.Clear();
                    ShowError(result.ErrorMessage ?? "Nome de utilizador ou password incorretos.");
                    PasswordBox.Focus();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Erro durante a autenticação: {ex.Message}");
            }
            finally
            {
                _isLoggingIn = false;
            }
        }


        /// <summary>
        /// Abre a janela principal
        /// </summary>
        private void OpenMainWindow(User user)
        {
            try
            {
                //var mainWindow = new MainWindow();
                var mainWindow = new MainWindow(user, _controller);
                mainWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                ShowError($"Erro ao abrir a aplicação principal: {ex.Message}");
            }
        }

        /// <summary>
        /// Mostra mensagem de erro
        /// </summary>
        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorBorder.Visibility = Visibility.Visible;

            // Anunciar erro para leitores de ecrã
            AutomationProperties.SetLiveSetting(ErrorBorder, AutomationLiveSetting.Assertive);
        }

        /// <summary>
        /// Limpa mensagem de erro
        /// </summary>
        private void ClearError()
        {
            ErrorBorder.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Cleanup quando a janela fecha
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Libertar recursos se necessário
            try
            {
                if (Application.Current.MainWindow == this)
                {
                    Ticket2HelpSystem.Shutdown();
                }
            }
            catch
            {
                // Ignorar erros no shutdown
            }
        }

        /// <summary>
        /// Suporte para teclas de atalho globais
        /// </summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            // Alt + F4 para fechar
            if (e.Key == Key.F4 && e.KeyboardDevice.Modifiers == ModifierKeys.Alt)
            {
                this.Close();
                return;
            }

            base.OnKeyDown(e);
        }
    }

}