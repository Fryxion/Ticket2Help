using System;
using System.Collections.Generic;
using System.Linq;
using Ticket2Help.BLL.Models;
using Ticket2Help.DAL.Repositories;

namespace Ticket2Help.BLL
{
    /// <summary>
    /// Classe para cálculos de estatísticas de tickets
    /// Compatível com o DAL existente
    /// </summary>
    public class TicketStatistics
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IUserRepository _userRepository;

        /// <summary>
        /// Construtor
        /// </summary>
        public TicketStatistics(ITicketRepository ticketRepository, IUserRepository userRepository)
        {
            _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        /// <summary>
        /// Calcula estatísticas básicas para um período
        /// </summary>
        public BasicStatistics CalculateBasicStatistics(DateTime startDate, DateTime endDate)
        {
            var tickets = _ticketRepository.GetTicketsByDateRange(startDate, endDate).ToList();

            return new BasicStatistics
            {
                TotalTickets = tickets.Count,
                HardwareTickets = tickets.Count(t => t.TipoTicket == DAL.Models.TipoTicket.Hardware),
                SoftwareTickets = tickets.Count(t => t.TipoTicket == DAL.Models.TipoTicket.Software),
                PendingTickets = tickets.Count(t => t.EstadoTicket == DAL.Models.EstadoTicket.PorAtender),
                InProgressTickets = tickets.Count(t => t.EstadoTicket == DAL.Models.EstadoTicket.EmAtendimento),
                CompletedTickets = tickets.Count(t => t.EstadoTicket == DAL.Models.EstadoTicket.Atendido),
                ResolvedTickets = tickets.Count(t => t.EstadoAtendimento == DAL.Models.EstadoAtendimento.Resolvido),
                UnresolvedTickets = tickets.Count(t => t.EstadoAtendimento == DAL.Models.EstadoAtendimento.NaoResolvido),
                StartDate = startDate,
                EndDate = endDate
            };
        }

        /// <summary>
        /// Calcula estatísticas por técnico
        /// </summary>
        public List<TechnicianStatistics> CalculateTechnicianStatistics(DateTime startDate, DateTime endDate)
        {
            var tickets = _ticketRepository.GetTicketsByDateRange(startDate, endDate)
                .Where(t => !string.IsNullOrEmpty(t.TecnicoId))
                .ToList();

            var technicianGroups = tickets.GroupBy(t => t.TecnicoId);
            var technicianStats = new List<TechnicianStatistics>();

            foreach (var group in technicianGroups)
            {
                var technician = _userRepository.GetUserById(group.Key);
                var techTickets = group.ToList();

                technicianStats.Add(new TechnicianStatistics
                {
                    TechnicianId = group.Key,
                    TechnicianName = technician?.Nome ?? $"Técnico #{group.Key}",
                    TotalTickets = techTickets.Count,
                    HardwareTickets = techTickets.Count(t => t.TipoTicket == DAL.Models.TipoTicket.Hardware),
                    SoftwareTickets = techTickets.Count(t => t.TipoTicket == DAL.Models.TipoTicket.Software),
                    ResolvedTickets = techTickets.Count(t => t.EstadoAtendimento == DAL.Models.EstadoAtendimento.Resolvido),
                    UnresolvedTickets = techTickets.Count(t => t.EstadoAtendimento == DAL.Models.EstadoAtendimento.NaoResolvido),
                    //AverageResolutionTime = CalculateAverageResolutionTime(techTickets)
                });
            }

            return technicianStats.OrderByDescending(ts => ts.TotalTickets).ToList();
        }

        /// <summary>
        /// Calcula tendências mensais
        /// </summary>
        public List<MonthlyTrend> CalculateMonthlyTrends(int months = 6)
        {
            var trends = new List<MonthlyTrend>();
            var endDate = DateTime.Now;

            for (int i = months - 1; i >= 0; i--)
            {
                var monthStart = endDate.AddMonths(-i).Date.AddDays(1 - endDate.AddMonths(-i).Day);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                var monthlyStats = CalculateBasicStatistics(monthStart, monthEnd);

                trends.Add(new MonthlyTrend
                {
                    Month = monthStart.ToString("MMM/yyyy"),
                    Year = monthStart.Year,
                    MonthNumber = monthStart.Month,
                    TotalTickets = monthlyStats.TotalTickets,
                    CompletedTickets = monthlyStats.CompletedTickets,
                    CompletionRate = monthlyStats.TotalTickets > 0 ?
                        (double)monthlyStats.CompletedTickets / monthlyStats.TotalTickets * 100 : 0
                });
            }

            return trends;
        }

        /// <summary>
        /// Calcula tempo médio de resolução em horas
        /// </summary>
        public double CalculateAverageResolutionTime(IEnumerable<Ticket> tickets)
        {
            var resolvedTickets = tickets.Where(t =>
                t.EstadoTicket == EstadoTicket.Atendido &&
                t.DataAtendimento.HasValue).ToList();

            if (!resolvedTickets.Any())
                return 0;

            var totalHours = resolvedTickets.Sum(t =>
                (t.DataAtendimento.Value - t.DataCriacao).TotalHours);

            return totalHours / resolvedTickets.Count;
        }

        /// <summary>
        /// Obtém tickets mais antigos não atendidos
        /// </summary>
        /*
        public List<Ticket> GetOldestPendingTickets(int count = 10)
        {
            return _ticketRepository.GetTicketsByEstado(EstadoTicket.PorAtender)
                .OrderBy(t => t.DataCriacao)
                .Take(count)
                .ToList();
        }
        */

        /// <summary>
        /// Obtém estatísticas de urgência
        /// </summary>
        /*
        public UrgencyStatistics CalculateUrgencyStatistics()
        {
            var allTickets = _ticketRepository.GetAllTickets().ToList();

            var urgentTickets = allTickets.Where(IsUrgentTicket).ToList();
            var pendingUrgent = urgentTickets.Where(t => t.EstadoTicket == EstadoTicket.PorAtender).ToList();

            return new UrgencyStatistics
            {
                TotalUrgentTickets = urgentTickets.Count,
                PendingUrgentTickets = pendingUrgent.Count,
                UrgentTicketsPercentage = allTickets.Count > 0 ?
                    (double)urgentTickets.Count / allTickets.Count * 100 : 0,
                OldestUrgentTicket = pendingUrgent.OrderBy(t => t.DataCriacao).FirstOrDefault()
            };
        }
        */

        /// <summary>
        /// Determina se um ticket é urgente
        /// </summary>
        private bool IsUrgentTicket(Ticket ticket)
        {
            return ticket switch
            {
                HardwareTicket hwTicket => hwTicket.IsUrgente(),
                SoftwareTicket swTicket => swTicket.IsUrgente(),
                _ => false
            };
        }

        /// <summary>
        /// Calcula produtividade por dia da semana
        /// </summary>
        public List<DayOfWeekStatistics> CalculateDayOfWeekStatistics(DateTime startDate, DateTime endDate)
        {
            var tickets = _ticketRepository.GetTicketsByDateRange(startDate, endDate).ToList();

            var dayStats = new List<DayOfWeekStatistics>();

            for (int i = 0; i < 7; i++)
            {
                var dayOfWeek = (DayOfWeek)i;
                var dayTickets = tickets.Where(t => t.DataCriacao.DayOfWeek == dayOfWeek).ToList();

                dayStats.Add(new DayOfWeekStatistics
                {
                    DayOfWeek = dayOfWeek,
                    DayName = dayOfWeek.ToString(),
                    TotalTickets = dayTickets.Count,
                    CompletedTickets = dayTickets.Count(t => t.EstadoTicket == DAL.Models.EstadoTicket.Atendido),
                    AverageTicketsPerDay = CalculateAverageForDay(dayOfWeek, startDate, endDate, dayTickets.Count)
                });
            }

            return dayStats;
        }

        /// <summary>
        /// Calcula média de tickets por dia da semana
        /// </summary>
        private double CalculateAverageForDay(DayOfWeek dayOfWeek, DateTime startDate, DateTime endDate, int totalTickets)
        {
            var totalDays = 0;
            var currentDate = startDate;

            while (currentDate <= endDate)
            {
                if (currentDate.DayOfWeek == dayOfWeek)
                    totalDays++;
                currentDate = currentDate.AddDays(1);
            }

            return totalDays > 0 ? (double)totalTickets / totalDays : 0;
        }
    }

    #region Classes de Estatísticas

    /// <summary>
    /// Estatísticas básicas
    /// </summary>
    public class BasicStatistics
    {
        public int TotalTickets { get; set; }
        public int HardwareTickets { get; set; }
        public int SoftwareTickets { get; set; }
        public int PendingTickets { get; set; }
        public int InProgressTickets { get; set; }
        public int CompletedTickets { get; set; }
        public int ResolvedTickets { get; set; }
        public int UnresolvedTickets { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public double CompletionRate => TotalTickets > 0 ? (double)CompletedTickets / TotalTickets * 100 : 0;
        public double ResolutionRate => CompletedTickets > 0 ? (double)ResolvedTickets / CompletedTickets * 100 : 0;
    }

    /// <summary>
    /// Estatísticas por técnico
    /// </summary>
    public class TechnicianStatistics
    {
        public string TechnicianId { get; set; }
        public string TechnicianName { get; set; }
        public int TotalTickets { get; set; }
        public int HardwareTickets { get; set; }
        public int SoftwareTickets { get; set; }
        public int ResolvedTickets { get; set; }
        public int UnresolvedTickets { get; set; }
        public double AverageResolutionTime { get; set; }

        public double ResolutionRate => TotalTickets > 0 ? (double)ResolvedTickets / TotalTickets * 100 : 0;
    }

    /// <summary>
    /// Tendência mensal
    /// </summary>
    public class MonthlyTrend
    {
        public string Month { get; set; }
        public int Year { get; set; }
        public int MonthNumber { get; set; }
        public int TotalTickets { get; set; }
        public int CompletedTickets { get; set; }
        public double CompletionRate { get; set; }
    }

    /// <summary>
    /// Estatísticas de urgência
    /// </summary>
    public class UrgencyStatistics
    {
        public int TotalUrgentTickets { get; set; }
        public int PendingUrgentTickets { get; set; }
        public double UrgentTicketsPercentage { get; set; }
        public Ticket OldestUrgentTicket { get; set; }
    }

    /// <summary>
    /// Estatísticas por dia da semana
    /// </summary>
    public class DayOfWeekStatistics
    {
        public DayOfWeek DayOfWeek { get; set; }
        public string DayName { get; set; }
        public int TotalTickets { get; set; }
        public int CompletedTickets { get; set; }
        public double AverageTicketsPerDay { get; set; }
    }

    #endregion
}