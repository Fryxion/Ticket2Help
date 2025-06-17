using System;

namespace Ticket2Help.DAL.Models
{
    /// <summary>
    /// Modelo base de ticket para o DAL
    /// Representa a estrutura comum das tabelas de tickets
    /// </summary>
    public abstract class Ticket
    {
        /// <summary>
        /// ID único do ticket (gerado automaticamente pela BD)
        /// </summary>
        public int TicketId { get; set; }

        /// <summary>
        /// Data e hora de criação do ticket
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
        /// Estado do atendimento
        /// </summary>
        public EstadoAtendimento? EstadoAtendimento { get; set; }

        /// <summary>
        /// ID do técnico que atendeu o ticket
        /// </summary>
        public string TecnicoId { get; set; }

        /// <summary>
        /// Construtor padrão
        /// </summary>
        protected Ticket()
        {
            DataCriacao = DateTime.Now;
            EstadoTicket = EstadoTicket.PorAtender;
        }
    }
}