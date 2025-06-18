using System;
using System.ComponentModel.DataAnnotations;

namespace Ticket2Help.BLL.Models
{
    /// <summary>
    /// Classe que representa um ticket de serviços de hardware
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
        /// Descrição da reparação efetuada (preenchida pelo técnico)
        /// </summary>
        public string DescricaoReparacao { get; set; }

        /// <summary>
        /// Peças utilizadas na reparação (preenchida pelo técnico)
        /// </summary>
        public string Pecas { get; set; }

        /// <summary>
        /// Tipo do ticket
        /// </summary>
        public override TipoTicket TipoTicket => TipoTicket.Hardware;

        /// <summary>
        /// Construtor padrão
        /// </summary>
        public HardwareTicket() : base()
        {
        }

        /// <summary>
        /// Construtor com parâmetros
        /// </summary>
        public HardwareTicket(string colaboradorId, string equipamento, string avaria)
            : base(colaboradorId)
        {
            Equipamento = equipamento;
            Avaria = avaria;
        }

        /// <summary>
        /// Valida se o ticket de hardware é válido
        /// </summary>
        public override bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Equipamento) &&
                   !string.IsNullOrWhiteSpace(Avaria) &&
                   !string.IsNullOrWhiteSpace(ColaboradorId);
        }

        /// <summary>
        /// Método para completar atendimento de hardware
        /// </summary>
        public void CompletarAtendimentoHardware(EstadoAtendimento estado, string descricaoReparacao, string pecas = null)
        {
            if (string.IsNullOrWhiteSpace(descricaoReparacao))
            {
                throw new ArgumentException("A descrição da reparação é obrigatória.");
            }

            DescricaoReparacao = descricaoReparacao;
            Pecas = pecas;

            base.CompletarAtendimento(estado);
        }

        /// <summary>
        /// Retorna informações específicas do ticket de hardware
        /// </summary>
        public override string GetInformacaoEspecifica()
        {
            var info = $"Equipamento: {Equipamento}\nAvaria: {Avaria}";

            if (!string.IsNullOrWhiteSpace(DescricaoReparacao))
            {
                info += $"\nReparação: {DescricaoReparacao}";
            }

            if (!string.IsNullOrWhiteSpace(Pecas))
            {
                info += $"\nPeças: {Pecas}";
            }

            return info;
        }

        /// <summary>
        /// Verifica se o ticket é urgente baseado em palavras-chave
        /// </summary>
        public bool IsUrgente()
        {
            var palavrasUrgentes = new[] { "servidor", "rede", "crítico", "emergência", "fogo", "fumo", "não liga" };
            var avariaLower = Avaria.ToLower();

            foreach (var palavra in palavrasUrgentes)
            {
                if (avariaLower.Contains(palavra))
                {
                    return true;
                }
            }

            return false;
        }
    }
}