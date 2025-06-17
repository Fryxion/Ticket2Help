using System;
using System.ComponentModel.DataAnnotations;

namespace Ticket2Help.BLL.Models
{
    /// <summary>
    /// Enumeração para os estados do ticket (compatível com seu DAL)
    /// </summary>
    public enum EstadoTicket
    {
        PorAtender = 0,
        EmAtendimento = 1,
        Atendido = 2
    }

    /// <summary>
    /// Enumeração para os estados de atendimento (compatível com seu DAL)
    /// </summary>
    public enum EstadoAtendimento
    {
        Aberto = 0,
        Resolvido = 1,
        NaoResolvido = 2
    }

    /// <summary>
    /// Enumeração para os tipos de ticket (compatível com seu DAL)
    /// </summary>
    public enum TipoTicket
    {
        Hardware = 0,
        Software = 1
    }

    /// <summary>
    /// Enumeração para tipos de utilizador (compatível com seu DAL)
    /// </summary>
    public enum TipoUtilizador
    {
        Colaborador = 0,
        Tecnico = 1,
        Administrador = 2
    }

    /// <summary>
    /// Classe base abstrata para todos os tipos de tickets
    /// Adaptada para compatibilidade com seu DAL
    /// </summary>
    public abstract class Ticket
    {
        /// <summary>
        /// ID único do ticket (gerado automaticamente pela BD)
        /// </summary>
        public int TicketId { get; set; }

        /// <summary>
        /// Data e hora de criação do ticket (gerada automaticamente)
        /// </summary>
        public DateTime DataCriacao { get; set; }

        /// <summary>
        /// ID do colaborador que submeteu o ticket
        /// </summary>
        public string ColaboradorId { get; set; }

        /// <summary>
        /// Estado atual do ticket
        /// </summary>
        public EstadoTicket EstadoTicket { get; set; }

        /// <summary>
        /// Tipo do ticket (Hardware ou Software)
        /// </summary>
        public abstract TipoTicket TipoTicket { get; }

        /// <summary>
        /// Data e hora de atendimento do ticket
        /// </summary>
        public DateTime? DataAtendimento { get; set; }

        /// <summary>
        /// Estado do atendimento (apenas aplicável quando o ticket está atendido)
        /// </summary>
        public EstadoAtendimento? EstadoAtendimento { get; set; }

        /// <summary>
        /// ID do técnico que atendeu o ticket
        /// </summary>
        public string TecnicoId { get; set; }

        /// <summary>
        /// Construtor da classe base
        /// </summary>
        protected Ticket()
        {
            DataCriacao = DateTime.Now;
            EstadoTicket = EstadoTicket.PorAtender;
        }

        /// <summary>
        /// Construtor com parâmetros
        /// </summary>
        protected Ticket(string colaboradorId) : this()
        {
            ColaboradorId = colaboradorId ?? throw new ArgumentNullException(nameof(colaboradorId));
        }

        /// <summary>
        /// Método para atender o ticket
        /// </summary>
        public virtual void AtenderTicket(string tecnicoId)
        {
            if (EstadoTicket != EstadoTicket.PorAtender)
            {
                throw new InvalidOperationException("Apenas tickets por atender podem ser atendidos.");
            }

            EstadoTicket = EstadoTicket.EmAtendimento;
            DataAtendimento = DateTime.Now;
            TecnicoId = tecnicoId;
        }

        /// <summary>
        /// Método para completar o atendimento
        /// </summary>
        public virtual void CompletarAtendimento(EstadoAtendimento estadoAtendimento)
        {
            if (EstadoTicket != EstadoTicket.EmAtendimento)
            {
                throw new InvalidOperationException("Apenas tickets em atendimento podem ser completados.");
            }

            EstadoTicket = EstadoTicket.Atendido;
            EstadoAtendimento = estadoAtendimento;
        }

        /// <summary>
        /// Método abstrato para validação específica de cada tipo de ticket
        /// </summary>
        public abstract bool IsValid();

        /// <summary>
        /// Método para obter informações específicas do ticket
        /// </summary>
        public abstract string GetInformacaoEspecifica();

        /// <summary>
        /// Override do ToString para representação textual do ticket
        /// </summary>
        public override string ToString()
        {
            return $"Ticket #{TicketId} - {TipoTicket} - {EstadoTicket}";
        }
    }
}