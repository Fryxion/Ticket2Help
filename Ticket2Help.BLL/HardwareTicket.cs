using System;
using System.ComponentModel.DataAnnotations;

namespace Ticket2Help.BLL.Models
{
    /// <summary>
    /// Classe que representa um ticket de serviços de hardware
    /// Herda da classe base Ticket e implementa funcionalidades específicas de hardware
    /// </summary>
    public class HardwareTicket : Ticket
    {
        /// <summary>
        /// Equipamento relacionado com o ticket
        /// </summary>
        [Required(ErrorMessage = "O equipamento é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome do equipamento não pode exceder 100 caracteres")]
        public string Equipment { get; set; }

        /// <summary>
        /// Descrição da avaria reportada
        /// </summary>
        [Required(ErrorMessage = "A descrição da avaria é obrigatória")]
        [StringLength(500, ErrorMessage = "A descrição da avaria não pode exceder 500 caracteres")]
        public string Malfunction { get; set; }

        /// <summary>
        /// Descrição da reparação efetuada (preenchida pelo técnico)
        /// </summary>
        [StringLength(1000, ErrorMessage = "A descrição da reparação não pode exceder 1000 caracteres")]
        public string RepairDescription { get; set; }

        /// <summary>
        /// Peças utilizadas na reparação (preenchida pelo técnico)
        /// </summary>
        [StringLength(500, ErrorMessage = "A lista de peças não pode exceder 500 caracteres")]
        public string Parts { get; set; }

        /// <summary>
        /// Propriedade que retorna o tipo do ticket
        /// </summary>
        public override TicketType Type => TicketType.Hardware;

        /// <summary>
        /// Construtor padrão
        /// </summary>
        public HardwareTicket() : base()
        {
        }

        /// <summary>
        /// Construtor com parâmetros
        /// </summary>
        /// <param name="sequentialNumber">Número sequencial do ticket</param>
        /// <param name="userId">ID do utilizador que criou o ticket</param>
        /// <param name="equipment">Equipamento relacionado</param>
        /// <param name="malfunction">Descrição da avaria</param>
        public HardwareTicket(int sequentialNumber, int userId, string equipment, string malfunction)
            : base(sequentialNumber, userId)
        {
            Equipment = equipment ?? throw new ArgumentNullException(nameof(equipment));
            Malfunction = malfunction ?? throw new ArgumentNullException(nameof(malfunction));
        }

        /// <summary>
        /// Valida se o ticket de hardware tem todos os campos obrigatórios preenchidos
        /// </summary>
        /// <returns>True se o ticket é válido</returns>
        public override bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Equipment) &&
                   !string.IsNullOrWhiteSpace(Malfunction) &&
                   UserId > 0 &&
                   SequentialNumber > 0;
        }

        /// <summary>
        /// Método para atender ticket de hardware
        /// Adiciona validações específicas para hardware
        /// </summary>
        /// <param name="technicianId">ID do técnico</param>
        public override void AttendTicket(int technicianId)
        {
            if (!IsValid())
            {
                throw new InvalidOperationException("Ticket de hardware inválido. Verifique se todos os campos obrigatórios estão preenchidos.");
            }

            base.AttendTicket(technicianId);
        }

        /// <summary>
        /// Método para completar o atendimento do ticket de hardware
        /// </summary>
        /// <param name="attendanceStatus">Estado do atendimento</param>
        /// <param name="repairDescription">Descrição da reparação</param>
        /// <param name="parts">Peças utilizadas</param>
        public void CompleteHardwareAttendance(AttendanceStatus attendanceStatus, string repairDescription, string parts = null)
        {
            if (string.IsNullOrWhiteSpace(repairDescription))
            {
                throw new ArgumentException("A descrição da reparação é obrigatória para completar o atendimento.");
            }

            RepairDescription = repairDescription;
            Parts = parts;

            base.CompleteAttendance(attendanceStatus);
        }

        /// <summary>
        /// Retorna informações específicas do ticket de hardware
        /// </summary>
        /// <returns>String com informações específicas</returns>
        public override string GetSpecificInfo()
        {
            var info = $"Equipamento: {Equipment}\nAvaria: {Malfunction}";

            if (!string.IsNullOrWhiteSpace(RepairDescription))
            {
                info += $"\nReparação: {RepairDescription}";
            }

            if (!string.IsNullOrWhiteSpace(Parts))
            {
                info += $"\nPeças: {Parts}";
            }

            return info;
        }

        /// <summary>
        /// Método para verificar se o ticket pode ser considerado urgente
        /// Baseado em palavras-chave na descrição da avaria
        /// </summary>
        /// <returns>True se for considerado urgente</returns>
        public bool IsUrgent()
        {
            var urgentKeywords = new[] { "servidor", "rede", "crítico", "emergência", "fogo", "fumo", "não liga" };
            var malfunctionLower = Malfunction.ToLower();

            foreach (var keyword in urgentKeywords)
            {
                if (malfunctionLower.Contains(keyword))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Calcula uma estimativa de tempo de reparação baseada no tipo de equipamento
        /// </summary>
        /// <returns>Estimativa em horas</returns>
        public int GetEstimatedRepairTime()
        {
            var equipmentLower = Equipment.ToLower();

            if (equipmentLower.Contains("servidor"))
                return 4;
            else if (equipmentLower.Contains("impressora"))
                return 1;
            else if (equipmentLower.Contains("computador") || equipmentLower.Contains("pc"))
                return 2;
            else if (equipmentLower.Contains("monitor"))
                return 1;
            else
                return 2; // Default
        }

        /// <summary>
        /// Override do ToString para representação específica do ticket de hardware
        /// </summary>
        /// <returns>Representação string do ticket</returns>
        public override string ToString()
        {
            return $"{base.ToString()} - {Equipment}: {Malfunction}";
        }
    }
}