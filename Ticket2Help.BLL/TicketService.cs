using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ticket2Help.BLL.Models;
using Ticket2Help.DAL.Repositories;

namespace Ticket2Help.BLL.Services
{
    /// <summary>
    /// Classe para resumo do dashboard
    /// </summary>
    public class DashboardSummary
    {
        public int TotalTickets { get; set; }
        public int TicketsAtendidos { get; set; }
        public int TicketsResolvidos { get; set; }
        public double PercentagemAtendidos { get; set; }
        public double PercentagemResolvidos { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }

        /// <summary>
        /// Obtém descrição resumida
        /// </summary>
        public string GetSummaryDescription()
        {
            return $"Período: {DataInicio:dd/MM/yyyy} - {DataFim:dd/MM/yyyy}\n" +
                   $"Total: {TotalTickets} tickets\n" +
                   $"Atendidos: {TicketsAtendidos} ({PercentagemAtendidos:F1}%)\n" +
                   $"Resolvidos: {TicketsResolvidos} ({PercentagemResolvidos:F1}%)";
        }
    }

    /// <summary>
    /// Interface para o serviço de tickets
    /// Define as operações de negócio relacionadas com tickets
    /// </summary>
    public interface ITicketService
    {
        /// <summary>
        /// Cria um novo ticket de hardware
        /// </summary>
        Task<int> CreateHardwareTicketAsync(string colaboradorId, string equipamento, string avaria);

        /// <summary>
        /// Cria um novo ticket de software
        /// </summary>
        Task<int> CreateSoftwareTicketAsync(string colaboradorId, string software, string descricaoNecessidade);

        /// <summary>
        /// Obtém um ticket pelo ID
        /// </summary>
        Task<Ticket> GetTicketByIdAsync(int ticketId);

        /// <summary>
        /// Obtém todos os tickets de um colaborador
        /// </summary>
        Task<IEnumerable<Ticket>> GetUserTicketsAsync(string colaboradorId);

        /// <summary>
        /// Obtém o próximo ticket para atendimento
        /// </summary>
        Task<Ticket> GetNextTicketForAttendanceAsync();

        /// <summary>
        /// Atende um ticket
        /// </summary>
        Task<bool> AttendTicketAsync(int ticketId, string technicianId);

        /// <summary>
        /// Completa o atendimento de um ticket de hardware
        /// </summary>
        Task<bool> CompleteHardwareTicketAsync(int ticketId, EstadoAtendimento status, string descricaoReparacao, string pecas = null);

        /// <summary>
        /// Completa o atendimento de um ticket de software
        /// </summary>
        Task<bool> CompleteSoftwareTicketAsync(int ticketId, EstadoAtendimento status, string descricaoIntervencao);

        /// <summary>
        /// Obtém todos os tickets disponíveis para atendimento
        /// </summary>
        Task<IEnumerable<Ticket>> GetPendingTicketsAsync();

        /// <summary>
        /// Obtém estatísticas de tickets
        /// </summary>
        Task<Dictionary<string, object>> GetTicketStatisticsAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Obtém estatísticas resumidas para dashboard
        /// </summary>
        Task<DashboardSummary> GetDashboardSummaryAsync(DateTime startDate, DateTime endDate);
    }

    /// <summary>
    /// Implementação do serviço de tickets
    /// Contém a lógica de negócio para gestão de tickets
    /// </summary>
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IUserRepository _userRepository;

        /// <summary>
        /// Construtor do serviço
        /// </summary>
        public TicketService(ITicketRepository ticketRepository, IUserRepository userRepository)
        {
            _ticketRepository = ticketRepository;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Cria um novo ticket de hardware
        /// </summary>
        public async Task<int> CreateHardwareTicketAsync(string colaboradorId, string equipamento, string avaria)
        {
            return await Task.Run(() =>
            {
                // Validar utilizador usando DAL
                var dalUser = _userRepository.GetUserById(colaboradorId);
                if (dalUser == null || !dalUser.Ativo)
                    throw new InvalidOperationException("Utilizador não pode criar tickets");

                // Criar ticket DAL
                var dalTicket = new DAL.Models.HardwareTicket(colaboradorId, equipamento, avaria);

                return _ticketRepository.InsertTicket(dalTicket);
            });
        }

        /// <summary>
        /// Cria um novo ticket de software
        /// </summary>
        public async Task<int> CreateSoftwareTicketAsync(string colaboradorId, string software, string descricaoNecessidade)
        {
            return await Task.Run(() =>
            {
                // Validar utilizador usando DAL
                var dalUser = _userRepository.GetUserById(colaboradorId);
                if (dalUser == null || !dalUser.Ativo)
                    throw new InvalidOperationException("Utilizador não pode criar tickets");

                // Criar ticket DAL
                var dalTicket = new DAL.Models.SoftwareTicket(colaboradorId, software, descricaoNecessidade);

                return _ticketRepository.InsertTicket(dalTicket);
            });
        }

        /// <summary>
        /// Obtém um ticket pelo ID
        /// </summary>
        public async Task<Ticket> GetTicketByIdAsync(int ticketId)
        {
            return await Task.Run(() =>
            {
                if (ticketId <= 0)
                    throw new ArgumentException("ID do ticket inválido", nameof(ticketId));

                var dalTicket = _ticketRepository.GetTicketById(ticketId);
                return ModelMapper.MapToBll(dalTicket);
            });
        }

        /// <summary>
        /// Obtém todos os tickets de um colaborador
        /// </summary>
        public async Task<IEnumerable<Ticket>> GetUserTicketsAsync(string colaboradorId)
        {
            return await Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(colaboradorId))
                    throw new ArgumentException("ID do colaborador inválido", nameof(colaboradorId));

                var dalTickets = _ticketRepository.GetTicketsByColaborador(colaboradorId);
                return dalTickets.Select(ModelMapper.MapToBll).Where(t => t != null).ToList();
            });
        }

        /// <summary>
        /// Obtém o próximo ticket para atendimento (FIFO)
        /// </summary>
        public async Task<Ticket> GetNextTicketForAttendanceAsync()
        {
            return await Task.Run(() =>
            {
                var dalTickets = _ticketRepository.GetTicketsByEstado(DAL.Models.EstadoTicket.PorAtender);
                var oldestTicket = dalTickets.OrderBy(t => t.DataCriacao).FirstOrDefault();
                return ModelMapper.MapToBll(oldestTicket);
            });
        }

        /// <summary>
        /// Atende um ticket
        /// </summary>
        public async Task<bool> AttendTicketAsync(int ticketId, string technicianId)
        {
            return await Task.Run(() =>
            {
                try
                {
                    // Validar técnico usando DAL
                    var dalTechnician = _userRepository.GetUserById(technicianId);
                    if (dalTechnician == null || !dalTechnician.Ativo ||
                        (dalTechnician.TipoUtilizador != DAL.Models.TipoUtilizador.Tecnico &&
                         dalTechnician.TipoUtilizador != DAL.Models.TipoUtilizador.Administrador))
                        throw new InvalidOperationException("Técnico não encontrado ou sem permissões");

                    // Obter e atualizar ticket
                    var dalTicket = _ticketRepository.GetTicketById(ticketId);
                    if (dalTicket == null)
                        throw new ArgumentException("Ticket não encontrado");

                    if (dalTicket.EstadoTicket != DAL.Models.EstadoTicket.PorAtender)
                        throw new InvalidOperationException("Apenas tickets por atender podem ser atendidos");

                    dalTicket.EstadoTicket = DAL.Models.EstadoTicket.EmAtendimento;
                    dalTicket.DataAtendimento = DateTime.Now;
                    dalTicket.TecnicoId = technicianId;

                    return _ticketRepository.UpdateTicket(dalTicket);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao atender ticket {ticketId}: {ex.Message}");
                    return false;
                }
            });
        }

        /// <summary>
        /// Completa o atendimento de um ticket de hardware
        /// </summary>
        public async Task<bool> CompleteHardwareTicketAsync(int ticketId, EstadoAtendimento status, string descricaoReparacao, string pecas = null)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var dalTicket = _ticketRepository.GetTicketById(ticketId) as DAL.Models.HardwareTicket;
                    if (dalTicket == null)
                        return false;

                    if (dalTicket.EstadoTicket != DAL.Models.EstadoTicket.EmAtendimento)
                        throw new InvalidOperationException("Apenas tickets em atendimento podem ser completados");

                    dalTicket.EstadoTicket = DAL.Models.EstadoTicket.Atendido;
                    dalTicket.EstadoAtendimento = (DAL.Models.EstadoAtendimento)status;
                    dalTicket.DescricaoReparacao = descricaoReparacao;
                    dalTicket.Pecas = pecas;

                    return _ticketRepository.UpdateTicket(dalTicket);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao completar atendimento de hardware {ticketId}: {ex.Message}");
                    return false;
                }
            });
        }

        /// <summary>
        /// Completa o atendimento de um ticket de software
        /// </summary>
        public async Task<bool> CompleteSoftwareTicketAsync(int ticketId, EstadoAtendimento status, string descricaoIntervencao)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var dalTicket = _ticketRepository.GetTicketById(ticketId) as DAL.Models.SoftwareTicket;
                    if (dalTicket == null)
                        return false;

                    if (dalTicket.EstadoTicket != DAL.Models.EstadoTicket.EmAtendimento)
                        throw new InvalidOperationException("Apenas tickets em atendimento podem ser completados");

                    dalTicket.EstadoTicket = DAL.Models.EstadoTicket.Atendido;
                    dalTicket.EstadoAtendimento = (DAL.Models.EstadoAtendimento)status;
                    dalTicket.DescricaoIntervencao = descricaoIntervencao;

                    return _ticketRepository.UpdateTicket(dalTicket);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao completar atendimento de software {ticketId}: {ex.Message}");
                    return false;
                }
            });
        }

        /// <summary>
        /// Obtém todos os tickets pendentes
        /// </summary>
        public async Task<IEnumerable<Ticket>> GetPendingTicketsAsync()
        {
            return await Task.Run(() =>
            {
                var dalTickets = _ticketRepository.GetTicketsByEstado(DAL.Models.EstadoTicket.PorAtender);
                return dalTickets.Select(ModelMapper.MapToBll).Where(t => t != null).ToList();
            });
        }

        /// <summary>
        /// Obtém estatísticas de tickets
        /// </summary>
        public async Task<Dictionary<string, object>> GetTicketStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            return await Task.Run(() =>
            {
                var hwStats = _ticketRepository.GetTicketStatistics(DAL.Models.TipoTicket.Hardware, startDate, endDate);
                var swStats = _ticketRepository.GetTicketStatistics(DAL.Models.TipoTicket.Software, startDate, endDate);

                return new Dictionary<string, object>
                {
                    ["Hardware"] = hwStats,
                    ["Software"] = swStats,
                    ["DataInicio"] = startDate,
                    ["DataFim"] = endDate,
                    ["TotalTickets"] = Convert.ToInt32(hwStats["TotalTickets"]) + Convert.ToInt32(swStats["TotalTickets"]),
                    ["TotalAtendidos"] = Convert.ToInt32(hwStats["TicketsAtendidos"]) + Convert.ToInt32(swStats["TicketsAtendidos"]),
                    ["TotalResolvidos"] = Convert.ToInt32(hwStats["TicketsResolvidos"]) + Convert.ToInt32(swStats["TicketsResolvidos"])
                };
            });
        }

        /// <summary>
        /// Obtém tickets por estado específico
        /// </summary>
        public async Task<IEnumerable<Ticket>> GetTicketsByStatusAsync(EstadoTicket estado)
        {
            return await Task.Run(() =>
            {
                var dalTickets = _ticketRepository.GetTicketsByEstado((DAL.Models.EstadoTicket)estado);
                return dalTickets.Select(ModelMapper.MapToBll).Where(t => t != null).ToList();
            });
        }

        /// <summary>
        /// Obtém tickets por intervalo de datas
        /// </summary>
        public async Task<IEnumerable<Ticket>> GetTicketsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await Task.Run(() =>
            {
                var dalTickets = _ticketRepository.GetTicketsByDateRange(startDate, endDate);
                return dalTickets.Select(ModelMapper.MapToBll).Where(t => t != null).ToList();
            });
        }

        /// <summary>
        /// Obtém todos os tickets
        /// </summary>
        public async Task<IEnumerable<Ticket>> GetAllTicketsAsync()
        {
            return await Task.Run(() =>
            {
                var dalTickets = _ticketRepository.GetAllTickets();
                return dalTickets.Select(ModelMapper.MapToBll).Where(t => t != null).ToList();
            });
        }

        /// <summary>
        /// Obtém estatísticas resumidas para dashboard
        /// </summary>
        public async Task<DashboardSummary> GetDashboardSummaryAsync(DateTime startDate, DateTime endDate)
        {
            return await Task.Run(() =>
            {
                var stats = GetTicketStatisticsAsync(startDate, endDate).Result;

                var totalTickets = Convert.ToInt32(stats["TotalTickets"]);
                var totalAtendidos = Convert.ToInt32(stats["TotalAtendidos"]);
                var totalResolvidos = Convert.ToInt32(stats["TotalResolvidos"]);

                return new DashboardSummary
                {
                    TotalTickets = totalTickets,
                    TicketsAtendidos = totalAtendidos,
                    TicketsResolvidos = totalResolvidos,
                    PercentagemAtendidos = totalTickets > 0 ? (double)totalAtendidos / totalTickets * 100 : 0,
                    PercentagemResolvidos = totalAtendidos > 0 ? (double)totalResolvidos / totalAtendidos * 100 : 0,
                    DataInicio = startDate,
                    DataFim = endDate
                };
            });
        }
    }
}