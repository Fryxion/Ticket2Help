using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Ticket2Help.BLL.Models;
using Ticket2Help.BLL.Services;

namespace Ticket2Help.UI.Windows
{
    /// <summary>
    /// Janela para registo de novos utilizadores do sistema
    /// Implementa funcionalidades de acessibilidade e validação robusta
    /// </summary>
    public partial class RegistoWindow : Window
    {
        #region Propriedades Privadas

        /// <summary>
        /// Serviço para operações de utilizadores
        /// </summary>
        private readonly IUserService _userService;

        /// <summary>
        /// Indica se o formulário está válido
        /// </summary>
        private bool _formularioValido = false;

        /// <summary>
        /// Indica se está a processar um registo
        /// </summary>
        private bool _processandoRegisto = false;

        #endregion

        #region Construtor

        /// <summary>
        /// Construtor da janela de registo
        /// </summary>
        /// <param name="userService">Serviço de utilizadores</param>
        public RegistoWindow(UserService userService)
        {
            InitializeComponent();

            _userService = userService ?? throw new ArgumentNullException(nameof(userService));

            InicializarInterface();
            ConfigurarAcessibilidade();
        }

        /// <summary>
        /// Inicializa a interface do utilizador
        /// </summary>
        private void InicializarInterface()
        {

            // Definir foco inicial
            txtNome.Focus();

            // Atualizar status inicial
            AtualizarStatus("Preencha todos os campos obrigatórios", false);
        }

        /// <summary>
        /// Configura funcionalidades de acessibilidade
        /// </summary>
        private void ConfigurarAcessibilidade()
        {
            // Configurar navegação por teclado
            this.KeyDown += RegistoWindow_KeyDown;

            // Configurar tooltips
            txtNome.ToolTip = "Digite o nome completo (mínimo 2 caracteres)";
            txtUsername.ToolTip = "Nome único para login (3-20 caracteres, sem espaços)";
            txtEmail.ToolTip = "Endereço de email válido";
            txtPassword.ToolTip = "Password com pelo menos 6 caracteres";
            txtConfirmarPassword.ToolTip = "Repita a password exatamente igual";

            // Configurar anúncios para leitores de tela
            AutomationProperties.SetLiveSetting(txtStatus, AutomationLiveSetting.Polite);
        }

        #endregion

        #region Eventos da Interface

        /// <summary>
        /// Manipulador de teclas de atalho
        /// </summary>
        private void RegistoWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    BtnCancelar_Click(sender, e);
                    e.Handled = true;
                    break;

                case Key.F5:
                    BtnLimpar_Click(sender, e);
                    e.Handled = true;
                    break;

                case Key.Enter:
                    if (Keyboard.Modifiers == ModifierKeys.Control && btnRegistar.IsEnabled)
                    {
                        BtnRegistar_Click(sender, e);
                        e.Handled = true;
                    }
                    break;
            }
        }

        /// <summary>
        /// Evento de validação quando username perde o foco
        /// </summary>
        private async void TxtUsername_LostFocus(object sender, RoutedEventArgs e)
        {
            await ValidarUsernameUnico();
        }

        /// <summary>
        /// Evento de validação geral dos campos
        /// </summary>
        private void ValidarCampos(object sender, RoutedEventArgs e)
        {
            ValidarFormulario();
        }

        /// <summary>
        /// Evento do botão Registar
        /// </summary>
        private async void BtnRegistar_Click(object sender, RoutedEventArgs e)
        {
            if (_processandoRegisto) return;

            try
            {
                _processandoRegisto = true;
                this.Cursor = Cursors.Wait;
                btnRegistar.IsEnabled = false;

                AtualizarStatus("⏳ Criando utilizador...", false);

                if (!ValidarFormulario())
                {
                    AtualizarStatus("❌ Corrija os erros antes de continuar", true);
                    return;
                }

                // Verificar username único novamente
                if (!await ValidarUsernameUnico())
                {
                    AtualizarStatus("❌ Nome de utilizador já existe", true);
                    txtUsername.Focus();
                    return;
                }

                // Criar utilizador
                var novoUtilizador = CriarUtilizadorAPartirFormulario();
                var sucesso = await _userService.CreateUserAsync(novoUtilizador, txtPassword.Password);

                if (sucesso)
                {
                    AtualizarStatus("✅ Utilizador criado com sucesso!", false);


                    MostrarSucesso("Registo Concluído",
                        $"O utilizador '{novoUtilizador.Nome}' foi criado com sucesso.\n" +
                        $"Username: {novoUtilizador.Username}\n" +
                        $"Tipo: {novoUtilizador.GetDescricaoTipo()}");

                    //this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    AtualizarStatus("❌ Erro ao criar utilizador", true);
                    MostrarErro("Erro no Registo", "Não foi possível criar o utilizador. Tente novamente.");
                }
            }
            catch (InvalidOperationException ex)
            {
                AtualizarStatus($"❌ {ex.Message}", true);
                MostrarErro("Erro de Validação", ex.Message);
            }
            catch (Exception ex)
            {
                AtualizarStatus("❌ Erro inesperado", true);
                MostrarErro("Erro Inesperado", $"Ocorreu um erro: {ex.Message}");
            }
            finally
            {
                _processandoRegisto = false;
                this.Cursor = Cursors.Arrow;
                ValidarFormulario(); // Reativar botão se formulário válido
            }
        }

        /// <summary>
        /// Evento do botão Limpar
        /// </summary>
        private void BtnLimpar_Click(object sender, RoutedEventArgs e)
        {
            var resultado = MessageBox.Show(
                "Tem a certeza que deseja limpar todos os campos?",
                "Limpar Formulário",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultado == MessageBoxResult.Yes)
            {
                LimparFormulario();
                AtualizarStatus("Formulário limpo", false);
                txtNome.Focus();
            }
        }

        /// <summary>
        /// Evento do botão Cancelar
        /// </summary>
        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            if (FormularioTemDados())
            {
                var resultado = MessageBox.Show(
                    "Tem dados não guardados. Deseja realmente sair?",
                    "Confirmar Saída",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (resultado == MessageBoxResult.No)
                    return;
            }

            //this.DialogResult = false;
            this.Close();
        }

        #endregion

        #region Validação

        /// <summary>
        /// Valida todo o formulário
        /// </summary>
        private bool ValidarFormulario()
        {
            bool valido = true;

            // Validar Nome
            valido &= ValidarNome();

            // Validar Username
            valido &= ValidarUsername();

            // Validar Email
            valido &= ValidarEmail();

            // Validar Password
            valido &= ValidarPassword();

            // Validar Confirmação de Password
            valido &= ValidarConfirmacaoPassword();

            // Atualizar indicador de força da password
            AtualizarIndicadorForcaPassword();

            // Atualizar estado do botão e status
            _formularioValido = valido;
            btnRegistar.IsEnabled = valido && !_processandoRegisto;

            if (valido)
            {
                AtualizarStatus("✅ Formulário válido - Pronto para registar", false);
            }

            return valido;
        }

        /// <summary>
        /// Valida o campo nome
        /// </summary>
        private bool ValidarNome()
        {
            var nome = txtNome.Text?.Trim();

            if (string.IsNullOrWhiteSpace(nome))
            {
                MostrarErroValidacao(lblErroNome, "Nome é obrigatório");
                return false;
            }

            if (nome.Length < 2)
            {
                MostrarErroValidacao(lblErroNome, "Nome deve ter pelo menos 2 caracteres");
                return false;
            }

            if (nome.Length > 100)
            {
                MostrarErroValidacao(lblErroNome, "Nome não pode ter mais de 100 caracteres");
                return false;
            }

            EsconderErroValidacao(lblErroNome);
            return true;
        }

        /// <summary>
        /// Valida o campo username
        /// </summary>
        private bool ValidarUsername()
        {
            var username = txtUsername.Text?.Trim();

            if (string.IsNullOrWhiteSpace(username))
            {
                MostrarErroValidacao(lblErroUsername, "Nome de utilizador é obrigatório");
                return false;
            }

            if (username.Length < 3)
            {
                MostrarErroValidacao(lblErroUsername, "Nome de utilizador deve ter pelo menos 3 caracteres");
                return false;
            }

            if (username.Length > 20)
            {
                MostrarErroValidacao(lblErroUsername, "Nome de utilizador não pode ter mais de 20 caracteres");
                return false;
            }

            if (username.Contains(" "))
            {
                MostrarErroValidacao(lblErroUsername, "Nome de utilizador não pode conter espaços");
                return false;
            }

            if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_]+$"))
            {
                MostrarErroValidacao(lblErroUsername, "Nome de utilizador pode conter apenas letras, números e underscore");
                return false;
            }

            EsconderErroValidacao(lblErroUsername);
            return true;
        }

        /// <summary>
        /// Valida se o username é único (assíncrono)
        /// </summary>
        private async Task<bool> ValidarUsernameUnico()
        {
            var username = txtUsername.Text?.Trim();

            if (string.IsNullOrWhiteSpace(username) || !ValidarUsername())
                return false;

            try
            {
                var existe = await _userService.UsernameExistsAsync(username);

                if (existe)
                {
                    MostrarErroValidacao(lblErroUsername, "Este nome de utilizador já existe");
                    return false;
                }

                EsconderErroValidacao(lblErroUsername);
                return true;
            }
            catch (Exception ex)
            {
                MostrarErroValidacao(lblErroUsername, $"Erro ao verificar username: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Valida o campo email
        /// </summary>
        private bool ValidarEmail()
        {
            var email = txtEmail.Text?.Trim();

            if (string.IsNullOrWhiteSpace(email))
            {
                MostrarErroValidacao(lblErroEmail, "Email é obrigatório");
                return false;
            }

            var emailRegex = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            if (!Regex.IsMatch(email, emailRegex))
            {
                MostrarErroValidacao(lblErroEmail, "Formato de email inválido");
                return false;
            }

            if (email.Length > 255)
            {
                MostrarErroValidacao(lblErroEmail, "Email não pode ter mais de 255 caracteres");
                return false;
            }

            EsconderErroValidacao(lblErroEmail);
            return true;
        }

        /// <summary>
        /// Valida o campo password
        /// </summary>
        private bool ValidarPassword()
        {
            var password = txtPassword.Password;

            if (string.IsNullOrEmpty(password))
            {
                MostrarErroValidacao(lblErroPassword, "Password é obrigatória");
                return false;
            }

            if (password.Length < 6)
            {
                MostrarErroValidacao(lblErroPassword, "Password deve ter pelo menos 6 caracteres");
                return false;
            }

            if (password.Length > 50)
            {
                MostrarErroValidacao(lblErroPassword, "Password não pode ter mais de 50 caracteres");
                return false;
            }

            EsconderErroValidacao(lblErroPassword);
            return true;
        }

        /// <summary>
        /// Valida a confirmação da password
        /// </summary>
        private bool ValidarConfirmacaoPassword()
        {
            var password = txtPassword.Password;
            var confirmacao = txtConfirmarPassword.Password;

            if (string.IsNullOrEmpty(confirmacao))
            {
                MostrarErroValidacao(lblErroConfirmarPassword, "Confirmação de password é obrigatória");
                return false;
            }

            if (password != confirmacao)
            {
                MostrarErroValidacao(lblErroConfirmarPassword, "As passwords não coincidem");
                return false;
            }

            EsconderErroValidacao(lblErroConfirmarPassword);
            return true;
        }

        /// <summary>
        /// Atualiza o indicador visual da força da password
        /// </summary>
        private void AtualizarIndicadorForcaPassword()
        {
            var password = txtPassword.Password;
            var (forca, cor, texto, largura) = CalcularForcaPassword(password);

            // Animar a barra de força
            var storyboard = new Storyboard();

            var animacaoLargura = new DoubleAnimation
            {
                To = largura,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase()
            };

            Storyboard.SetTarget(animacaoLargura, barraForcaPassword);
            Storyboard.SetTargetProperty(animacaoLargura, new PropertyPath("Width"));
            storyboard.Children.Add(animacaoLargura);

            storyboard.Begin();

            // Atualizar cor e texto
            barraForcaPassword.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(cor));
            lblForcaPassword.Text = texto;
            lblForcaPassword.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(cor));
        }

        /// <summary>
        /// Calcula a força da password
        /// </summary>
        private (int forca, string cor, string texto, double largura) CalcularForcaPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return (0, "#E74C3C", "Vazia", 0);

            int pontuacao = 0;

            // Comprimento
            if (password.Length >= 6) pontuacao += 1;
            if (password.Length >= 8) pontuacao += 1;
            if (password.Length >= 12) pontuacao += 1;

            // Complexidade
            if (password.Any(char.IsLower)) pontuacao += 1;
            if (password.Any(char.IsUpper)) pontuacao += 1;
            if (password.Any(char.IsDigit)) pontuacao += 1;
            if (password.Any(c => "!@#$%^&*()_+-=[]{}|;:,.<>?".Contains(c))) pontuacao += 1;

            return pontuacao switch
            {
                <= 2 => (1, "#E74C3C", "Fraca", 25),
                <= 4 => (2, "#F39C12", "Razoável", 50),
                <= 6 => (3, "#F1C40F", "Boa", 75),
                _ => (4, "#27AE60", "Forte", 100)
            };
        }

        #endregion

        #region Métodos Auxiliares

        /// <summary>
        /// Mostra erro de validação num label
        /// </summary>
        private void MostrarErroValidacao(TextBlock label, string mensagem)
        {
            label.Text = mensagem;
            label.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Esconde erro de validação
        /// </summary>
        private void EsconderErroValidacao(TextBlock label)
        {
            label.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Atualiza a barra de status
        /// </summary>
        private void AtualizarStatus(string mensagem, bool isErro)
        {
            if (txtStatus != null)
            {
                txtStatus.Text = mensagem;
                txtStatus.Parent.GetType().GetProperty("Background")?.SetValue(txtStatus.Parent,
                    new SolidColorBrush((Color)ColorConverter.ConvertFromString(isErro ? "#E74C3C" : "#34495E")));
            }
        }

        /// <summary>
        /// Cria objeto User a partir do formulário
        /// </summary>
        private User CriarUtilizadorAPartirFormulario()
        {
            var tipoUtilizador = TipoUtilizador.Colaborador;

            return new User
            {
                UserId = Guid.NewGuid().ToString(),
                Nome = txtNome.Text.Trim(),
                Username = txtUsername.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                TipoUtilizador = tipoUtilizador,
                Ativo = true,
                DataCriacao = DateTime.Now
            };
        }

        /// <summary>
        /// Limpa todos os campos do formulário
        /// </summary>
        private void LimparFormulario()
        {
            txtNome.Text = "";
            txtUsername.Text = "";
            txtEmail.Text = "";
            txtPassword.Password = "";
            txtConfirmarPassword.Password = "";

            // Esconder todos os erros
            EsconderErroValidacao(lblErroNome);
            EsconderErroValidacao(lblErroUsername);
            EsconderErroValidacao(lblErroEmail);
            EsconderErroValidacao(lblErroPassword);
            EsconderErroValidacao(lblErroConfirmarPassword);

            // Reset indicador de força
            barraForcaPassword.Width = 0;
            lblForcaPassword.Text = "Vazia";
            lblForcaPassword.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E74C3C"));

            _formularioValido = false;
            btnRegistar.IsEnabled = false;
        }

        /// <summary>
        /// Verifica se o formulário tem dados preenchidos
        /// </summary>
        private bool FormularioTemDados()
        {
            return !string.IsNullOrWhiteSpace(txtNome.Text) ||
                   !string.IsNullOrWhiteSpace(txtUsername.Text) ||
                   !string.IsNullOrWhiteSpace(txtEmail.Text) ||
                   !string.IsNullOrEmpty(txtPassword.Password) ||
                   !string.IsNullOrEmpty(txtConfirmarPassword.Password);
        }

        /// <summary>
        /// Envia email de boas-vindas (implementação placeholder)
        /// </summary>
        private async Task EnviarEmailBoasVindas(User utilizador)
        {
            try
            {
                // Simulação de envio de email
                await Task.Delay(1000);

                // Aqui implementaria o envio real do email
                // Exemplo: await _emailService.SendWelcomeEmailAsync(utilizador);

                System.Diagnostics.Debug.WriteLine($"Email de boas-vindas enviado para {utilizador.Email}");
            }
            catch (Exception ex)
            {
                // Log do erro mas não falha o registo
                System.Diagnostics.Debug.WriteLine($"Erro ao enviar email: {ex.Message}");
                AtualizarStatus("⚠️ Utilizador criado, mas erro ao enviar email", false);
            }
        }

        /// <summary>
        /// Mostra mensagem de sucesso
        /// </summary>
        private void MostrarSucesso(string titulo, string mensagem)
        {
            MessageBox.Show(mensagem, titulo, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Mostra mensagem de erro
        /// </summary>
        private void MostrarErro(string titulo, string mensagem)
        {
            MessageBox.Show(mensagem, titulo, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Cleanup quando a janela é fechada
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            // Limpar recursos se necessário
            base.OnClosed(e);
        }

        #endregion
    }
}