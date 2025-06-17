using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using Ticket2Help.DAL.Models;
using Ticket2Help.DAL.Data;
using System.Data;

namespace Ticket2Help.DAL.Repositories
{
    /// <summary>
    /// Implementação concreta do repositório de utilizadores para SQL Server
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly DatabaseContext _context;

        /// <summary>
        /// Construtor que inicializa o repositório com o contexto da base de dados
        /// </summary>
        /// <param name="context">Contexto da base de dados</param>
        public UserRepository(DatabaseContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Autentica um utilizador com username e password
        /// </summary>
        /// <param name="username">Nome de utilizador</param>
        /// <param name="password">Password do utilizador</param>
        /// <returns>Utilizador autenticado ou null se as credenciais forem inválidas</returns>
        public User AuthenticateUser(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            const string query = @"
                SELECT UserId, Username, PasswordHash, Nome, Email, TipoUtilizador, Ativo, DataCriacao
                FROM Users 
                WHERE Username = @Username AND Ativo = 1";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Username", username)
            };

            using (var reader = _context.ExecuteReader(query, parameters))
            {
                if (reader.Read())
                {
                    var storedPasswordHash = reader.GetString("PasswordHash");
                    var inputPasswordHash = password;

                    if (storedPasswordHash == inputPasswordHash)
                    {
                        return MapReaderToUser(reader);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Obtém um utilizador pelo seu ID
        /// </summary>
        /// <param name="userId">ID do utilizador</param>
        /// <returns>Utilizador encontrado ou null se não existir</returns>
        public User GetUserById(string userId)
        {
            const string query = @"
                SELECT UserId, Username, PasswordHash, Nome, Email, TipoUtilizador, Ativo, DataCriacao
                FROM Users 
                WHERE UserId = @UserId";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@UserId", userId)
            };

            using (var reader = _context.ExecuteReader(query, parameters))
            {
                if (reader.Read())
                {
                    return MapReaderToUser(reader);
                }
            }

            return null;
        }

        /// <summary>
        /// Obtém todos os utilizadores da base de dados
        /// </summary>
        /// <returns>Lista de todos os utilizadores</returns>
        public List<User> GetAllUsers()
        {
            var users = new List<User>();
            const string query = @"
                SELECT UserId, Username, PasswordHash, Nome, Email, TipoUtilizador, Ativo, DataCriacao
                FROM Users 
                ORDER BY Nome";

            using (var reader = _context.ExecuteReader(query))
            {
                while (reader.Read())
                {
                    users.Add(MapReaderToUser(reader));
                }
            }

            return users;
        }

        /// <summary>
        /// Obtém utilizadores filtrados por tipo (Técnico ou Colaborador)
        /// </summary>
        /// <param name="tipoUtilizador">Tipo de utilizador</param>
        /// <returns>Lista de utilizadores do tipo especificado</returns>
        public List<User> GetUsersByTipo(TipoUtilizador tipoUtilizador)
        {
            var users = new List<User>();
            const string query = @"
                SELECT UserId, Username, PasswordHash, Nome, Email, TipoUtilizador, Ativo, DataCriacao
                FROM Users 
                WHERE TipoUtilizador = @TipoUtilizador AND Ativo = 1
                ORDER BY Nome";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@TipoUtilizador", (int)tipoUtilizador)
            };

            using (var reader = _context.ExecuteReader(query, parameters))
            {
                while (reader.Read())
                {
                    users.Add(MapReaderToUser(reader));
                }
            }

            return users;
        }

        /// <summary>
        /// Insere um novo utilizador na base de dados
        /// </summary>
        /// <param name="user">Utilizador a ser inserido</param>
        /// <returns>True se a inserção foi bem-sucedida, False caso contrário</returns>
        public bool InsertUser(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (UsernameExists(user.Username))
            {
                return false; // Username já existe
            }

            const string query = @"
                INSERT INTO Users (UserId, Username, PasswordHash, Nome, Email, TipoUtilizador, Ativo, DataCriacao)
                VALUES (@UserId, @Username, @PasswordHash, @Nome, @Email, @TipoUtilizador, @Ativo, @DataCriacao)";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@UserId", user.UserId),
                new SqlParameter("@Username", user.Username),
                new SqlParameter("@PasswordHash", user.PasswordHash),
                new SqlParameter("@Nome", user.Nome),
                new SqlParameter("@Email", (object)user.Email ?? DBNull.Value),
                new SqlParameter("@TipoUtilizador", (int)user.TipoUtilizador),
                new SqlParameter("@Ativo", user.Ativo),
                new SqlParameter("@DataCriacao", user.DataCriacao)
            };

            try
            {
                var rowsAffected = _context.ExecuteNonQuery(query, parameters);
                return rowsAffected > 0;
            }
            catch (SqlException)
            {
                return false;
            }
        }

        /// <summary>
        /// Atualiza um utilizador existente na base de dados
        /// </summary>
        /// <param name="user">Utilizador com dados atualizados</param>
        /// <returns>True se a atualização foi bem-sucedida, False caso contrário</returns>
        public bool UpdateUser(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            const string query = @"
                UPDATE Users 
                SET Username = @Username,
                    PasswordHash = @PasswordHash,
                    Nome = @Nome,
                    Email = @Email,
                    TipoUtilizador = @TipoUtilizador,
                    Ativo = @Ativo
                WHERE UserId = @UserId";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@UserId", user.UserId),
                new SqlParameter("@Username", user.Username),
                new SqlParameter("@PasswordHash", user.PasswordHash),
                new SqlParameter("@Nome", user.Nome),
                new SqlParameter("@Email", (object)user.Email ?? DBNull.Value),
                new SqlParameter("@TipoUtilizador", (int)user.TipoUtilizador),
                new SqlParameter("@Ativo", user.Ativo)
            };

            try
            {
                var rowsAffected = _context.ExecuteNonQuery(query, parameters);
                return rowsAffected > 0;
            }
            catch (SqlException)
            {
                return false;
            }
        }

        /// <summary>
        /// Remove um utilizador da base de dados (desativa em vez de remover)
        /// </summary>
        /// <param name="userId">ID do utilizador a ser removido</param>
        /// <returns>True se a remoção foi bem-sucedida, False caso contrário</returns>
        public bool DeleteUser(string userId)
        {
            // Em vez de remover fisicamente, desativamos o utilizador
            const string query = @"
                UPDATE Users 
                SET Ativo = 0
                WHERE UserId = @UserId";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@UserId", userId)
            };

            try
            {
                var rowsAffected = _context.ExecuteNonQuery(query, parameters);
                return rowsAffected > 0;
            }
            catch (SqlException)
            {
                return false;
            }
        }

        /// <summary>
        /// Verifica se um username já existe na base de dados
        /// </summary>
        /// <param name="username">Username a verificar</param>
        /// <returns>True se o username já existe, False caso contrário</returns>
        public bool UsernameExists(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return false;
            }

            const string query = "SELECT COUNT(*) FROM Users WHERE Username = @Username";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Username", username)
            };

            try
            {
                var count = (int)_context.ExecuteScalar(query, parameters);
                return count > 0;
            }
            catch (SqlException)
            {
                return false;
            }
        }

        #region Métodos Privados

        /// <summary>
        /// Mapeia os dados do SqlDataReader para um objeto User
        /// </summary>
        /// <param name="reader">SqlDataReader com os dados</param>
        /// <returns>Objeto User mapeado</returns>
        private User MapReaderToUser(SqlDataReader reader)
        {
            return new User
            {
                UserId = reader.GetString("UserId"),
                Username = reader.GetString("Username"),
                PasswordHash = reader.GetString("PasswordHash"),
                Nome = reader.GetString("Nome"),
                Email = reader.IsDBNull("Email") ? null : reader.GetString("Email"),
                TipoUtilizador = (TipoUtilizador)reader.GetInt32("TipoUtilizador"),
                Ativo = reader.GetBoolean("Ativo"),
                DataCriacao = reader.GetDateTime("DataCriacao")
            };
        }

        /// <summary>
        /// Gera um hash SHA256 para a password
        /// </summary>
        /// <param name="password">Password em texto plano</param>
        /// <returns>Hash SHA256 da password</returns>
        private string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Password não pode ser nula ou vazia", nameof(password));
            }

            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        #endregion
    }
}