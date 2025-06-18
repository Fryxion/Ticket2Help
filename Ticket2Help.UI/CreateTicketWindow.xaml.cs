using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ticket2Help.BLL.Controllers;
using Ticket2Help.BLL.Models;

namespace Ticket2Help.UI
{
    /// <summary>
    /// Janela simples para criar novos tickets
    /// </summary>
    public partial class CreateTicketWindow : Window
    {
        #region Campos Privados

        private readonly User _currentUser;
        private readonly TicketController _controller;
        private bool _isCreating = false;

        #endregion

        #region Propriedades Públicas

        /// <summary>
        /// Indica se o ticket foi criado com sucesso
        /// </summary>
        public bool TicketCreated { get; private set; } = false;

        /// <summary>
        /// ID do ticket criado (se sucesso)
        /// </summary>
        public int CreatedTicketId { get; private set; } = 0;

        #endregion

        #region Construtor

        /// <summary>
        /// Construtor da janela
        /// </summary>
        public CreateTicketWindow(User currentUser, TicketController controller)
        {
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));

            InitializeComponent();
            SetupWindow();
            SetupAccessibility();
            LoadUserInfo();
        }

        #endregion

        #region Configuração Inicial

        /// <summary>
        /// Configura a janela
        /// </summary>
        private void SetupWindow()
        {
            // Focar no primeiro campo quando a janela abre
            Loaded += (s, e) => TicketTypeComboBox.Focus();
        }

        /// <summary>
        /// Configura recursos de acessibilidade
        /// </summary>
        private void SetupAccessibility()
        {
            // Atalhos de teclado
            KeyDown += CreateTicketWindow_KeyDown;

            // Enter nos campos de texto cria o ticket
            EquipmentTextBox.KeyDown += TextBox_KeyDown;
            MalfunctionTextBox.KeyDown += TextBox_KeyDown;
            SoftwareTextBox.KeyDown += TextBox_KeyDown;
            NeedDescriptionTextBox.KeyDown += TextBox_KeyDown;
            AdditionalInfoTextBox.KeyDown += TextBox_KeyDown;
        }

        /// <summary>
        /// Carrega informações do utilizador
        /// </summary>
        private void LoadUserInfo()
        {
            UserNameText.Text = _currentUser.Nome;
            UserEmailText.Text = _currentUser.Email;
        }

        #endregion

        #region Event Handlers - Interface

        /// <summary>
        /// Manipula mudança do tipo de ticket
        /// </summary>
        private void TicketTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;

            ClearValidation();

            var selectedItem = TicketTypeComboBox.SelectedItem as ComboBoxItem;
            var ticketType = selectedItem?.Tag?.ToString();

            // Mostrar/esconder painéis apropriados
            if (ticketType == "Hardware")
            {
                HardwarePanel.Visibility = Visibility.Visible;
                SoftwarePanel.Visibility = Visibility.Collapsed;

                // Focar no primeiro campo do hardware
                Dispatcher.BeginInvoke(new Action(() => EquipmentTextBox.Focus()));
            }
            else if (ticketType == "Software")
            {
                HardwarePanel.Visibility = Visibility.Collapsed;
                SoftwarePanel.Visibility = Visibility.Visible;

                // Focar no primeiro campo do software
                Dispatcher.BeginInvoke(new Action(() => SoftwareTextBox.Focus()));
            }
            else
            {
                HardwarePanel.Visibility = Visibility.Collapsed;
                SoftwarePanel.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Criar ticket
        /// </summary>
        private async void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isCreating) return;

            await CreateTicketAsync();
        }

        /// <summary>
        /// Cancelar criação
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isCreating)
            {
                var result = MessageBox.Show(
                    "Um ticket está sendo criado. Tem certeza que deseja cancelar?",
                    "Cancelar Criação",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return;
            }

            DialogResult = false;
            Close();
        }

        #endregion

        #region Event Handlers - Teclado

        /// <summary>
        /// Manipula teclas de atalho da janela
        /// </summary>
        private void CreateTicketWindow_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    CancelButton_Click(sender, new RoutedEventArgs());
                    e.Handled = true;
                    break;

                case Key.F1:
                    ShowHelp();
                    e.Handled = true;
                    break;
            }
        }

        /// <summary>
        /// Manipula Enter em campos de texto
        /// </summary>
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Control)
            {
                CreateButton_Click(sender, new RoutedEventArgs());
                e.Handled = true;
            }
        }

        #endregion

        #region Criação de Ticket

        /// <summary>
        /// Cria o ticket de forma assíncrona
        /// </summary>
        private async Task CreateTicketAsync()
        {
            try
            {
                // Validar formulário
                if (!ValidateForm())
                    return;

                _isCreating = true;
                ShowLoading("Criando ticket...");

                var selectedItem = TicketTypeComboBox.SelectedItem as ComboBoxItem;
                var ticketType = selectedItem?.Tag?.ToString();

                // Criar ticket baseado no tipo
                if (ticketType == "Hardware")
                {
                    await CreateHardwareTicketAsync();
                }
                else if (ticketType == "Software")
                {
                    await CreateSoftwareTicketAsync();
                }

            }
            catch (Exception ex)
            {
                HideLoading();
                ShowError($"Erro inesperado ao criar ticket: {ex.Message}");
            }
            finally
            {
                _isCreating = false;
            }
        }

        /// <summary>
        /// Cria ticket de hardware
        /// </summary>
        private async Task CreateHardwareTicketAsync()
        {
            var equipment = EquipmentTextBox.Text.Trim();
            var malfunction = MalfunctionTextBox.Text.Trim();

            // Adicionar informações extras se fornecidas
            if (!string.IsNullOrWhiteSpace(AdditionalInfoTextBox.Text))
            {
                malfunction += $"\n\nInformações adicionais:\n{AdditionalInfoTextBox.Text.Trim()}";
            }

            var result = await _controller.CreateHardwareTicketAsync(_currentUser.UserId, equipment, malfunction);

            HideLoading();

            if (result.IsSuccess)
            {
                TicketCreated = true;
                CreatedTicketId = result.Data;

                MessageBox.Show(
                    $"Ticket de hardware #{result.Data} criado com sucesso!\n\n" +
                    $"Equipamento: {equipment}\n" +
                    $"Seu ticket foi adicionado à fila de atendimento.",
                    "Ticket Criado",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            else
            {
                ShowError($"Erro ao criar ticket de hardware:\n{result.ErrorMessage}");
            }
        }

        /// <summary>
        /// Cria ticket de software
        /// </summary>
        private async Task CreateSoftwareTicketAsync()
        {
            var software = SoftwareTextBox.Text.Trim();
            var needDescription = NeedDescriptionTextBox.Text.Trim();

            // Adicionar informações extras se fornecidas
            if (!string.IsNullOrWhiteSpace(AdditionalInfoTextBox.Text))
            {
                needDescription += $"\n\nInformações adicionais:\n{AdditionalInfoTextBox.Text.Trim()}";
            }

            var result = await _controller.CreateSoftwareTicketAsync(_currentUser.UserId, software, needDescription);

            HideLoading();

            if (result.IsSuccess)
            {
                TicketCreated = true;
                CreatedTicketId = result.Data;

                MessageBox.Show(
                    $"Ticket de software #{result.Data} criado com sucesso!\n\n" +
                    $"Software: {software}\n" +
                    $"Seu ticket foi adicionado à fila de atendimento.",
                    "Ticket Criado",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            else
            {
                ShowError($"Erro ao criar ticket de software:\n{result.ErrorMessage}");
            }
        }

        #endregion

        #region Validação

        /// <summary>
        /// Valida o formulário
        /// </summary>
        private bool ValidateForm()
        {
            ClearValidation();

            var selectedItem = TicketTypeComboBox.SelectedItem as ComboBoxItem;
            var ticketType = selectedItem?.Tag?.ToString();

            // Validar tipo selecionado
            if (string.IsNullOrEmpty(ticketType))
            {
                ShowValidationError("Por favor, selecione o tipo de ticket.");
                TicketTypeComboBox.Focus();
                return false;
            }

            // Validar campos específicos
            if (ticketType == "Hardware")
            {
                if (string.IsNullOrWhiteSpace(EquipmentTextBox.Text))
                {
                    ShowValidationError("Por favor, informe o equipamento.");
                    EquipmentTextBox.Focus();
                    return false;
                }

                if (string.IsNullOrWhiteSpace(MalfunctionTextBox.Text))
                {
                    ShowValidationError("Por favor, descreva a avaria encontrada.");
                    MalfunctionTextBox.Focus();
                    return false;
                }
            }
            else if (ticketType == "Software")
            {
                if (string.IsNullOrWhiteSpace(SoftwareTextBox.Text))
                {
                    ShowValidationError("Por favor, informe o software.");
                    SoftwareTextBox.Focus();
                    return false;
                }

                if (string.IsNullOrWhiteSpace(NeedDescriptionTextBox.Text))
                {
                    ShowValidationError("Por favor, descreva sua necessidade.");
                    NeedDescriptionTextBox.Focus();
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Mostra mensagem de validação
        /// </summary>
        private void ShowValidationError(string message)
        {
            ValidationMessageText.Text = $"⚠️ {message}";
            ValidationMessageText.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Limpa mensagens de validação
        /// </summary>
        private void ClearValidation()
        {
            ValidationMessageText.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region Métodos Auxiliares

        /// <summary>
        /// Mostra overlay de loading
        /// </summary>
        private void ShowLoading(string message = "Processando...")
        {
            LoadingText.Text = message;
            LoadingOverlay.Visibility = Visibility.Visible;
            CreateButton.IsEnabled = false;
        }

        /// <summary>
        /// Esconde overlay de loading
        /// </summary>
        private void HideLoading()
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;
            CreateButton.IsEnabled = true;
        }

        /// <summary>
        /// Mostra mensagem de erro
        /// </summary>
        private void ShowError(string message)
        {
            MessageBox.Show(message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// Mostra ajuda
        /// </summary>
        private void ShowHelp()
        {
            var helpText = "AJUDA - CRIAR TICKET\n\n" +
                          "Como criar um ticket:\n" +
                          "1. Selecione o tipo (Hardware ou Software)\n" +
                          "2. Preencha os campos obrigatórios (*)\n" +
                          "3. Adicione informações extras se necessário\n" +
                          "4. Clique em 'Criar Ticket'\n\n" +
                          "Atalhos:\n" +
                          "• Ctrl+Enter: Criar ticket\n" +
                          "• Esc: Cancelar\n" +
                          "• F1: Esta ajuda\n\n" +
                          "Dicas:\n" +
                          "• Seja específico na descrição\n" +
                          "• Inclua detalhes do problema\n" +
                          "• Mencione quando o problema começou";

            MessageBox.Show(helpText, "Ajuda", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion
    }
}