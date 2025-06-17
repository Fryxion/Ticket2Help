using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Ticket2Help.DAL.Models;

namespace Ticket2Help.DAL.Repositories
{
    /// <summary>
    /// Interface que define as operações de acesso a dados para tickets
    /// </summary>
    public interface ITicketRepository
    {
        /// <summary>
        /// Obtém todos os tickets da base de dados
        /// </summary>
        /// <returns>Lista de todos os tickets</returns>
        List<Ticket> GetAllTickets();

        /// <summary>
        /// Obtém um ticket específico pelo seu ID
        /// </summary>
        /// <param name="ticketId">ID do ticket</param>
        /// <returns>Ticket encontrado ou null se não existir</returns>
        Ticket GetTicketById(int ticketId);

        /// <summary>
        /// Obtém todos os tickets de um colaborador específico
        /// </summary>
        /// <param name="colaboradorId">ID do colaborador</param>
        /// <returns>Lista de tickets do colaborador</returns>
        List<Ticket> GetTicketsByColaborador(string colaboradorId);

        /// <summary>
        /// Obtém tickets filtrados por estado
        /// </summary>
        /// <param name="estado">Estado do ticket</param>
        /// <returns>Lista de tickets com o estado especificado</returns>
        List<Ticket> GetTicketsByEstado(EstadoTicket estado);

        /// <summary>
        /// Obtém tickets dentro de um intervalo de datas
        /// </summary>
        /// <param name="dataInicio">Data de início</param>
        /// <param name="dataFim">Data de fim</param>
        /// <returns>Lista de tickets no intervalo especificado</returns>
        List<Ticket> GetTicketsByDateRange(DateTime dataInicio, DateTime dataFim);

        /// <summary>
        /// Insere um novo ticket na base de dados
        /// </summary>
        /// <param name="ticket">Ticket a ser inserido</param>
        /// <returns>ID do ticket inserido</returns>
        int InsertTicket(Ticket ticket);

        /// <summary>
        /// Atualiza um ticket existente na base de dados
        /// </summary>
        /// <param name="ticket">Ticket com dados atualizados</param>
        /// <returns>True se a atualização foi bem-sucedida, False caso contrário</returns>
        bool UpdateTicket(Ticket ticket);

        /// <summary>
        /// Remove um ticket da base de dados
        /// </summary>
        /// <param name="ticketId">ID do ticket a ser removido</param>
        /// <returns>True se a remoção foi bem-sucedida, False caso contrário</returns>
        bool DeleteTicket(int ticketId);

        /// <summary>
        /// Obtém estatísticas de tickets por tipo
        /// </summary>
        /// <param name="tipoTicket">Tipo de ticket (Hardware ou Software)</param>
        /// <param name="dataInicio">Data de início para análise</param>
        /// <param name="dataFim">Data de fim para análise</param>
        /// <returns>Estatísticas do tipo de ticket especificado</returns>
        Dictionary<string, object> GetTicketStatistics(TipoTicket tipoTicket, DateTime dataInicio, DateTime dataFim);
    }
}