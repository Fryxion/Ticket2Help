using System;

namespace Ticket2Help.DAL.Models
{
    /// <summary>
    /// Modelo de utilizador para o DAL
    /// Representa a estrutura da tabela Users na base de dados
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
    }
}