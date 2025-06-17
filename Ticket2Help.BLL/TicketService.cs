using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ticket2Help.BLL.Models;
using Ticket2Help.BLL.Factory;
using Ticket2Help.BLL.Strategy;
using Ticket2Help.DAL.Interfaces;
using Ticket2Help.DAL.Repositories;

namespace Ticket2Help.BLL.Services
{
    /// <summary>
    /// Interface para o serviço de tickets
    /// Define as operações de negócio relacionadas com tickets
    /// </summary>
    public interface ITicketService
    {
        /// <summary>
        /// Cria um novo ticket
        /// </summary>
        /// <param name="type">Tipo do ticket</param>
        /// <param name="userId">ID do utilizador</param>
        /// <param name="specificData">Dados específicos do ticket</param>
        /// <returns>Ticket criado</returns>
        Task<Ticket> CreateTicketAsync(TicketType type, int userId, object specificData);

        /// <summary>
        /// Obtém um ticket pelo ID
        /// </summary>
        /// <param name="ticketId">ID do ticket</param>
        /// <returns>Ticket encontrado</returns>
        Task<Ticket> GetTicketByIdAsync(int ticketId);

        /// <summary>
        /// Obtém todos os tickets de um utilizador
        /// </summary>
        /// <param name="userId">ID do utilizador</param>
        /// <returns>Lista de tickets</returns>
        Task<IEnumerable<Ticket>> GetUserTicketsAsync(int userId);

        /// <summary>
        /// Obtém o próximo ticket para atendimento
        /// </summary>
        /// <returns>Próximo ticket ou null</returns>
        Task<Ticket> GetNextTicketForAttendanceAsync();

        /// <summary>
        /// Atende um ticket
        /// </summary>
        /// <param name="ticketId">ID do ticket</param>
        /// <param name="technicianId">ID do técnico</param>
        /// <returns>True se bem-sucedido</returns>
        Task<bool> AttendTicketAsync(int ticketId, int technicianId);

        /// <summary>
        /// Completa o atendimento de um ticket
        /// </summary>
        /// <param name="ticketId">ID do ticket</param>
        /// <param name="attendanceStatus">Estado do atendimento</param>
        /// <param name="details">Detalhes específicos</param>
        /// <returns>True se bem-sucedido</returns>
        Task<bool> CompleteTicketAttendanceAsync(int ticketId, AttendanceStatus attendanceStatus, object details);

        /// <summary>
        /// Obtém todos os tickets disponíveis para atendimento
        /// </summary>
        /// <returns>Lista de tickets por atender</returns>
        Task<IEnumerable<Ticket>> GetPendingTicketsAsync();

        /// <summary>
        /// Define a estratégia de atendimento
        /// </summary>
        /// <param name="strategy">Estratégia a usar</param>
        void SetAttendanceStrategy(ITicketAttendanceStrategy strategy);
    }

    /// <summary>
    /// Implementação do serviço de tickets
    /// Contém a lógica de negócio para gestão de tickets
    /// </summary>
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITicketFactory _ticketFactory;
        private readonly TicketAttendanceContext _attendanceContext;
        private int _lastSequentialNumber;

        /// <summary>
        /// Construtor do serviço
        /// </summary>
        /// <param name="ticketRepository">Repositório de tickets</param>
        /// <param name="userRepository">Repositório de utilizadores</param>
        /// <param name="ticketFactory">Factory de tickets</param>
        public TicketService(
            ITicketRepository ticketRepository,
            IUserRepository userRepository,
            ITicketFactory ticketFactory)
        {
            _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _ticketFactory = ticketFactory ?? throw new ArgumentNullException(nameof(ticketFactory));
            _attendanceContext = new TicketAttendanceContext();

            InitializeSequentialNumber();
        }

        /// <summary>
        /// Cria um novo ticket
        /// </summary>
        /// <param name="type">Tipo do ticket</param>
        /// <param name="userId">ID do utilizador</param>
        /// <param name="specificData">Dados específicos</param>
        /// <returns>Ticket criado</returns>
        public async Task<Ticket> CreateTicketAsync(TicketType type, int userId, object specificData)
        {
            // Validar utilizador
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                throw new ArgumentException("Utilizador não encontrado", nameof(userId));

            if (!user.CanCreateTickets())
                throw new InvalidOperationException("Utilizador não tem permissões para criar tickets");

            // Gerar próximo número sequencial
            var sequentialNumber = GetNextSequentialNumber();

            // Criar ticket usando a factory
            var ticket = _ticketFactory.CreateTicket(type, sequentialNumber, userId, specificData);

            // Validar ticket
            if (!ticket.IsValid())
                throw new InvalidOperationException("Dados do ticket são inválidos");

            // Guardar na base de dados
            var ticketId = await _ticketRepository.AddTicketAsync(ticket);
            ticket.Id = ticketId;

            return ticket;
        }

        /// <summary>
        /// Obtém um ticket pelo ID
        /// </summary>
        /// <param name="ticketId">ID do ticket</param>
        /// <returns>Ticket encontrado</returns>
        public async Task<Ticket> GetTicketByIdAsync(int ticketId)
        {
            if (ticketId <= 0)
                throw new ArgumentException("ID do ticket inválido", nameof(ticketId));

            return await _ticketRepository.GetTicketByIdAsync(ticketId);
        }

        /// <summary>
        /// Obtém todos os tickets de um utilizador
        /// </summary>
        /// <param name="userId">ID do utilizador</param>
        /// <returns>Lista de tickets</returns>
        public async Task<IEnumerable<Ticket>> GetUserTicketsAsync(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException("ID do utilizador inválido", nameof(userId));

            return await _ticketRepository.GetTicketsByUserIdAsync(userId);
        }

        /// <summary>
        /// Obtém o próximo ticket para atendimento usando a estratégia configurada
        /// </summary>
        /// <returns>Próximo ticket</returns>
        public async Task<Ticket> GetNextTicketForAttendanceAsync()
        {
            var pendingTickets = await GetPendingTicketsAsync();
            return _attendanceContext.SelectNextTicket(pendingTickets);
        }

        /// <summary>
        /// Atende um ticket
        /// </summary>
        /// <param name="ticketId">ID do ticket</param>
        /// <param name="technicianId">ID do técnico</param>
        /// <returns>True se bem-sucedido</returns>
        public async Task<bool> AttendTicketAsync(int ticketId, int technicianId)
        {
            try
            {
                // Validar técnico
                var technician = await _userRepository.GetUserByIdAsync(technicianId);
                if (technician == null || !technician.CanAttendTickets())
                    throw new InvalidOperationException("Técnico não encontrado ou sem permissões");

                // Obter ticket
                var ticket = await _ticketRepository.GetTicketByIdAsync(ticketId);
                if (ticket == null)
                    throw new ArgumentException("Ticket não encontrado");

                // Atender ticket
                ticket.AttendTicket(technicianId);

                // Atualizar na base de dados
                await _ticketRepository.UpdateTicketAsync(ticket);

                return true;
            }
            catch (Exception ex)
            {
                // Log do erro (implementar logging adequado)
                Console.WriteLine($"Erro ao atender ticket {ticketId}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Completa o atendimento de um ticket
        /// </summary>
        /// <param name="ticketId">ID do ticket</param>
        /// <param name="attendanceStatus">Estado do atendimento</param>
        /// <param name="details">Detalhes específicos</param>
        /// <returns>True se bem-sucedido</returns>
        public async Task<bool> CompleteTicketAttendanceAsync(int ticketId, AttendanceStatus attendanceStatus, object details)
        {
            try
            {
                var ticket = await _ticketRepository.GetTicketByIdAsync(ticketId);
                if (ticket == null)
                    throw new ArgumentException("Ticket não encontrado");

                // Completar atendimento baseado no tipo
                switch (ticket)
                {
                    case HardwareTicket hwTicket:
                        await CompleteHardwareTicketAsync(hwTicket, attendanceStatus, details);
                        break;
                    case SoftwareTicket swTicket:
                        await CompleteSoftwareTicketAsync(swTicket, attendanceStatus, details);
                        break;
                    default:
                        ticket.CompleteAttendance(attendanceStatus);
                        break;
                }

                // Atualizar na base de dados
                await _ticketRepository.UpdateTicketAsync(ticket);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao completar atendimento do ticket {ticketId}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Obtém todos os tickets pendentes
        /// </summary>
        /// <returns>Lista de tickets por atender</returns>
        public async Task<IEnumerable<Ticket>> GetPendingTicketsAsync()
        {
            var allTickets = await _ticketRepository.GetAllTicketsAsync();
            return allTickets.Where(t => t.Status == TicketStatus.PorAtender);
        }

        /// <summary>
        /// Define a estratégia de atendimento
        /// </summary>
        /// <param name="strategy">Estratégia a usar</param>
        public void SetAttendanceStrategy(ITicketAttendanceStrategy strategy)
        {
            if (strategy == null)
                throw new ArgumentNullException(nameof(strategy));

            _attendanceContext.Strategy = strategy;
        }

        /// <summary>
        /// Completa o atendimento de um ticket de hardware
        /// </summary>
        private async Task CompleteHardwareTicketAsync(HardwareTicket ticket, AttendanceStatus status, object details)
        {
            if (details is HardwareAttendanceDetails hwDetails)
            {
                ticket.CompleteHardwareAttendance(status, hwDetails.RepairDescription, hwDetails.Parts);
            }
            else if (details is string description)
            {
                ticket.CompleteHardwareAttendance(status, description);
            }
            else
            {
                throw new ArgumentException("Detalhes inválidos para ticket de hardware");
            }
        }

        /// <summary>
        /// Completa o atendimento de um ticket de software
        /// </summary>
        private async Task CompleteSoftwareTicketAsync(SoftwareTicket ticket, AttendanceStatus status, object details)
        {
            if (details is SoftwareAttendanceDetails swDetails)
            {
                ticket.CompleteSoftwareAttendance(status, swDetails.InterventionDescription);
            }
            else if (details is string description)
            {
                ticket.CompleteSoftwareAttendance(status, description);
            }
            else
            {
                throw new ArgumentException("Detalhes inválidos para ticket de software");
            }
        }

        /// <summary>
        /// Inicializa o número sequencial baseado nos tickets existentes
        /// </summary>
        private async void InitializeSequentialNumber()
        {
            try
            {
                var lastTicket = await _ticketRepository.GetLastTicketAsync();
                _lastSequentialNumber = lastTicket?.SequentialNumber ?? 0;
            }
            catch
            {
                _lastSequentialNumber = 0;
            }
        }

        /// <summary>
        /// Gera o próximo número sequencial
        /// </summary>
        private int GetNextSequentialNumber()
        {
            return ++_lastSequentialNumber;
        }
    }

    /// <summary>
    /// Classe para detalhes de atendimento de hardware
    /// </summary>
    public class HardwareAttendanceDetails
    {
        public string RepairDescription { get; set; }
        public string Parts { get; set; }

        public HardwareAttendanceDetails(string repairDescription, string parts = null)
        {
            RepairDescription = repairDescription;
            Parts = parts;
        }
    }

    /// <summary>
    /// Classe para detalhes de atendimento de software
    /// </summary>
    public class SoftwareAttendanceDetails
    {
        public string InterventionDescription { get; set; }

        public SoftwareAttendanceDetails(string interventionDescription)
        {
            InterventionDescription = interventionDescription;
        }
    }