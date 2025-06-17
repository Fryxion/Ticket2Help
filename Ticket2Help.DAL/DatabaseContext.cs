using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using Microsoft.IdentityModel.Protocols;

namespace Ticket2Help.DAL.Data
{
    /// <summary>
    /// Classe responsável pela gestão da conexão com a base de dados SQL Server
    /// </summary>
    public class DatabaseContext : IDisposable
    {
        private SqlConnection _connection;
        private bool _disposed = false;

        /// <summary>
        /// String de conexão obtida do ficheiro de configuração
        /// </summary>
        private readonly string _connectionString;

        /// <summary>
        /// Construtor que inicializa o contexto da base de dados
        /// </summary>
        public DatabaseContext()
        {
            // Obtém a string de conexão do ficheiro app.config
            _connectionString = ConfigurationManager.ConnectionStrings["Ticket2HelpDB"]?.ConnectionString
                ?? throw new InvalidOperationException("String de conexão 'Ticket2HelpDB' não encontrada no ficheiro de configuração.");
        }

        /// <summary>
        /// Construtor que permite especificar uma string de conexão personalizada
        /// </summary>
        /// <param name="connectionString">String de conexão personalizada</param>
        public DatabaseContext(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        /// <summary>
        /// Obtém uma conexão ativa com a base de dados
        /// </summary>
        /// <returns>Conexão SQL ativa</returns>
        public SqlConnection GetConnection()
        {
            if (_connection == null)
            {
                _connection = new SqlConnection(_connectionString);
            }

            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }

            return _connection;
        }

        /// <summary>
        /// Testa se a conexão com a base de dados está funcional
        /// </summary>
        /// <returns>True se a conexão está operacional, False caso contrário</returns>
        public bool TestConnection()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    return connection.State == ConnectionState.Open;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Executa um comando SQL que não retorna dados (INSERT, UPDATE, DELETE)
        /// </summary>
        /// <param name="commandText">Comando SQL a executar</param>
        /// <param name="parameters">Parâmetros do comando</param>
        /// <returns>Número de linhas afetadas</returns>
        public int ExecuteNonQuery(string commandText, params SqlParameter[] parameters)
        {
            using (var command = new SqlCommand(commandText, GetConnection()))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                return command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Executa um comando SQL que retorna um valor único (COUNT, MAX, etc.)
        /// </summary>
        /// <param name="commandText">Comando SQL a executar</param>
        /// <param name="parameters">Parâmetros do comando</param>
        /// <returns>Valor escalar retornado pelo comando</returns>
        public object ExecuteScalar(string commandText, params SqlParameter[] parameters)
        {
            using (var command = new SqlCommand(commandText, GetConnection()))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                return command.ExecuteScalar();
            }
        }

        /// <summary>
        /// Executa um comando SQL que retorna dados (SELECT)
        /// </summary>
        /// <param name="commandText">Comando SQL a executar</param>
        /// <param name="parameters">Parâmetros do comando</param>
        /// <returns>SqlDataReader com os dados retornados</returns>
        public SqlDataReader ExecuteReader(string commandText, params SqlParameter[] parameters)
        {
            var command = new SqlCommand(commandText, GetConnection());

            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }

            return command.ExecuteReader(CommandBehavior.CloseConnection);
        }

        /// <summary>
        /// Executa um comando SQL e retorna um DataTable com os resultados
        /// </summary>
        /// <param name="commandText">Comando SQL a executar</param>
        /// <param name="parameters">Parâmetros do comando</param>
        /// <returns>DataTable com os dados retornados</returns>
        public DataTable ExecuteDataTable(string commandText, params SqlParameter[] parameters)
        {
            using (var command = new SqlCommand(commandText, GetConnection()))
            {
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                using (var adapter = new SqlDataAdapter(command))
                {
                    var dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    return dataTable;
                }
            }
        }

        /// <summary>
        /// Inicia uma transação na base de dados
        /// </summary>
        /// <param name="isolationLevel">Nível de isolamento da transação</param>
        /// <returns>Objeto SqlTransaction para controlar a transação</returns>
        public SqlTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return GetConnection().BeginTransaction(isolationLevel);
        }

        /// <summary>
        /// Implementação do padrão Dispose para libertar recursos
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Método protegido para libertar recursos geridos e não geridos
        /// </summary>
        /// <param name="disposing">Indica se deve libertar recursos geridos</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _connection?.Close();
                    _connection?.Dispose();
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// Destructor que garante a libertação de recursos
        /// </summary>
        ~DatabaseContext()
        {
            Dispose(false);
        }
    }
}