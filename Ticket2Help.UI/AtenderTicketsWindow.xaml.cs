using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ticket2Help.BLL.Models;
using Ticket2Help.BLL.Services;
using Ticket2Help.BLL.Controllers;

namespace Ticket2Help.UI.Windows
{
    /// <summary>
    /// Janela para atendimento de tickets por técnicos de helpdesk
    /// Implementa funcionalidades de acessibilidade e interface simples
    /// </summary>
    public partial class AtenderTicketsWindow : Window
    {
        #region Propriedades Privadas

        /// <summary>
        /// Controlador para operações de tickets
        /// </summary>
        private readonly TicketController _ticketController;

        /// <summary>
        /// Utilizador técnico atualmente logado
        /// </summary>
        private readonly User _tecnicoLogado;

        /// <summary>
        /// Lista de tickets disponíveis para atendimento
        /// </summary>
        private List<TicketViewModel> _ticketsPorAtender;

        /// <summary>
        /// Ticket atualmente em atendimento (mantém referência mesmo quando removido da lista)
        /// </summary>
        private TicketViewModel _ticketEmAtendimento;

        /// <summary>
        /// Ticket atualmente selecionado na lista
        /// </summary>
        private TicketViewModel _ticketSelecionado;

        /// <summary>
        /// Estado atual da interface (visualizando, atendendo)
        /// </summary>
        private EstadoInterface _estadoAtual;

        #endregion

        #region Enums

        /// <summary>
        /// Estados possíveis da interface de atendimento
        /// </summary>
        private enum EstadoInterface
        {
            Visualizando,
            Atendendo
        }

        #endregion

        #region Construtor

        /// <summary>
        /// Construtor da janela de atendimento de tickets
        /// </summary>
        /// <param name="tecnicoLogado">Utilizador técnico logado</param>
        /// <param name="ticketController">Controller de tickets (opcional - será criado se não fornecido)</param>
        public AtenderTicketsWindow(User tecnicoLogado, TicketController ticketController = null)
        {
            InitializeComponent();

            _tecnicoLogado = tecnicoLogado ?? throw new ArgumentNullException(nameof(tecnicoLogado));

            // Se foi fornecido um controller, usar esse, senão criar um novo com dependências padrão
            _ticketController = ticketController ?? CriarTicketControllerPadrao();

            _ticketsPorAtender = new List<TicketViewModel>();
            _estadoAtual = EstadoInterface.Visualizando;

            InicializarInterface();
            CarregarTicketsPorAtender();
        }

        /// <summary>
        /// Cria um TicketController com dependências padrão
        /// Este método pode ser modificado para usar um container de IoC se necessário
        /// </summary>
        private TicketController CriarTicketControllerPadrao()
        {
            try
            {
                // Tentar usar ServiceLocator se disponível
                var ticketService = ServiceLocator.GetService<ITicketService>();
                var userService = ServiceLocator.GetService<IUserService>();
                var statisticsService = ServiceLocator.GetService<IStatisticsService>();

                return new TicketController(ticketService, userService, statisticsService);
            }
            catch
            {
                // Fallback: criar serviços diretamente (implementar conforme necessário)
                // Este é um exemplo - as implementações reais devem vir das suas classes de serviço
                throw new InvalidOperationException("Não foi possível resolver as dependências do TicketController. " +
                    "Forneça um TicketController no construtor ou configure o ServiceLocator.");
            }
        }

        #endregion

        #region Métodos de Inicialização

        /// <summary>
        /// Inicializa os componentes da interface
        /// </summary>
        private void InicializarInterface()
        {
            // Configurar informações do técnico
            txtTecnicoLogado.Text = _tecnicoLogado.Nome;

            // Configurar acessibilidade
            ConfigurarAcessibilidade();

            // Configurar estado inicial
            AtualizarEstadoInterface();
        }

        /// <summary>
        /// Configura funcionalidades de acessibilidade
        /// </summary>
        private void ConfigurarAcessibilidade()
        {
            // Configurar navegação por teclado
            this.KeyDown += AtenderTicketsWindow_KeyDown;

            // Configurar focus inicial
            this.Loaded += (s, e) => btnAtualizarLista.Focus();
        }

        #endregion

        #region Métodos de Carregamento de Dados

        /// <summary>
        /// Carrega a lista de tickets por atender
        /// </summary>
        private async void CarregarTicketsPorAtender()
        {
            try
            {
                // Mostrar cursor de espera
                this.Cursor = Cursors.Wait;

                // Obter tickets pendentes usando o método correto do controller
                var result = await _ticketController.GetPendingTicketsAsync();

                if (result.IsSuccess)
                {
                    // Converter para ViewModel
                    _ticketsPorAtender = result.Data.Select(t => new TicketViewModel(t)).ToList();

                    // Aplicar filtro se necessário
                    AplicarFiltro();

                    // Atualizar contadores
                    AtualizarInformacoes();
                }
                else
                {
                    MostrarErro("Erro ao carregar tickets", result.ErrorMessage);
                    _ticketsPorAtender = new List<TicketViewModel>();
                }
            }
            catch (Exception ex)
            {
                MostrarErro("Erro ao carregar tickets", ex.Message);
                _ticketsPorAtender = new List<TicketViewModel>();
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        /// <summary>
        /// Aplica filtro baseado na seleção do ComboBox
        /// </summary>
        private void AplicarFiltro()
        {
            var filtroSelecionado = ((ComboBoxItem)cmbFiltroTipo.SelectedItem)?.Content?.ToString();

            IEnumerable<TicketViewModel> ticketsFiltrados = _ticketsPorAtender;

            switch (filtroSelecionado)
            {
                case "Hardware":
                    ticketsFiltrados = _ticketsPorAtender.Where(t => t.TipoTicket == TipoTicket.Hardware);
                    break;
                case "Software":
                    ticketsFiltrados = _ticketsPorAtender.Where(t => t.TipoTicket == TipoTicket.Software);
                    break;
                default: // "Todos"
                    ticketsFiltrados = _ticketsPorAtender;
                    break;
            }

            lvTickets.ItemsSource = ticketsFiltrados.OrderBy(t => t.DataCriacao).ToList();
        }

        /// <summary>
        /// Atualiza informações e contadores na interface
        /// </summary>
        private void AtualizarInformacoes()
        {
            var totalTickets = _ticketsPorAtender.Count;
            var ticketsHardware = _ticketsPorAtender.Count(t => t.TipoTicket == TipoTicket.Hardware);
            var ticketsSoftware = _ticketsPorAtender.Count(t => t.TipoTicket == TipoTicket.Software);

            // Atualizar título da janela com contador
            this.Title = $"Atender Tickets - {totalTickets} tickets por atender - Ticket2Help";
        }

        #endregion

        #region Métodos de Interface

        /// <summary>
        /// Atualiza o estado da interface baseado no estado atual
        /// </summary>
        private void AtualizarEstadoInterface()
        {
            switch (_estadoAtual)
            {
                case EstadoInterface.Visualizando:
                    btnIniciarAtendimento.IsEnabled = _ticketSelecionado != null;
                    btnFinalizarAtendimento.IsEnabled = false;
                    btnCancelar.IsEnabled = false;

                    // Desabilitar campos de edição
                    DesabilitarCamposEdicao();
                    break;

                case EstadoInterface.Atendendo:
                    btnIniciarAtendimento.IsEnabled = false;
                    btnFinalizarAtendimento.IsEnabled = true;
                    btnCancelar.IsEnabled = true;

                    // Habilitar campos de edição
                    HabilitarCamposEdicao();
                    break;
            }
        }

        /// <summary>
        /// Desabilita campos de edição
        /// </summary>
        private void DesabilitarCamposEdicao()
        {
            txtDescricaoReparacao.IsEnabled = false;
            txtPecas.IsEnabled = false;
            txtDescricaoIntervencao.IsEnabled = false;
            cmbEstadoAtendimento.IsEnabled = false;
        }

        /// <summary>
        /// Habilita campos de edição
        /// </summary>
        private void HabilitarCamposEdicao()
        {
            // Usar ticket em atendimento em vez do selecionado
            var ticketAtual = _ticketEmAtendimento ?? _ticketSelecionado;

            if (ticketAtual?.TipoTicket == TipoTicket.Hardware)
            {
                txtDescricaoReparacao.IsEnabled = true;
                txtPecas.IsEnabled = true;

                // Focar no primeiro campo apenas se estiver visível e vazio
                if (string.IsNullOrEmpty(txtDescricaoReparacao.Text))
                {
                    txtDescricaoReparacao.Focus();
                }
            }
            else if (ticketAtual?.TipoTicket == TipoTicket.Software)
            {
                txtDescricaoIntervencao.IsEnabled = true;

                // Focar no campo apenas se estiver visível e vazio
                if (string.IsNullOrEmpty(txtDescricaoIntervencao.Text))
                {
                    txtDescricaoIntervencao.Focus();
                }
            }

            cmbEstadoAtendimento.IsEnabled = true;
        }

        /// <summary>
        /// Mostra detalhes do ticket selecionado
        /// </summary>
        /// <param name="ticket">Ticket a mostrar</param>
        private void MostrarDetalhesTicket(TicketViewModel ticket)
        {
            if (ticket == null)
            {
                LimparDetalhes();
                return;
            }

            // Informações básicas
            txtTicketId.Text = ticket.TicketId.ToString();
            txtTipo.Text = ticket.TipoTicketText;
            txtColaborador.Text = ticket.ColaboradorNome ?? "Nome não disponível";
            txtDataCriacao.Text = ticket.DataCriacaoFormatted;
            txtTempoEspera.Text = $"{ticket.HorasEspera} horas";

            // Mostrar painel específico baseado no tipo
            if (ticket.TipoTicket == TipoTicket.Hardware)
            {
                MostrarDetalhesHardware(ticket);
            }
            else
            {
                MostrarDetalhesSoftware(ticket);
            }

            // Se estivermos em modo de atendimento, garantir que os campos de edição ficam visíveis
            if (_estadoAtual == EstadoInterface.Atendendo)
            {
                if (ticket.TipoTicket == TipoTicket.Hardware)
                {
                    pnlAtendimentoHardware.Visibility = Visibility.Visible;
                    pnlAtendimentoSoftware.Visibility = Visibility.Collapsed;
                }
                else
                {
                    pnlAtendimentoHardware.Visibility = Visibility.Collapsed;
                    pnlAtendimentoSoftware.Visibility = Visibility.Visible;
                }
            }
        }

        /// <summary>
        /// Mostra detalhes específicos de ticket de hardware
        /// </summary>
        private void MostrarDetalhesHardware(TicketViewModel ticket)
        {
            pnlHardware.Visibility = Visibility.Visible;
            pnlSoftware.Visibility = Visibility.Collapsed;
            pnlAtendimentoHardware.Visibility = Visibility.Visible;
            pnlAtendimentoSoftware.Visibility = Visibility.Collapsed;

            txtEquipamento.Text = ticket.Equipamento ?? "Não especificado";
            txtAvaria.Text = ticket.Avaria ?? "Não especificado";
        }

        /// <summary>
        /// Mostra detalhes específicos de ticket de software
        /// </summary>
        private void MostrarDetalhesSoftware(TicketViewModel ticket)
        {
            pnlHardware.Visibility = Visibility.Collapsed;
            pnlSoftware.Visibility = Visibility.Visible;
            pnlAtendimentoHardware.Visibility = Visibility.Collapsed;
            pnlAtendimentoSoftware.Visibility = Visibility.Visible;

            txtSoftware.Text = ticket.Software ?? "Não especificado";
            txtDescricaoNecessidade.Text = ticket.DescricaoNecessidade ?? "Não especificado";
        }

        /// <summary>
        /// Limpa os detalhes do ticket
        /// </summary>
        private void LimparDetalhes()
        {
            txtTicketId.Text = "";
            txtTipo.Text = "";
            txtColaborador.Text = "";
            txtDataCriacao.Text = "";
            txtTempoEspera.Text = "";

            // Ocultar todos os painéis
            pnlHardware.Visibility = Visibility.Collapsed;
            pnlSoftware.Visibility = Visibility.Collapsed;
            pnlAtendimentoHardware.Visibility = Visibility.Collapsed;
            pnlAtendimentoSoftware.Visibility = Visibility.Collapsed;

            // Limpar campos de edição
            txtDescricaoReparacao.Text = "";
            txtPecas.Text = "";
            txtDescricaoIntervencao.Text = "";
            cmbEstadoAtendimento.SelectedIndex = 1; // Resolvido por padrão
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Evento de seleção de ticket na lista
        /// </summary>
        private void LvTickets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _ticketSelecionado = lvTickets.SelectedItem as TicketViewModel;
            MostrarDetalhesTicket(_ticketSelecionado);
            AtualizarEstadoInterface();
        }

        /// <summary>
        /// Evento de tecla pressionada na lista de tickets
        /// </summary>
        private void LvTickets_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && _ticketSelecionado != null)
            {
                if (_estadoAtual == EstadoInterface.Visualizando)
                {
                    BtnIniciarAtendimento_Click(null, null);
                }
            }
        }

        /// <summary>
        /// Evento de mudança de filtro
        /// </summary>
        private void CmbFiltroTipo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_ticketsPorAtender != null)
            {
                AplicarFiltro();
                AtualizarInformacoes();
            }
        }

        /// <summary>
        /// Evento de teclas globais da janela
        /// </summary>
        private void AtenderTicketsWindow_KeyDown(object sender, KeyEventArgs e)
        {
            // Atalhos de teclado para acessibilidade
            if (e.Key == Key.F5)
            {
                BtnAtualizarLista_Click(null, null);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                if (_estadoAtual == EstadoInterface.Atendendo)
                {
                    BtnCancelar_Click(null, null);
                }
                else
                {
                    BtnFechar_Click(null, null);
                }
                e.Handled = true;
            }
            else if (e.Key == Key.Enter && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                if (_estadoAtual == EstadoInterface.Atendendo && btnFinalizarAtendimento.IsEnabled)
                {
                    BtnFinalizarAtendimento_Click(null, null);
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Evento do botão Atualizar Lista
        /// </summary>
        private void BtnAtualizarLista_Click(object sender, RoutedEventArgs e)
        {
            CarregarTicketsPorAtender();

            // Anunciar atualização para leitores de tela
            var totalTickets = _ticketsPorAtender.Count;
            AnunciarParaLeitorTela($"Lista atualizada. {totalTickets} tickets disponíveis para atendimento.");
        }

        /// <summary>
        /// Evento do botão Iniciar Atendimento
        /// </summary>
        private async void BtnIniciarAtendimento_Click(object sender, RoutedEventArgs e)
        {
            if (_ticketSelecionado == null)
            {
                MostrarAviso("Seleção necessária", "Por favor, selecione um ticket para iniciar o atendimento.");
                return;
            }

            try
            {
                this.Cursor = Cursors.Wait;

                // Iniciar atendimento usando o método correto do controller
                var result = await _ticketController.AttendTicketAsync(_ticketSelecionado.TicketId, _tecnicoLogado.UserId);

                if (result.IsSuccess && result.Data)
                {
                    // IMPORTANTE: Manter referência do ticket em atendimento
                    _ticketEmAtendimento = _ticketSelecionado;

                    // Mudar estado para atendimento
                    _estadoAtual = EstadoInterface.Atendendo;

                    // Manter os detalhes do ticket visíveis
                    MostrarDetalhesTicket(_ticketEmAtendimento);

                    // Atualizar interface para modo de atendimento
                    AtualizarEstadoInterface();


                    AplicarFiltro();
                    AtualizarInformacoes();

                    var ticketId = _ticketEmAtendimento.TicketId;
                    AnunciarParaLeitorTela($"Atendimento iniciado para o ticket {ticketId}. Preencha os campos de atendimento.");

                    MostrarSucesso("Atendimento Iniciado", $"O atendimento do ticket {ticketId} foi iniciado com sucesso.");
                }
                else
                {
                    MostrarErro("Erro", result.ErrorMessage ?? "Não foi possível iniciar o atendimento.");
                }
            }
            catch (Exception ex)
            {
                MostrarErro("Erro ao iniciar atendimento", ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        /// <summary>
        /// Evento do botão Finalizar Atendimento
        /// </summary>
        private async void BtnFinalizarAtendimento_Click(object sender, RoutedEventArgs e)
        {
            // Usar ticket em atendimento em vez do selecionado
            if (_ticketEmAtendimento == null)
            {
                MostrarAviso("Ticket não encontrado", "Não há ticket em atendimento para finalizar.");
                return;
            }

            if (!ValidarDadosAtendimento())
            {
                return;
            }

            try
            {
                this.Cursor = Cursors.Wait;

                // Preparar dados do atendimento
                var dadosAtendimento = PrepararDadosAtendimento();

                OperationResult<bool> result;
                var ticketId = _ticketEmAtendimento.TicketId;

                // Finalizar atendimento baseado no tipo de ticket usando métodos corretos do controller
                if (_ticketEmAtendimento.TipoTicket == TipoTicket.Hardware)
                {
                    result = await _ticketController.CompleteHardwareTicketAsync(
                        ticketId,
                        dadosAtendimento.EstadoAtendimento,
                        dadosAtendimento.DescricaoReparacao,
                        dadosAtendimento.Pecas
                    );
                }
                else
                {
                    result = await _ticketController.CompleteSoftwareTicketAsync(
                        ticketId,
                        dadosAtendimento.EstadoAtendimento,
                        dadosAtendimento.DescricaoIntervencao
                    );
                }

                if (result.IsSuccess && result.Data)
                {
                    var estadoTexto = ((ComboBoxItem)cmbEstadoAtendimento.SelectedItem)?.Content?.ToString() ?? "Desconhecido";

                    MostrarSucesso("Atendimento Finalizado",
                        $"O atendimento do ticket {ticketId} foi finalizado com estado: {estadoTexto}.");

                    // Agora sim, remover da lista de pendentes
                    var ticketParaRemover = _ticketsPorAtender.FirstOrDefault(t => t.TicketId == _ticketSelecionado.TicketId);
                    if (ticketParaRemover != null)
                    {
                        _ticketsPorAtender.Remove(ticketParaRemover);
                    }

                    lvTickets.SelectedItem = null;

                    // Limpar ticket em atendimento
                    _ticketEmAtendimento = null;

                    // Voltar ao estado de visualização
                    _estadoAtual = EstadoInterface.Visualizando;
                    _ticketSelecionado = null;
                    LimparDetalhes();
                    AtualizarEstadoInterface();

                    // Recarregar lista
                    CarregarTicketsPorAtender();

                    AnunciarParaLeitorTela($"Atendimento finalizado com sucesso. Estado: {estadoTexto}.");
                }
                else
                {
                    MostrarErro("Erro", result.ErrorMessage ?? "Não foi possível finalizar o atendimento.");
                }
            }
            catch (Exception ex)
            {
                MostrarErro("Erro ao finalizar atendimento", ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        /// <summary>
        /// Evento do botão Cancelar
        /// </summary>
        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            if (_estadoAtual != EstadoInterface.Atendendo)
            {
                // Se não estamos em atendimento, apenas voltar ao estado normal
                _estadoAtual = EstadoInterface.Visualizando;
                AtualizarEstadoInterface();
                return;
            }

            var resultado = MessageBox.Show(
                "Deseja realmente cancelar o atendimento atual? As informações preenchidas serão perdidas.",
                "Cancelar Atendimento",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultado == MessageBoxResult.Yes)
            {
                try
                {
                    // Limpar ticket em atendimento
                    _ticketEmAtendimento = null;

                    // Voltar ao estado de visualização
                    _estadoAtual = EstadoInterface.Visualizando;

                    // Agora sim, remover da lista de pendentes
                    var ticketParaRemover = _ticketsPorAtender.FirstOrDefault(t => t.TicketId == _ticketSelecionado.TicketId);
                    if (ticketParaRemover != null)
                    {
                        _ticketsPorAtender.Remove(ticketParaRemover);
                    }

                    lvTickets.SelectedItem = null;

                    // Limpar campos e detalhes
                    LimparDetalhes();

                    // Atualizar estado da interface
                    AtualizarEstadoInterface();

                    // Recarregar lista (o ticket voltará a aparecer como pendente)
                    CarregarTicketsPorAtender();

                    AnunciarParaLeitorTela("Atendimento cancelado.");
                }
                catch (Exception ex)
                {
                    MostrarErro("Erro ao cancelar atendimento", ex.Message);
                }
            }
        }

        /// <summary>
        /// Limpa apenas os campos de edição, mantendo as informações do ticket
        /// </summary>
        private void LimparCamposEdicao()
        {
            txtDescricaoReparacao.Text = "";
            txtPecas.Text = "";
            txtDescricaoIntervencao.Text = "";
            cmbEstadoAtendimento.SelectedIndex = 1; // Resolvido por padrão
        }

        /// <summary>
        /// Evento do botão Fechar
        /// </summary>
        private void BtnFechar_Click(object sender, RoutedEventArgs e)
        {
            if (_estadoAtual == EstadoInterface.Atendendo)
            {
                var resultado = MessageBox.Show(
                    "Existe um atendimento em andamento. Deseja realmente fechar a janela?",
                    "Atendimento em Andamento",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (resultado == MessageBoxResult.No)
                {
                    return;
                }

                // Cancelar atendimento automaticamente
                BtnCancelar_Click(null, null);
            }

            this.Close();
        }

        #endregion

        #region Métodos de Validação e Preparação de Dados

        /// <summary>
        /// Valida os dados preenchidos para o atendimento
        /// </summary>
        /// <returns>True se os dados são válidos</returns>
        private bool ValidarDadosAtendimento()
        {
            // Usar ticket em atendimento em vez do selecionado
            var ticketAtual = _ticketEmAtendimento ?? _ticketSelecionado;

            if (ticketAtual == null)
            {
                MostrarErro("Erro", "Nenhum ticket em atendimento encontrado.");
                return false;
            }

            // Validações específicas por tipo
            if (ticketAtual.TipoTicket == TipoTicket.Hardware)
            {
                if (string.IsNullOrWhiteSpace(txtDescricaoReparacao?.Text))
                {
                    MostrarAviso("Campo obrigatório", "A descrição da reparação é obrigatória.");
                    txtDescricaoReparacao?.Focus();
                    return false;
                }
            }
            else if (ticketAtual.TipoTicket == TipoTicket.Software)
            {
                if (string.IsNullOrWhiteSpace(txtDescricaoIntervencao?.Text))
                {
                    MostrarAviso("Campo obrigatório", "A descrição da intervenção é obrigatória.");
                    txtDescricaoIntervencao?.Focus();
                    return false;
                }
            }

            // Validar estado do atendimento
            if (cmbEstadoAtendimento?.SelectedItem == null)
            {
                MostrarAviso("Campo obrigatório", "Selecione o estado do atendimento.");
                cmbEstadoAtendimento?.Focus();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Prepara os dados do atendimento para envio
        /// </summary>
        /// <returns>Objeto com dados do atendimento</returns>
        private AtendimentoData PrepararDadosAtendimento()
        {
            var estadoSelecionado = (ComboBoxItem)cmbEstadoAtendimento.SelectedItem;
            var estadoAtendimento = (EstadoAtendimento)Convert.ToInt32(estadoSelecionado.Tag);

            var dados = new AtendimentoData
            {
                TecnicoId = _tecnicoLogado.UserId,
                EstadoAtendimento = estadoAtendimento,
                DataAtendimento = DateTime.Now
            };

            if (_ticketSelecionado.TipoTicket == TipoTicket.Hardware)
            {
                dados.DescricaoReparacao = txtDescricaoReparacao.Text.Trim();
                dados.Pecas = txtPecas.Text.Trim();
            }
            else
            {
                dados.DescricaoIntervencao = txtDescricaoIntervencao.Text.Trim();
            }

            return dados;
        }

        #endregion

        #region Métodos de Utilidade

        /// <summary>
        /// Mostra mensagem de erro
        /// </summary>
        private void MostrarErro(string titulo, string mensagem)
        {
            MessageBox.Show(mensagem, titulo, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// Mostra mensagem de aviso
        /// </summary>
        private void MostrarAviso(string titulo, string mensagem)
        {
            MessageBox.Show(mensagem, titulo, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        /// <summary>
        /// Mostra mensagem de sucesso
        /// </summary>
        private void MostrarSucesso(string titulo, string mensagem)
        {
            MessageBox.Show(mensagem, titulo, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Anuncia mensagem para leitores de tela
        /// </summary>
        private void AnunciarParaLeitorTela(string mensagem)
        {
            // Implementação simplificada - apenas mostrar na barra de status ou título
            // Para uma implementação mais robusta, considere usar bibliotecas específicas de acessibilidade
            this.Title = $"{this.Title.Split('-')[0].Trim()} - {mensagem}";

            // Restaurar título original após 3 segundos
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(3);
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                AtualizarInformacoes(); // Restaura o título com contador
            };
            timer.Start();
        }

        #endregion
    }

    #region Classes Auxiliares

    /// <summary>
    /// ViewModel para exibição de tickets na interface
    /// </summary>
    public class TicketViewModel
    {
        public int TicketId { get; set; }
        public DateTime DataCriacao { get; set; }
        public string ColaboradorId { get; set; }
        public string ColaboradorNome { get; set; }
        public TipoTicket TipoTicket { get; set; }
        public EstadoTicket EstadoTicket { get; set; }

        // Campos específicos de Hardware
        public string Equipamento { get; set; }
        public string Avaria { get; set; }

        // Campos específicos de Software
        public string Software { get; set; }
        public string DescricaoNecessidade { get; set; }

        // Propriedades calculadas para exibição
        public string TipoTicketText => TipoTicket == TipoTicket.Hardware ? "Hardware" : "Software";
        public string DataCriacaoFormatted => DataCriacao.ToString("dd/MM/yyyy HH:mm");
        public int HorasEspera => (int)(DateTime.Now - DataCriacao).TotalHours;
        public string Prioridade
        {
            get
            {
                if (HorasEspera > 72) return "Alta";
                if (HorasEspera > 24) return "Média";
                return "Normal";
            }
        }

        public TicketViewModel(Ticket ticket)
        {
            TicketId = ticket.TicketId;
            DataCriacao = ticket.DataCriacao;
            ColaboradorId = ticket.ColaboradorId;
            TipoTicket = ticket.TipoTicket;
            EstadoTicket = ticket.EstadoTicket;

            // Tentar obter o nome do colaborador (pode precisar de ser carregado separadamente)
            ColaboradorNome = ObterNomeColaborador(ticket.ColaboradorId);

            if (ticket is HardwareTicket ht)
            {
                Equipamento = ht.Equipamento;
                Avaria = ht.Avaria;
            }
            else if (ticket is SoftwareTicket st)
            {
                Software = st.Software;
                DescricaoNecessidade = st.DescricaoNecessidade;
            }
        }

        /// <summary>
        /// Obtém o nome do colaborador pelo ID
        /// Este método pode ser melhorado para usar cache ou um serviço específico
        /// </summary>
        private string ObterNomeColaborador(string colaboradorId)
        {
            try
            {
                // Se o ticket já tem o nome do colaborador carregado (implementação específica do seu sistema)
                // senão, retornar um placeholder ou tentar carregar
                return colaboradorId ?? "Colaborador não identificado";
            }
            catch
            {
                return "Colaborador não identificado";
            }
        }
    }

    /// <summary>
    /// Classe para dados de atendimento
    /// </summary>
    public class AtendimentoData
    {
        public string TecnicoId { get; set; }
        public DateTime DataAtendimento { get; set; }
        public EstadoAtendimento EstadoAtendimento { get; set; }
        public string DescricaoReparacao { get; set; }
        public string Pecas { get; set; }
        public string DescricaoIntervencao { get; set; }
    }

    #endregion

    #region Service Locator Simples

    /// <summary>
    /// Service Locator simples para resolver dependências
    /// Em projetos maiores, considere usar um container de IoC como Unity, Autofac, etc.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        /// <summary>
        /// Regista um serviço
        /// </summary>
        public static void RegisterService<T>(T service)
        {
            _services[typeof(T)] = service;
        }

        /// <summary>
        /// Obtém um serviço registado
        /// </summary>
        public static T GetService<T>()
        {
            if (_services.TryGetValue(typeof(T), out var service))
            {
                return (T)service;
            }

            throw new InvalidOperationException($"Serviço do tipo {typeof(T).Name} não foi registado.");
        }

        /// <summary>
        /// Verifica se um serviço está registado
        /// </summary>
        public static bool IsServiceRegistered<T>()
        {
            return _services.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Remove um serviço registado
        /// </summary>
        public static void UnregisterService<T>()
        {
            _services.Remove(typeof(T));
        }

        /// <summary>
        /// Limpa todos os serviços registados
        /// </summary>
        public static void Clear()
        {
            _services.Clear();
        }
    }

    #endregion
}