using System;
using System.ComponentModel.DataAnnotations;

namespace Ticket2Help.BLL.Models
{
    /// <summary>
    /// Classe que representa um ticket de serviços de software
    /// Herda da classe base Ticket e implementa funcionalidades específicas de software
    /// </summary>
    public class SoftwareTicket : Ticket
    {
        /// <summary>
        /// Nome do software relacionado com o ticket
        /// </summary>
        [Required(ErrorMessage = "O nome do software é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome do software não pode exceder 100 caracteres")]
        public string Software { get; set; }

        /// <summary>
        /// Descrição da necessidade ou problema reportado
        /// </summary>
        [Required(ErrorMessage = "A descrição da necessidade é obrigatória")]
        [StringLength(1000, ErrorMessage = "A descrição da necessidade não pode exceder 1000 caracteres")]
        public string NeedDescription { get; set; }

        /// <summary>
        /// Descrição da intervenção efetuada (preenchida pelo técnico)
        /// </summary>
        [StringLength(1000, ErrorMessage = "A descrição da intervenção não pode exceder 1000 caracteres")]
        public string InterventionDescription { get; set; }

        /// <summary>
        /// Propriedade que retorna o tipo do ticket
        /// </summary>
        public override TicketType Type => TicketType.Software;

        /// <summary>
        /// Construtor padrão
        /// </summary>
        public SoftwareTicket() : base()
        {
        }

        /// <summary>
        /// Construtor com parâmetros
        /// </summary>
        /// <param name="sequentialNumber">Número sequencial do ticket</param>
        /// <param name="userId">ID do utilizador que criou o ticket</param>
        /// <param name="software">Software relacionado</param>
        /// <param name="needDescription">Descrição da necessidade</param>
        public SoftwareTicket(int sequentialNumber, int userId, string software, string needDescription)
            : base(sequentialNumber, userId)
        {
            Software = software ?? throw new ArgumentNullException(nameof(software));
            NeedDescription = needDescription ?? throw new ArgumentNullException(nameof(needDescription));
        }

        /// <summary>
        /// Valida se o ticket de software tem todos os campos obrigatórios preenchidos
        /// </summary>
        /// <returns>True se o ticket é válido</returns>
        public override bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Software) &&
                   !string.IsNullOrWhiteSpace(NeedDescription) &&
                   UserId > 0 &&
                   SequentialNumber > 0;
        }

        /// <summary>
        /// Método para atender ticket de software
        /// Adiciona validações específicas para software
        /// </summary>
        /// <param name="technicianId">ID do técnico</param>
        public override void AttendTicket(int technicianId)
        {
            if (!IsValid())
            {
                throw new InvalidOperationException("Ticket de software inválido. Verifique se todos os campos obrigatórios estão preenchidos.");
            }

            base.AttendTicket(technicianId);
        }

        /// <summary>
        /// Método para completar o atendimento do ticket de software
        /// </summary>
        /// <param name="attendanceStatus">Estado do atendimento</param>
        /// <param name="interventionDescription">Descrição da intervenção</param>
        public void CompleteSoftwareAttendance(AttendanceStatus attendanceStatus, string interventionDescription)
        {
            if (string.IsNullOrWhiteSpace(interventionDescription))
            {
                throw new ArgumentException("A descrição da intervenção é obrigatória para completar o atendimento.");
            }

            InterventionDescription = interventionDescription;

            base.CompleteAttendance(attendanceStatus);
        }

        /// <summary>
        /// Retorna informações específicas do ticket de software
        /// </summary>
        /// <returns>String com informações específicas</returns>
        public override string GetSpecificInfo()
        {
            var info = $"Software: {Software}\nDescrição: {NeedDescription}";

            if (!string.IsNullOrWhiteSpace(InterventionDescription))
            {
                info += $"\nIntervenção: {InterventionDescription}";
            }

            return info;
        }

        /// <summary>
        /// Método para verificar se o ticket pode ser considerado urgente
        /// Baseado em palavras-chave na descrição da necessidade
        /// </summary>
        /// <returns>True se for considerado urgente</returns>
        public bool IsUrgent()
        {
            var urgentKeywords = new[] { "sistema", "crítico", "emergência", "produção", "não funciona", "erro crítico", "falha total" };
            var needDescriptionLower = NeedDescription.ToLower();

            foreach (var keyword in urgentKeywords)
            {
                if (needDescriptionLower.Contains(keyword))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Calcula uma estimativa de tempo de resolução baseada no tipo de software
        /// </summary>
        /// <returns>Estimativa em horas</returns>
        public int GetEstimatedResolutionTime()
        {
            var softwareLower = Software.ToLower();

            if (softwareLower.Contains("sistema operativo") || softwareLower.Contains("windows") || softwareLower.Contains("linux"))
                return 3;
            else if (softwareLower.Contains("office") || softwareLower.Contains("word") || softwareLower.Contains("excel"))
                return 1;
            else if (softwareLower.Contains("antivírus") || softwareLower.Contains("antivirus"))
                return 2;
            else if (softwareLower.Contains("adobe") || softwareLower.Contains("photoshop"))
                return 2;
            else if (softwareLower.Contains("email") || softwareLower.Contains("outlook"))
                return 1;
            else
                return 2; // Default
        }

        /// <summary>
        /// Determina a categoria do problema de software
        /// </summary>
        /// <returns>Categoria do problema</returns>
        public string GetProblemCategory()
        {
            var needLower = NeedDescription.ToLower();

            if (needLower.Contains("instalação") || needLower.Contains("instalar"))
                return "Instalação";
            else if (needLower.Contains("atualização") || needLower.Contains("update"))
                return "Atualização";
            else if (needLower.Contains("configuração") || needLower.Contains("configurar"))
                return "Configuração";
            else if (needLower.Contains("erro") || needLower.Contains("problema"))
                return "Resolução de Erro";
            else if (needLower.Contains("licença") || needLower.Contains("ativação"))
                return "Licenciamento";
            else if (needLower.Contains("formação") || needLower.Contains("aprender"))
                return "Formação";
            else
                return "Outros";
        }

        /// <summary>
        /// Override do ToString para representação específica do ticket de software
        /// </summary>
        /// <returns>Representação string do ticket</returns>
        public override string ToString()
        {
            return $"{base.ToString()} - {Software}: {GetProblemCategory()}";
        }
    }
}