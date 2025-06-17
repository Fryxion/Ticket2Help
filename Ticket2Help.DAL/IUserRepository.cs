using System.Collections.Generic;
using Ticket2Help.BLL.Models;

namespace Ticket2Help.DAL.Repositories
{
    /// <summary>
    /// Interface que define as operações de acesso a dados para utilizadores
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Autentica um utilizador com username e password
        /// </summary>
        /// <param name="username">Nome de utilizador</param>
        /// <param name="password">Password do utilizador</param>
        /// <returns>Utilizador autenticado ou null se as credenciais forem inválidas</returns>
        User AuthenticateUser(string username, string password);

        /// <summary>
        /// Obtém um utilizador pelo seu ID
        /// </summary>
        /// <param name="userId">ID do utilizador</param>
        /// <returns>Utilizador encontrado ou null se não existir</returns>
        User GetUserById(string userId);

        /// <summary>
        /// Obtém todos os utilizadores da base de dados
        /// </summary>
        /// <returns>Lista de todos os utilizadores</returns>
        List<User> GetAllUsers();

        /// <summary>
        /// Obtém utilizadores filtrados por tipo (Técnico ou Colaborador)
        /// </summary>
        /// <param name="tipoUtilizador">Tipo de utilizador</param>
        /// <returns>Lista de utilizadores do tipo especificado</returns>
        List<User> GetUsersByTipo(TipoUtilizador tipoUtilizador);

        /// <summary>
        /// Insere um novo utilizador na base de dados
        /// </summary>
        /// <param name="user">Utilizador a ser inserido</param>
        /// <returns>True se a inserção foi bem-sucedida, False caso contrário</returns>
        bool InsertUser(User user);

        /// <summary>
        /// Atualiza um utilizador existente na base de dados
        /// </summary>
        /// <param name="user">Utilizador com dados atualizados</param>
        /// <returns>True se a atualização foi bem-sucedida, False caso contrário</returns>
        bool UpdateUser(User user);

        /// <summary>
        /// Remove um utilizador da base de dados
        /// </summary>
        /// <param name="userId">ID do utilizador a ser removido</param>
        /// <returns>True se a remoção foi bem-sucedida, False caso contrário</returns>
        bool DeleteUser(string userId);

        /// <summary>
        /// Verifica se um username já existe na base de dados
        /// </summary>
        /// <param name="username">Username a verificar</param>
        /// <returns>True se o username já existe, False caso contrário</returns>
        bool UsernameExists(string username);
    }
}