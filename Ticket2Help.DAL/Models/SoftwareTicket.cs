using System;

namespace Ticket2Help.DAL.Models
{
    /// <summary>
    /// Modelo de ticket de software para o DAL
    /// Representa a estrutura da tabela SoftwareTickets
    /// </summary>
    public class SoftwareTicket : Ticket
    {
        /// <summary>
        /// Nome do software relacionado
        /// </summary>
        public string Software { get; set; }

        /// <summary>
        /// Descrição da necessidade ou problema
        /// </summary>
        public string DescricaoNecessidade { get; set; }

        /// <summary>
        /// Descrição da intervenção efetuada
        /// </summary>
        public string DescricaoIntervencao { get; set; }

        /// <summary>
        /// Tipo do ticket
        /// </summary>
        public override TipoTicket TipoTicket => TipoTicket.Software;

        /// <summary>
        /// Construtor padrão
        /// </summary>
        public SoftwareTicket() : base() { }

        /// <summary>
        /// Construtor com parâmetros
        /// </summary>
        public SoftwareTicket(string colaboradorId, string software, string descricaoNecessidade)
            : base()
        {
            ColaboradorId = colaboradorId;
            Software = software;
            DescricaoNecessidade = descricaoNecessidade;
        }
    }
}