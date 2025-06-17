using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Ticket2Help.BLL.Services;
using Ticket2Help.BLL.Factory;
using Ticket2Help.BLL.Controllers;
using Ticket2Help.BLL.Strategy;
using Ticket2Help.DAL.Interfaces;
using Ticket2Help.DAL.Repositories;
using Ticket2Help.DAL;
using Ticket2Help.BLL.Models;
using Ticket2Help.DAL.Data;

namespace Ticket2Help.BLL.Configuration
{
    /// <summary>
    /// Classe responsável pela configuração e inicialização do sistema
    /// Implementa o padrão Dependency Injection para configurar todas as dependências
    /// </summary>
    public static class SystemBootstrap
    {
        /// <summary>
        /// Configura todos os serviços do sistema
        /// </summary>
        /// <param name="services">Collection de serviços</param>
        /// <param name="configuration">Configuração da aplicação</param>
        /// <returns>ServiceCollection configurada</returns>
        public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configurar Entity Framework / Base de Dados
            ConfigureDataAccess(services, configuration);

            // Configurar serviços de negócio
            ConfigureBusinessServices(services);

            // Configurar factories e estratégias
            ConfigureFactoriesAndStrategies(services);

            // Configurar controllers
            ConfigureControllers(services);

            return services;
        }

        /// <summary>
        /// Configura o acesso a dados
        /// </summary>
        /// <param name="services">Collection de serviços</param>
        /// <param name="configuration">Configuração</param>
        private static void ConfigureDataAccess(IServiceCollection services, IConfiguration configuration)
        {
            // Registar o contexto da base de dados
            services.AddDbContext<DatabaseContext>(options =>
            {
                // Configurar connection string
                var connectionString = configuration.GetConnectionString("DefaultConnection")
                    ?? "Server=localhost;Database=Ticket2HelpDB;Trusted_Connection=true;TrustServerCertificate=true;";

                options.UseSqlServer(connectionString);
            });

            // Registar repositórios
            services.AddScoped<ITicketRepository, TicketRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
        }

        /// <summary>
        /// Configura os serviços de negócio
        /// </summary>
        /// <param name="services">Collection de serviços</param>
        private static void ConfigureBusinessServices(IServiceCollection services)
        {
            // Serviços principais
            services.AddScoped<ITicketService, TicketService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IStatisticsService, StatisticsService>();

            // Outros serviços auxiliares
            services.AddScoped<ReportGenerator>();
        }

        /// <summary>
        /// Configura factories e estratégias
        /// </summary>
        /// <param name="services">Collection de serviços</param>
        private static void ConfigureFactoriesAndStrategies(IServiceCollection services)
        {
            // Factories
            services.AddScoped<ITicketFactory, TicketFactory>();

            // Estratégias (registar todas as disponíveis)
            services.AddTransient<FifoAttendanceStrategy>();
            services.AddTransient<LifoAttendanceStrategy>();
            services.AddTransient<PriorityAttendanceStrategy>();
            services.AddTransient<RoundRobinAttendanceStrategy>();

            // Factory para criar estratégias baseadas em tipo
            services.AddScoped<IAttendanceStrategyFactory, AttendanceStrategyFactory>();

            // Contexto de estratégia com estratégia padrão (FIFO)
            services.AddScoped<TicketAttendanceContext>(provider =>
                new TicketAttendanceContext(new FifoAttendanceStrategy()));
        }

        /// <summary>
        /// Configura os controllers
        /// </summary>
        /// <param name="services">Collection de serviços</param>
        private static void ConfigureControllers(IServiceCollection services)
        {
            services.AddScoped<TicketController>();
        }

        /// <summary>
        /// Inicializa a aplicação e cria dados iniciais se necessário
        /// </summary>
        /// <param name="serviceProvider">Provider de serviços</param>
        /// <returns>Task da inicialização</returns>
        public static async Task InitializeApplicationAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            try
            {
                // Inicializar base de dados
                await InitializeDatabaseAsync(scope.ServiceProvider);

                // Criar utilizadores padrão se necessário
                await CreateDefaultUsersAsync(scope.ServiceProvider);

                Console.WriteLine("Sistema inicializado com sucesso!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro na inicialização: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Inicializa a base de dados
        /// </summary>
        /// <param name="serviceProvider">Provider de serviços</param>
        private static async Task InitializeDatabaseAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<DatabaseContext>();

            // Garantir que a base de dados é criada
            await context.Database.EnsureCreatedAsync();

            // Aplicar migrações pendentes (se existirem)
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                await context.Database.MigrateAsync();
            }
        }

        /// <summary>
        /// Cria utilizadores padrão se não existirem
        /// </summary>
        /// <param name="serviceProvider">Provider de serviços</param>
        private static async Task CreateDefaultUsersAsync(IServiceProvider serviceProvider)
        {
            var userService = serviceProvider.GetRequiredService<IUserService>();

            // Criar utilizadores padrão apenas se não existirem
            await userService.CreateDefaultUsersAsync();
        }
    }

    /// <summary>
    /// Factory para criar estratégias de atendimento
    /// </summary>
    public interface IAttendanceStrategyFactory
    {
        /// <summary>
        /// Cria uma estratégia baseada no nome
        /// </summary>
        /// <param name="strategyName">Nome da estratégia</param>
        /// <returns>Estratégia criada</returns>
        ITicketAttendanceStrategy CreateStrategy(string strategyName);

        /// <summary>
        /// Obtém todas as estratégias disponíveis
        /// </summary>
        /// <returns>Lista de estratégias</returns>
        List<ITicketAttendanceStrategy> GetAllStrategies();
    }

    /// <summary>
    /// Implementação da factory de estratégias
    /// </summary>
    public class AttendanceStrategyFactory : IAttendanceStrategyFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public AttendanceStrategyFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Cria uma estratégia baseada no nome
        /// </summary>
        /// <param name="strategyName">Nome da estratégia</param>
        /// <returns>Estratégia criada</returns>
        public ITicketAttendanceStrategy CreateStrategy(string strategyName)
        {
            return strategyName?.ToLower() switch
            {
                "fifo" => _serviceProvider.GetService<FifoAttendanceStrategy>(),
                "lifo" => _serviceProvider.GetService<LifoAttendanceStrategy>(),
                "prioridade" or "priority" => _serviceProvider.GetService<PriorityAttendanceStrategy>(),
                "roundrobin" or "round_robin" => _serviceProvider.GetService<RoundRobinAttendanceStrategy>(),
                "hardware" => new TypeBasedAttendanceStrategy(TicketType.Hardware),
                "software" => new TypeBasedAttendanceStrategy(TicketType.Software),
                _ => new FifoAttendanceStrategy() // Estratégia padrão
            };
        }

        /// <summary>
        /// Obtém todas as estratégias disponíveis
        /// </summary>
        /// <returns>Lista de estratégias</returns>
        public List<ITicketAttendanceStrategy> GetAllStrategies()
        {
            return new List<ITicketAttendanceStrategy>
            {
                CreateStrategy("fifo"),
                CreateStrategy("lifo"),
                CreateStrategy("priority"),
                CreateStrategy("roundrobin"),
                CreateStrategy("hardware"),
                CreateStrategy("software")
            };
        }
    }

    /// <summary>
    /// Classe para configurações da aplicação
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Configurações da base de dados
        /// </summary>
        public DatabaseSettings Database { get; set; } = new();

        /// <summary>
        /// Configurações da aplicação
        /// </summary>
        public ApplicationSettings Application { get; set; } = new();

        /// <summary>
        /// Configurações de logging
        /// </summary>
        public LoggingSettings Logging { get; set; } = new();
    }

    /// <summary>
    /// Configurações da base de dados
    /// </summary>
    public class DatabaseSettings
    {
        /// <summary>
        /// String de conexão com a base de dados
        /// </summary>
        public string ConnectionString { get; set; } = "Server=localhost;Database=Ticket2HelpDB;Trusted_Connection=true;TrustServerCertificate=true;";

        /// <summary>
        /// Timeout para comandos da base de dados (em segundos)
        /// </summary>
        public int CommandTimeout { get; set; } = 30;

        /// <summary>
        /// Indica se deve criar a base de dados automaticamente
        /// </summary>
        public bool EnsureCreated { get; set; } = true;
    }

    /// <summary>
    /// Configurações da aplicação
    /// </summary>
    public class ApplicationSettings
    {
        /// <summary>
        /// Nome da aplicação
        /// </summary>
        public string Name { get; set; } = "Ticket2Help";

        /// <summary>
        /// Versão da aplicação
        /// </summary>
        public string Version { get; set; } = "1.0.0";

        /// <summary>
        /// Estratégia de atendimento padrão
        /// </summary>
        public string DefaultAttendanceStrategy { get; set; } = "FIFO";

        /// <summary>
        /// Indica se deve criar utilizadores padrão
        /// </summary>
        public bool CreateDefaultUsers { get; set; } = true;

        /// <summary>
        /// Número máximo de tickets que um técnico pode ter em atendimento
        /// </summary>
        public int MaxTicketsPerTechnician { get; set; } = 5;

        /// <summary>
        /// Tempo limite para atendimento de tickets (em horas)
        /// </summary>
        public int TicketAttendanceTimeoutHours { get; set; } = 24;
    }

    /// <summary>
    /// Configurações de logging
    /// </summary>
    public class LoggingSettings
    {
        /// <summary>
        /// Nível mínimo de log
        /// </summary>
        public string MinimumLevel { get; set; } = "Information";

        /// <summary>
        /// Indica se deve fazer log para ficheiro
        /// </summary>
        public bool LogToFile { get; set; } = true;

        /// <summary>
        /// Caminho do ficheiro de log
        /// </summary>
        public string LogFilePath { get; set; } = "logs/ticket2help.log";

        /// <summary>
        /// Indica se deve fazer log para consola
        /// </summary>
        public bool LogToConsole { get; set; } = true;
    }

    /// <summary>
    /// Classe utilitária para validação de configurações
    /// </summary>
    public static class ConfigurationValidator
    {
        /// <summary>
        /// Valida as configurações da aplicação
        /// </summary>
        /// <param name="settings">Configurações a validar</param>
        /// <returns>Lista de erros de validação</returns>
        public static List<string> ValidateSettings(AppSettings settings)
        {
            var errors = new List<string>();

            // Validar configurações da base de dados
            if (string.IsNullOrWhiteSpace(settings.Database.ConnectionString))
            {
                errors.Add("Connection string da base de dados é obrigatória");
            }

            if (settings.Database.CommandTimeout <= 0)
            {
                errors.Add("Command timeout deve ser maior que zero");
            }

            // Validar configurações da aplicação
            if (string.IsNullOrWhiteSpace(settings.Application.Name))
            {
                errors.Add("Nome da aplicação é obrigatório");
            }

            if (settings.Application.MaxTicketsPerTechnician <= 0)
            {
                errors.Add("Número máximo de tickets por técnico deve ser maior que zero");
            }

            if (settings.Application.TicketAttendanceTimeoutHours <= 0)
            {
                errors.Add("Timeout de atendimento deve ser maior que zero");
            }

            return errors;
        }

        /// <summary>
        /// Valida e lança exceção se houver erros
        /// </summary>
        /// <param name="settings">Configurações a validar</param>
        /// <exception cref="InvalidOperationException">Se houver erros de validação</exception>
        public static void ValidateAndThrow(AppSettings settings)
        {
            var errors = ValidateSettings(settings);
            if (errors.Any())
            {
                throw new InvalidOperationException($"Erros de configuração: {string.Join(", ", errors)}");
            }
        }
    }
}