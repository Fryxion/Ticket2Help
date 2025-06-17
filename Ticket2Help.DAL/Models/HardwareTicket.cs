using System;

namespace Ticket2Help.DAL.Models
{
    /// <summary>
    /// Modelo de ticket de hardware para o DAL
    /// Representa a estrutura da tabela HardwareTickets
    /// </summary>
    public class HardwareTicket : Ticket
    {
        /// <summary>
        /// Equipamento relacionado com o ticket
        /// </summary>
        public string Equipamento { get; set; }

        /// <summary>
        /// Descrição da avaria reportada
        /// </summary>
        public string Avaria { get; set; }

        /// <summary>
        /// Descrição da reparação efetuada
        /// </summary>
        public string DescricaoReparacao { get; set; }

        /// <summary>
        /// Peças utilizadas na reparação
        /// </summary>
        public string Pecas { get; set; }

        /// <summary>
        /// Tipo do ticket
        /// </summary>
        public override TipoTicket TipoTicket => TipoTicket.Hardware;

        /// <summary>
        /// Construtor padrão
        /// </summary>
        public HardwareTicket() : base() { }

        /// <summary>
        /// Construtor com parâmetros
        /// </summary>
        public HardwareTicket(string colaboradorId, string equipamento, string avaria)
            : base()
        {
            ColaboradorId = colaboradorId;
            Equipamento = equipamento;
            Avaria = avaria;
        }
    }
}