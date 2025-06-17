using System;
using System.Linq;
using BllModels = Ticket2Help.BLL.Models;
using DalModels = Ticket2Help.DAL.Models;

namespace Ticket2Help.BLL.Services
{
    /// <summary>
    /// Classe responsável pelo mapeamento entre modelos DAL e BLL
    /// </summary>
    public static class ModelMapper
    {
        #region User Mapping

        /// <summary>
        /// Mapeia User do DAL para o BLL
        /// </summary>
        public static BllModels.User MapToBll(DalModels.User dalUser)
        {
            if (dalUser == null) return null;

            return new BllModels.User
            {
                UserId = dalUser.UserId,
                Username = dalUser.Username,
                PasswordHash = dalUser.PasswordHash,
                Nome = dalUser.Nome,
                Email = dalUser.Email,
                TipoUtilizador = (BllModels.TipoUtilizador)dalUser.TipoUtilizador,
                Ativo = dalUser.Ativo,
                DataCriacao = dalUser.DataCriacao
            };
        }

        /// <summary>
        /// Mapeia User do BLL para o DAL
        /// </summary>
        public static DalModels.User MapToDal(BllModels.User bllUser)
        {
            if (bllUser == null) return null;

            return new DalModels.User
            {
                UserId = bllUser.UserId,
                Username = bllUser.Username,
                PasswordHash = bllUser.PasswordHash,
                Nome = bllUser.Nome,
                Email = bllUser.Email,
                TipoUtilizador = (DalModels.TipoUtilizador)bllUser.TipoUtilizador,
                Ativo = bllUser.Ativo,
                DataCriacao = bllUser.DataCriacao
            };
        }

        #endregion

        #region Ticket Mapping

        /// <summary>
        /// Mapeia Ticket do DAL para o BLL
        /// </summary>
        public static BllModels.Ticket MapToBll(DalModels.Ticket dalTicket)
        {
            if (dalTicket == null) return null;

            return dalTicket switch
            {
                DalModels.HardwareTicket hwTicket => MapToBll(hwTicket),
                DalModels.SoftwareTicket swTicket => MapToBll(swTicket),
                _ => null
            };
        }

        /// <summary>
        /// Mapeia HardwareTicket do DAL para o BLL
        /// </summary>
        public static BllModels.HardwareTicket MapToBll(DalModels.HardwareTicket dalTicket)
        {
            if (dalTicket == null) return null;

            return new BllModels.HardwareTicket
            {
                TicketId = dalTicket.TicketId,
                DataCriacao = dalTicket.DataCriacao,
                ColaboradorId = dalTicket.ColaboradorId,
                EstadoTicket = (BllModels.EstadoTicket)dalTicket.EstadoTicket,
                DataAtendimento = dalTicket.DataAtendimento,
                EstadoAtendimento = dalTicket.EstadoAtendimento.HasValue ?
                    (BllModels.EstadoAtendimento?)dalTicket.EstadoAtendimento : null,
                TecnicoId = dalTicket.TecnicoId,
                Equipamento = dalTicket.Equipamento,
                Avaria = dalTicket.Avaria,
                DescricaoReparacao = dalTicket.DescricaoReparacao,
                Pecas = dalTicket.Pecas
            };
        }

        /// <summary>
        /// Mapeia SoftwareTicket do DAL para o BLL
        /// </summary>
        public static BllModels.SoftwareTicket MapToBll(DalModels.SoftwareTicket dalTicket)
        {
            if (dalTicket == null) return null;

            return new BllModels.SoftwareTicket
            {
                TicketId = dalTicket.TicketId,
                DataCriacao = dalTicket.DataCriacao,
                ColaboradorId = dalTicket.ColaboradorId,
                EstadoTicket = (BllModels.EstadoTicket)dalTicket.EstadoTicket,
                DataAtendimento = dalTicket.DataAtendimento,
                EstadoAtendimento = dalTicket.EstadoAtendimento.HasValue ?
                    (BllModels.EstadoAtendimento?)dalTicket.EstadoAtendimento : null,
                TecnicoId = dalTicket.TecnicoId,
                Software = dalTicket.Software,
                DescricaoNecessidade = dalTicket.DescricaoNecessidade,
                DescricaoIntervencao = dalTicket.DescricaoIntervencao
            };
        }

        /// <summary>
        /// Mapeia Ticket do BLL para o DAL
        /// </summary>
        public static DalModels.Ticket MapToDal(BllModels.Ticket bllTicket)
        {
            if (bllTicket == null) return null;

            return bllTicket switch
            {
                BllModels.HardwareTicket hwTicket => MapToDal(hwTicket),
                BllModels.SoftwareTicket swTicket => MapToDal(swTicket),
                _ => null
            };
        }

        /// <summary>
        /// Mapeia HardwareTicket do BLL para o DAL
        /// </summary>
        public static DalModels.HardwareTicket MapToDal(BllModels.HardwareTicket bllTicket)
        {
            if (bllTicket == null) return null;

            return new DalModels.HardwareTicket
            {
                TicketId = bllTicket.TicketId,
                DataCriacao = bllTicket.DataCriacao,
                ColaboradorId = bllTicket.ColaboradorId,
                EstadoTicket = (DalModels.EstadoTicket)bllTicket.EstadoTicket,
                DataAtendimento = bllTicket.DataAtendimento,
                EstadoAtendimento = bllTicket.EstadoAtendimento.HasValue ?
                    (DalModels.EstadoAtendimento?)bllTicket.EstadoAtendimento : null,
                TecnicoId = bllTicket.TecnicoId,
                Equipamento = bllTicket.Equipamento,
                Avaria = bllTicket.Avaria,
                DescricaoReparacao = bllTicket.DescricaoReparacao,
                Pecas = bllTicket.Pecas
            };
        }

        /// <summary>
        /// Mapeia SoftwareTicket do BLL para o DAL
        /// </summary>
        public static DalModels.SoftwareTicket MapToDal(BllModels.SoftwareTicket bllTicket)
        {
            if (bllTicket == null) return null;

            return new DalModels.SoftwareTicket
            {
                TicketId = bllTicket.TicketId,
                DataCriacao = bllTicket.DataCriacao,
                ColaboradorId = bllTicket.ColaboradorId,
                EstadoTicket = (DalModels.EstadoTicket)bllTicket.EstadoTicket,
                DataAtendimento = bllTicket.DataAtendimento,
                EstadoAtendimento = bllTicket.EstadoAtendimento.HasValue ?
                    (DalModels.EstadoAtendimento?)bllTicket.EstadoAtendimento : null,
                TecnicoId = bllTicket.TecnicoId,
                Software = bllTicket.Software,
                DescricaoNecessidade = bllTicket.DescricaoNecessidade,
                DescricaoIntervencao = bllTicket.DescricaoIntervencao
            };
        }

        #endregion
    }
}