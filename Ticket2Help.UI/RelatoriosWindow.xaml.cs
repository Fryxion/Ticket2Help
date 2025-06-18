using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;
using Ticket2Help.BLL.Models;
using Ticket2Help.BLL.Services;
using Ticket2Help.BLL.Controllers;

namespace Ticket2Help.UI.Windows
{
    /// <summary>
    /// Janela para geração e visualização de relatórios do sistema
    /// Implementa funcionalidades de acessibilidade e interface intuitiva
    /// </summary>
    public partial class RelatoriosWindow : Window
    {
        #region Propriedades Privadas

        /// <summary>
        /// Controller para operações de tickets e estatísticas
        /// </summary>
        private readonly TicketController _ticketController;

        /// <summary>
        /// Utilizador atualmente logado
        /// </summary>
        private readonly User _usuarioLogado;

        /// <summary>
        /// Dados do último relatório gerado
        /// </summary>
        private DashboardStatistics _ultimoRelatorio;

        /// <summary>
        /// Timer para atualizar data/hora na barra de status
        /// </summary>
        private readonly DispatcherTimer _timerStatus;

        /// <summary>
        /// Indica se há um relatório carregado
        /// </summary>
        private bool _relatorioCarregado = false;

        #endregion

        #region Construtor

        /// <summary>
        /// Construtor da janela de relatórios
        /// </summary>
        /// <param name="usuarioLogado">Utilizador logado</param>
        /// <param name="ticketController">Controller de tickets (opcional)</param>
        public RelatoriosWindow(User usuarioLogado, TicketController ticketController)
        {
            InitializeComponent();

            _usuarioLogado = usuarioLogado;
            _ticketController = ticketController ?? CriarTicketControllerPadrao();

            InicializarInterface();
            ConfigurarAcessibilidade();

        }

        /// <summary>
        /// Cria um TicketController com dependências padrão
        /// </summary>
        private TicketController CriarTicketControllerPadrao()
        {
            try
            {
                var ticketService = ServiceLocator.GetService<ITicketService>();
                var userService = ServiceLocator.GetService<IUserService>();
                var statisticsService = ServiceLocator.GetService<IStatisticsService>();

                return new TicketController(ticketService, userService, statisticsService);
            }
            catch
            {
                throw new InvalidOperationException("Não foi possível resolver as dependências do TicketController. " +
                    "Forneça um TicketController no construtor ou configure o ServiceLocator.");
            }
        }

        #endregion

        #region Inicialização

        /// <summary>
        /// Inicializa a interface do utilizador
        /// </summary>
        private void InicializarInterface()
        {
            txtUsuarioLogado.Text = $"Utilizador: {_usuarioLogado.Nome ?? _usuarioLogado.Username}";

            // Configurar datas padrão (último mês)
            dpDataFim.SelectedDate = DateTime.Today;
            dpDataInicio.SelectedDate = DateTime.Today.AddDays(-30);

            // Configurar status inicial
            txtStatus.Text = "Interface carregada - Selecione as opções de relatório";
            txtTituloRelatorio.Text = "Nenhum relatório gerado";
        }

        /// <summary>
        /// Configura funcionalidades de acessibilidade
        /// </summary>
        private void ConfigurarAcessibilidade()
        {
            // Configurar navegação por teclado
            this.KeyDown += RelatoriosWindow_KeyDown;

            // Configurar tooltips informativos
            btnGerar.ToolTip = "Tecla de atalho: Ctrl+G";

            // Configurar focus inicial
            this.Loaded += (s, e) => cmbTipoRelatorio.Focus();
        }

        #endregion

        #region Eventos da Interface

        /// <summary>
        /// Evento de mudança do tipo de relatório
        /// </summary>
        private void CmbTipoRelatorio_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbTipoRelatorio.SelectedItem is ComboBoxItem item)
            {
                var tipoSelecionado = item.Content.ToString();

                if (txtTituloRelatorio != null)
                {
                    txtTituloRelatorio.Text = "Configurado: " + tipoSelecionado;
                }


                AnunciarParaLeitorTela($"Tipo de relatório alterado para {tipoSelecionado}");
            }
        }

        /// <summary>
        /// Eventos dos botões de período rápido
        /// </summary>
        private void BtnPeriodoRapido_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string periodo)
            {
                var hoje = DateTime.Today;

                switch (periodo)
                {
                    case "hoje":
                        dpDataInicio.SelectedDate = hoje;
                        dpDataFim.SelectedDate = hoje;
                        break;

                    case "ontem":
                        var ontem = hoje.AddDays(-1);
                        dpDataInicio.SelectedDate = ontem;
                        dpDataFim.SelectedDate = ontem;
                        break;

                    case "7dias":
                        dpDataInicio.SelectedDate = hoje.AddDays(-7);
                        dpDataFim.SelectedDate = hoje;
                        break;

                    case "30dias":
                        dpDataInicio.SelectedDate = hoje.AddDays(-30);
                        dpDataFim.SelectedDate = hoje;
                        break;

                    case "mes":
                        dpDataInicio.SelectedDate = new DateTime(hoje.Year, hoje.Month, 1);
                        dpDataFim.SelectedDate = hoje;
                        break;
                }

                AnunciarParaLeitorTela($"Período alterado para {btn.Content}");
            }
        }

        /// <summary>
        /// Evento do botão Gerar Relatório
        /// </summary>
        private async void BtnGerar_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarParametros())
                return;

            try
            {
                this.Cursor = Cursors.Wait;
                btnGerar.IsEnabled = false;
                txtStatus.Text = "Gerando relatório...";
                txtStatusGeracao.Text = "⏳ Processando dados...";

                var parametros = ObterParametrosRelatorio();
                var resultado = await GerarRelatorio(parametros);

                if (resultado.IsSuccess)
                {
                    _ultimoRelatorio = resultado.Data;
                    _relatorioCarregado = true;

                    // Exibir relatório
                    ExibirRelatorio(parametros.TipoRelatorio, _ultimoRelatorio);

                    txtStatus.Text = "Relatório gerado com sucesso";
                    txtStatusGeracao.Text = "✅ Concluído";

                    AnunciarParaLeitorTela("Relatório gerado com sucesso. Use Tab para navegar pelos resultados.");
                }
                else
                {
                    MostrarErro("Erro ao gerar relatório", resultado.ErrorMessage);
                    txtStatusGeracao.Text = "❌ Erro na geração";
                }
            }
            catch (Exception ex)
            {
                MostrarErro("Erro inesperado", ex.Message);
                txtStatusGeracao.Text = "❌ Erro inesperado";
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
                btnGerar.IsEnabled = true;
            }
        }

        /// <summary>
        /// Evento do botão Exportar
        /// </summary>
        private void BtnExportar_Click(object sender, RoutedEventArgs e)
        {
            if (!_relatorioCarregado)
            {
                MostrarAviso("Sem dados", "Gere um relatório antes de exportar.");
                return;
            }

            var saveDialog = new SaveFileDialog
            {
                Title = "Exportar Relatório",
                Filter = "Arquivos Excel (*.xlsx)|*.xlsx|Arquivos CSV (*.csv)|*.csv|Arquivos PDF (*.pdf)|*.pdf|Todos os arquivos (*.*)|*.*",
                DefaultExt = "xlsx",
                FileName = $"Relatorio_{DateTime.Now:yyyyMMdd_HHmmss}"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    ExportarRelatorio(saveDialog.FileName);
                    MostrarSucesso("Exportação", $"Relatório exportado para:\n{saveDialog.FileName}");
                    AnunciarParaLeitorTela("Relatório exportado com sucesso");
                }
                catch (Exception ex)
                {
                    MostrarErro("Erro na exportação", ex.Message);
                }
            }
        }

        /// <summary>
        /// Evento do botão Imprimir
        /// </summary>
        private void BtnImprimir_Click(object sender, RoutedEventArgs e)
        {
            if (!_relatorioCarregado)
            {
                MostrarAviso("Sem dados", "Gere um relatório antes de imprimir.");
                return;
            }

            try
            {
                ImprimirRelatorio();
                AnunciarParaLeitorTela("Relatório enviado para impressão");
            }
            catch (Exception ex)
            {
                MostrarErro("Erro na impressão", ex.Message);
            }
        }

        /// <summary>
        /// Evento do botão Ajuda
        /// </summary>
        private void BtnAjuda_Click(object sender, RoutedEventArgs e)
        {
            var ajuda = @"🔍 AJUDA - RELATÓRIOS TICKET2HELP

📋 TIPOS DE RELATÓRIO:
• Resumo Geral: Visão geral de todos os tickets
• Tickets por Técnico: Desempenho individual dos técnicos
• Estatísticas por Período: Análise temporal dos dados
• Desempenho do Sistema: Métricas de eficiência
• Relatório de Hardware: Foco em tickets de hardware
• Relatório de Software: Foco em tickets de software

⏰ CONFIGURAÇÃO DE PERÍODO:
• Use os botões rápidos para períodos comuns
• Configure datas específicas com os seletores
• Período padrão: últimos 30 dias

🔧 FILTROS DISPONÍVEIS:
• Estado: Por Atender, Em Atendimento, Resolvido, etc.
• Tipo: Hardware, Software ou Todos
• Técnico: Específico ou Todos

⌨️ ATALHOS DE TECLADO:
• Ctrl+G: Gerar relatório
• Ctrl+E: Exportar relatório
• Ctrl+P: Imprimir relatório
• F1: Esta ajuda
• Tab: Navegar entre controlos

📤 EXPORTAÇÃO:
Formatos suportados: Excel (.xlsx), CSV (.csv), PDF (.pdf)

❓ Para mais ajuda, contacte o administrador do sistema.";

            MessageBox.Show(ajuda, "Ajuda - Relatórios", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Evento do botão Fechar
        /// </summary>
        private void BtnFechar_Click(object sender, RoutedEventArgs e)
        {
            var relatoriosWindow = new MainWindow(_usuarioLogado, _ticketController);
            relatoriosWindow.Show();
            this.Close();
        }

        /// <summary>
        /// Manipulador de teclas de atalho
        /// </summary>
        private void RelatoriosWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1)
            {
                BtnAjuda_Click(sender, e);
                e.Handled = true;
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.G:
                        if (btnGerar.IsEnabled)
                            BtnGerar_Click(sender, e);
                        e.Handled = true;
                        break;
                }
            }
        }

        #endregion

        #region Lógica de Relatórios

        /// <summary>
        /// Valida os parâmetros antes de gerar o relatório
        /// </summary>
        private bool ValidarParametros()
        {
            if (dpDataInicio.SelectedDate == null)
            {
                MostrarAviso("Data inválida", "Selecione a data de início.");
                dpDataInicio.Focus();
                return false;
            }

            if (dpDataFim.SelectedDate == null)
            {
                MostrarAviso("Data inválida", "Selecione a data de fim.");
                dpDataFim.Focus();
                return false;
            }

            if (dpDataInicio.SelectedDate > dpDataFim.SelectedDate)
            {
                MostrarAviso("Período inválido", "A data de início deve ser anterior à data de fim.");
                dpDataInicio.Focus();
                return false;
            }

            if (dpDataInicio.SelectedDate > DateTime.Today)
            {
                MostrarAviso("Data futura", "A data de início não pode ser no futuro.");
                dpDataInicio.Focus();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Obtém os parâmetros configurados para o relatório
        /// </summary>
        private ParametrosRelatorio ObterParametrosRelatorio()
        {
            var tipoRelatorio = ((ComboBoxItem)cmbTipoRelatorio.SelectedItem).Content.ToString();


            return new ParametrosRelatorio
            {
                TipoRelatorio = tipoRelatorio,
                DataInicio = dpDataInicio.SelectedDate.Value,
                DataFim = dpDataFim.SelectedDate.Value,
            };
        }

        /// <summary>
        /// Gera o relatório baseado nos parâmetros
        /// </summary>
        private async Task<OperationResult<DashboardStatistics>> GerarRelatorio(ParametrosRelatorio parametros)
        {
            try
            {
                // Usar o método apropriado baseado no tipo de relatório
                switch (parametros.TipoRelatorio)
                {
                    case "Resumo Geral":
                    case "Estatísticas por Período":
                        return await _ticketController.GenerateDashboardStatisticsAsync(
                            parametros.DataInicio, parametros.DataFim);

                    case "Tickets por Técnico":
                    case "Desempenho do Sistema":
                    case "Relatório de Hardware":
                    case "Relatório de Software":
                        // Para tipos específicos, usar o mesmo método mas aplicar filtros na apresentação
                        return await _ticketController.GenerateDashboardStatisticsAsync(
                            parametros.DataInicio, parametros.DataFim);

                    default:
                        return OperationResult<DashboardStatistics>.Failure("Tipo de relatório não suportado");
                }
            }
            catch (Exception ex)
            {
                return OperationResult<DashboardStatistics>.Failure($"Erro ao gerar relatório: {ex.Message}");
            }
        }

        /// <summary>
        /// Exibe o relatório na interface
        /// </summary>
        private void ExibirRelatorio(string tipoRelatorio, DashboardStatistics dados)
        {
            var painelRelatorio = new StackPanel();

            // Cabeçalho do relatório
            var titulo = new TextBlock
            {
                Text = $"📊 {tipoRelatorio}",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 15),
                Foreground = System.Windows.Media.Brushes.DarkBlue
            };
            painelRelatorio.Children.Add(titulo);

            // Período do relatório
            var periodo = new TextBlock
            {
                Text = $"Período: {dados.AnalysisPeriod.StartDate:dd/MM/yyyy} a {dados.AnalysisPeriod.EndDate:dd/MM/yyyy}",
                FontSize = 12,
                FontStyle = FontStyles.Italic,
                Margin = new Thickness(0, 0, 0, 20),
                Foreground = System.Windows.Media.Brushes.Gray
            };
            painelRelatorio.Children.Add(periodo);

            // Estatísticas principais
            AdicionarSecaoEstatisticas(painelRelatorio, dados);

            // Conteúdo específico por tipo
            switch (tipoRelatorio)
            {
                case "Resumo Geral":
                    AdicionarResumoGeral(painelRelatorio, dados);
                    break;

                case "Tickets por Técnico":
                    AdicionarRelatorioTecnicos(painelRelatorio, dados);
                    break;

                case "Estatísticas por Período":
                    AdicionarEstatisticasPeriodo(painelRelatorio, dados);
                    break;

                case "Desempenho do Sistema":
                    AdicionarDesempenhoSistema(painelRelatorio, dados);
                    break;

                case "Relatório de Hardware":
                    AdicionarRelatorioHardware(painelRelatorio, dados);
                    break;

                case "Relatório de Software":
                    AdicionarRelatorioSoftware(painelRelatorio, dados);
                    break;
            }

            // Atualizar conteúdo
            contentRelatorio.Content = painelRelatorio;
            txtTituloRelatorio.Text = $"{tipoRelatorio} - {dados.TotalTickets} tickets analisados";
        }

        /// <summary>
        /// Adiciona seção de estatísticas principais
        /// </summary>
        private void AdicionarSecaoEstatisticas(StackPanel painel, DashboardStatistics dados)
        {
            var grid = new Grid();

            // Definir colunas
            for (int i = 0; i < 4; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }

            // Cartões de estatística
            AdicionarCartaoEstatistica(grid, 0, "Total de Tickets", dados.TotalTickets.ToString(), "#3498DB");
            AdicionarCartaoEstatistica(grid, 1, "Atendidos", dados.AttendedTickets.ToString(), "#27AE60");
            AdicionarCartaoEstatistica(grid, 2, "Resolvidos", dados.ResolvedTickets.ToString(), "#2ECC71");
            AdicionarCartaoEstatistica(grid, 3, "Taxa Resolução", $"{dados.ResolvedTicketsPercentage:F1}%", "#E74C3C");

            painel.Children.Add(grid);

            // Separador
            painel.Children.Add(new Separator { Margin = new Thickness(0, 20, 0, 20) });
        }

        /// <summary>
        /// Adiciona um cartão de estatística
        /// </summary>
        private void AdicionarCartaoEstatistica(Grid grid, int coluna, string titulo, string valor, string cor)
        {
            var border = new Border
            {
                Background = System.Windows.Media.Brushes.White,
                BorderBrush = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom(cor),
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(15),
                Margin = new Thickness(5)
            };

            Grid.SetColumn(border, coluna);

            var stackPanel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var textoTitulo = new TextBlock
            {
                Text = titulo,
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = System.Windows.Media.Brushes.Gray
            };

            var textoValor = new TextBlock
            {
                Text = valor,
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom(cor)
            };

            stackPanel.Children.Add(textoTitulo);
            stackPanel.Children.Add(textoValor);
            border.Child = stackPanel;
            grid.Children.Add(border);
        }

        /// <summary>
        /// Adiciona conteúdo do resumo geral
        /// </summary>
        private void AdicionarResumoGeral(StackPanel painel, DashboardStatistics dados)
        {
            var titulo = new TextBlock
            {
                Text = "📋 Resumo Detalhado",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            };
            painel.Children.Add(titulo);

            // Tickets por tipo
            var tiposGrid = new Grid();
            tiposGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            tiposGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            AdicionarCartaoEstatistica(tiposGrid, 0, "Hardware", dados.HardwareTickets.ToString(), "#E67E22");
            AdicionarCartaoEstatistica(tiposGrid, 1, "Software", dados.SoftwareTickets.ToString(), "#9B59B6");

            painel.Children.Add(tiposGrid);

            // Tickets por status
            if (dados.TicketsByStatus != null && dados.TicketsByStatus.Any())
            {
                painel.Children.Add(new Separator { Margin = new Thickness(0, 15, 0, 15) });

                var statusTitulo = new TextBlock
                {
                    Text = "📊 Distribuição por Status",
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 0, 0, 10)
                };
                painel.Children.Add(statusTitulo);

                foreach (var status in dados.TicketsByStatus)
                {
                    var statusPanel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Margin = new Thickness(0, 5, 0, 5)
                    };

                    var statusTexto = new TextBlock
                    {
                        Text = $"{status.Key}:",
                        Width = 150,
                        FontWeight = FontWeights.SemiBold
                    };

                    var statusValor = new TextBlock
                    {
                        Text = $"{status.Value} tickets",
                        FontWeight = FontWeights.Bold,
                        Foreground = System.Windows.Media.Brushes.DarkBlue
                    };

                    statusPanel.Children.Add(statusTexto);
                    statusPanel.Children.Add(statusValor);
                    painel.Children.Add(statusPanel);
                }
            }
        }

        /// <summary>
        /// Adiciona relatório por técnicos
        /// </summary>
        private void AdicionarRelatorioTecnicos(StackPanel painel, DashboardStatistics dados)
        {
            var titulo = new TextBlock
            {
                Text = "👥 Desempenho por Técnico",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            };
            painel.Children.Add(titulo);

            if (dados.TicketsByTechnician != null && dados.TicketsByTechnician.Any())
            {
                foreach (var tecnico in dados.TicketsByTechnician.OrderByDescending(t => t.Value))
                {
                    var tecnicoPanel = new Border
                    {
                        Background = System.Windows.Media.Brushes.LightGray,
                        Padding = new Thickness(10),
                        Margin = new Thickness(0, 5, 0, 5),
                        CornerRadius = new CornerRadius(3)
                    };

                    var tecnicoStack = new StackPanel
                    {
                        Orientation = Orientation.Horizontal
                    };

                    var nomeTexto = new TextBlock
                    {
                        Text = tecnico.Key,
                        Width = 200,
                        FontWeight = FontWeights.SemiBold
                    };

                    var ticketsTexto = new TextBlock
                    {
                        Text = $"{tecnico.Value} tickets atendidos",
                        FontWeight = FontWeights.Bold,
                        Foreground = System.Windows.Media.Brushes.DarkGreen
                    };

                    tecnicoStack.Children.Add(nomeTexto);
                    tecnicoStack.Children.Add(ticketsTexto);
                    tecnicoPanel.Child = tecnicoStack;
                    painel.Children.Add(tecnicoPanel);
                }
            }
            else
            {
                var semDados = new TextBlock
                {
                    Text = "Nenhum dado de técnico disponível para o período selecionado.",
                    FontStyle = FontStyles.Italic,
                    Foreground = System.Windows.Media.Brushes.Gray
                };
                painel.Children.Add(semDados);
            }
        }

        /// <summary>
        /// Adiciona estatísticas por período
        /// </summary>
        private void AdicionarEstatisticasPeriodo(StackPanel painel, DashboardStatistics dados)
        {
            var titulo = new TextBlock
            {
                Text = "📈 Análise Temporal",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            };
            painel.Children.Add(titulo);

            var diasPeriodo = (dados.AnalysisPeriod.EndDate - dados.AnalysisPeriod.StartDate).Days + 1;
            var mediaDiaria = dados.TotalTickets / (double)diasPeriodo;

            var analisePanel = new StackPanel();

            // Métricas temporais
            var metricas = new[]
            {
                new { Label = "Período analisado", Valor = $"{diasPeriodo} dias" },
                new { Label = "Média diária", Valor = $"{mediaDiaria:F1} tickets/dia" },
                new { Label = "Taxa de atendimento", Valor = $"{dados.AttendedTicketsPercentage:F1}%" },
                new { Label = "Taxa de resolução", Valor = $"{dados.ResolvedTicketsPercentage:F1}%" }
            };

            foreach (var metrica in metricas)
            {
                var metricaPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 8, 0, 8)
                };

                var labelTexto = new TextBlock
                {
                    Text = $"{metrica.Label}:",
                    Width = 180,
                    FontWeight = FontWeights.SemiBold
                };

                var valorTexto = new TextBlock
                {
                    Text = metrica.Valor,
                    FontWeight = FontWeights.Bold,
                    Foreground = System.Windows.Media.Brushes.DarkBlue
                };

                metricaPanel.Children.Add(labelTexto);
                metricaPanel.Children.Add(valorTexto);
                analisePanel.Children.Add(metricaPanel);
            }

            painel.Children.Add(analisePanel);
        }

        /// <summary>
        /// Adiciona relatório de desempenho do sistema
        /// </summary>
        private void AdicionarDesempenhoSistema(StackPanel painel, DashboardStatistics dados)
        {
            var titulo = new TextBlock
            {
                Text = "⚡ Indicadores de Desempenho",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            };
            painel.Children.Add(titulo);

            // Calcular indicadores
            var ticketsPendentes = dados.TotalTickets - dados.AttendedTickets;
            var eficienciaGeral = dados.TotalTickets > 0 ? (dados.ResolvedTickets / (double)dados.TotalTickets) * 100 : 0;

            var indicadores = new Border
            {
                Background = System.Windows.Media.Brushes.AliceBlue,
                BorderBrush = System.Windows.Media.Brushes.SteelBlue,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(15),
                CornerRadius = new CornerRadius(5)
            };

            var indicadoresStack = new StackPanel();

            // Status do sistema
            var statusSistema = eficienciaGeral >= 80 ? "🟢 Excelente" :
                              eficienciaGeral >= 60 ? "🟡 Bom" :
                              eficienciaGeral >= 40 ? "🟠 Regular" : "🔴 Necessita Atenção";

            var statusTexto = new TextBlock
            {
                Text = $"Status do Sistema: {statusSistema}",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 15)
            };
            indicadoresStack.Children.Add(statusTexto);

            // Métricas de desempenho
            var metricas = new[]
            {
                new { Label = "Eficiência Geral", Valor = $"{eficienciaGeral:F1}%", Cor = eficienciaGeral >= 70 ? "Green" : "Red" },
                new { Label = "Tickets Pendentes", Valor = ticketsPendentes.ToString(), Cor = ticketsPendentes == 0 ? "Green" : "Orange" },
                new { Label = "Hardware vs Software", Valor = $"{dados.HardwareTickets}H / {dados.SoftwareTickets}S", Cor = "Blue" }
            };

            foreach (var metrica in metricas)
            {
                var metricaPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 5, 0, 5)
                };

                var labelTexto = new TextBlock
                {
                    Text = $"{metrica.Label}:",
                    Width = 180,
                    FontWeight = FontWeights.SemiBold
                };

                var valorTexto = new TextBlock
                {
                    Text = metrica.Valor,
                    FontWeight = FontWeights.Bold,
                    Foreground = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom(metrica.Cor)
                };

                metricaPanel.Children.Add(labelTexto);
                metricaPanel.Children.Add(valorTexto);
                indicadoresStack.Children.Add(metricaPanel);
            }

            indicadores.Child = indicadoresStack;
            painel.Children.Add(indicadores);
        }

        /// <summary>
        /// Adiciona relatório específico de hardware
        /// </summary>
        private void AdicionarRelatorioHardware(StackPanel painel, DashboardStatistics dados)
        {
            var titulo = new TextBlock
            {
                Text = "🔧 Relatório de Hardware",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            };
            painel.Children.Add(titulo);

            var hwPanel = new Border
            {
                Background = System.Windows.Media.Brushes.MistyRose,
                BorderBrush = System.Windows.Media.Brushes.IndianRed,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(15),
                CornerRadius = new CornerRadius(5)
            };

            var hwStack = new StackPanel();

            var hwStats = new[]
            {
                new { Label = "Total de Tickets Hardware", Valor = dados.HardwareTickets.ToString() },
                new { Label = "Percentual do Total", Valor = dados.TotalTickets > 0 ? $"{(dados.HardwareTickets / (double)dados.TotalTickets * 100):F1}%" : "0%" },
                new { Label = "Status", Valor = dados.HardwareTickets > dados.SoftwareTickets ? "Hardware predominante" : "Software predominante" }
            };

            foreach (var stat in hwStats)
            {
                var statPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 8, 0, 8)
                };

                var labelTexto = new TextBlock
                {
                    Text = $"{stat.Label}:",
                    Width = 200,
                    FontWeight = FontWeights.SemiBold
                };

                var valorTexto = new TextBlock
                {
                    Text = stat.Valor,
                    FontWeight = FontWeights.Bold,
                    Foreground = System.Windows.Media.Brushes.DarkRed
                };

                statPanel.Children.Add(labelTexto);
                statPanel.Children.Add(valorTexto);
                hwStack.Children.Add(statPanel);
            }

            hwPanel.Child = hwStack;
            painel.Children.Add(hwPanel);
        }

        /// <summary>
        /// Adiciona relatório específico de software
        /// </summary>
        private void AdicionarRelatorioSoftware(StackPanel painel, DashboardStatistics dados)
        {
            var titulo = new TextBlock
            {
                Text = "💻 Relatório de Software",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            };
            painel.Children.Add(titulo);

            var swPanel = new Border
            {
                Background = System.Windows.Media.Brushes.AliceBlue,
                BorderBrush = System.Windows.Media.Brushes.RoyalBlue,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(15),
                CornerRadius = new CornerRadius(5)
            };

            var swStack = new StackPanel();

            var swStats = new[]
            {
                new { Label = "Total de Tickets Software", Valor = dados.SoftwareTickets.ToString() },
                new { Label = "Percentual do Total", Valor = dados.TotalTickets > 0 ? $"{(dados.SoftwareTickets / (double)dados.TotalTickets * 100):F1}%" : "0%" },
                new { Label = "Status", Valor = dados.SoftwareTickets > dados.HardwareTickets ? "Software predominante" : "Hardware predominante" }
            };

            foreach (var stat in swStats)
            {
                var statPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 8, 0, 8)
                };

                var labelTexto = new TextBlock
                {
                    Text = $"{stat.Label}:",
                    Width = 200,
                    FontWeight = FontWeights.SemiBold
                };

                var valorTexto = new TextBlock
                {
                    Text = stat.Valor,
                    FontWeight = FontWeights.Bold,
                    Foreground = System.Windows.Media.Brushes.DarkBlue
                };

                statPanel.Children.Add(labelTexto);
                statPanel.Children.Add(valorTexto);
                swStack.Children.Add(statPanel);
            }

            swPanel.Child = swStack;
            painel.Children.Add(swPanel);
        }

        #endregion

        #region Exportação e Impressão

        /// <summary>
        /// Exporta o relatório para arquivo
        /// </summary>
        private void ExportarRelatorio(string caminhoArquivo)
        {
            var extensao = System.IO.Path.GetExtension(caminhoArquivo).ToLower();

            switch (extensao)
            {
                case ".xlsx":
                    ExportarParaExcel(caminhoArquivo);
                    break;

                case ".csv":
                    ExportarParaCsv(caminhoArquivo);
                    break;

                case ".pdf":
                    ExportarParaPdf(caminhoArquivo);
                    break;

                default:
                    throw new NotSupportedException($"Formato {extensao} não suportado");
            }
        }

        /// <summary>
        /// Exporta para formato Excel
        /// </summary>
        private void ExportarParaExcel(string caminho)
        {
            // Implementação simplificada - em produção usar biblioteca como EPPlus
            var dados = new List<string[]>
            {
                new[] { "Métrica", "Valor" },
                new[] { "Total de Tickets", _ultimoRelatorio.TotalTickets.ToString() },
                new[] { "Tickets Atendidos", _ultimoRelatorio.AttendedTickets.ToString() },
                new[] { "Tickets Resolvidos", _ultimoRelatorio.ResolvedTickets.ToString() },
                new[] { "Tickets Hardware", _ultimoRelatorio.HardwareTickets.ToString() },
                new[] { "Tickets Software", _ultimoRelatorio.SoftwareTickets.ToString() },
                new[] { "Taxa de Atendimento (%)", _ultimoRelatorio.AttendedTicketsPercentage.ToString("F1") },
                new[] { "Taxa de Resolução (%)", _ultimoRelatorio.ResolvedTicketsPercentage.ToString("F1") }
            };

            // Salvar como CSV temporariamente (pode ser melhorado com biblioteca Excel)
            using (var writer = new System.IO.StreamWriter(caminho, false, System.Text.Encoding.UTF8))
            {
                foreach (var linha in dados)
                {
                    writer.WriteLine(string.Join("\t", linha));
                }
            }
        }

        /// <summary>
        /// Exporta para formato CSV
        /// </summary>
        private void ExportarParaCsv(string caminho)
        {
            var dados = new List<string[]>
            {
                new[] { "Métrica", "Valor" },
                new[] { "Total de Tickets", _ultimoRelatorio.TotalTickets.ToString() },
                new[] { "Tickets Atendidos", _ultimoRelatorio.AttendedTickets.ToString() },
                new[] { "Tickets Resolvidos", _ultimoRelatorio.ResolvedTickets.ToString() },
                new[] { "Tickets Hardware", _ultimoRelatorio.HardwareTickets.ToString() },
                new[] { "Tickets Software", _ultimoRelatorio.SoftwareTickets.ToString() },
                new[] { "Taxa de Atendimento (%)", _ultimoRelatorio.AttendedTicketsPercentage.ToString("F1") },
                new[] { "Taxa de Resolução (%)", _ultimoRelatorio.ResolvedTicketsPercentage.ToString("F1") }
            };

            using (var writer = new System.IO.StreamWriter(caminho, false, System.Text.Encoding.UTF8))
            {
                foreach (var linha in dados)
                {
                    writer.WriteLine(string.Join(",", linha.Select(campo => $"\"{campo}\"")));
                }
            }
        }

        /// <summary>
        /// Exporta para formato PDF
        /// </summary>
        private void ExportarParaPdf(string caminho)
        {
            // Implementação simplificada usando PrintDocument
           /*
            var printDoc = new System.Drawing.Printing.PrintDocument();
            printDoc.PrintPage += (sender, e) =>
            {
                var fonte = new System.Drawing.Font("Arial", 12);
                var y = 100;
                var x = 100;

                e.Graphics.DrawString("RELATÓRIO TICKET2HELP", new System.Drawing.Font("Arial", 16, System.Drawing.FontStyle.Bold), System.Drawing.Brushes.Black, x, y);
                y += 40;

                e.Graphics.DrawString($"Período: {_ultimoRelatorio.AnalysisPeriod.StartDate:dd/MM/yyyy} a {_ultimoRelatorio.AnalysisPeriod.EndDate:dd/MM/yyyy}", fonte, System.Drawing.Brushes.Black, x, y);
                y += 30;

                var dados = new[]
                {
                    $"Total de Tickets: {_ultimoRelatorio.TotalTickets}",
                    $"Tickets Atendidos: {_ultimoRelatorio.AttendedTickets}",
                    $"Tickets Resolvidos: {_ultimoRelatorio.ResolvedTickets}",
                    $"Tickets Hardware: {_ultimoRelatorio.HardwareTickets}",
                    $"Tickets Software: {_ultimoRelatorio.SoftwareTickets}",
                    $"Taxa de Atendimento: {_ultimoRelatorio.AttendedTicketsPercentage:F1}%",
                    $"Taxa de Resolução: {_ultimoRelatorio.ResolvedTicketsPercentage:F1}%"
                };

                foreach (var dado in dados)
                {
                    e.Graphics.DrawString(dado, fonte, System.Drawing.Brushes.Black, x, y);
                    y += 25;
                }
            };

            // Salvar como arquivo (implementação simplificada)
            throw new NotImplementedException("Exportação PDF requer biblioteca adicional como iTextSharp");
           */
        }

        /// <summary>
        /// Imprime o relatório
        /// </summary>
        private void ImprimirRelatorio()
        {
            var printDialog = new PrintDialog();

            if (printDialog.ShowDialog() == true)
            {
                // Criar documento para impressão
                var flowDoc = new FlowDocument();

                // Título
                var titulo = new Paragraph(new Run("RELATÓRIO TICKET2HELP"))
                {
                    FontSize = 18,
                    FontWeight = FontWeights.Bold,
                    TextAlignment = TextAlignment.Center
                };
                flowDoc.Blocks.Add(titulo);

                // Período
                var periodo = new Paragraph(new Run($"Período: {_ultimoRelatorio.AnalysisPeriod.StartDate:dd/MM/yyyy} a {_ultimoRelatorio.AnalysisPeriod.EndDate:dd/MM/yyyy}"))
                {
                    FontSize = 12,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 20)
                };
                flowDoc.Blocks.Add(periodo);

                // Dados
                var dados = new[]
                {
                    $"Total de Tickets: {_ultimoRelatorio.TotalTickets}",
                    $"Tickets Atendidos: {_ultimoRelatorio.AttendedTickets}",
                    $"Tickets Resolvidos: {_ultimoRelatorio.ResolvedTickets}",
                    $"Tickets Hardware: {_ultimoRelatorio.HardwareTickets}",
                    $"Tickets Software: {_ultimoRelatorio.SoftwareTickets}",
                    $"Taxa de Atendimento: {_ultimoRelatorio.AttendedTicketsPercentage:F1}%",
                    $"Taxa de Resolução: {_ultimoRelatorio.ResolvedTicketsPercentage:F1}%"
                };

                foreach (var dado in dados)
                {
                    var paragrafo = new Paragraph(new Run(dado))
                    {
                        FontSize = 12,
                        Margin = new Thickness(0, 5, 0, 5)
                    };
                    flowDoc.Blocks.Add(paragrafo);
                }

                // Rodapé
                var rodape = new Paragraph(new Run($"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm:ss} | Utilizador: {_usuarioLogado.Nome ?? _usuarioLogado.Username}"))
                {
                    FontSize = 10,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 20, 0, 0),
                    Foreground = System.Windows.Media.Brushes.Gray
                };
                flowDoc.Blocks.Add(rodape);

                // Configurar documento para impressão
                IDocumentPaginatorSource idpSource = flowDoc;
                printDialog.PrintDocument(idpSource.DocumentPaginator, "Relatório Ticket2Help");
            }
        }

        #endregion

        #region Métodos Auxiliares

        /// <summary>
        /// Mostra mensagem de erro
        /// </summary>
        private void MostrarErro(string titulo, string mensagem)
        {
            MessageBox.Show(mensagem, titulo, MessageBoxButton.OK, MessageBoxImage.Error);
            txtStatus.Text = $"Erro: {titulo}";
        }

        /// <summary>
        /// Mostra mensagem de aviso
        /// </summary>
        private void MostrarAviso(string titulo, string mensagem)
        {
            MessageBox.Show(mensagem, titulo, MessageBoxButton.OK, MessageBoxImage.Warning);
            txtStatus.Text = $"Aviso: {titulo}";
        }

        /// <summary>
        /// Mostra mensagem de sucesso
        /// </summary>
        private void MostrarSucesso(string titulo, string mensagem)
        {
            MessageBox.Show(mensagem, titulo, MessageBoxButton.OK, MessageBoxImage.Information);
            txtStatus.Text = $"Sucesso: {titulo}";
        }

        /// <summary>
        /// Anuncia mensagem para leitores de tela
        /// </summary>
        private void AnunciarParaLeitorTela(string mensagem)
        {
            // Implementação simplificada - atualizar status
            if(txtStatus != null)
                txtStatus.Text = mensagem;

            // Para melhor acessibilidade, poderia usar System.Speech ou bibliotecas específicas
            System.Diagnostics.Debug.WriteLine($"Anúncio para leitor de tela: {mensagem}");
        }

        /// <summary>
        /// Cleanup quando a janela é fechada
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            _timerStatus?.Stop();
            base.OnClosed(e);
        }

        #endregion
    }

    #region Classes Auxiliares

    /// <summary>
    /// Classe para parâmetros de geração de relatório
    /// </summary>
    public class ParametrosRelatorio
    {
        public string TipoRelatorio { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public string Estado { get; set; }
        public string TipoTicket { get; set; }
        public string TecnicoId { get; set; }
    }

    #endregion
}