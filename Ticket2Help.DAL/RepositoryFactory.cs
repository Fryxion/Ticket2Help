using System;
using Ticket2Help.DAL.Data;

namespace Ticket2Help.DAL.Repositories
{
    /// <summary>
    /// Factory responsável pela criação e gestão de repositórios
    /// Implementa o padrão Factory para criação de objetos de acesso a dados
    /// </summary>
    public class RepositoryFactory : IDisposable
    {
        private readonly DatabaseContext _context;
        private bool _disposed = false;

        // Instâncias dos repositórios (Singleton pattern por contexto)
        private ITicketRepository _ticketRepository;
        private IUserRepository _userRepository;

        /// <summary>
        /// Construtor que inicializa a factory com um contexto de base de dados
        /// </summary>
        public RepositoryFactory()
        {
            _context = new DatabaseContext();
        }

        /// <summary>
        /// Construtor que permite especificar uma string de conexão personalizada
        /// </summary>
        /// <param name="connectionString">String de conexão personalizada</param>
        public RepositoryFactory(string connectionString)
        {
            _context = new DatabaseContext(connectionString);
        }

        /// <summary>
        /// Construtor que permite injeção de dependência do contexto
        /// </summary>
        /// <param name="context">Contexto da base de dados</param>
        public RepositoryFactory(DatabaseContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Obtém uma instância do repositório de tickets
        /// </summary>
        /// <returns>Instância do ITicketRepository</returns>
        public ITicketRepository GetTicketRepository()
        {
            if (_ticketRepository == null)
            {
                _ticketRepository = new TicketRepository(_context);
            }
            return _ticketRepository;
        }

        /// <summary>
        /// Obtém uma instância do repositório de utilizadores
        /// </summary>
        /// <returns>Instância do IUserRepository</returns>
        public IUserRepository GetUserRepository()
        {
            if (_userRepository == null)
            {
                _userRepository = new UserRepository(_context);
            }
            return _userRepository;
        }

        /// <summary>
        /// Obtém o contexto da base de dados (para operações avançadas ou transações)
        /// </summary>
        /// <returns>Instância do DatabaseContext</returns>
        public DatabaseContext GetDatabaseContext()
        {
            return _context;
        }

        /// <summary>
        /// Testa a conectividade com a base de dados
        /// </summary>
        /// <returns>True se a conexão está operacional, False caso contrário</returns>
        public bool TestDatabaseConnection()
        {
            return _context.TestConnection();
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
                    // Libertar recursos geridos
                    _context?.Dispose();
                }

                // Libertar recursos não geridos (se existirem)
                _ticketRepository = null;
                _userRepository = null;

                _disposed = true;
            }
        }

        /// <summary>
        /// Destructor que garante a libertação de recursos
        /// </summary>
        ~RepositoryFactory()
        {
            Dispose(false);
        }
    }

    /// <summary>
    /// Classe estática para facilitar o acesso aos repositórios
    /// Implementa o padrão Singleton para a factory
    /// </summary>
    public static class RepositoryManager
    {
        private static RepositoryFactory _instance;
        private static readonly object _lock = new object();

        /// <summary>
        /// Obtém a instância singleton da factory de repositórios
        /// </summary>
        public static RepositoryFactory Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new RepositoryFactory();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Inicializa o manager com uma string de conexão específica
        /// </summary>
        /// <param name="connectionString">String de conexão</param>
        public static void Initialize(string connectionString)
        {
            lock (_lock)
            {
                _instance?.Dispose();
                _instance = new RepositoryFactory(connectionString);
            }
        }

        /// <summary>
        /// Inicializa o manager com um contexto específico
        /// </summary>
        /// <param name="context">Contexto da base de dados</param>
        public static void Initialize(DatabaseContext context)
        {
            lock (_lock)
            {
                _instance?.Dispose();
                _instance = new RepositoryFactory(context);
            }
        }

        /// <summary>
        /// Liberta os recursos da instância singleton
        /// </summary>
        public static void Dispose()
        {
            lock (_lock)
            {
                _instance?.Dispose();
                _instance = null;
            }
        }

        /// <summary>
        /// Reinicia a instância singleton
        /// </summary>
        public static void Reset()
        {
            Dispose();
        }
    }
}