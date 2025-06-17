using System;
using System.ComponentModel.DataAnnotations;

namespace Ticket2Help.BLL.Models
{
    /// <summary>
    /// Enumeração para os estados do ticket
    /// </summary>
    public enum TicketStatus
    {
        PorAtender = 0,
        EmAtendimento = 1,
        Atendido = 2
    }

    /// <summary>
    /// Enumeração para os estados de atendimento
    /// </summary>
    public enum AttendanceStatus
    {
        Aberto = 0,
        Resolvido = 1,
        NaoResolvido = 2
    }

    /// <summary>
    /// Enumeração para os tipos de ticket
    /// </summary>
    public enum TicketType
    {
        Hardware = 0,
        Software = 1
    }

    /// <summary>
    /// Classe base abstrata para todos os tipos de tickets
    /// Implementa o padrão Template Method para a estrutura comum dos tickets
    /// </summary>
    public abstract class Ticket
    {
        /// <summary>
        /// ID único do ticket (gerado automaticamente)
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Número sequencial do ticket
        /// </summary>
        [Required]
        public int SequentialNumber { get; set; }

        /// <summary>
        /// Data e hora de criação do ticket (gerada automaticamente)
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Data e hora de atendimento do ticket
        /// </summary>
        public DateTime? AttendedDate { get; set; }

        /// <summary>
        /// Código do colaborador que submeteu o ticket
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// Estado atual do ticket
        /// </summary>
        [Required]
        public TicketStatus Status { get; set; }

        /// <summary>
        /// Estado do atendimento (apenas aplicável quando o ticket está atendido)
        /// </summary>
        public AttendanceStatus? AttendanceStatus { get; set; }

        /// <summary>
        /// ID do técnico que atendeu o ticket
        /// </summary>
        public int? TechnicianId { get; set; }

        /// <summary>
        /// Tipo do ticket (Hardware ou Software)
        /// </summary>
        [Required]
        public abstract TicketType Type { get; }

        /// <summary>
        /// Construtor da classe base
        /// Inicializa os valores padrão
        /// </summary>
        protected Ticket()
        {
            CreatedDate = DateTime.Now;
            Status = TicketStatus.PorAtender;
        }

        /// <summary>
        /// Construtor com parâmetros
        /// </summary>
        /// <param name="sequentialNumber">Número sequencial do ticket</param>
        /// <param name="userId">ID do utilizador que criou o ticket</param>
        protected Ticket(int sequentialNumber, int userId) : this()
        {
            SequentialNumber = sequentialNumber;
            UserId = userId;
        }

        /// <summary>
        /// Método para atender o ticket
        /// Template Method - define o fluxo comum de atendimento
        /// </summary>
        /// <param name="technicianId">ID do técnico que está a atender</param>
        public virtual void AttendTicket(int technicianId)
        {
            if (Status != TicketStatus.PorAtender)
            {
                throw new InvalidOperationException("Apenas tickets por atender podem ser atendidos.");
            }

            Status = TicketStatus.EmAtendimento;
            AttendedDate = DateTime.Now;
            TechnicianId = technicianId;
        }

        /// <summary>
        /// Método para completar o atendimento
        /// </summary>
        /// <param name="attendanceStatus">Estado final do atendimento</param>
        public virtual void CompleteAttendance(AttendanceStatus attendanceStatus)
        {
            if (Status != TicketStatus.EmAtendimento)
            {
                throw new InvalidOperationException("Apenas tickets em atendimento podem ser completados.");
            }

            Status = TicketStatus.Atendido;
            AttendanceStatus = attendanceStatus;
        }

        /// <summary>
        /// Método abstrato para validação específica de cada tipo de ticket
        /// </summary>
        /// <returns>True se o ticket é válido</returns>
        public abstract bool IsValid();

        /// <summary>
        /// Método para obter informações específicas do ticket
        /// Template Method - cada subclasse implementa suas especificidades
        /// </summary>
        /// <returns>String com informações específicas</returns>
        public abstract string GetSpecificInfo();

        /// <summary>
        /// Override do ToString para representação textual do ticket
        /// </summary>
        /// <returns>Representação string do ticket</returns>
        public override string ToString()
        {
            return $"Ticket #{SequentialNumber} - {Type} - {Status}";
        }
    }
}