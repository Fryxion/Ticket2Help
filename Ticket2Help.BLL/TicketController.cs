using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ticket2Help.BLL.Models;
using Ticket2Help.BLL.Services;

namespace Ticket2Help.BLL.Controllers
{
    /// <summary>
    /// Controller principal para gestão de tickets
    /// Implementa o padrão MVC - Controller
    /// Faz a mediação entre a UI e os serviços de negócio
    /// </summary>
    public class TicketController
    {
        private readonly ITicketService _ticketService;
        private readonly IUserService _userService;
        private readonly IStatisticsService _statisticsService;

        /// <summary>
        /// Evento disparado quando um ticket é criado
        /// </summary>
        public event EventHandler<TicketEventArgs> TicketCreated;

        /// <summary>
        /// Evento disparado quando um ticket é atendido
        /// </summary>
        public event EventHandler<TicketEventArgs> TicketAttended;

        /// <summary>
        /// Evento disparado quando um atendimento é completado
        /// </summary>
        public event EventHandler<TicketEventArgs> AttendanceCompleted;

        /// <summary>
        /// Construtor do controller
        /// </summary>
        public TicketController(
            ITicketService ticketService,
            IUserService userService,
            IStatisticsService statisticsService = null)
        {
            _ticketService = ticketService ?? throw new ArgumentNullException(nameof(ticketService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _statisticsService = statisticsService; // Opcional
        }

        #region Gestão de Tickets

        /// <summary>
        /// Cria um novo ticket de hardware
        /// </summary>
        public async Task<OperationResult<int>> CreateHardwareTicketAsync(string colaboradorId, string equipamento, string avaria)
        {
            try
            {
                // Validar permissões
                var user = await _userService.GetUserByIdAsync(colaboradorId);
                if (user == null || !user.PodeCriarTickets())
                {
                    return OperationResult<int>.Failure("Utilizador não tem permissões para criar tickets");
                }

                // Criar ticket
                var ticketId = await _ticketService.CreateHardwareTicketAsync(colaboradorId, equipamento, avaria);

                // Disparar evento
                var ticket = await _ticketService.GetTicketByIdAsync(ticketId);
                TicketCreated?.Invoke(this, new TicketEventArgs(ticket));

                return OperationResult<int>.Success(ticketId);
            }
            catch (Exception ex)
            {
                return OperationResult<int>.Failure($"Erro ao criar ticket de hardware: {ex.Message}");
            }
        }

        /// <summary>
        /// Cria um novo ticket de software
        /// </summary>
        public async Task<OperationResult<int>> CreateSoftwareTicketAsync(string colaboradorId, string software, string descricaoNecessidade)
        {
            try
            {
                // Validar permissões
                var user = await _userService.GetUserByIdAsync(colaboradorId);
                if (user == null || !user.PodeCriarTickets())
                {
                    return OperationResult<int>.Failure("Utilizador não tem permissões para criar tickets");
                }

                // Criar ticket
                var ticketId = await _ticketService.CreateSoftwareTicketAsync(colaboradorId, software, descricaoNecessidade);

                // Disparar evento
                var ticket = await _ticketService.GetTicketByIdAsync(ticketId);
                TicketCreated?.Invoke(this, new TicketEventArgs(ticket));

                return OperationResult<int>.Success(ticketId);
            }
            catch (Exception ex)
            {
                return OperationResult<int>.Failure($"Erro ao criar ticket de software: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtém os tickets de um colaborador
        /// </summary>
        public async Task<OperationResult<IEnumerable<Ticket>>> GetUserTicketsAsync(string colaboradorId)
        {
            try
            {
                var tickets = await _ticketService.GetUserTicketsAsync(colaboradorId);
                return OperationResult<IEnumerable<Ticket>>.Success(tickets);
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<Ticket>>.Failure($"Erro ao obter tickets: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtém um ticket pelo ID
        /// </summary>
        public async Task<OperationResult<Ticket>> GetTicketByIdAsync(int ticketId)
        {
            try
            {
                var ticket = await _ticketService.GetTicketByIdAsync(ticketId);
                if (ticket == null)
                {
                    return OperationResult<Ticket>.Failure("Ticket não encontrado");
                }

                return OperationResult<Ticket>.Success(ticket);
            }
            catch (Exception ex)
            {
                return OperationResult<Ticket>.Failure($"Erro ao obter ticket: {ex.Message}");
            }
        }

        #endregion

        #region Atendimento de Tickets

        /// <summary>
        /// Obtém o próximo ticket para atendimento
        /// </summary>
        public async Task<OperationResult<Ticket>> GetNextTicketForAttendanceAsync()
        {
            try
            {
                var ticket = await _ticketService.GetNextTicketForAttendanceAsync();
                if (ticket == null)
                {
                    return OperationResult<Ticket>.Failure("Não há tickets disponíveis para atendimento");
                }

                return OperationResult<Ticket>.Success(ticket);
            }
            catch (Exception ex)
            {
                return OperationResult<Ticket>.Failure($"Erro ao obter próximo ticket: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtém todos os tickets pendentes
        /// </summary>
        public async Task<OperationResult<IEnumerable<Ticket>>> GetPendingTicketsAsync()
        {
            try
            {
                var tickets = await _ticketService.GetPendingTicketsAsync();
                return OperationResult<IEnumerable<Ticket>>.Success(tickets);
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<Ticket>>.Failure($"Erro ao obter tickets pendentes: {ex.Message}");
            }
        }

        /// <summary>
        /// Atende um ticket
        /// </summary>
        public async Task<OperationResult<bool>> AttendTicketAsync(int ticketId, string technicianId)
        {
            try
            {
                // Validar permissões do técnico
                var technician = await _userService.GetUserByIdAsync(technicianId);
                if (technician == null || !technician.PodeAtenderTickets())
                {
                    return OperationResult<bool>.Failure("Técnico não tem permissões para atender tickets");
                }

                // Atender ticket
                var success = await _ticketService.AttendTicketAsync(ticketId, technicianId);

                if (success)
                {
                    // Obter ticket atualizado e disparar evento
                    var ticket = await _ticketService.GetTicketByIdAsync(ticketId);
                    TicketAttended?.Invoke(this, new TicketEventArgs(ticket));
                }

                return OperationResult<bool>.Success(success);
            }
            catch (Exception ex)
            {
                return OperationResult<bool>.Failure($"Erro ao atender ticket: {ex.Message}");
            }
        }

        /// <summary>
        /// Completa o atendimento de um ticket de hardware
        /// </summary>
        public async Task<OperationResult<bool>> CompleteHardwareTicketAsync(
            int ticketId,
            EstadoAtendimento attendanceStatus,
            string repairDescription,
            string parts = null)
        {
            try
            {
                var success = await _ticketService.CompleteHardwareTicketAsync(ticketId, attendanceStatus, repairDescription, parts);

                if (success)
                {
                    // Obter ticket atualizado e disparar evento
                    var ticket = await _ticketService.GetTicketByIdAsync(ticketId);
                    AttendanceCompleted?.Invoke(this, new TicketEventArgs(ticket));
                }

                return OperationResult<bool>.Success(success);
            }
            catch (Exception ex)
            {
                return OperationResult<bool>.Failure($"Erro ao completar atendimento de hardware: {ex.Message}");
            }
        }

        /// <summary>
        /// Completa o atendimento de um ticket de software
        /// </summary>
        public async Task<OperationResult<bool>> CompleteSoftwareTicketAsync(
            int ticketId,
            EstadoAtendimento attendanceStatus,
            string interventionDescription)
        {
            try
            {
                var success = await _ticketService.CompleteSoftwareTicketAsync(ticketId, attendanceStatus, interventionDescription);

                if (success)
                {
                    // Obter ticket atualizado e disparar evento
                    var ticket = await _ticketService.GetTicketByIdAsync(ticketId);
                    AttendanceCompleted?.Invoke(this, new TicketEventArgs(ticket));
                }

                return OperationResult<bool>.Success(success);
            }
            catch (Exception ex)
            {
                return OperationResult<bool>.Failure($"Erro ao completar atendimento de software: {ex.Message}");
            }
        }

        #endregion

        #region Gestão de Utilizadores

        /// <summary>
        /// Autentica utilizador
        /// </summary>
        public async Task<OperationResult<User>> AuthenticateUserAsync(string username, string password)
        {
            try
            {
                var user = await _userService.AuthenticateAsync(username, password);
                if (user == null)
                    return OperationResult<User>.Failure("Credenciais inválidas");

                return OperationResult<User>.Success(user);
            }
            catch (Exception ex)
            {
                return OperationResult<User>.Failure($"Erro na autenticação: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtém técnicos ativos
        /// </summary>
        public async Task<OperationResult<IEnumerable<User>>> GetActiveTechniciansAsync()
        {
            try
            {
                var technicians = await _userService.GetActiveTechniciansAsync();
                return OperationResult<IEnumerable<User>>.Success(technicians);
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<User>>.Failure($"Erro ao obter técnicos: {ex.Message}");
            }
        }

        /// <summary>
        /// Cria um novo utilizador
        /// </summary>
        public async Task<OperationResult<bool>> CreateUserAsync(User user, string password)
        {
            try
            {
                var success = await _userService.CreateUserAsync(user, password);
                return OperationResult<bool>.Success(success);
            }
            catch (Exception ex)
            {
                return OperationResult<bool>.Failure($"Erro ao criar utilizador: {ex.Message}");
            }
        }

        #endregion

        #region Estatísticas e Relatórios

        /// <summary>
        /// Gera estatísticas do dashboard para um período
        /// </summary>
        public async Task<OperationResult<DashboardStatistics>> GenerateDashboardStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                if (_statisticsService == null)
                {
                    // Fallback: usar estatísticas básicas do TicketService
                    var basicStats = await _ticketService.GetTicketStatisticsAsync(startDate, endDate);
                    var dashboardData = ConvertToDashboardStatistics(basicStats, startDate, endDate);
                    return OperationResult<DashboardStatistics>.Success(dashboardData);
                }

                var statistics = await _statisticsService.GenerateDashboardStatisticsAsync(startDate, endDate);
                return OperationResult<DashboardStatistics>.Success(statistics);
            }
            catch (Exception ex)
            {
                return OperationResult<DashboardStatistics>.Failure($"Erro ao gerar estatísticas: {ex.Message}");
            }
        }

        /// <summary>
        /// Gera estatísticas para o mês atual
        /// </summary>
        public async Task<OperationResult<DashboardStatistics>> GenerateCurrentMonthStatisticsAsync()
        {
            var now = DateTime.Now;
            var startDate = new DateTime(now.Year, now.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            return await GenerateDashboardStatisticsAsync(startDate, endDate);
        }

        /// <summary>
        /// Gera estatísticas para os últimos 30 dias
        /// </summary>
        public async Task<OperationResult<DashboardStatistics>> GenerateLast30DaysStatisticsAsync()
        {
            var endDate = DateTime.Now;
            var startDate = endDate.AddDays(-30);

            return await GenerateDashboardStatisticsAsync(startDate, endDate);
        }

        /// <summary>
        /// Obtém resumo simples do dashboard
        /// </summary>
        public async Task<OperationResult<DashboardSummary>> GetDashboardSummaryAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var summary = await _ticketService.GetDashboardSummaryAsync(startDate, endDate);
                return OperationResult<DashboardSummary>.Success(summary);
            }
            catch (Exception ex)
            {
                return OperationResult<DashboardSummary>.Failure($"Erro ao obter resumo: {ex.Message}");
            }
        }

        #endregion

        #region Métodos Auxiliares

        /// <summary>
        /// Converte estatísticas básicas em DashboardStatistics
        /// </summary>
        private DashboardStatistics ConvertToDashboardStatistics(Dictionary<string, object> basicStats, DateTime startDate, DateTime endDate)
        {
            var hwStats = (Dictionary<string, object>)basicStats["Hardware"];
            var swStats = (Dictionary<string, object>)basicStats["Software"];

            var totalTickets = Convert.ToInt32(basicStats["TotalTickets"]);
            var totalAtendidos = Convert.ToInt32(basicStats["TotalAtendidos"]);

            return new DashboardStatistics
            {
                AnalysisPeriod = new DateRange(startDate, endDate),
                TotalTickets = totalTickets,
                AttendedTickets = totalAtendidos,
                HardwareTickets = Convert.ToInt32(hwStats["TotalTickets"]),
                SoftwareTickets = Convert.ToInt32(swStats["TotalTickets"]),
                ResolvedTickets = Convert.ToInt32(basicStats["TotalResolvidos"]),
                AttendedTicketsPercentage = totalTickets > 0 ? (double)totalAtendidos / totalTickets * 100 : 0,
                ResolvedTicketsPercentage = totalAtendidos > 0 ? (double)Convert.ToInt32(basicStats["TotalResolvidos"]) / totalAtendidos * 100 : 0,
                TicketsByStatus = new Dictionary<string, int>
                {
                    ["Por Atender"] = totalTickets - totalAtendidos,
                    ["Atendido"] = totalAtendidos
                },
                TicketsByTechnician = new Dictionary<string, int>()
            };
        }

        #endregion
    }

    /// <summary>
    /// Classe para resultados de operações
    /// Implementa um padrão comum para retorno de resultados com sucesso/erro
    /// </summary>
    /// <typeparam name="T">Tipo do resultado</typeparam>
    public class OperationResult<T>
    {
        /// <summary>
        /// Indica se a operação foi bem-sucedida
        /// </summary>
        public bool IsSuccess { get; private set; }

        /// <summary>
        /// Mensagem de erro (se houver)
        /// </summary>
        public string ErrorMessage { get; private set; }

        /// <summary>
        /// Resultado da operação
        /// </summary>
        public T Data { get; private set; }

        private OperationResult(bool isSuccess, T data, string errorMessage)
        {
            IsSuccess = isSuccess;
            Data = data;
            ErrorMessage = errorMessage;
        }

        /// <summary>
        /// Cria um resultado de sucesso
        /// </summary>
        public static OperationResult<T> Success(T data)
        {
            return new OperationResult<T>(true, data, null);
        }

        /// <summary>
        /// Cria um resultado de erro
        /// </summary>
        public static OperationResult<T> Failure(string errorMessage)
        {
            return new OperationResult<T>(false, default(T), errorMessage);
        }
    }

    /// <summary>
    /// Classe para argumentos de eventos de tickets
    /// </summary>
    public class TicketEventArgs : EventArgs
    {
        /// <summary>
        /// Ticket relacionado com o evento
        /// </summary>
        public Ticket Ticket { get; }

        /// <summary>
        /// Data e hora do evento
        /// </summary>
        public DateTime EventTime { get; }

        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="ticket">Ticket relacionado</param>
        public TicketEventArgs(Ticket ticket)
        {
            Ticket = ticket ?? throw new ArgumentNullException(nameof(ticket));
            EventTime = DateTime.Now;
        }
    }
}