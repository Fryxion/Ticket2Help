using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Ticket2Help.BLL.Models;
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
        /// <returns>True se criado com sucesso</returns>
        Task<bool> CreateUserAsync(User user, string password);

        /// <summary>
        /// Obtém um utilizador pelo ID
        /// </summary>
        /// <param name="userId">ID do utilizador</param>
        /// <returns>Utilizador encontrado</returns>
        Task<User> GetUserByIdAsync(string userId);

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
        Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);

        /// <summary>
        /// Ativa ou desativa um utilizador
        /// </summary>
        /// <param name="userId">ID do utilizador</param>
        /// <param name="isActive">Estado ativo</param>
        /// <returns>True se bem-sucedido</returns>
        Task<bool> SetUserActiveStatusAsync(string userId, bool isActive);

        /// <summary>
        /// Verifica se um nome de utilizador já existe
        /// </summary>
        /// <param name="username">Nome de utilizador</param>
        /// <param name="excludeUserId">ID do utilizador a excluir da verificação</param>
        /// <returns>True se já existe</returns>
        Task<bool> UsernameExistsAsync(string username, string excludeUserId = null);
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
        /// Autentica um utilizador usando o DAL existente
        /// </summary>
        public async Task<User> AuthenticateAsync(string username, string password)
        {
            return await Task.Run(() =>
            {
                var dalUser = _userRepository.AuthenticateUser(username, password);
                return ModelMapper.MapToBll(dalUser);
            });
        }

        /// <summary>
        /// Cria um novo utilizador
        /// </summary>
        public async Task<bool> CreateUserAsync(User user, string password)
        {
            return await Task.Run(() =>
            {
                if (user == null)
                    throw new ArgumentNullException(nameof(user));

                if (string.IsNullOrWhiteSpace(password))
                    throw new ArgumentException("Password é obrigatória", nameof(password));

                if (!user.IsValid())
                    throw new InvalidOperationException("Dados do utilizador são inválidos");

                if (_userRepository.UsernameExists(user.Username))
                    throw new InvalidOperationException("Nome de utilizador já existe");

                // Mapear para DAL e definir password
                var dalUser = ModelMapper.MapToDal(user);
                dalUser.PasswordHash = password;

                return _userRepository.InsertUser(dalUser);
            });
        }

        /// <summary>
        /// Obtém um utilizador pelo ID
        /// </summary>
        public async Task<User> GetUserByIdAsync(string userId)
        {
            return await Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(userId))
                    throw new ArgumentException("ID do utilizador inválido", nameof(userId));

                var dalUser = _userRepository.GetUserById(userId);
                return ModelMapper.MapToBll(dalUser);
            });
        }

        /// <summary>
        /// Obtém um utilizador pelo nome de utilizador
        /// </summary>
        public async Task<User> GetUserByUsernameAsync(string username)
        {
            return await Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(username))
                    throw new ArgumentException("Nome de utilizador é obrigatório", nameof(username));

                var allUsers = _userRepository.GetAllUsers();
                var dalUser = allUsers.FirstOrDefault(u =>
                    u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

                return ModelMapper.MapToBll(dalUser);
            });
        }

        /// <summary>
        /// Obtém todos os técnicos ativos
        /// </summary>
        public async Task<IEnumerable<User>> GetActiveTechniciansAsync()
        {
            return await Task.Run(() =>
            {
                var dalUsers = _userRepository.GetUsersByTipo(DAL.Models.TipoUtilizador.Tecnico);
                return dalUsers.Where(u => u.Ativo).Select(ModelMapper.MapToBll).ToList();
            });
        }

        /// <summary>
        /// Atualiza um utilizador
        /// </summary>
        public async Task<bool> UpdateUserAsync(User user)
        {
            return await Task.Run(() =>
            {
                if (user == null)
                    throw new ArgumentNullException(nameof(user));

                if (!user.IsValid())
                    throw new InvalidOperationException("Dados do utilizador são inválidos");

                var dalUser = ModelMapper.MapToDal(user);
                return _userRepository.UpdateUser(dalUser);
            });
        }

        /// <summary>
        /// Altera a password de um utilizador
        /// </summary>
        public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            return await Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(currentPassword))
                    throw new ArgumentException("Password atual é obrigatória", nameof(currentPassword));

                if (string.IsNullOrWhiteSpace(newPassword))
                    throw new ArgumentException("Nova password é obrigatória", nameof(newPassword));

                if (newPassword.Length < 6)
                    throw new ArgumentException("Nova password deve ter pelo menos 6 caracteres", nameof(newPassword));

                var dalUser = _userRepository.GetUserById(userId);
                if (dalUser == null)
                    throw new ArgumentException("Utilizador não encontrado");

                var hashedCurrentPassword = HashPassword(currentPassword);
                if (dalUser.PasswordHash != hashedCurrentPassword)
                    throw new InvalidOperationException("Password atual incorreta");

                dalUser.PasswordHash = HashPassword(newPassword);
                return _userRepository.UpdateUser(dalUser);
            });
        }

        /// <summary>
        /// Ativa ou desativa um utilizador
        /// </summary>
        public async Task<bool> SetUserActiveStatusAsync(string userId, bool isActive)
        {
            return await Task.Run(() =>
            {
                var dalUser = _userRepository.GetUserById(userId);
                if (dalUser == null)
                    return false;

                dalUser.Ativo = isActive;
                return _userRepository.UpdateUser(dalUser);
            });
        }

        /// <summary>
        /// Verifica se um nome de utilizador já existe
        /// </summary>
        public async Task<bool> UsernameExistsAsync(string username, string excludeUserId = null)
        {
            return await Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(username))
                    return false;

                // Se temos um ID a excluir, verificamos se é o mesmo utilizador
                if (!string.IsNullOrWhiteSpace(excludeUserId))
                {
                    var existingUser = GetUserByUsernameAsync(username).Result;
                    if (existingUser != null && existingUser.UserId == excludeUserId)
                        return false;
                }

                return _userRepository.UsernameExists(username);
            });
        }

        /// <summary>
        /// Gera hash seguro para a password (compatível com o UserRepository)
        /// </summary>
        private string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password não pode ser nula ou vazia", nameof(password));

            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}