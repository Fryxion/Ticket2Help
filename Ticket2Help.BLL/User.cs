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
    /// <summary>
    /// Classe que representa um utilizador do sistema
    /// Compatível com a tabela Users do seu DAL
    /// </summary>
    public class User
    {
        /// <summary>
        /// ID único do utilizador
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Nome de utilizador para login
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Hash da password
        /// </summary>
        public string PasswordHash { get; set; }

        /// <summary>
        /// Nome completo do utilizador
        /// </summary>
        public string Nome { get; set; }

        /// <summary>
        /// Email do utilizador
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Tipo/Papel do utilizador no sistema
        /// </summary>
        public TipoUtilizador TipoUtilizador { get; set; }

        /// <summary>
        /// Indica se o utilizador está ativo
        /// </summary>
        public bool Ativo { get; set; }

        /// <summary>
        /// Data de criação da conta
        /// </summary>
        public DateTime DataCriacao { get; set; }

        /// <summary>
        /// Construtor padrão
        /// </summary>
        public User()
        {
            DataCriacao = DateTime.Now;
            Ativo = true;
            TipoUtilizador = TipoUtilizador.Colaborador;
        }

        /// <summary>
        /// Verifica se o utilizador é técnico
        /// </summary>
        public bool IsTecnico()
        {
            return TipoUtilizador == TipoUtilizador.Tecnico || TipoUtilizador == TipoUtilizador.Administrador;
        }

        /// <summary>
        /// Verifica se o utilizador é administrador
        /// </summary>
        public bool IsAdministrador()
        {
            return TipoUtilizador == TipoUtilizador.Administrador;
        }

        /// <summary>
        /// Verifica se pode criar tickets
        /// </summary>
        public bool PodeCriarTickets()
        {
            return Ativo;
        }

        /// <summary>
        /// Verifica se pode atender tickets
        /// </summary>
        public bool PodeAtenderTickets()
        {
            return Ativo && IsTecnico();
        }

        /// <summary>
        /// Obtém descrição do tipo de utilizador
        /// </summary>
        public string GetDescricaoTipo()
        {
            return TipoUtilizador switch
            {
                TipoUtilizador.Colaborador => "Colaborador",
                TipoUtilizador.Tecnico => "Técnico de Helpdesk",
                TipoUtilizador.Administrador => "Administrador",
                _ => "Desconhecido"
            };
        }

        /// <summary>
        /// Validação básica do utilizador
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(UserId) &&
                   !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Nome);
        }

        /// <summary>
        /// Representação string do utilizador
        /// </summary>
        public override string ToString()
        {
            return $"{Nome} ({Username}) - {GetDescricaoTipo()}";
        }
    }
}