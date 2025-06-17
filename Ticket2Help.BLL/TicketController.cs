using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ticket2Help.BLL.Models;
using Ticket2Help.BLL.Services;
using Ticket2Help.BLL.Factory;
using Ticket2Help.BLL.Strategy;

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
        private readonly IStatisticsService _statisticsService;
        private readonly IUserService _userService;

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
        /// <param name="ticketService">Serviço de tickets</param>
        /// <param name="statisticsService">Serviço de estatísticas</param>
        /// <param name="userService">Serviço de utilizadores</param>
        public TicketController(
            ITicketService ticketService,
            IStatisticsService statisticsService,
            IUserService userService)
        {
            _ticketService = ticketService ?? throw new ArgumentNullException(nameof(ticketService));
            _statisticsService = statisticsService ?? throw new ArgumentNullException(nameof(statisticsService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        #region Gestão de Tickets

        /// <summary>
        /// Cria um novo ticket de hardware
        /// </summary>
        /// <param name="userId">ID do utilizador</param>
        /// <param name="equipment">Equipamento</param>
        /// <param name="malfunction">Descrição da avaria</param>
        /// <returns>Resultado da operação</returns>
        public async Task<OperationResult<Ticket>> CreateHardwareTicketAsync(int userId, string equipment, string malfunction)
        {
            try
            {
                // Validar permissões
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null || !user.CanCreateTickets())
                {
                    return OperationResult<Ticket>.Failure("Utilizador não tem permissões para criar tickets");
                }

                // Criar dados específicos
                var hardwareData = new HardwareTicketData(equipment, malfunction);

                // Criar ticket
                var ticket = await _ticketService.CreateTicketAsync(TicketType.Hardware, userId, hardwareData);

                // Disparar evento
                TicketCreated?.Invoke(this, new TicketEventArgs(ticket));

                return OperationResult<Ticket>.Success(ticket);
            }
            catch (Exception ex)
            {
                return OperationResult<Ticket>.Failure($"Erro ao criar ticket de hardware: {ex.Message}");
            }
        }

        /// <summary>
        /// Cria um novo ticket de software
        /// </summary>
        /// <param name="userId">ID do utilizador</param>
        /// <param name="software">Software</param>
        /// <param name="needDescription">Descrição da necessidade</param>
        /// <returns>Resultado da operação</returns>
        public async Task<OperationResult<Ticket>> CreateSoftwareTicketAsync(int userId, string software, string needDescription)
        {
            try
            {
                // Validar permissões
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null || !user.CanCreateTickets())
                {
                    return OperationResult<Ticket>.Failure("Utilizador não tem permissões para criar tickets");
                }

                // Criar dados específicos
                var softwareData = new SoftwareTicketData(software, needDescription);

                // Criar ticket
                var ticket = await _ticketService.CreateTicketAsync(TicketType.Software, userId, softwareData);

                // Disparar evento
                TicketCreated?.Invoke(this, new TicketEventArgs(ticket));

                return OperationResult<Ticket>.Success(ticket);
            }
            catch (Exception ex)
            {
                return OperationResult<Ticket>.Failure($"Erro ao criar ticket de software: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtém os tickets de um utilizador
        /// </summary>
        /// <param name="userId">ID do utilizador</param>
        /// <returns>Lista de tickets</returns>
        public async Task<OperationResult<IEnumerable<Ticket>>> GetUserTicketsAsync(int userId)
        {
            try
            {
                var tickets = await _ticketService.GetUserTicketsAsync(userId);
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
        /// <param name="ticketId">ID do ticket</param>
        /// <returns>Ticket encontrado</returns>
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
        /// <returns>Próximo ticket</returns>
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
        /// <returns>Lista de tickets pendentes</returns>
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
        /// <param name="ticketId">ID do ticket</param>
        /// <param name="technicianId">ID do técnico</param>
        /// <returns>Resultado da operação</returns>
        public async Task<OperationResult<bool>> AttendTicketAsync(int ticketId, int technicianId)
        {
            try
            {
                // Validar permissões do técnico
                var technician = await _userService.GetUserByIdAsync(technicianId);
                if (technician == null || !technician.CanAttendTickets())
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
        /// <param name="ticketId">ID do ticket</param>
        /// <param name="attendanceStatus">Estado do atendimento</param>
        /// <param name="repairDescription">Descrição da reparação</param>
        /// <param name="parts">Peças utilizadas</param>
        /// <returns>Resultado da operação</returns>
        public async Task<OperationResult<bool>> CompleteHardwareTicketAsync(
            int ticketId,
            AttendanceStatus attendanceStatus,
            string repairDescription,
            string parts = null)
        {
            try
            {
                var details = new HardwareAttendanceDetails(repairDescription, parts);
                var success = await _ticketService.CompleteTicketAttendanceAsync(ticketId, attendanceStatus, details);

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
        /// <param name="ticketId">ID do ticket</param>
        /// <param name="attendanceStatus">Estado do atendimento</param>
        /// <param name="interventionDescription">Descrição da intervenção</param>
        /// <returns>Resultado da operação</returns>
        public async Task<OperationResult<bool>> CompleteSoftwareTicketAsync(
            int ticketId,
            AttendanceStatus attendanceStatus,
            string interventionDescription)
        {
            try
            {
                var details = new SoftwareAttendanceDetails(interventionDescription);
                var success = await _ticketService.CompleteTicketAttendanceAsync(ticketId, attendanceStatus, details);

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

        /// <summary>
        /// Define a estratégia de atendimento
        /// </summary>
        /// <param name="strategy">Estratégia a usar</param>
        /// <returns>Resultado da operação</returns>
        public OperationResult<bool> SetAttendanceStrategy(ITicketAttendanceStrategy strategy)
        {
            try
            {
                _ticketService.SetAttendanceStrategy(strategy);
                return OperationResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return OperationResult<bool>.Failure($"Erro ao definir estratégia: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtém as estratégias de atendimento disponíveis
        /// </summary>
        /// <returns>Lista de estratégias</returns>
        public OperationResult<List<ITicketAttendanceStrategy>> GetAvailableStrategies()
        {
            try
            {
                var strategies = TicketAttendanceContext.GetAvailableStrategies();
                return OperationResult<List<ITicketAttendanceStrategy>>.Success(strategies);
            }
            catch (Exception ex)
            {
                return OperationResult<List<ITicketAttendanceStrategy>>.Failure($"Erro ao obter estratégias: {ex.Message}");
            }
        }

        #endregion

        #region Estatísticas e Relatórios

        /// <summary>
        /// Gera estatísticas do dashboard para um período
        /// </summary>
        /// <param name="startDate">Data de início</param>
        /// <param name="endDate">Data de fim</param>
        /// <returns>Estatísticas do dashboard</returns>
        public async Task<OperationResult<DashboardStatistics>> GenerateDashboardStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
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
        /// <returns>Estatísticas do mês atual</returns>
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
        /// <returns>Estatísticas dos últimos 30 dias</returns>
        public async Task<OperationResult<DashboardStatistics>> GenerateLast30DaysStatisticsAsync()
        {
            var endDate = DateTime.Now;
            var startDate = endDate.AddDays(-30);

            return await GenerateDashboardStatisticsAsync(startDate, endDate);
        }

        /// <summary>
        /// Obtém a percentagem de tickets atendidos num período
        /// </summary>
        /// <param name="startDate">Data de início</param>
        /// <param name="endDate">Data de fim</param>
        /// <returns>Percentagem de tickets atendidos</returns>
        public async Task<OperationResult<double>> GetAttendedPercentageAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var percentage = await _statisticsService.GetAttendedTicketsPercentageAsync(startDate, endDate);
                return OperationResult<double>.Success(percentage);
            }
            catch (Exception ex)
            {
                return OperationResult<double>.Failure($"Erro ao calcular percentagem: {ex.Message}");
            }
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
        /// <param name="data">Dados do resultado</param>
        /// <returns>Resultado de sucesso</returns>
        public static OperationResult<T> Success(T data)
        {
            return new OperationResult<T>(true, data, null);
        }

        /// <summary>
        /// Cria um resultado de erro
        /// </summary>
        /// <param name="errorMessage">Mensagem de erro</param>
        /// <returns>Resultado de erro</returns>
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