using System;
using Ticket2Help.BLL.Models;

namespace Ticket2Help.BLL.Factory
{
    /// <summary>
    /// Interface para a Factory de tickets
    /// Define o contrato para criação de diferentes tipos de tickets
    /// </summary>
    public interface ITicketFactory
    {
        /// <summary>
        /// Cria um ticket baseado no tipo especificado
        /// </summary>
        /// <param name="type">Tipo do ticket a criar</param>
        /// <param name="sequentialNumber">Número sequencial do ticket</param>
        /// <param name="userId">ID do utilizador que cria o ticket</param>
        /// <param name="specificData">Dados específicos do tipo de ticket</param>
        /// <returns>Instância do ticket criado</returns>
        Ticket CreateTicket(TicketType type, int sequentialNumber, int userId, object specificData);

        /// <summary>
        /// Cria um ticket de hardware
        /// </summary>
        /// <param name="sequentialNumber">Número sequencial</param>
        /// <param name="userId">ID do utilizador</param>
        /// <param name="equipment">Equipamento</param>
        /// <param name="malfunction">Descrição da avaria</param>
        /// <returns>Ticket de hardware</returns>
        HardwareTicket CreateHardwareTicket(int sequentialNumber, int userId, string equipment, string malfunction);

        /// <summary>
        /// Cria um ticket de software
        /// </summary>
        /// <param name="sequentialNumber">Número sequencial</param>
        /// <param name="userId">ID do utilizador</param>
        /// <param name="software">Software</param>
        /// <param name="needDescription">Descrição da necessidade</param>
        /// <returns>Ticket de software</returns>
        SoftwareTicket CreateSoftwareTicket(int sequentialNumber, int userId, string software, string needDescription);
    }

    /// <summary>
    /// Classe para dados específicos de tickets de hardware
    /// </summary>
    public class HardwareTicketData
    {
        public string Equipment { get; set; }
        public string Malfunction { get; set; }

        public HardwareTicketData(string equipment, string malfunction)
        {
            Equipment = equipment;
            Malfunction = malfunction;
        }
    }

    /// <summary>
    /// Classe para dados específicos de tickets de software
    /// </summary>
    public class SoftwareTicketData
    {
        public string Software { get; set; }
        public string NeedDescription { get; set; }

        public SoftwareTicketData(string software, string needDescription)
        {
            Software = software;
            NeedDescription = needDescription;
        }
    }

    /// <summary>
    /// Implementação concreta da Factory de tickets
    /// Implementa o padrão Factory Method para criação de diferentes tipos de tickets
    /// </summary>
    public class TicketFactory : ITicketFactory
    {
        /// <summary>
        /// Cria um ticket baseado no tipo especificado
        /// </summary>
        /// <param name="type">Tipo do ticket</param>
        /// <param name="sequentialNumber">Número sequencial</param>
        /// <param name="userId">ID do utilizador</param>
        /// <param name="specificData">Dados específicos</param>
        /// <returns>Ticket criado</returns>
        /// <exception cref="ArgumentException">Quando o tipo não é suportado</exception>
        /// <exception cref="ArgumentNullException">Quando os dados específicos são nulos</exception>
        public Ticket CreateTicket(TicketType type, int sequentialNumber, int userId, object specificData)
        {
            if (specificData == null)
                throw new ArgumentNullException(nameof(specificData));

            return type switch
            {
                TicketType.Hardware => CreateHardwareTicketFromData(sequentialNumber, userId, specificData),
                TicketType.Software => CreateSoftwareTicketFromData(sequentialNumber, userId, specificData),
                _ => throw new ArgumentException($"Tipo de ticket não suportado: {type}")
            };
        }

        /// <summary>
        /// Cria um ticket de hardware
        /// </summary>
        /// <param name="sequentialNumber">Número sequencial</param>
        /// <param name="userId">ID do utilizador</param>
        /// <param name="equipment">Equipamento</param>
        /// <param name="malfunction">Descrição da avaria</param>
        /// <returns>Ticket de hardware</returns>
        public HardwareTicket CreateHardwareTicket(int sequentialNumber, int userId, string equipment, string malfunction)
        {
            ValidateCommonParameters(sequentialNumber, userId);
            ValidateHardwareParameters(equipment, malfunction);

            return new HardwareTicket(sequentialNumber, userId, equipment, malfunction);
        }

        /// <summary>
        /// Cria um ticket de software
        /// </summary>
        /// <param name="sequentialNumber">Número sequencial</param>
        /// <param name="userId">ID do utilizador</param>
        /// <param name="software">Software</param>
        /// <param name="needDescription">Descrição da necessidade</param>
        /// <returns>Ticket de software</returns>
        public SoftwareTicket CreateSoftwareTicket(int sequentialNumber, int userId, string software, string needDescription)
        {
            ValidateCommonParameters(sequentialNumber, userId);
            ValidateSoftwareParameters(software, needDescription);

            return new SoftwareTicket(sequentialNumber, userId, software, needDescription);
        }

        /// <summary>
        /// Cria um ticket de hardware a partir de dados genéricos
        /// </summary>
        private HardwareTicket CreateHardwareTicketFromData(int sequentialNumber, int userId, object specificData)
        {
            if (specificData is HardwareTicketData hardwareData)
            {
                return CreateHardwareTicket(sequentialNumber, userId, hardwareData.Equipment, hardwareData.Malfunction);
            }
            else if (specificData is dynamic dynamicData)
            {
                try
                {
                    string equipment = dynamicData.Equipment?.ToString();
                    string malfunction = dynamicData.Malfunction?.ToString();
                    return CreateHardwareTicket(sequentialNumber, userId, equipment, malfunction);
                }
                catch
                {
                    throw new ArgumentException("Dados específicos inválidos para ticket de hardware");
                }
            }
            else
            {
                throw new ArgumentException("Tipo de dados específicos não suportado para ticket de hardware");
            }
        }

        /// <summary>
        /// Cria um ticket de software a partir de dados genéricos
        /// </summary>
        private SoftwareTicket CreateSoftwareTicketFromData(int sequentialNumber, int userId, object specificData)
        {
            if (specificData is SoftwareTicketData softwareData)
            {
                return CreateSoftwareTicket(sequentialNumber, userId, softwareData.Software, softwareData.NeedDescription);
            }
            else if (specificData is dynamic dynamicData)
            {
                try
                {
                    string software = dynamicData.Software?.ToString();
                    string needDescription = dynamicData.NeedDescription?.ToString();
                    return CreateSoftwareTicket(sequentialNumber, userId, software, needDescription);
                }
                catch
                {
                    throw new ArgumentException("Dados específicos inválidos para ticket de software");
                }
            }
            else
            {
                throw new ArgumentException("Tipo de dados específicos não suportado para ticket de software");
            }
        }

        /// <summary>
        /// Valida parâmetros comuns a todos os tickets
        /// </summary>
        private static void ValidateCommonParameters(int sequentialNumber, int userId)
        {
            if (sequentialNumber <= 0)
                throw new ArgumentException("O número sequencial deve ser maior que zero", nameof(sequentialNumber));

            if (userId <= 0)
                throw new ArgumentException("O ID do utilizador deve ser maior que zero", nameof(userId));
        }

        /// <summary>
        /// Valida parâmetros específicos de tickets de hardware
        /// </summary>
        private static void ValidateHardwareParameters(string equipment, string malfunction)
        {
            if (string.IsNullOrWhiteSpace(equipment))
                throw new ArgumentException("O equipamento é obrigatório", nameof(equipment));

            if (string.IsNullOrWhiteSpace(malfunction))
                throw new ArgumentException("A descrição da avaria é obrigatória", nameof(malfunction));
        }

        /// <summary>
        /// Valida parâmetros específicos de tickets de software
        /// </summary>
        private static void ValidateSoftwareParameters(string software, string needDescription)
        {
            if (string.IsNullOrWhiteSpace(software))
                throw new ArgumentException("O software é obrigatório", nameof(software));

            if (string.IsNullOrWhiteSpace(needDescription))
                throw new ArgumentException("A descrição da necessidade é obrigatória", nameof(needDescription));
        }
    }