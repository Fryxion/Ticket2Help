using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ticket2Help.BLL.Models;
using Ticket2Help.DAL.Interfaces;
using Ticket2Help.DAL.Repositories;

namespace Ticket2Help.BLL.Services
{
    /// <summary>
    /// Classe para representar estatísticas do dashboard
    /// </summary>
    public class DashboardStatistics
    {
        /// <summary>
        /// Percentagem de tickets atendidos no período
        /// </summary>
        public double AttendedTicketsPercentage { get; set; }

        /// <summary>
        /// Percentagem de tickets resolvidos
        /// </summary>
        public double ResolvedTicketsPercentage { get; set; }

        /// <summary>
        /// Percentagem de tickets não resolvidos
        /// </summary>
        public double UnresolvedTicketsPercentage { get; set; }

        /// <summary>
        /// Tempo médio de atendimento para tickets de hardware (em horas)
        /// </summary>
        public double AverageHardwareAttendanceTime { get; set; }

        /// <summary>
        /// Tempo médio de atendimento para tickets de software (em horas)
        /// </summary>
        public double AverageSoftwareAttendanceTime { get; set; }

        /// <summary>
        /// Total de tickets no período
        /// </summary>
        public int TotalTickets { get; set; }

        /// <summary>
        /// Total de tickets atendidos
        /// </summary>
        public int AttendedTickets { get; set; }

        /// <summary>
        /// Total de tickets resolvidos
        /// </summary>
        public int ResolvedTickets { get; set; }

        /// <summary>
        /// Total de tickets não resolvidos
        /// </summary>
        public int UnresolvedTickets { get; set; }

        /// <summary>
        /// Total de tickets de hardware
        /// </summary>
        public int HardwareTickets { get; set; }

        /// <summary>
        /// Total de tickets de software
        /// </summary>
        public int SoftwareTickets { get; set; }

        /// <summary>
        /// Tickets por técnico
        /// </summary>
        public Dictionary<string, int> TicketsByTechnician { get; set; }

        /// <summary>
        /// Tickets por estado
        /// </summary>
        public Dictionary<string, int> TicketsByStatus { get; set; }

        /// <summary>
        /// Período da análise
        /// </summary>
        public DateRange AnalysisPeriod { get; set; }

        public DashboardStatistics()
        {
            TicketsByTechnician = new Dictionary<string, int>();
            TicketsByStatus = new Dictionary<string, int>();
        }
    }

    /// <summary>
    /// Classe para representar um período de datas
    /// </summary>
    public class DateRange
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public DateRange(DateTime startDate, DateTime endDate)
        {
            StartDate = startDate;
            EndDate = endDate;
        }

        public int TotalDays => (EndDate - StartDate).Days + 1;
    }

    /// <summary>
    /// Interface para o serviço de estatísticas
    /// </summary>
    public interface IStatisticsService
    {
        /// <summary>
        /// Gera estatísticas para o dashboard num período específico
        /// </summary>
        /// <param name="startDate">Data de início</param>
        /// <param name="endDate">Data de fim</param>
        /// <returns>Estatísticas do dashboard</returns>
        Task<DashboardStatistics> GenerateDashboardStatisticsAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Calcula a percentagem de tickets atendidos num período
        /// </summary>
        /// <param name="startDate">Data de início</param>
        /// <param name="endDate">Data de fim</param>
        /// <returns>Percentagem de tickets atendidos</returns>
        Task<double> GetAttendedTicketsPercentageAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Calcula as percentagens de tickets resolvidos e não resolvidos
        /// </summary>
        /// <returns>Tupla com percentagens (resolvidos, não resolvidos)</returns>
        Task<(double resolved, double unresolved)> GetResolutionPercentagesAsync();

        /// <summary>
        /// Calcula o tempo médio de atendimento por tipo de ticket
        /// </summary>
        /// <param name="type">Tipo de ticket</param>
        /// <returns>Tempo médio em horas</returns>
        Task<double> GetAverageAttendanceTimeAsync(TicketType type);

        /// <summary>
        /// Obtém o número de tickets por técnico
        /// </summary>
        /// <returns>Dicionário com técnico e número de tickets</returns>
        Task<Dictionary<string, int>> GetTicketsByTechnicianAsync();

        /// <summary>
        /// Obtém a distribuição de tickets por estado
        /// </summary>
        /// <returns>Dicionário com estado e número de tickets</returns>
        Task<Dictionary<string, int>> GetTicketsByStatusAsync();
    }

    /// <summary>
    /// Implementação do serviço de estatísticas
    /// </summary>
    public class StatisticsService : IStatisticsService
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IUserRepository _userRepository;

        /// <summary>
        /// Construtor do serviço
        /// </summary>
        /// <param name="ticketRepository">Repositório de tickets</param>
        /// <param name="userRepository">Repositório de utilizadores</param>
        public StatisticsService(ITicketRepository ticketRepository, IUserRepository userRepository)
        {
            _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        /// <summary>
        /// Gera estatísticas completas para o dashboard
        /// </summary>
        /// <param name="startDate">Data de início</param>
        /// <param name="endDate">Data de fim</param>
        /// <returns>Estatísticas do dashboard</returns>
        public async Task<DashboardStatistics> GenerateDashboardStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            // Validar datas
            if (startDate > endDate)
                throw new ArgumentException("Data de início não pode ser posterior à data de fim");

            var statistics = new DashboardStatistics
            {
                AnalysisPeriod = new DateRange(startDate, endDate)
            };

            // Obter todos os tickets no período
            var allTickets = await _ticketRepository.GetTicketsByDateRangeAsync(startDate, endDate);
            var ticketsList = allTickets.ToList();

            // Estatísticas básicas
            statistics.TotalTickets = ticketsList.Count;
            statistics.AttendedTickets = ticketsList.Count(t => t.Status == TicketStatus.Atendido);
            statistics.HardwareTickets = ticketsList.Count(t => t.Type == TicketType.Hardware);
            statistics.SoftwareTickets = ticketsList.Count(t => t.Type == TicketType.Software);

            // Calcular percentagens
            if (statistics.TotalTickets > 0)
            {
                statistics.AttendedTicketsPercentage = (double)statistics.AttendedTickets / statistics.TotalTickets * 100;
            }

            // Estatísticas de resolução
            var attendedTickets = ticketsList.Where(t => t.Status == TicketStatus.Atendido).ToList();
            if (attendedTickets.Any())
            {
                statistics.ResolvedTickets = attendedTickets.Count(t => t.AttendanceStatus == AttendanceStatus.Resolvido);
                statistics.UnresolvedTickets = attendedTickets.Count(t => t.AttendanceStatus == AttendanceStatus.NaoResolvido);

                statistics.ResolvedTicketsPercentage = (double)statistics.ResolvedTickets / attendedTickets.Count * 100;
                statistics.UnresolvedTicketsPercentage = (double)statistics.UnresolvedTickets / attendedTickets.Count * 100;
            }

            // Tempos médios de atendimento
            statistics.AverageHardwareAttendanceTime = await CalculateAverageAttendanceTime(ticketsList, TicketType.Hardware);
            statistics.AverageSoftwareAttendanceTime = await CalculateAverageAttendanceTime(ticketsList, TicketType.Software);

            // Distribuições
            statistics.TicketsByTechnician = await GenerateTicketsByTechnicianDictionary(attendedTickets);
            statistics.TicketsByStatus = GenerateTicketsByStatusDictionary(ticketsList);

            return statistics;
        }

        /// <summary>
        /// Calcula a percentagem de tickets atendidos num período
        /// </summary>
        public async Task<double> GetAttendedTicketsPercentageAsync(DateTime startDate, DateTime endDate)
        {
            var tickets = await _ticketRepository.GetTicketsByDateRangeAsync(startDate, endDate);
            var ticketsList = tickets.ToList();

            if (!ticketsList.Any())
                return 0;

            var attendedCount = ticketsList.Count(t => t.Status == TicketStatus.Atendido);
            return (double)attendedCount / ticketsList.Count * 100;
        }

        /// <summary>
        /// Calcula as percentagens de resolução
        /// </summary>
        public async Task<(double resolved, double unresolved)> GetResolutionPercentagesAsync()
        {
            var allTickets = await _ticketRepository.GetAllTicketsAsync();
            var attendedTickets = allTickets.Where(t => t.Status == TicketStatus.Atendido).ToList();

            if (!attendedTickets.Any())
                return (0, 0);

            var resolvedCount = attendedTickets.Count(t => t.AttendanceStatus == AttendanceStatus.Resolvido);
            var unresolvedCount = attendedTickets.Count(t => t.AttendanceStatus == AttendanceStatus.NaoResolvido);

            var resolvedPercentage = (double)resolvedCount / attendedTickets.Count * 100;
            var unresolvedPercentage = (double)unresolvedCount / attendedTickets.Count * 100;

            return (resolvedPercentage, unresolvedPercentage);
        }

        /// <summary>
        /// Calcula o tempo médio de atendimento por tipo
        /// </summary>
        public async Task<double> GetAverageAttendanceTimeAsync(TicketType type)
        {
            var allTickets = await _ticketRepository.GetAllTicketsAsync();
            var typeTickets = allTickets
                .Where(t => t.Type == type && t.Status == TicketStatus.Atendido && t.AttendedDate.HasValue)
                .ToList();

            if (!typeTickets.Any())
                return 0;

            var totalHours = typeTickets
                .Sum(t => (t.AttendedDate.Value - t.CreatedDate).TotalHours);

            return totalHours / typeTickets.Count;
        }

        /// <summary>
        /// Obtém distribuição de tickets por técnico
        /// </summary>
        public async Task<Dictionary<string, int>> GetTicketsByTechnicianAsync()
        {
            var allTickets = await _ticketRepository.GetAllTicketsAsync();
            var attendedTickets = allTickets.Where(t => t.TechnicianId.HasValue).ToList();

            return await GenerateTicketsByTechnicianDictionary(attendedTickets);
        }

        /// <summary>
        /// Obtém distribuição de tickets por estado
        /// </summary>
        public async Task<Dictionary<string, int>> GetTicketsByStatusAsync()
        {
            var allTickets = await _ticketRepository.GetAllTicketsAsync();
            return GenerateTicketsByStatusDictionary(allTickets.ToList());
        }

        /// <summary>
        /// Calcula o tempo médio de atendimento para uma lista de tickets
        /// </summary>
        private async Task<double> CalculateAverageAttendanceTime(List<Ticket> tickets, TicketType type)
        {
            var typeTickets = tickets
                .Where(t => t.Type == type && t.Status == TicketStatus.Atendido && t.AttendedDate.HasValue)
                .ToList();

            if (!typeTickets.Any())
                return 0;

            var totalHours = typeTickets
                .Sum(t => (t.AttendedDate.Value - t.CreatedDate).TotalHours);

            return totalHours / typeTickets.Count;
        }

        /// <summary>
        /// Gera dicionário de tickets por técnico
        /// </summary>
        private async Task<Dictionary<string, int>> GenerateTicketsByTechnicianDictionary(List<Ticket> tickets)
        {
            var result = new Dictionary<string, int>();

            var technicianIds = tickets.Where(t => t.TechnicianId.HasValue)
                                    .Select(t => t.TechnicianId.Value)
                                    .Distinct();

            foreach (var techId in technicianIds)
            {
                var technician = await _userRepository.GetUserByIdAsync(techId);
                var techName = technician?.FullName ?? $"Técnico #{techId}";
                var ticketCount = tickets.Count(t => t.TechnicianId == techId);

                result[techName] = ticketCount;
            }

            return result.OrderByDescending(kvp => kvp.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        /// <summary>
        /// Gera dicionário de tickets por estado
        /// </summary>
        private Dictionary<string, int> GenerateTicketsByStatusDictionary(List<Ticket> tickets)
        {
            return new Dictionary<string, int>
            {
                ["Por Atender"] = tickets.Count(t => t.Status == TicketStatus.PorAtender),
                ["Em Atendimento"] = tickets.Count(t => t.Status == TicketStatus.EmAtendimento),
                ["Atendido"] = tickets.Count(t => t.Status == TicketStatus.Atendido)
            };
        }
    }

    /// <summary>
    /// Classe para gerar relatórios detalhados
    /// </summary>
    public class ReportGenerator
    {
        private readonly IStatisticsService _statisticsService;

        public ReportGenerator(IStatisticsService statisticsService)
        {
            _statisticsService = statisticsService ?? throw new ArgumentNullException(nameof(statisticsService));
        }

        /// <summary>
        /// Gera um relatório em texto
        /// </summary>
        /// <param name="statistics">Estatísticas a incluir</param>
        /// <returns>Relatório em formato texto</returns>
        public string GenerateTextReport(DashboardStatistics statistics)
        {
            var report = $@"
=== RELATÓRIO DE TICKETS ===
Período: {statistics.AnalysisPeriod.StartDate:dd/MM/yyyy} - {statistics.AnalysisPeriod.EndDate:dd/MM/yyyy}

RESUMO GERAL:
- Total de Tickets: {statistics.TotalTickets}
- Tickets Atendidos: {statistics.AttendedTickets} ({statistics.AttendedTicketsPercentage:F1}%)
- Tickets de Hardware: {statistics.HardwareTickets}
- Tickets de Software: {statistics.SoftwareTickets}

RESOLUÇÃO:
- Tickets Resolvidos: {statistics.ResolvedTickets} ({statistics.ResolvedTicketsPercentage:F1}%)
- Tickets Não Resolvidos: {statistics.UnresolvedTickets} ({statistics.UnresolvedTicketsPercentage:F1}%)

TEMPOS MÉDIOS DE ATENDIMENTO:
- Hardware: {statistics.AverageHardwareAttendanceTime:F1} horas
- Software: {statistics.AverageSoftwareAttendanceTime:F1} horas

DISTRIBUIÇÃO POR TÉCNICO:";

            foreach (var tech in statistics.TicketsByTechnician)
            {
                report += $"\n- {tech.Key}: {tech.Value} tickets";
            }

            report += "\n\nDISTRIBUIÇÃO POR ESTADO:";
            foreach (var status in statistics.TicketsByStatus)
            {
                report += $"\n- {status.Key}: {status.Value} tickets";
            }

            return report;
        }
    }
}