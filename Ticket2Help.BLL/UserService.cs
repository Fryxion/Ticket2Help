using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Ticket2Help.BLL.Models;
using Ticket2Help.DAL.Interfaces;
using Ticket2Help.DAL.Repositories;

namespace Ticket2Help.BLL.Services
{
    /// <summary>
    /// Interface para o serviço de utilizadores
    /// Define as operações de negócio relacionadas com utilizadores
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Autentica um utilizador
        /// </summary>
        /// <param name="username">Nome de utilizador</param>
        /// <param name="password">Password</param>
        /// <returns>Utilizador autenticado ou null</returns>
        Task<User> AuthenticateAsync(string username, string password);

        /// <summary>
        /// Cria um novo utilizador
        /// </summary>
        /// <param name="user">Dados do utilizador</param>
        /// <param name="password">Password em texto limpo</param>
        /// <returns>Utilizador criado</returns>
        Task<User> CreateUserAsync(User user, string password);

        /// <summary>
        /// Obtém um utilizador pelo ID
        /// </summary>
        /// <param name="userId">ID do utilizador</param>
        /// <returns>Utilizador encontrado</returns>
        Task<User> GetUserByIdAsync(int userId);

        /// <summary>
        /// Obtém um utilizador pelo nome de utilizador
        /// </summary>
        /// <param name="username">Nome de utilizador</param>
        /// <returns>Utilizador encontrado</returns>
        Task<User> GetUserByUsernameAsync(string username);

        /// <summary>
        /// Obtém todos os técnicos ativos
        /// </summary>
        /// <returns>Lista de técnicos</returns>
        Task<IEnumerable<User>> GetActiveTechniciansAsync();

        /// <summary>
        /// Atualiza um utilizador
        /// </summary>
        /// <param name="user">Utilizador a atualizar</param>
        /// <returns>True se bem-sucedido</returns>
        Task<bool> UpdateUserAsync(User user);

        /// <summary>
        /// Altera a password de um utilizador
        /// </summary>
        /// <param name="userId">ID do utilizador</param>
        /// <param name="currentPassword">Password atual</param>
        /// <param name="newPassword">Nova password</param>
        /// <returns>True se bem-sucedido</returns>
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);

        /// <summary>
        /// Ativa ou desativa um utilizador
        /// </summary>
        /// <param name="userId">ID do utilizador</param>
        /// <param name="isActive">Estado ativo</param>
        /// <returns>True se bem-sucedido</returns>
        Task<bool> SetUserActiveStatusAsync(int userId, bool isActive);

        /// <summary>
        /// Verifica se um nome de utilizador já existe
        /// </summary>
        /// <param name="username">Nome de utilizador</param>
        /// <param name="excludeUserId">ID do utilizador a excluir da verificação (para updates)</param>
        /// <returns>True se já existe</returns>
        Task<bool> UsernameExistsAsync(string username, int? excludeUserId = null);

        /// <summary>
        /// Verifica se um email já existe
        /// </summary>
        /// <param name="email">Email</param>
        /// <param name="excludeUserId">ID do utilizador a excluir da verificação (para updates)</param>
        /// <returns>True se já existe</returns>
        Task<bool> EmailExistsAsync(string email, int? excludeUserId = null);
    }

    /// <summary>
    /// Implementação do serviço de utilizadores
    /// Contém a lógica de negócio para gestão de utilizadores
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        /// <summary>
        /// Construtor do serviço
        /// </summary>
        /// <param name="userRepository">Repositório de utilizadores</param>
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        /// <summary>
        /// Autentica um utilizador
        /// </summary>
        /// <param name="username">Nome de utilizador</param>
        /// <param name="password">Password</param>
        /// <returns>Utilizador autenticado ou null</returns>
        public async Task<User> AuthenticateAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return null;

            // Obter utilizador pelo nome
            var user = await _userRepository.GetUserByUsernameAsync(username);
            if (user == null || !user.IsActive)
                return null;

            // Verificar password
            var hashedPassword = HashPassword(password);
            if (user.Password != hashedPassword)
                return null;

            // Atualizar último login
            user.UpdateLastLogin();
            await _userRepository.UpdateUserAsync(user);

            return user;
        }

        /// <summary>
        /// Cria um novo utilizador
        /// </summary>
        /// <param name="user">Dados do utilizador</param>
        /// <param name="password">Password em texto limpo</param>
        /// <returns>Utilizador criado</returns>
        public async Task<User> CreateUserAsync(User user, string password)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password é obrigatória", nameof(password));

            // Validar dados
            if (!user.IsValid())
                throw new InvalidOperationException("Dados do utilizador são inválidos");

            // Verificar se username já existe
            if (await UsernameExistsAsync(user.Username))
                throw new InvalidOperationException("Nome de utilizador já existe");

            // Verificar se email já existe
            if (await EmailExistsAsync(user.Email))
                throw new InvalidOperationException("Email já existe");

            // Hash da password
            user.Password = HashPassword(password);

            // Criar utilizador
            var userId = await _userRepository.AddUserAsync(user);
            user.Id = userId;

            return user;
        }

        /// <summary>
        /// Obtém um utilizador pelo ID
        /// </summary>
        /// <param name="userId">ID do utilizador</param>
        /// <returns>Utilizador encontrado</returns>
        public async Task<User> GetUserByIdAsync(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException("ID do utilizador inválido", nameof(userId));

            return await _userRepository.GetUserByIdAsync(userId);
        }

        /// <summary>
        /// Obtém um utilizador pelo nome de utilizador
        /// </summary>
        /// <param name="username">Nome de utilizador</param>
        /// <returns>Utilizador encontrado</returns>
        public async Task<User> GetUserByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Nome de utilizador é obrigatório", nameof(username));

            return await _userRepository.GetUserByUsernameAsync(username);
        }

        /// <summary>
        /// Obtém todos os técnicos ativos
        /// </summary>
        /// <returns>Lista de técnicos</returns>
        public async Task<IEnumerable<User>> GetActiveTechniciansAsync()
        {
            var allUsers = await _userRepository.GetAllUsersAsync();
            var technicians = new List<User>();

            foreach (var user in allUsers)
            {
                if (user.IsActive && user.IsTechnician())
                {
                    technicians.Add(user);
                }
            }

            return technicians;
        }

        /// <summary>
        /// Atualiza um utilizador
        /// </summary>
        /// <param name="user">Utilizador a atualizar</param>
        /// <returns>True se bem-sucedido</returns>
        public async Task<bool> UpdateUserAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (!user.IsValid())
                throw new InvalidOperationException("Dados do utilizador são inválidos");

            // Verificar se username já existe (excluindo o próprio utilizador)
            if (await UsernameExistsAsync(user.Username, user.Id))
                throw new InvalidOperationException("Nome de utilizador já existe");

            // Verificar se email já existe (excluindo o próprio utilizador)
            if (await EmailExistsAsync(user.Email, user.Id))
                throw new InvalidOperationException("Email já existe");

            try
            {
                await _userRepository.UpdateUserAsync(user);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Altera a password de um utilizador
        /// </summary>
        /// <param name="userId">ID do utilizador</param>
        /// <param name="currentPassword">Password atual</param>
        /// <param name="newPassword">Nova password</param>
        /// <returns>True se bem-sucedido</returns>
        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(currentPassword))
                throw new ArgumentException("Password atual é obrigatória", nameof(currentPassword));

            if (string.IsNullOrWhiteSpace(newPassword))
                throw new ArgumentException("Nova password é obrigatória", nameof(newPassword));

            if (newPassword.Length < 6)
                throw new ArgumentException("Nova password deve ter pelo menos 6 caracteres", nameof(newPassword));

            // Obter utilizador
            var user = await GetUserByIdAsync(userId);
            if (user == null)
                throw new ArgumentException("Utilizador não encontrado");

            // Verificar password atual
            var hashedCurrentPassword = HashPassword(currentPassword);
            if (user.Password != hashedCurrentPassword)
                throw new InvalidOperationException("Password atual incorreta");

            // Atualizar password
            user.Password = HashPassword(newPassword);

            try
            {
                await _userRepository.UpdateUserAsync(user);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Ativa ou desativa um utilizador
        /// </summary>
        /// <param name="userId">ID do utilizador</param>
        /// <param name="isActive">Estado ativo</param>
        /// <returns>True se bem-sucedido</returns>
        public async Task<bool> SetUserActiveStatusAsync(int userId, bool isActive)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null)
                return false;

            user.IsActive = isActive;

            try
            {
                await _userRepository.UpdateUserAsync(user);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Verifica se um nome de utilizador já existe
        /// </summary>
        /// <param name="username">Nome de utilizador</param>
        /// <param name="excludeUserId">ID do utilizador a excluir da verificação</param>
        /// <returns>True se já existe</returns>
        public async Task<bool> UsernameExistsAsync(string username, int? excludeUserId = null)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            var existingUser = await _userRepository.GetUserByUsernameAsync(username);

            if (existingUser == null)
                return false;

            // Se temos um ID a excluir e é o mesmo utilizador, não considera como duplicado
            if (excludeUserId.HasValue && existingUser.Id == excludeUserId.Value)
                return false;

            return true;
        }

        /// <summary>
        /// Verifica se um email já existe
        /// </summary>
        /// <param name="email">Email</param>
        /// <param name="excludeUserId">ID do utilizador a excluir da verificação</param>
        /// <returns>True se já existe</returns>
        public async Task<bool> EmailExistsAsync(string email, int? excludeUserId = null)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            var existingUser = await _userRepository.GetUserByEmailAsync(email);

            if (existingUser == null)
                return false;

            // Se temos um ID a excluir e é o mesmo utilizador, não considera como duplicado
            if (excludeUserId.HasValue && existingUser.Id == excludeUserId.Value)
                return false;

            return true;
        }

        /// <summary>
        /// Gera hash seguro para a password
        /// Utiliza SHA256 para simplificação (em produção, usar bcrypt ou similar)
        /// </summary>
        /// <param name="password">Password em texto limpo</param>
        /// <returns>Hash da password</returns>
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                // Adiciona um "salt" fixo (em produção, usar salt único por utilizador)
                var saltedPassword = password + "Ticket2Help_Salt_2024";
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));

                return Convert.ToBase64String(hashedBytes);
            }
        }

        /// <summary>
        /// Cria utilizadores padrão do sistema (apenas para desenvolvimento/testes)
        /// </summary>
        /// <returns>True se criados com sucesso</returns>
        public async Task<bool> CreateDefaultUsersAsync()
        {
            try
            {
                // Verificar se já existem utilizadores
                var existingUsers = await _userRepository.GetAllUsersAsync();
                if (existingUsers.Any())
                    return true; // Já existem utilizadores

                // Criar administrador padrão
                var admin = new User("admin", "", "Administrador do Sistema", "admin@ticket2help.com")
                {
                    Role = UserRole.Administrator,
                    Department = "TI",
                    Position = "Administrador"
                };
                await CreateUserAsync(admin, "admin123");

                // Criar técnico padrão
                var technician = new User("tecnico", "", "João Silva", "joao.silva@ticket2help.com")
                {
                    Role = UserRole.Technician,
                    Department = "TI",
                    Position = "Técnico de Helpdesk"
                };
                await CreateUserAsync(technician, "tecnico123");

                // Criar utilizador padrão
                var user = new User("user", "", "Maria Santos", "maria.santos@ticket2help.com")
                {
                    Role = UserRole.User,
                    Department = "Contabilidade",
                    Position = "Contabilista"
                };
                await CreateUserAsync(user, "user123");

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}