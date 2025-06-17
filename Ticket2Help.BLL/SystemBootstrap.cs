using System;
using System.Threading.Tasks;
using Ticket2Help.BLL.Controllers;
using Ticket2Help.BLL.Services;
using Ticket2Help.DAL.Data;
using Ticket2Help.DAL.Repositories;
using static Ticket2Help.BLL.Services.DashboardSummary;

namespace Ticket2Help.BLL.Configuration
{
    /// <summary>
    /// Classe responsável pela inicialização do sistema
    /// Adaptada para ser compatível com o seu DAL existente
    /// </summary>
    public static class SystemBootstrap
    {
        private static RepositoryFactory _repositoryFactory;
        private static TicketService _ticketService;
        private static UserService _userService;
        private static TicketController _ticketController;

        /// <summary>
        /// Inicializa o sistema com a configuração padrão
        /// </summary>
        public static async Task InicializarSistemaAsync()
        {
            try
            {
                // Criar factory de repositórios
                _repositoryFactory = new RepositoryFactory();

                // Testar conexão
                if (!_repositoryFactory.TestDatabaseConnection())
                {
                    throw new InvalidOperationException("Não foi possível conectar à base de dados");
                }

                // Inicializar serviços
                InicializarServicos();

                Console.WriteLine("Sistema inicializado com sucesso!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro na inicialização: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Inicializa o sistema com uma string de conexão personalizada
        /// </summary>
        /// <param name="connectionString">String de conexão</param>
        public static async Task InicializarSistemaAsync(string connectionString)
        {
            try
            {
                // Criar factory com conexão personalizada
                _repositoryFactory = new RepositoryFactory(connectionString);

                // Testar conexão
                if (!_repositoryFactory.TestDatabaseConnection())
                {
                    throw new InvalidOperationException("Não foi possível conectar à base de dados");
                }

                // Inicializar serviços
                InicializarServicos();

                Console.WriteLine("Sistema inicializado com sucesso!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro na inicialização: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Inicializa os serviços de negócio
        /// </summary>
        private static void InicializarServicos()
        {
            // Obter repositórios
            var ticketRepository = _repositoryFactory.GetTicketRepository();
            var userRepository = _repositoryFactory.GetUserRepository();

            // Criar serviços
            _ticketService = new TicketService(ticketRepository, userRepository);
            _userService = new UserService(userRepository);

            // Criar controller
            _ticketController = new TicketController(_ticketService, _userService);
        }

        /// <summary>
        /// Obtém o controller principal do sistema
        /// </summary>
        /// <returns>Instância do TicketController</returns>
        public static TicketController ObterController()
        {
            if (_ticketController == null)
            {
                throw new InvalidOperationException("Sistema não foi inicializado. Chame InicializarSistemaAsync() primeiro.");
            }

            return _ticketController;
        }

        /// <summary>
        /// Obtém o serviço de tickets
        /// </summary>
        /// <returns>Instância do TicketService</returns>
        public static TicketService ObterTicketService()
        {
            if (_ticketService == null)
            {
                throw new InvalidOperationException("Sistema não foi inicializado. Chame InicializarSistemaAsync() primeiro.");
            }

            return _ticketService;
        }

        /// <summary>
        /// Obtém o serviço de utilizadores
        /// </summary>
        /// <returns>Instância do UserService</returns>
        public static UserService ObterUserService()
        {
            if (_userService == null)
            {
                throw new InvalidOperationException("Sistema não foi inicializado. Chame InicializarSistemaAsync() primeiro.");
            }

            return _userService;
        }

        /// <summary>
        /// Obtém a factory de repositórios
        /// </summary>
        /// <returns>Instância do RepositoryFactory</returns>
        public static RepositoryFactory ObterRepositoryFactory()
        {
            if (_repositoryFactory == null)
            {
                throw new InvalidOperationException("Sistema não foi inicializado. Chame InicializarSistemaAsync() primeiro.");
            }

            return _repositoryFactory;
        }

        /// <summary>
        /// Testa a conectividade do sistema
        /// </summary>
        /// <returns>True se todos os componentes estão funcionais</returns>
        public static bool TestarSistema()
        {
            try
            {
                if (_repositoryFactory == null)
                    return false;

                // Testar conexão com a base de dados
                if (!_repositoryFactory.TestDatabaseConnection())
                    return false;

                // Testar repositórios
                var ticketRepo = _repositoryFactory.GetTicketRepository();
                var userRepo = _repositoryFactory.GetUserRepository();

                if (ticketRepo == null || userRepo == null)
                    return false;

                Console.WriteLine("Teste do sistema concluído com sucesso!");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro no teste do sistema: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Liberta os recursos do sistema
        /// </summary>
        public static void LiberarRecursos()
        {
            try
            {
                _repositoryFactory?.Dispose();
                _repositoryFactory = null;
                _ticketService = null;
                _userService = null;
                _ticketController = null;

                Console.WriteLine("Recursos do sistema libertados.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao libertar recursos: {ex.Message}");
            }
        }

        /// <summary>
        /// Reinicia o sistema
        /// </summary>
        public static async Task ReiniciarSistemaAsync()
        {
            LiberarRecursos();
            await InicializarSistemaAsync();
        }

        /// <summary>
        /// Reinicia o sistema com nova string de conexão
        /// </summary>
        /// <param name="novaConnectionString">Nova string de conexão</param>
        public static async Task ReiniciarSistemaAsync(string novaConnectionString)
        {
            LiberarRecursos();
            await InicializarSistemaAsync(novaConnectionString);
        }
    }

    /// <summary>
    /// Classe para facilitar a integração com WPF
    /// Fornece métodos estáticos de fácil acesso
    /// </summary>
    public static class Ticket2HelpSystem
    {
        /// <summary>
        /// Inicializa o sistema (método de conveniência)
        /// </summary>
        public static async Task StartAsync()
        {
            await SystemBootstrap.InicializarSistemaAsync();
        }

        /// <summary>
        /// Inicializa o sistema com conexão personalizada
        /// </summary>
        /// <param name="connectionString">String de conexão</param>
        public static async Task StartAsync(string connectionString)
        {
            await SystemBootstrap.InicializarSistemaAsync(connectionString);
        }

        /// <summary>
        /// Obtém o controller principal
        /// </summary>
        public static TicketController Controller
        {
            get => SystemBootstrap.ObterController();
        }

        /// <summary>
        /// Obtém o serviço de tickets
        /// </summary>
        public static TicketService TicketService
        {
            get => SystemBootstrap.ObterTicketService();
        }

        /// <summary>
        /// Obtém o serviço de utilizadores
        /// </summary>
        public static UserService UserService
        {
            get => SystemBootstrap.ObterUserService();
        }

        /// <summary>
        /// Testa o sistema
        /// </summary>
        public static bool IsSystemHealthy => SystemBootstrap.TestarSistema();

        /// <summary>
        /// Encerra o sistema
        /// </summary>
        public static void Shutdown()
        {
            SystemBootstrap.LiberarRecursos();
        }
    }
}