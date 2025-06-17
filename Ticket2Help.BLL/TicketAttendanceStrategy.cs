using System;
using System.Collections.Generic;
using System.Linq;
using Ticket2Help.BLL.Models;

namespace Ticket2Help.BLL.Strategy
{
    /// <summary>
    /// Interface para estratégias de atendimento de tickets
    /// Define o contrato para diferentes algoritmos de seleção de tickets
    /// </summary>
    public interface ITicketAttendanceStrategy
    {
        /// <summary>
        /// Seleciona o próximo ticket a ser atendido baseado na estratégia implementada
        /// </summary>
        /// <param name="tickets">Lista de tickets disponíveis para atendimento</param>
        /// <returns>Ticket selecionado ou null se não houver tickets</returns>
        Ticket SelectNextTicket(IEnumerable<Ticket> tickets);

        /// <summary>
        /// Nome da estratégia
        /// </summary>
        string StrategyName { get; }

        /// <summary>
        /// Descrição da estratégia
        /// </summary>
        string Description { get; }
    }

    /// <summary>
    /// Estratégia FIFO (First In, First Out)
    /// Atende os tickets por ordem de chegada
    /// </summary>
    public class FifoAttendanceStrategy : ITicketAttendanceStrategy
    {
        public string StrategyName => "FIFO";
        public string Description => "Primeiro a entrar, primeiro a ser atendido";

        /// <summary>
        /// Seleciona o ticket mais antigo
        /// </summary>
        /// <param name="tickets">Lista de tickets</param>
        /// <returns>Ticket mais antigo</returns>
        public Ticket SelectNextTicket(IEnumerable<Ticket> tickets)
        {
            var availableTickets = tickets?.Where(t => t.Status == TicketStatus.PorAtender);

            if (availableTickets == null || !availableTickets.Any())
                return null;

            return availableTickets.OrderBy(t => t.CreatedDate).First();
        }
    }

    /// <summary>
    /// Estratégia LIFO (Last In, First Out)
    /// Atende os tickets mais recentes primeiro
    /// </summary>
    public class LifoAttendanceStrategy : ITicketAttendanceStrategy
    {
        public string StrategyName => "LIFO";
        public string Description => "Último a entrar, primeiro a ser atendido";

        /// <summary>
        /// Seleciona o ticket mais recente
        /// </summary>
        /// <param name="tickets">Lista de tickets</param>
        /// <returns>Ticket mais recente</returns>
        public Ticket SelectNextTicket(IEnumerable<Ticket> tickets)
        {
            var availableTickets = tickets?.Where(t => t.Status == TicketStatus.PorAtender);

            if (availableTickets == null || !availableTickets.Any())
                return null;

            return availableTickets.OrderByDescending(t => t.CreatedDate).First();
        }
    }

    /// <summary>
    /// Estratégia por Prioridade
    /// Atende primeiro os tickets mais urgentes, depois por ordem de chegada
    /// </summary>
    public class PriorityAttendanceStrategy : ITicketAttendanceStrategy
    {
        public string StrategyName => "Prioridade";
        public string Description => "Atende primeiro os tickets urgentes, depois por ordem de chegada";

        /// <summary>
        /// Seleciona o ticket com maior prioridade
        /// </summary>
        /// <param name="tickets">Lista de tickets</param>
        /// <returns>Ticket prioritário</returns>
        public Ticket SelectNextTicket(IEnumerable<Ticket> tickets)
        {
            var availableTickets = tickets?.Where(t => t.Status == TicketStatus.PorAtender);

            if (availableTickets == null || !availableTickets.Any())
                return null;

            // Primeiro, verifica se há tickets urgentes
            var urgentTickets = availableTickets.Where(IsUrgentTicket);

            if (urgentTickets.Any())
            {
                // Se há tickets urgentes, seleciona o mais antigo entre eles
                return urgentTickets.OrderBy(t => t.CreatedDate).First();
            }

            // Se não há tickets urgentes, seleciona o mais antigo de todos
            return availableTickets.OrderBy(t => t.CreatedDate).First();
        }

        /// <summary>
        /// Determina se um ticket é urgente
        /// </summary>
        /// <param name="ticket">Ticket a verificar</param>
        /// <returns>True se for urgente</returns>
        private bool IsUrgentTicket(Ticket ticket)
        {
            return ticket switch
            {
                HardwareTicket hwTicket => hwTicket.IsUrgent(),
                SoftwareTicket swTicket => swTicket.IsUrgent(),
                _ => false
            };
        }
    }

    /// <summary>
    /// Estratégia por Tipo de Ticket
    /// Prioriza um tipo de ticket sobre outro
    /// </summary>
    public class TypeBasedAttendanceStrategy : ITicketAttendanceStrategy
    {
        private readonly TicketType _priorityType;

        public string StrategyName => $"Prioridade {_priorityType}";
        public string Description => $"Atende primeiro tickets de {_priorityType}, depois os outros";

        /// <summary>
        /// Construtor que define o tipo prioritário
        /// </summary>
        /// <param name="priorityType">Tipo de ticket prioritário</param>
        public TypeBasedAttendanceStrategy(TicketType priorityType)
        {
            _priorityType = priorityType;
        }

        /// <summary>
        /// Seleciona tickets do tipo prioritário primeiro
        /// </summary>
        /// <param name="tickets">Lista de tickets</param>
        /// <returns>Ticket selecionado</returns>
        public Ticket SelectNextTicket(IEnumerable<Ticket> tickets)
        {
            var availableTickets = tickets?.Where(t => t.Status == TicketStatus.PorAtender);

            if (availableTickets == null || !availableTickets.Any())
                return null;

            // Primeiro, procura tickets do tipo prioritário
            var priorityTickets = availableTickets.Where(t => t.Type == _priorityType);

            if (priorityTickets.Any())
            {
                return priorityTickets.OrderBy(t => t.CreatedDate).First();
            }

            // Se não há tickets do tipo prioritário, seleciona qualquer outro
            return availableTickets.OrderBy(t => t.CreatedDate).First();
        }
    }

    /// <summary>
    /// Estratégia Round Robin
    /// Alterna entre tipos de tickets
    /// </summary>
    public class RoundRobinAttendanceStrategy : ITicketAttendanceStrategy
    {
        private TicketType _lastSelectedType = TicketType.Software; // Começa com Software para primeiro ser Hardware

        public string StrategyName => "Round Robin";
        public string Description => "Alterna entre tipos de tickets";

        /// <summary>
        /// Seleciona tickets alternando entre tipos
        /// </summary>
        /// <param name="tickets">Lista de tickets</param>
        /// <returns>Ticket selecionado</returns>
        public Ticket SelectNextTicket(IEnumerable<Ticket> tickets)
        {
            var availableTickets = tickets?.Where(t => t.Status == TicketStatus.PorAtender);

            if (availableTickets == null || !availableTickets.Any())
                return null;

            // Determina o próximo tipo a selecionar
            var nextType = _lastSelectedType == TicketType.Hardware ? TicketType.Software : TicketType.Hardware;

            // Procura tickets do próximo tipo
            var nextTypeTickets = availableTickets.Where(t => t.Type == nextType);

            if (nextTypeTickets.Any())
            {
                _lastSelectedType = nextType;
                return nextTypeTickets.OrderBy(t => t.CreatedDate).First();
            }

            // Se não há tickets do próximo tipo, seleciona do tipo atual
            var currentTypeTickets = availableTickets.Where(t => t.Type == _lastSelectedType);

            if (currentTypeTickets.Any())
            {
                return currentTypeTickets.OrderBy(t => t.CreatedDate).First();
            }

            // Fallback: seleciona qualquer ticket disponível
            return availableTickets.OrderBy(t => t.CreatedDate).First();
        }
    }

    /// <summary>
    /// Contexto para as estratégias de atendimento
    /// Implementa o padrão Strategy
    /// </summary>
    public class TicketAttendanceContext
    {
        private ITicketAttendanceStrategy _strategy;

        /// <summary>
        /// Estratégia atual
        /// </summary>
        public ITicketAttendanceStrategy Strategy
        {
            get => _strategy;
            set => _strategy = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Construtor com estratégia inicial
        /// </summary>
        /// <param name="strategy">Estratégia a usar</param>
        public TicketAttendanceContext(ITicketAttendanceStrategy strategy)
        {
            Strategy = strategy;
        }

        /// <summary>
        /// Construtor padrão - usa estratégia FIFO
        /// </summary>
        public TicketAttendanceContext()
        {
            Strategy = new FifoAttendanceStrategy();
        }

        /// <summary>
        /// Seleciona o próximo ticket usando a estratégia configurada
        /// </summary>
        /// <param name="tickets">Lista de tickets</param>
        /// <returns>Ticket selecionado</returns>
        public Ticket SelectNextTicket(IEnumerable<Ticket> tickets)
        {
            return Strategy.SelectNextTicket(tickets);
        }

        /// <summary>
        /// Obtém todas as estratégias disponíveis
        /// </summary>
        /// <returns>Lista de estratégias</returns>
        public static List<ITicketAttendanceStrategy> GetAvailableStrategies()
        {
            return new List<ITicketAttendanceStrategy>
            {
                new FifoAttendanceStrategy(),
                new LifoAttendanceStrategy(),
                new PriorityAttendanceStrategy(),
                new TypeBasedAttendanceStrategy(TicketType.Hardware),
                new TypeBasedAttendanceStrategy(TicketType.Software),
                new RoundRobinAttendanceStrategy()
            };
        }
    }
}