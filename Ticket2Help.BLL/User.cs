using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Ticket2Help.BLL.Models
{
    /// <summary>
    /// Enumeração para os tipos de utilizador no sistema
    /// </summary>
    public enum UserRole
    {
        /// <summary>
        /// Utilizador comum que pode criar tickets
        /// </summary>
        User = 0,

        /// <summary>
        /// Técnico de helpdesk que pode atender tickets
        /// </summary>
        Technician = 1,

        /// <summary>
        /// Administrador do sistema
        /// </summary>
        Administrator = 2
    }

    /// <summary>
    /// Classe que representa um utilizador do sistema
    /// Contém informações básicas e permissões do utilizador
    /// </summary>
    public class User
    {
        /// <summary>
        /// ID único do utilizador
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nome de utilizador para login (único no sistema)
        /// </summary>
        [Required(ErrorMessage = "O nome de utilizador é obrigatório")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "O nome de utilizador deve ter entre 3 e 50 caracteres")]
        public string Username { get; set; }

        /// <summary>
        /// Password do utilizador (deve ser guardada encriptada)
        /// </summary>
        [Required(ErrorMessage = "A password é obrigatória")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "A password deve ter pelo menos 6 caracteres")]
        public string Password { get; set; }

        /// <summary>
        /// Nome completo do utilizador
        /// </summary>
        [Required(ErrorMessage = "O nome completo é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome completo não pode exceder 100 caracteres")]
        public string FullName { get; set; }

        /// <summary>
        /// Email do utilizador
        /// </summary>
        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [StringLength(100, ErrorMessage = "O email não pode exceder 100 caracteres")]
        public string Email { get; set; }

        /// <summary>
        /// Departamento do utilizador
        /// </summary>
        [StringLength(50, ErrorMessage = "O departamento não pode exceder 50 caracteres")]
        public string Department { get; set; }

        /// <summary>
        /// Função/Cargo do utilizador
        /// </summary>
        [StringLength(50, ErrorMessage = "A função não pode exceder 50 caracteres")]
        public string Position { get; set; }

        /// <summary>
        /// Tipo/Papel do utilizador no sistema
        /// </summary>
        [Required]
        public UserRole Role { get; set; }

        /// <summary>
        /// Indica se o utilizador está ativo no sistema
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Data de criação da conta
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Data do último login
        /// </summary>
        public DateTime? LastLoginDate { get; set; }

        /// <summary>
        /// Construtor padrão
        /// </summary>
        public User()
        {
            CreatedDate = DateTime.Now;
            IsActive = true;
            Role = UserRole.User;
        }

        /// <summary>
        /// Construtor com parâmetros básicos
        /// </summary>
        /// <param name="username">Nome de utilizador</param>
        /// <param name="password">Password</param>
        /// <param name="fullName">Nome completo</param>
        /// <param name="email">Email</param>
        public User(string username, string password, string fullName, string email) : this()
        {
            Username = username ?? throw new ArgumentNullException(nameof(username));
            Password = password ?? throw new ArgumentNullException(nameof(password));
            FullName = fullName ?? throw new ArgumentNullException(nameof(fullName));
            Email = email ?? throw new ArgumentNullException(nameof(email));
        }

        /// <summary>
        /// Verifica se o utilizador é um técnico de helpdesk
        /// </summary>
        /// <returns>True se for técnico</returns>
        public bool IsTechnician()
        {
            return Role == UserRole.Technician || Role == UserRole.Administrator;
        }

        /// <summary>
        /// Verifica se o utilizador é administrador
        /// </summary>
        /// <returns>True se for administrador</returns>
        public bool IsAdministrator()
        {
            return Role == UserRole.Administrator;
        }

        /// <summary>
        /// Verifica se o utilizador pode criar tickets
        /// </summary>
        /// <returns>True se pode criar tickets</returns>
        public bool CanCreateTickets()
        {
            return IsActive && (Role == UserRole.User || Role == UserRole.Technician || Role == UserRole.Administrator);
        }

        /// <summary>
        /// Verifica se o utilizador pode atender tickets
        /// </summary>
        /// <returns>True se pode atender tickets</returns>
        public bool CanAttendTickets()
        {
            return IsActive && IsTechnician();
        }

        /// <summary>
        /// Verifica se o utilizador pode gerar relatórios/mapas
        /// </summary>
        /// <returns>True se pode gerar relatórios</returns>
        public bool CanGenerateReports()
        {
            return IsActive && IsTechnician();
        }

        /// <summary>
        /// Atualiza a data do último login
        /// </summary>
        public void UpdateLastLogin()
        {
            LastLoginDate = DateTime.Now;
        }

        /// <summary>
        /// Valida se os dados do utilizador são válidos
        /// </summary>
        /// <returns>True se os dados são válidos</returns>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   !string.IsNullOrWhiteSpace(FullName) &&
                   !string.IsNullOrWhiteSpace(Email) &&
                   Email.Contains("@") &&
                   Username.Length >= 3 &&
                   Password.Length >= 6;
        }

        /// <summary>
        /// Obtém uma descrição textual do papel do utilizador
        /// </summary>
        /// <returns>Descrição do papel</returns>
        public string GetRoleDescription()
        {
            return Role switch
            {
                UserRole.User => "Utilizador",
                UserRole.Technician => "Técnico de Helpdesk",
                UserRole.Administrator => "Administrador",
                _ => "Desconhecido"
            };
        }

        /// <summary>
        /// Gera uma representação string do utilizador
        /// </summary>
        /// <returns>String representativa</returns>
        public override string ToString()
        {
            return $"{FullName} ({Username}) - {GetRoleDescription()}";
        }

        /// <summary>
        /// Override do Equals para comparação de utilizadores
        /// </summary>
        /// <param name="obj">Objeto a comparar</param>
        /// <returns>True se são iguais</returns>
        public override bool Equals(object obj)
        {
            if (obj is User otherUser)
            {
                return Id == otherUser.Id && Username.Equals(otherUser.Username, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        /// <summary>
        /// Override do GetHashCode
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Username?.ToLower());
        }
    }
}