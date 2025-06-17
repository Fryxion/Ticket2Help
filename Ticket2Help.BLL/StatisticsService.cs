using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ticket2Help.BLL.Models;
using Ticket2Help.DAL.Repositories;

namespace Ticket2Help.BLL.Services
{
    /// <summary>
    /// Classe para representar estatísticas do dashboard
    /// Compatível com o DAL existente
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

        /// <summary>
        /// Tempo médio geral de atendimento
        /// </summary>
        public double AverageAttendanceTime { get; set; }

        /// <summary>
        /// Taxa de resolução (resolvidos / atendidos)
        /// </summary>
        public double ResolutionRate { get; set; }

        /// <summary>
        /// Produtividade diária (tickets por dia)
        /// </summary>
        public double DailyProductivity { get; set; }

        /// <summary>
        /// Tendência em relação ao período anterior
        /// </summary>
        public TrendAnalysis Trend { get; set; }

        public DashboardStatistics()
        {
            TicketsByTechnician = new Dictionary<string, int>();
            TicketsByStatus = new Dictionary<string, int>();
            Trend = new TrendAnalysis();
        }

        /// <summary>
        /// Obtém um resumo textual das estatísticas
        /// </summary>
        public string GetSummary()
        {
            return $@"
📊 RESUMO ESTATÍSTICAS
━━━━━━━━━━━━━━━━━━━━━
📅 Período: {AnalysisPeriod.StartDate:dd/MM/yyyy} - {AnalysisPeriod.EndDate:dd/MM/yyyy}

📈 GERAL:
   • Total de Tickets: {TotalTickets}
   • Taxa de Atendimento: {AttendedTicketsPercentage:F1}%
   • Taxa de Resolução: {ResolutionRate:F1}%
   • Produtividade: {DailyProductivity:F1} tickets/dia

⚙️ POR TIPO:
   • Hardware: {HardwareTickets} ({(TotalTickets > 0 ? (double)HardwareTickets / TotalTickets * 100 : 0):F1}%)
   • Software: {SoftwareTickets} ({(TotalTickets > 0 ? (double)SoftwareTickets / TotalTickets * 100 : 0):F1}%)

⏱️ TEMPOS MÉDIOS:
   • Hardware: {AverageHardwareAttendanceTime:F1}h
   • Software: {AverageSoftwareAttendanceTime:F1}h
   • Geral: {AverageAttendanceTime:F1}h

🎯 RESOLUÇÃO:
   • Resolvidos: {ResolvedTickets} ({ResolvedTicketsPercentage:F1}%)
   • Não Resolvidos: {UnresolvedTickets} ({UnresolvedTicketsPercentage:F1}%)";
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
        public bool IsValid => StartDate <= EndDate;
        public TimeSpan Duration => EndDate - StartDate;
    }

    /// <summary>
    /// Análise de tendências
    /// </summary>
    public class TrendAnalysis
    {
        public double TotalTicketsChange { get; set; }
        public double AttendanceRateChange { get; set; }
        public double ResolutionRateChange { get; set; }
        public double ProductivityChange { get; set; }
        public string TrendDirection { get; set; } = "stable";

        public string GetTrendSummary()
        {
            var direction = TrendDirection switch
            {
                "up" => "📈 Crescimento",
                "down" => "📉 Declínio",
                _ => "➡️ Estável"
            };

            return $"{direction}: Tickets {TotalTicketsChange:+0.0;-0.0}%, " +
                   $"Atendimento {AttendanceRateChange:+0.0;-0.0}%, " +
                   $"Resolução {ResolutionRateChange:+0.0;-0.0}%";
        }
    }

    /// <summary>
    /// Estatísticas detalhadas por período
    /// </summary>
    public class PeriodStatistics
    {
        public string PeriodName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalTickets { get; set; }
        public int AttendedTickets { get; set; }
        public int ResolvedTickets { get; set; }
        public double AttendanceRate { get; set; }
        public double ResolutionRate { get; set; }
        public double AverageAttendanceTime { get; set; }
    }

    /// <summary>
    /// Estatísticas por técnico
    /// </summary>
    public class TechnicianPerformance
    {
        public string TechnicianId { get; set; }
        public string TechnicianName { get; set; }
        public int TotalTicketsAttended { get; set; }
        public int ResolvedTickets { get; set; }
        public int UnresolvedTickets { get; set; }
        public double ResolutionRate { get; set; }
        public double AverageAttendanceTime { get; set; }
        public int HardwareTickets { get; set; }
        public int SoftwareTickets { get; set; }
        public string PerformanceRating { get; set; }

        public void CalculatePerformanceRating()
        {
            var score = 0;

            // Critérios de avaliação
            if (ResolutionRate >= 90) score += 3;
            else if (ResolutionRate >= 80) score += 2;
            else if (ResolutionRate >= 70) score += 1;

            if (AverageAttendanceTime <= 24) score += 3; // Menos de 1 dia
            else if (AverageAttendanceTime <= 48) score += 2; // Menos de 2 dias
            else if (AverageAttendanceTime <= 72) score += 1; // Menos de 3 dias

            if (TotalTicketsAttended >= 50) score += 2;
            else if (TotalTicketsAttended >= 20) score += 1;

            PerformanceRating = score switch
            {
                >= 7 => "⭐ Excelente",
                >= 5 => "👍 Bom",
                >= 3 => "👌 Regular",
                _ => "⚠️ Necessita Melhoria"
            };
        }
    }

    /// <summary>
    /// Interface para o serviço de estatísticas
    /// Compatível com o DAL existente
    /// </summary>
    public interface IStatisticsService
    {
        /// <summary>
        /// Gera estatísticas para o dashboard num período específico
        /// </summary>
        Task<DashboardStatistics> GenerateDashboardStatisticsAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Gera estatísticas com comparação de tendências
        /// </summary>
        Task<DashboardStatistics> GenerateStatisticsWithTrendAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Calcula a percentagem de tickets atendidos num período
        /// </summary>
        Task<double> GetAttendedTicketsPercentageAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Calcula as percentagens de tickets resolvidos e não resolvidos
        /// </summary>
        Task<(double resolved, double unresolved)> GetResolutionPercentagesAsync();

        /// <summary>
        /// Calcula o tempo médio de atendimento por tipo de ticket
        /// </summary>
        Task<double> GetAverageAttendanceTimeAsync(DAL.Models.TipoTicket type);

        /// <summary>
        /// Obtém o número de tickets por técnico
        /// </summary>
        Task<Dictionary<string, int>> GetTicketsByTechnicianAsync();

        /// <summary>
        /// Obtém a distribuição de tickets por estado
        /// </summary>
        Task<Dictionary<string, int>> GetTicketsByStatusAsync();

        /// <summary>
        /// Obtém performance detalhada dos técnicos
        /// </summary>
        Task<List<TechnicianPerformance>> GetTechnicianPerformanceAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Obtém estatísticas por períodos (mensal, semanal, etc.)
        /// </summary>
        Task<List<PeriodStatistics>> GetPeriodStatisticsAsync(DateTime startDate, DateTime endDate, string periodType = "monthly");

        /// <summary>
        /// Obtém métricas de produtividade usando o DAL
        /// </summary>
        Task<ProductivityMetrics> GetProductivityMetricsAsync(DateTime startDate, DateTime endDate);
    }

    /// <summary>
    /// Métricas de produtividade
    /// </summary>
    public class ProductivityMetrics
    {
        public double TicketsPerDay { get; set; }
        public double TicketsPerTechnician { get; set; }
        public double TicketsPerHour { get; set; }
        public double PeakHour { get; set; }
        public double PeakDay { get; set; }
        public Dictionary<int, int> HourlyDistribution { get; set; }
        public Dictionary<DayOfWeek, int> DailyDistribution { get; set; }

        public ProductivityMetrics()
        {
            HourlyDistribution = new Dictionary<int, int>();
            DailyDistribution = new Dictionary<DayOfWeek, int>();
        }
    }

    /// <summary>
    /// Implementação do serviço de estatísticas
    /// Compatível com o seu DAL existente
    /// </summary>
    public class StatisticsService : IStatisticsService
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IUserRepository _userRepository;

        /// <summary>
        /// Construtor do serviço
        /// </summary>
        public StatisticsService(ITicketRepository ticketRepository, IUserRepository userRepository)
        {
            _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        /// <summary>
        /// Gera estatísticas completas para o dashboard usando os métodos do seu DAL
        /// </summary>
        public async Task<DashboardStatistics> GenerateDashboardStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            return await Task.Run(() =>
            {
                if (startDate > endDate)
                    throw new ArgumentException("Data de início não pode ser posterior à data de fim");

                var statistics = new DashboardStatistics
                {
                    AnalysisPeriod = new DateRange(startDate, endDate)
                };

                // Usar métodos do DAL para obter estatísticas
                var hwStats = _ticketRepository.GetTicketStatistics(DAL.Models.TipoTicket.Hardware, startDate, endDate);
                var swStats = _ticketRepository.GetTicketStatistics(DAL.Models.TipoTicket.Software, startDate, endDate);

                // Extrair dados das estatísticas do DAL
                statistics.HardwareTickets = Convert.ToInt32(hwStats["TotalTickets"]);
                statistics.SoftwareTickets = Convert.ToInt32(swStats["TotalTickets"]);
                statistics.TotalTickets = statistics.HardwareTickets + statistics.SoftwareTickets;

                var hwAttended = Convert.ToInt32(hwStats["TicketsAtendidos"]);
                var swAttended = Convert.ToInt32(swStats["TicketsAtendidos"]);
                statistics.AttendedTickets = hwAttended + swAttended;

                var hwResolved = Convert.ToInt32(hwStats["TicketsResolvidos"]);
                var swResolved = Convert.ToInt32(swStats["TicketsResolvidos"]);
                statistics.ResolvedTickets = hwResolved + swResolved;

                var hwUnresolved = Convert.ToInt32(hwStats["TicketsNaoResolvidos"]);
                var swUnresolved = Convert.ToInt32(swStats["TicketsNaoResolvidos"]);
                statistics.UnresolvedTickets = hwUnresolved + swUnresolved;

                // Calcular percentagens
                if (statistics.TotalTickets > 0)
                {
                    statistics.AttendedTicketsPercentage = (double)statistics.AttendedTickets / statistics.TotalTickets * 100;
                    statistics.DailyProductivity = (double)statistics.TotalTickets / statistics.AnalysisPeriod.TotalDays;
                }

                if (statistics.AttendedTickets > 0)
                {
                    statistics.ResolvedTicketsPercentage = (double)statistics.ResolvedTickets / statistics.AttendedTickets * 100;
                    statistics.UnresolvedTicketsPercentage = (double)statistics.UnresolvedTickets / statistics.AttendedTickets * 100;
                    statistics.ResolutionRate = statistics.ResolvedTicketsPercentage;
                }

                // Tempos médios de atendimento do DAL
                statistics.AverageHardwareAttendanceTime = Convert.ToDouble(hwStats["TempoMedioAtendimento"]);
                statistics.AverageSoftwareAttendanceTime = Convert.ToDouble(swStats["TempoMedioAtendimento"]);

                // Calcular tempo médio geral ponderado
                var totalAttendedTime = (statistics.AverageHardwareAttendanceTime * hwAttended) +
                                       (statistics.AverageSoftwareAttendanceTime * swAttended);
                statistics.AverageAttendanceTime = statistics.AttendedTickets > 0 ?
                    totalAttendedTime / statistics.AttendedTickets : 0;

                // Distribuições usando dados do DAL
                statistics.TicketsByTechnician = GenerateTicketsByTechnicianFromDAL(startDate, endDate);
                statistics.TicketsByStatus = GenerateTicketsByStatusFromDAL();

                return statistics;
            });
        }

        /// <summary>
        /// Gera estatísticas com análise de tendência
        /// </summary>
        public async Task<DashboardStatistics> GenerateStatisticsWithTrendAsync(DateTime startDate, DateTime endDate)
        {
            var currentStats = await GenerateDashboardStatisticsAsync(startDate, endDate);

            // Calcular período anterior para comparação
            var periodDuration = endDate - startDate;
            var previousStartDate = startDate - periodDuration;
            var previousEndDate = startDate.AddDays(-1);

            if (previousStartDate < DateTime.MinValue.AddDays(periodDuration.Days))
            {
                // Se não há dados suficientes para comparação, retorna sem tendência
                return currentStats;
            }

            var previousStats = await GenerateDashboardStatisticsAsync(previousStartDate, previousEndDate);

            // Calcular mudanças
            currentStats.Trend = new TrendAnalysis
            {
                TotalTicketsChange = CalculatePercentageChange(previousStats.TotalTickets, currentStats.TotalTickets),
                AttendanceRateChange = currentStats.AttendedTicketsPercentage - previousStats.AttendedTicketsPercentage,
                ResolutionRateChange = currentStats.ResolutionRate - previousStats.ResolutionRate,
                ProductivityChange = CalculatePercentageChange(previousStats.DailyProductivity, currentStats.DailyProductivity)
            };

            // Determinar direção geral da tendência
            var positiveChanges = 0;
            var negativeChanges = 0;

            if (currentStats.Trend.TotalTicketsChange > 5) positiveChanges++;
            else if (currentStats.Trend.TotalTicketsChange < -5) negativeChanges++;

            if (currentStats.Trend.AttendanceRateChange > 2) positiveChanges++;
            else if (currentStats.Trend.AttendanceRateChange < -2) negativeChanges++;

            if (currentStats.Trend.ResolutionRateChange > 2) positiveChanges++;
            else if (currentStats.Trend.ResolutionRateChange < -2) negativeChanges++;

            currentStats.Trend.TrendDirection = positiveChanges > negativeChanges ? "up" :
                                               negativeChanges > positiveChanges ? "down" : "stable";

            return currentStats;
        }

        /// <summary>
        /// Calcula a percentagem de tickets atendidos num período usando o DAL
        /// </summary>
        public async Task<double> GetAttendedTicketsPercentageAsync(DateTime startDate, DateTime endDate)
        {
            return await Task.Run(() =>
            {
                var hwStats = _ticketRepository.GetTicketStatistics(DAL.Models.TipoTicket.Hardware, startDate, endDate);
                var swStats = _ticketRepository.GetTicketStatistics(DAL.Models.TipoTicket.Software, startDate, endDate);

                var totalTickets = Convert.ToInt32(hwStats["TotalTickets"]) + Convert.ToInt32(swStats["TotalTickets"]);
                var attendedTickets = Convert.ToInt32(hwStats["TicketsAtendidos"]) + Convert.ToInt32(swStats["TicketsAtendidos"]);

                return totalTickets > 0 ? (double)attendedTickets / totalTickets * 100 : 0;
            });
        }

        /// <summary>
        /// Calcula as percentagens de resolução usando o DAL
        /// </summary>
        public async Task<(double resolved, double unresolved)> GetResolutionPercentagesAsync()
        {
            return await Task.Run(() =>
            {
                // Obter todos os tickets atendidos
                var attendedTickets = _ticketRepository.GetTicketsByEstado(DAL.Models.EstadoTicket.Atendido);
                var attendedList = attendedTickets.ToList();

                if (!attendedList.Any())
                    return (0, 0);

                var resolvedCount = attendedList.Count(t => t.EstadoAtendimento == DAL.Models.EstadoAtendimento.Resolvido);
                var unresolvedCount = attendedList.Count(t => t.EstadoAtendimento == DAL.Models.EstadoAtendimento.NaoResolvido);

                var resolvedPercentage = (double)resolvedCount / attendedList.Count * 100;
                var unresolvedPercentage = (double)unresolvedCount / attendedList.Count * 100;

                return (resolvedPercentage, unresolvedPercentage);
            });
        }

        /// <summary>
        /// Calcula o tempo médio de atendimento por tipo usando o DAL
        /// </summary>
        public async Task<double> GetAverageAttendanceTimeAsync(DAL.Models.TipoTicket type)
        {
            return await Task.Run(() =>
            {
                var now = DateTime.Now;
                var startDate = now.AddMonths(-6); // Últimos 6 meses

                var stats = _ticketRepository.GetTicketStatistics(type, startDate, now);
                return Convert.ToDouble(stats["TempoMedioAtendimento"]);
            });
        }

        /// <summary>
        /// Obtém distribuição de tickets por técnico usando o DAL
        /// </summary>
        public async Task<Dictionary<string, int>> GetTicketsByTechnicianAsync()
        {
            return await Task.Run(() => GenerateTicketsByTechnicianFromDAL(DateTime.Now.AddMonths(-1), DateTime.Now));
        }

        /// <summary>
        /// Obtém distribuição de tickets por estado usando o DAL
        /// </summary>
        public async Task<Dictionary<string, int>> GetTicketsByStatusAsync()
        {
            return await Task.Run(() => GenerateTicketsByStatusFromDAL());
        }

        /// <summary>
        /// Obtém performance detalhada dos técnicos usando o DAL
        /// </summary>
        public async Task<List<TechnicianPerformance>> GetTechnicianPerformanceAsync(DateTime startDate, DateTime endDate)
        {
            return await Task.Run(() =>
            {
                var tickets = _ticketRepository.GetTicketsByDateRange(startDate, endDate);
                var attendedTickets = tickets.Where(t => !string.IsNullOrEmpty(t.TecnicoId)).ToList();

                var technicianGroups = attendedTickets.GroupBy(t => t.TecnicoId);
                var performances = new List<TechnicianPerformance>();

                foreach (var group in technicianGroups)
                {
                    var technician = _userRepository.GetUserById(group.Key);
                    var techTickets = group.ToList();

                    var performance = new TechnicianPerformance
                    {
                        TechnicianId = group.Key,
                        TechnicianName = technician?.Nome ?? $"Técnico #{group.Key}",
                        TotalTicketsAttended = techTickets.Count,
                        ResolvedTickets = techTickets.Count(t => t.EstadoAtendimento == DAL.Models.EstadoAtendimento.Resolvido),
                        UnresolvedTickets = techTickets.Count(t => t.EstadoAtendimento == DAL.Models.EstadoAtendimento.NaoResolvido),
                        HardwareTickets = techTickets.Count(t => t.TipoTicket == DAL.Models.TipoTicket.Hardware),
                        SoftwareTickets = techTickets.Count(t => t.TipoTicket == DAL.Models.TipoTicket.Software),
                        //AverageAttendanceTime = CalculateAverageAttendanceTime(techTickets)
                    };

                    performance.ResolutionRate = performance.TotalTicketsAttended > 0 ?
                        (double)performance.ResolvedTickets / performance.TotalTicketsAttended * 100 : 0;

                    performance.CalculatePerformanceRating();
                    performances.Add(performance);
                }

                return performances.OrderByDescending(p => p.ResolutionRate).ToList();
            });
        }

        /// <summary>
        /// Obtém estatísticas por períodos usando o DAL
        /// </summary>
        public async Task<List<PeriodStatistics>> GetPeriodStatisticsAsync(DateTime startDate, DateTime endDate, string periodType = "monthly")
        {
            return await Task.Run(() =>
            {
                var periods = new List<PeriodStatistics>();
                var currentDate = startDate;

                while (currentDate <= endDate)
                {
                    DateTime periodEnd;
                    string periodName;

                    switch (periodType.ToLower())
                    {
                        case "weekly":
                            periodEnd = currentDate.AddDays(6);
                            periodName = $"Semana {GetWeekOfYear(currentDate)}/{currentDate.Year}";
                            break;
                        case "daily":
                            periodEnd = currentDate;
                            periodName = currentDate.ToString("dd/MM/yyyy");
                            break;
                        default: // monthly
                            periodEnd = new DateTime(currentDate.Year, currentDate.Month, DateTime.DaysInMonth(currentDate.Year, currentDate.Month));
                            periodName = currentDate.ToString("MMM/yyyy");
                            break;
                    }

                    if (periodEnd > endDate) periodEnd = endDate;

                    // Usar o DAL para obter estatísticas do período
                    var hwStats = _ticketRepository.GetTicketStatistics(DAL.Models.TipoTicket.Hardware, currentDate, periodEnd);
                    var swStats = _ticketRepository.GetTicketStatistics(DAL.Models.TipoTicket.Software, currentDate, periodEnd);

                    var totalTickets = Convert.ToInt32(hwStats["TotalTickets"]) + Convert.ToInt32(swStats["TotalTickets"]);
                    var attendedTickets = Convert.ToInt32(hwStats["TicketsAtendidos"]) + Convert.ToInt32(swStats["TicketsAtendidos"]);
                    var resolvedTickets = Convert.ToInt32(hwStats["TicketsResolvidos"]) + Convert.ToInt32(swStats["TicketsResolvidos"]);

                    var avgHwTime = Convert.ToDouble(hwStats["TempoMedioAtendimento"]);
                    var avgSwTime = Convert.ToDouble(swStats["TempoMedioAtendimento"]);
                    var hwAttended = Convert.ToInt32(hwStats["TicketsAtendidos"]);
                    var swAttended = Convert.ToInt32(swStats["TicketsAtendidos"]);

                    var avgTime = attendedTickets > 0 ?
                        ((avgHwTime * hwAttended) + (avgSwTime * swAttended)) / attendedTickets : 0;

                    var periodStat = new PeriodStatistics
                    {
                        PeriodName = periodName,
                        StartDate = currentDate,
                        EndDate = periodEnd,
                        TotalTickets = totalTickets,
                        AttendedTickets = attendedTickets,
                        ResolvedTickets = resolvedTickets,
                        AttendanceRate = totalTickets > 0 ? (double)attendedTickets / totalTickets * 100 : 0,
                        ResolutionRate = attendedTickets > 0 ? (double)resolvedTickets / attendedTickets * 100 : 0,
                        AverageAttendanceTime = avgTime
                    };

                    periods.Add(periodStat);

                    // Avançar para o próximo período
                    currentDate = periodType.ToLower() switch
                    {
                        "weekly" => currentDate.AddDays(7),
                        "daily" => currentDate.AddDays(1),
                        _ => currentDate.AddMonths(1).AddDays(-(currentDate.Day - 1)) // Primeiro dia do próximo mês
                    };
                }

                return periods;
            });
        }

        /// <summary>
        /// Obtém métricas de produtividade usando o DAL
        /// </summary>
        public async Task<ProductivityMetrics> GetProductivityMetricsAsync(DateTime startDate, DateTime endDate)
        {
            return await Task.Run(() =>
            {
                var tickets = _ticketRepository.GetTicketsByDateRange(startDate, endDate);
                var ticketsList = tickets.ToList();
                var totalDays = (endDate - startDate).Days + 1;

                var technicians = _userRepository.GetUsersByTipo(DAL.Models.TipoUtilizador.Tecnico);
                var technicianCount = technicians.Count();

                var metrics = new ProductivityMetrics
                {
                    TicketsPerDay = totalDays > 0 ? (double)ticketsList.Count / totalDays : 0,
                    TicketsPerTechnician = technicianCount > 0 ? (double)ticketsList.Count / technicianCount : 0,
                    TicketsPerHour = totalDays > 0 ? (double)ticketsList.Count / (totalDays * 24) : 0
                };

                // Distribuição por hora
                var hourlyGroups = ticketsList.GroupBy(t => t.DataCriacao.Hour);
                foreach (var group in hourlyGroups)
                {
                    metrics.HourlyDistribution[group.Key] = group.Count();
                }

                // Distribuição por dia da semana
                var dailyGroups = ticketsList.GroupBy(t => t.DataCriacao.DayOfWeek);
                foreach (var group in dailyGroups)
                {
                    metrics.DailyDistribution[group.Key] = group.Count();
                }

                // Encontrar horário e dia de pico
                if (metrics.HourlyDistribution.Any())
                {
                    metrics.PeakHour = metrics.HourlyDistribution.OrderByDescending(kv => kv.Value).First().Key;
                }

                if (metrics.DailyDistribution.Any())
                {
                    var peakDayOfWeek = metrics.DailyDistribution.OrderByDescending(kv => kv.Value).First().Key;
                    metrics.PeakDay = (int)peakDayOfWeek;
                }

                return metrics;
            });
        }

        #region Métodos Auxiliares Privados

        /// <summary>
        /// Gera dicionário de tickets por técnico usando métodos do DAL
        /// </summary>
        private Dictionary<string, int> GenerateTicketsByTechnicianFromDAL(DateTime startDate, DateTime endDate)
        {
            var result = new Dictionary<string, int>();

            // Obter tickets do período
            var tickets = _ticketRepository.GetTicketsByDateRange(startDate, endDate);
            var attendedTickets = tickets.Where(t => !string.IsNullOrEmpty(t.TecnicoId)).ToList();

            var technicianGroups = attendedTickets.GroupBy(t => t.TecnicoId);

            foreach (var group in technicianGroups)
            {
                var technician = _userRepository.GetUserById(group.Key);
                var techName = technician?.Nome ?? $"Técnico #{group.Key}";
                var ticketCount = group.Count();

                result[techName] = ticketCount;
            }

            return result.OrderByDescending(kvp => kvp.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        /// <summary>
        /// Gera dicionário de tickets por estado usando métodos do DAL
        /// </summary>
        private Dictionary<string, int> GenerateTicketsByStatusFromDAL()
        {
            return new Dictionary<string, int>
            {
                ["Por Atender"] = _ticketRepository.GetTicketsByEstado(DAL.Models.EstadoTicket.PorAtender).Count(),
                ["Em Atendimento"] = _ticketRepository.GetTicketsByEstado(DAL.Models.EstadoTicket.EmAtendimento).Count(),
                ["Atendido"] = _ticketRepository.GetTicketsByEstado(DAL.Models.EstadoTicket.Atendido).Count()
            };
        }

        /// <summary>
        /// Calcula tempo médio de atendimento para uma lista de tickets
        /// </summary>
        private double CalculateAverageAttendanceTime(List<Ticket> tickets)
        {
            var attendedTickets = tickets.Where(t =>
                t.EstadoTicket == EstadoTicket.Atendido &&
                t.DataAtendimento.HasValue).ToList();

            if (!attendedTickets.Any())
                return 0;

            var totalHours = attendedTickets.Sum(t =>
                (t.DataAtendimento.Value - t.DataCriacao).TotalHours);

            return totalHours / attendedTickets.Count;
        }

        /// <summary>
        /// Calcula mudança percentual entre dois valores
        /// </summary>
        private double CalculatePercentageChange(double oldValue, double newValue)
        {
            if (oldValue == 0)
                return newValue > 0 ? 100 : 0;

            return ((newValue - oldValue) / oldValue) * 100;
        }

        /// <summary>
        /// Obtém o número da semana no ano
        /// </summary>
        private int GetWeekOfYear(DateTime date)
        {
            var jan1 = new DateTime(date.Year, 1, 1);
            var daysOffset = DayOfWeek.Thursday - jan1.DayOfWeek;

            var firstThursday = jan1.AddDays(daysOffset);
            var cal = System.Globalization.CultureInfo.CurrentCulture.Calendar;
            var firstWeek = cal.GetWeekOfYear(firstThursday, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            var weekNum = cal.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            if (firstWeek <= 1)
            {
                weekNum -= 1;
            }

            if (weekNum < 1)
            {
                weekNum = GetWeekOfYear(date.AddYears(-1).AddDays(-10));
            }

            return weekNum;
        }

        #endregion
    }

    /// <summary>
    /// Classe para gerar relatórios detalhados usando o DAL
    /// </summary>
    public class ReportGenerator
    {
        private readonly IStatisticsService _statisticsService;
        private readonly ITicketRepository _ticketRepository;
        private readonly IUserRepository _userRepository;

        public ReportGenerator(IStatisticsService statisticsService, ITicketRepository ticketRepository, IUserRepository userRepository)
        {
            _statisticsService = statisticsService ?? throw new ArgumentNullException(nameof(statisticsService));
            _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        /// <summary>
        /// Gera um relatório completo em texto usando dados do DAL
        /// </summary>
        public async Task<string> GenerateCompleteTextReportAsync(DateTime startDate, DateTime endDate)
        {
            var statistics = await _statisticsService.GenerateStatisticsWithTrendAsync(startDate, endDate);
            var technicianPerf = await _statisticsService.GetTechnicianPerformanceAsync(startDate, endDate);
            var productivity = await _statisticsService.GetProductivityMetricsAsync(startDate, endDate);

            var report = $@"
╔══════════════════════════════════════════════════════════════════════════════╗
║                           RELATÓRIO COMPLETO DE TICKETS                      ║
╚══════════════════════════════════════════════════════════════════════════════╝

{statistics.GetSummary()}

{statistics.Trend.GetTrendSummary()}

📊 PRODUTIVIDADE:
   • Tickets por dia: {productivity.TicketsPerDay:F1}
   • Tickets por técnico: {productivity.TicketsPerTechnician:F1}
   • Horário de pico: {productivity.PeakHour:00}h
   • Dia de pico: {(DayOfWeek)productivity.PeakDay}

👥 PERFORMANCE DOS TÉCNICOS:
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━";

            foreach (var tech in technicianPerf.Take(10))
            {
                report += $@"
   {tech.PerformanceRating} {tech.TechnicianName}
      • Tickets: {tech.TotalTicketsAttended} | Resolvidos: {tech.ResolvedTickets} ({tech.ResolutionRate:F1}%)
      • Tempo médio: {tech.AverageAttendanceTime:F1}h | HW: {tech.HardwareTickets} | SW: {tech.SoftwareTickets}";
            }

            report += $@"

📈 DISTRIBUIÇÃO POR ESTADO:";
            foreach (var status in statistics.TicketsByStatus)
            {
                var percentage = statistics.TotalTickets > 0 ? (double)status.Value / statistics.TotalTickets * 100 : 0;
                report += $@"
   • {status.Key}: {status.Value} tickets ({percentage:F1}%)";
            }

            report += $@"

⏰ DISTRIBUIÇÃO HORÁRIA (TOP 5):";
            var topHours = productivity.HourlyDistribution
                .OrderByDescending(kv => kv.Value)
                .Take(5);

            foreach (var hour in topHours)
            {
                report += $@"
   • {hour.Key:00}h: {hour.Value} tickets";
            }

            report += $@"

📅 DISTRIBUIÇÃO SEMANAL:";
            foreach (var day in productivity.DailyDistribution.OrderBy(kv => (int)kv.Key))
            {
                var dayName = day.Key switch
                {
                    DayOfWeek.Monday => "Segunda",
                    DayOfWeek.Tuesday => "Terça",
                    DayOfWeek.Wednesday => "Quarta",
                    DayOfWeek.Thursday => "Quinta",
                    DayOfWeek.Friday => "Sexta",
                    DayOfWeek.Saturday => "Sábado",
                    DayOfWeek.Sunday => "Domingo",
                    _ => day.Key.ToString()
                };
                report += $@"
   • {dayName}: {day.Value} tickets";
            }

            // Adicionar seção de tickets urgentes usando o DAL
            var urgentTickets = GetUrgentTicketsFromDAL();
            if (urgentTickets.Any())
            {
                report += $@"

🚨 TICKETS URGENTES PENDENTES ({urgentTickets.Count}):";
                foreach (var ticket in urgentTickets.Take(5))
                {
                    var daysOld = (DateTime.Now - ticket.DataCriacao).Days;
                    report += $@"
   • #{ticket.TicketId} - {ticket.TipoTicket} ({daysOld} dias) - {GetTicketDescription(ticket)}";
                }
            }

            report += $@"

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Relatório gerado em: {DateTime.Now:dd/MM/yyyy HH:mm:ss}
Base de dados: {_ticketRepository.GetType().Name}
";

            return report;
        }

        /// <summary>
        /// Gera relatório executivo resumido usando o DAL
        /// </summary>
        public async Task<string> GenerateExecutiveSummaryAsync(DateTime startDate, DateTime endDate)
        {
            var statistics = await _statisticsService.GenerateStatisticsWithTrendAsync(startDate, endDate);
            var topTechnicians = await _statisticsService.GetTechnicianPerformanceAsync(startDate, endDate);

            var report = $@"
╔══════════════════════════════════════════════════════════════════════════════╗
║                              RESUMO EXECUTIVO                                ║
╚══════════════════════════════════════════════════════════════════════════════╝

📊 INDICADORES PRINCIPAIS ({statistics.AnalysisPeriod.StartDate:dd/MM} - {statistics.AnalysisPeriod.EndDate:dd/MM}):

🎯 PERFORMANCE GERAL:
   • Taxa de Atendimento: {statistics.AttendedTicketsPercentage:F1}% ({statistics.AttendedTickets}/{statistics.TotalTickets})
   • Taxa de Resolução: {statistics.ResolutionRate:F1}% ({statistics.ResolvedTickets}/{statistics.AttendedTickets})
   • Produtividade: {statistics.DailyProductivity:F1} tickets/dia

⚡ TEMPOS DE RESPOSTA:
   • Tempo médio geral: {statistics.AverageAttendanceTime:F1} horas
   • Hardware: {statistics.AverageHardwareAttendanceTime:F1}h | Software: {statistics.AverageSoftwareAttendanceTime:F1}h

📈 TENDÊNCIA: {statistics.Trend.GetTrendSummary()}

🏆 TOP 3 TÉCNICOS:";

            foreach (var tech in topTechnicians.Take(3))
            {
                report += $@"
   {tech.PerformanceRating} {tech.TechnicianName} - {tech.TotalTicketsAttended} tickets ({tech.ResolutionRate:F1}% resolução)";
            }

            var needsAttention = topTechnicians.Where(t => t.ResolutionRate < 70 || t.AverageAttendanceTime > 48).ToList();
            if (needsAttention.Any())
            {
                report += $@"

⚠️  NECESSITA ATENÇÃO:";
                foreach (var tech in needsAttention.Take(3))
                {
                    report += $@"
   • {tech.TechnicianName}: {tech.ResolutionRate:F1}% resolução, {tech.AverageAttendanceTime:F1}h tempo médio";
                }
            }

            // Adicionar alertas específicos usando dados do DAL
            var pendingCount = _ticketRepository.GetTicketsByEstado(DAL.Models.EstadoTicket.PorAtender).Count();
            var oldTickets = _ticketRepository.GetTicketsByEstado(DAL.Models.EstadoTicket.PorAtender)
                .Where(t => (DateTime.Now - t.DataCriacao).Days > 7).Count();

            report += $@"

💡 RECOMENDAÇÕES:";

            if (statistics.AttendedTicketsPercentage < 80)
            {
                report += @"
   • Aumentar capacidade de atendimento - taxa abaixo de 80%";
            }

            if (statistics.ResolutionRate < 85)
            {
                report += @"
   • Melhorar qualidade do atendimento - taxa de resolução abaixo de 85%";
            }

            if (statistics.AverageAttendanceTime > 48)
            {
                report += @"
   • Otimizar processos - tempo médio acima de 48h";
            }

            if (oldTickets > 0)
            {
                report += $@"
   • Priorizar {oldTickets} tickets pendentes há mais de 7 dias";
            }

            if (pendingCount > 50)
            {
                report += $@"
   • Analisar capacidade da equipe - {pendingCount} tickets pendentes";
            }

            if (statistics.Trend.TrendDirection == "down")
            {
                report += @"
   • Investigar causas do declínio na performance";
            }

            return report;
        }

        /// <summary>
        /// Gera relatório mensal automaticamente
        /// </summary>
        public async Task<string> GenerateCurrentMonthReportAsync()
        {
            var now = DateTime.Now;
            var startDate = new DateTime(now.Year, now.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            return await GenerateCompleteTextReportAsync(startDate, endDate);
        }

        /// <summary>
        /// Gera relatório dos últimos 30 dias
        /// </summary>
        public async Task<string> GenerateLast30DaysReportAsync()
        {
            var endDate = DateTime.Now;
            var startDate = endDate.AddDays(-30);

            return await GenerateCompleteTextReportAsync(startDate, endDate);
        }

        /// <summary>
        /// Gera relatório semanal
        /// </summary>
        public async Task<string> GenerateWeeklyReportAsync()
        {
            var today = DateTime.Now.Date;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek + 1); // Segunda-feira
            var endOfWeek = startOfWeek.AddDays(6); // Domingo

            return await GenerateCompleteTextReportAsync(startOfWeek, endOfWeek);
        }

        /// <summary>
        /// Obtém tickets urgentes usando métodos do DAL
        /// </summary>
        private List<Ticket> GetUrgentTicketsFromDAL()
        {
            var pendingTickets = _ticketRepository.GetTicketsByEstado(DAL.Models.EstadoTicket.PorAtender);
            var urgentTickets = new List<Ticket>();

            /*
            foreach (var ticket in pendingTickets)
            {
                if (IsTicketUrgent(ticket))
                {
                    urgentTickets.Add(ticket);
                }
            }
            */

            return urgentTickets.OrderBy(t => t.DataCriacao).ToList();
        }

        /// <summary>
        /// Determina se um ticket é urgente
        /// </summary>
        private bool IsTicketUrgent(Ticket ticket)
        {
            return ticket switch
            {
                HardwareTicket hwTicket => hwTicket.IsUrgente(),
                SoftwareTicket swTicket => swTicket.IsUrgente(),
                _ => false
            };
        }

        /// <summary>
        /// Obtém descrição resumida do ticket
        /// </summary>
        private string GetTicketDescription(Ticket ticket)
        {
            return ticket switch
            {
                HardwareTicket hwTicket => $"{hwTicket.Equipamento}: {hwTicket.Avaria}".Substring(0, Math.Min(50, $"{hwTicket.Equipamento}: {hwTicket.Avaria}".Length)),
                SoftwareTicket swTicket => $"{swTicket.Software}: {swTicket.DescricaoNecessidade}".Substring(0, Math.Min(50, $"{swTicket.Software}: {swTicket.DescricaoNecessidade}".Length)),
                _ => "Descrição não disponível"
            };
        }
    }
}