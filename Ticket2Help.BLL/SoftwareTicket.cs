using System;
using System.ComponentModel.DataAnnotations;

namespace Ticket2Help.BLL.Models
{
    /// <summary>
    /// Classe que representa um ticket de serviços de software
    /// Herda da classe base Ticket e implementa funcionalidades específicas de software
    /// </summary>
    /// <summary>
    /// Classe que representa um ticket de serviços de software
    /// Compatível com a tabela SoftwareTickets do seu DAL
    /// </summary>
    public class SoftwareTicket : Ticket
    {
        /// <summary>
        /// Nome do software relacionado com o ticket
        /// </summary>
        public string Software { get; set; }

        /// <summary>
        /// Descrição da necessidade ou problema reportado
        /// </summary>
        public string DescricaoNecessidade { get; set; }

        /// <summary>
        /// Descrição da intervenção efetuada (preenchida pelo técnico)
        /// </summary>
        public string DescricaoIntervencao { get; set; }

        /// <summary>
        /// Tipo do ticket
        /// </summary>
        public override TipoTicket TipoTicket => TipoTicket.Software;

        /// <summary>
        /// Construtor padrão
        /// </summary>
        public SoftwareTicket() : base()
        {
        }

        /// <summary>
        /// Construtor com parâmetros
        /// </summary>
        public SoftwareTicket(string colaboradorId, string software, string descricaoNecessidade)
            : base(colaboradorId)
        {
            Software = software ?? throw new ArgumentNullException(nameof(software));
            DescricaoNecessidade = descricaoNecessidade ?? throw new ArgumentNullException(nameof(descricaoNecessidade));
        }

        /// <summary>
        /// Valida se o ticket de software é válido
        /// </summary>
        public override bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Software) &&
                   !string.IsNullOrWhiteSpace(DescricaoNecessidade) &&
                   !string.IsNullOrWhiteSpace(ColaboradorId);
        }

        /// <summary>
        /// Método para completar atendimento de software
        /// </summary>
        public void CompletarAtendimentoSoftware(EstadoAtendimento estado, string descricaoIntervencao)
        {
            if (string.IsNullOrWhiteSpace(descricaoIntervencao))
            {
                throw new ArgumentException("A descrição da intervenção é obrigatória.");
            }

            DescricaoIntervencao = descricaoIntervencao;

            base.CompletarAtendimento(estado);
        }

        /// <summary>
        /// Retorna informações específicas do ticket de software
        /// </summary>
        public override string GetInformacaoEspecifica()
        {
            var info = $"Software: {Software}\nDescrição: {DescricaoNecessidade}";

            if (!string.IsNullOrWhiteSpace(DescricaoIntervencao))
            {
                info += $"\nIntervenção: {DescricaoIntervencao}";
            }

            return info;
        }

        /// <summary>
        /// Verifica se o ticket é urgente baseado em palavras-chave
        /// </summary>
        public bool IsUrgente()
        {
            var palavrasUrgentes = new[] { "sistema", "crítico", "emergência", "produção", "não funciona", "erro crítico" };
            var descricaoLower = DescricaoNecessidade.ToLower();

            foreach (var palavra in palavrasUrgentes)
            {
                if (descricaoLower.Contains(palavra))
                {
                    return true;
                }
            }

            return false;
        }
    }
}