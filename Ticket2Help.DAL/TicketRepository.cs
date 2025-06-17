using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Net.Sockets;
using Ticket2Help.DAL.Models;
using Ticket2Help.DAL.Data;

namespace Ticket2Help.DAL.Repositories
{
    /// <summary>
    /// Implementação concreta do repositório de tickets para SQL Server
    /// </summary>
    public class TicketRepository : ITicketRepository
    {
        private readonly DatabaseContext _context;

        /// <summary>
        /// Construtor que inicializa o repositório com o contexto da base de dados
        /// </summary>
        /// <param name="context">Contexto da base de dados</param>
        public TicketRepository(DatabaseContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Obtém todos os tickets da base de dados
        /// </summary>
        /// <returns>Lista de todos os tickets</returns>
        public List<Ticket> GetAllTickets()
        {
            var tickets = new List<Ticket>();
            const string query = @"
                SELECT t.TicketId, t.DataCriacao, t.ColaboradorId, t.EstadoTicket, t.TipoTicket,
                       t.DataAtendimento, t.EstadoAtendimento, t.TecnicoId,
                       -- Campos específicos de Hardware
                       ht.Equipamento, ht.Avaria, ht.DescricaoReparacao, ht.Pecas,
                       -- Campos específicos de Software
                       st.Software, st.DescricaoNecessidade, st.DescricaoIntervencao
                FROM Tickets t
                LEFT JOIN HardwareTickets ht ON t.TicketId = ht.TicketId
                LEFT JOIN SoftwareTickets st ON t.TicketId = st.TicketId
                ORDER BY t.DataCriacao DESC";

            using (var reader = _context.ExecuteReader(query))
            {
                while (reader.Read())
                {
                    tickets.Add(MapReaderToTicket(reader));
                }
            }

            return tickets;
        }

        /// <summary>
        /// Obtém um ticket específico pelo seu ID
        /// </summary>
        /// <param name="ticketId">ID do ticket</param>
        /// <returns>Ticket encontrado ou null se não existir</returns>
        public Ticket GetTicketById(int ticketId)
        {
            const string query = @"
                SELECT t.TicketId, t.DataCriacao, t.ColaboradorId, t.EstadoTicket, t.TipoTicket,
                       t.DataAtendimento, t.EstadoAtendimento, t.TecnicoId,
                       -- Campos específicos de Hardware
                       ht.Equipamento, ht.Avaria, ht.DescricaoReparacao, ht.Pecas,
                       -- Campos específicos de Software
                       st.Software, st.DescricaoNecessidade, st.DescricaoIntervencao
                FROM Tickets t
                LEFT JOIN HardwareTickets ht ON t.TicketId = ht.TicketId
                LEFT JOIN SoftwareTickets st ON t.TicketId = st.TicketId
                WHERE t.TicketId = @TicketId";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@TicketId", ticketId)
            };

            using (var reader = _context.ExecuteReader(query, parameters))
            {
                if (reader.Read())
                {
                    return MapReaderToTicket(reader);
                }
            }

            return null;
        }

        /// <summary>
        /// Obtém todos os tickets de um colaborador específico
        /// </summary>
        /// <param name="colaboradorId">ID do colaborador</param>
        /// <returns>Lista de tickets do colaborador</returns>
        public List<Ticket> GetTicketsByColaborador(string colaboradorId)
        {
            var tickets = new List<Ticket>();
            const string query = @"
                SELECT t.TicketId, t.DataCriacao, t.ColaboradorId, t.EstadoTicket, t.TipoTicket,
                       t.DataAtendimento, t.EstadoAtendimento, t.TecnicoId,
                       -- Campos específicos de Hardware
                       ht.Equipamento, ht.Avaria, ht.DescricaoReparacao, ht.Pecas,
                       -- Campos específicos de Software
                       st.Software, st.DescricaoNecessidade, st.DescricaoIntervencao
                FROM Tickets t
                LEFT JOIN HardwareTickets ht ON t.TicketId = ht.TicketId
                LEFT JOIN SoftwareTickets st ON t.TicketId = st.TicketId
                WHERE t.ColaboradorId = @ColaboradorId
                ORDER BY t.DataCriacao DESC";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@ColaboradorId", colaboradorId)
            };

            using (var reader = _context.ExecuteReader(query, parameters))
            {
                while (reader.Read())
                {
                    tickets.Add(MapReaderToTicket(reader));
                }
            }

            return tickets;
        }

        /// <summary>
        /// Obtém tickets filtrados por estado
        /// </summary>
        /// <param name="estado">Estado do ticket</param>
        /// <returns>Lista de tickets com o estado especificado</returns>
        public List<Ticket> GetTicketsByEstado(EstadoTicket estado)
        {
            var tickets = new List<Ticket>();
            const string query = @"
                SELECT t.TicketId, t.DataCriacao, t.ColaboradorId, t.EstadoTicket, t.TipoTicket,
                       t.DataAtendimento, t.EstadoAtendimento, t.TecnicoId,
                       -- Campos específicos de Hardware
                       ht.Equipamento, ht.Avaria, ht.DescricaoReparacao, ht.Pecas,
                       -- Campos específicos de Software
                       st.Software, st.DescricaoNecessidade, st.DescricaoIntervencao
                FROM Tickets t
                LEFT JOIN HardwareTickets ht ON t.TicketId = ht.TicketId
                LEFT JOIN SoftwareTickets st ON t.TicketId = st.TicketId
                WHERE t.EstadoTicket = @EstadoTicket
                ORDER BY t.DataCriacao ASC";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@EstadoTicket", (int)estado)
            };

            using (var reader = _context.ExecuteReader(query, parameters))
            {
                while (reader.Read())
                {
                    tickets.Add(MapReaderToTicket(reader));
                }
            }

            return tickets;
        }

        /// <summary>
        /// Obtém tickets dentro de um intervalo de datas
        /// </summary>
        /// <param name="dataInicio">Data de início</param>
        /// <param name="dataFim">Data de fim</param>
        /// <returns>Lista de tickets no intervalo especificado</returns>
        public List<Ticket> GetTicketsByDateRange(DateTime dataInicio, DateTime dataFim)
        {
            var tickets = new List<Ticket>();
            const string query = @"
                SELECT t.TicketId, t.DataCriacao, t.ColaboradorId, t.EstadoTicket, t.TipoTicket,
                       t.DataAtendimento, t.EstadoAtendimento, t.TecnicoId,
                       -- Campos específicos de Hardware
                       ht.Equipamento, ht.Avaria, ht.DescricaoReparacao, ht.Pecas,
                       -- Campos específicos de Software
                       st.Software, st.DescricaoNecessidade, st.DescricaoIntervencao
                FROM Tickets t
                LEFT JOIN HardwareTickets ht ON t.TicketId = ht.TicketId
                LEFT JOIN SoftwareTickets st ON t.TicketId = st.TicketId
                WHERE t.DataCriacao >= @DataInicio AND t.DataCriacao <= @DataFim
                ORDER BY t.DataCriacao DESC";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@DataInicio", dataInicio),
                new SqlParameter("@DataFim", dataFim)
            };

            using (var reader = _context.ExecuteReader(query, parameters))
            {
                while (reader.Read())
                {
                    tickets.Add(MapReaderToTicket(reader));
                }
            }

            return tickets;
        }

        /// <summary>
        /// Insere um novo ticket na base de dados
        /// </summary>
        /// <param name="ticket">Ticket a ser inserido</param>
        /// <returns>ID do ticket inserido</returns>
        public int InsertTicket(Ticket ticket)
        {
            using (var transaction = _context.BeginTransaction())
            {
                try
                {
                    // Inserir ticket base
                    const string insertTicketQuery = @"
                        INSERT INTO Tickets (DataCriacao, ColaboradorId, EstadoTicket, TipoTicket, DataAtendimento, EstadoAtendimento, TecnicoId)
                        OUTPUT INSERTED.TicketId
                        VALUES (@DataCriacao, @ColaboradorId, @EstadoTicket, @TipoTicket, @DataAtendimento, @EstadoAtendimento, @TecnicoId)";

                    var ticketParameters = new SqlParameter[]
                    {
                        new SqlParameter("@DataCriacao", ticket.DataCriacao),
                        new SqlParameter("@ColaboradorId", ticket.ColaboradorId),
                        new SqlParameter("@EstadoTicket", (int)ticket.EstadoTicket),
                        new SqlParameter("@TipoTicket", (int)ticket.TipoTicket),
                        new SqlParameter("@DataAtendimento", (object)ticket.DataAtendimento ?? DBNull.Value),
                        new SqlParameter("@EstadoAtendimento", ticket.EstadoAtendimento.HasValue ? (int)ticket.EstadoAtendimento.Value : (object)DBNull.Value),
                        new SqlParameter("@TecnicoId", (object)ticket.TecnicoId ?? DBNull.Value)
                    };

                    using (var command = new SqlCommand(insertTicketQuery, _context.GetConnection(), transaction))
                    {
                        command.Parameters.AddRange(ticketParameters);
                        var ticketId = (int)command.ExecuteScalar();

                        // Inserir dados específicos baseados no tipo
                        if (ticket is HardwareTicket hardwareTicket)
                        {
                            InsertHardwareTicket(ticketId, hardwareTicket, transaction);
                        }
                        else if (ticket is SoftwareTicket softwareTicket)
                        {
                            InsertSoftwareTicket(ticketId, softwareTicket, transaction);
                        }

                        transaction.Commit();
                        return ticketId;
                    }
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Atualiza um ticket existente na base de dados
        /// </summary>
        /// <param name="ticket">Ticket com dados atualizados</param>
        /// <returns>True se a atualização foi bem-sucedida, False caso contrário</returns>
        public bool UpdateTicket(Ticket ticket)
        {
            using (var transaction = _context.BeginTransaction())
            {
                try
                {
                    // Atualizar ticket base
                    const string updateTicketQuery = @"
                        UPDATE Tickets 
                        SET EstadoTicket = @EstadoTicket, 
                            DataAtendimento = @DataAtendimento, 
                            EstadoAtendimento = @EstadoAtendimento, 
                            TecnicoId = @TecnicoId
                        WHERE TicketId = @TicketId";

                    var ticketParameters = new SqlParameter[]
                    {
                        new SqlParameter("@TicketId", ticket.TicketId),
                        new SqlParameter("@EstadoTicket", (int)ticket.EstadoTicket),
                        new SqlParameter("@DataAtendimento", (object)ticket.DataAtendimento ?? DBNull.Value),
                        new SqlParameter("@EstadoAtendimento", ticket.EstadoAtendimento.HasValue ? (int)ticket.EstadoAtendimento.Value : (object)DBNull.Value),
                        new SqlParameter("@TecnicoId", (object)ticket.TecnicoId ?? DBNull.Value)
                    };

                    using (var command = new SqlCommand(updateTicketQuery, _context.GetConnection(), transaction))
                    {
                        command.Parameters.AddRange(ticketParameters);
                        var rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            // Atualizar dados específicos baseados no tipo
                            if (ticket is HardwareTicket hardwareTicket)
                            {
                                UpdateHardwareTicket(hardwareTicket, transaction);
                            }
                            else if (ticket is SoftwareTicket softwareTicket)
                            {
                                UpdateSoftwareTicket(softwareTicket, transaction);
                            }

                            transaction.Commit();
                            return true;
                        }
                    }

                    transaction.Rollback();
                    return false;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Remove um ticket da base de dados
        /// </summary>
        /// <param name="ticketId">ID do ticket a ser removido</param>
        /// <returns>True se a remoção foi bem-sucedida, False caso contrário</returns>
        public bool DeleteTicket(int ticketId)
        {
            using (var transaction = _context.BeginTransaction())
            {
                try
                {
                    // Remover dados específicos primeiro (devido a foreign keys)
                    const string deleteHardwareQuery = "DELETE FROM HardwareTickets WHERE TicketId = @TicketId";
                    const string deleteSoftwareQuery = "DELETE FROM SoftwareTickets WHERE TicketId = @TicketId";
                    const string deleteTicketQuery = "DELETE FROM Tickets WHERE TicketId = @TicketId";

                    var parameter = new SqlParameter("@TicketId", ticketId);

                    using (var command = new SqlCommand(deleteHardwareQuery, _context.GetConnection(), transaction))
                    {
                        command.Parameters.Add(parameter);
                        command.ExecuteNonQuery();
                    }

                    using (var command = new SqlCommand(deleteSoftwareQuery, _context.GetConnection(), transaction))
                    {
                        command.Parameters.Clear();
                        command.Parameters.Add(new SqlParameter("@TicketId", ticketId));
                        command.ExecuteNonQuery();
                    }

                    using (var command = new SqlCommand(deleteTicketQuery, _context.GetConnection(), transaction))
                    {
                        command.Parameters.Clear();
                        command.Parameters.Add(new SqlParameter("@TicketId", ticketId));
                        var rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            transaction.Commit();
                            return true;
                        }
                    }

                    transaction.Rollback();
                    return false;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Obtém estatísticas de tickets por tipo
        /// </summary>
        /// <param name="tipoTicket">Tipo de ticket (Hardware ou Software)</param>
        /// <param name="dataInicio">Data de início para análise</param>
        /// <param name="dataFim">Data de fim para análise</param>
        /// <returns>Estatísticas do tipo de ticket especificado</returns>
        public Dictionary<string, object> GetTicketStatistics(TipoTicket tipoTicket, DateTime dataInicio, DateTime dataFim)
        {
            var statistics = new Dictionary<string, object>();

            const string query = @"
                SELECT 
                    COUNT(*) as TotalTickets,
                    SUM(CASE WHEN EstadoTicket = 2 THEN 1 ELSE 0 END) as TicketsAtendidos,
                    SUM(CASE WHEN EstadoAtendimento = 1 THEN 1 ELSE 0 END) as TicketsResolvidos,
                    SUM(CASE WHEN EstadoAtendimento = 2 THEN 1 ELSE 0 END) as TicketsNaoResolvidos,
                    AVG(CASE WHEN DataAtendimento IS NOT NULL 
                        THEN DATEDIFF(MINUTE, DataCriacao, DataAtendimento) 
                        ELSE NULL END) as TempoMedioAtendimentoMinutos
                FROM Tickets 
                WHERE TipoTicket = @TipoTicket 
                    AND DataCriacao >= @DataInicio 
                    AND DataCriacao <= @DataFim";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@TipoTicket", (int)tipoTicket),
                new SqlParameter("@DataInicio", dataInicio),
                new SqlParameter("@DataFim", dataFim)
            };

            using (var reader = _context.ExecuteReader(query, parameters))
            {
                if (reader.Read())
                {
                    statistics["TotalTickets"] = reader["TotalTickets"];
                    statistics["TicketsAtendidos"] = reader["TicketsAtendidos"];
                    statistics["TicketsResolvidos"] = reader["TicketsResolvidos"];
                    statistics["TicketsNaoResolvidos"] = reader["TicketsNaoResolvidos"];
                    statistics["TempoMedioAtendimentoMinutos"] = reader["TempoMedioAtendimentoMinutos"] != DBNull.Value
                        ? reader["TempoMedioAtendimentoMinutos"] : 0;
                }
            }

            return statistics;
        }

        #region Métodos Privados

        /// <summary>
        /// Mapeia os dados do SqlDataReader para um objeto Ticket
        /// </summary>
        /// <param name="reader">SqlDataReader com os dados</param>
        /// <returns>Objeto Ticket mapeado</returns>
        private Ticket MapReaderToTicket(SqlDataReader reader)
        {
            var tipoTicket = (TipoTicket)reader.GetInt32("TipoTicket");

            Ticket ticket;

            if (tipoTicket == TipoTicket.Hardware)
            {
                ticket = new HardwareTicket
                {
                    Equipamento = reader.IsDBNull("Equipamento") ? null : reader.GetString("Equipamento"),
                    Avaria = reader.IsDBNull("Avaria") ? null : reader.GetString("Avaria"),
                    DescricaoReparacao = reader.IsDBNull("DescricaoReparacao") ? null : reader.GetString("DescricaoReparacao"),
                    Pecas = reader.IsDBNull("Pecas") ? null : reader.GetString("Pecas")
                };
            }
            else
            {
                ticket = new SoftwareTicket
                {
                    Software = reader.IsDBNull("Software") ? null : reader.GetString("Software"),
                    DescricaoNecessidade = reader.IsDBNull("DescricaoNecessidade") ? null : reader.GetString("DescricaoNecessidade"),
                    DescricaoIntervencao = reader.IsDBNull("DescricaoIntervencao") ? null : reader.GetString("DescricaoIntervencao")
                };
            }

            // Mapear propriedades comuns
            ticket.TicketId = reader.GetInt32("TicketId");
            ticket.DataCriacao = reader.GetDateTime("DataCriacao");
            ticket.ColaboradorId = reader.GetString("ColaboradorId");
            ticket.EstadoTicket = (EstadoTicket)reader.GetInt32("EstadoTicket");
            //ticket.TipoTicket = tipoTicket;
            ticket.DataAtendimento = reader.IsDBNull("DataAtendimento") ? null : reader.GetDateTime("DataAtendimento");
            ticket.EstadoAtendimento = reader.IsDBNull("EstadoAtendimento") ? null : (EstadoAtendimento)reader.GetInt32("EstadoAtendimento");
            ticket.TecnicoId = reader.IsDBNull("TecnicoId") ? null : reader.GetString("TecnicoId");

            return ticket;
        }

        /// <summary>
        /// Insere dados específicos de um ticket de hardware
        /// </summary>
        /// <param name="ticketId">ID do ticket</param>
        /// <param name="hardwareTicket">Dados do ticket de hardware</param>
        /// <param name="transaction">Transação ativa</param>
        private void InsertHardwareTicket(int ticketId, HardwareTicket hardwareTicket, SqlTransaction transaction)
        {
            const string query = @"
                INSERT INTO HardwareTickets (TicketId, Equipamento, Avaria, DescricaoReparacao, Pecas)
                VALUES (@TicketId, @Equipamento, @Avaria, @DescricaoReparacao, @Pecas)";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@TicketId", ticketId),
                new SqlParameter("@Equipamento", (object)hardwareTicket.Equipamento ?? DBNull.Value),
                new SqlParameter("@Avaria", (object)hardwareTicket.Avaria ?? DBNull.Value),
                new SqlParameter("@DescricaoReparacao", (object)hardwareTicket.DescricaoReparacao ?? DBNull.Value),
                new SqlParameter("@Pecas", (object)hardwareTicket.Pecas ?? DBNull.Value)
            };

            using (var command = new SqlCommand(query, _context.GetConnection(), transaction))
            {
                command.Parameters.AddRange(parameters);
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Insere dados específicos de um ticket de software
        /// </summary>
        /// <param name="ticketId">ID do ticket</param>
        /// <param name="softwareTicket">Dados do ticket de software</param>
        /// <param name="transaction">Transação ativa</param>
        private void InsertSoftwareTicket(int ticketId, SoftwareTicket softwareTicket, SqlTransaction transaction)
        {
            const string query = @"
                INSERT INTO SoftwareTickets (TicketId, Software, DescricaoNecessidade, DescricaoIntervencao)
                VALUES (@TicketId, @Software, @DescricaoNecessidade, @DescricaoIntervencao)";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@TicketId", ticketId),
                new SqlParameter("@Software", (object)softwareTicket.Software ?? DBNull.Value),
                new SqlParameter("@DescricaoNecessidade", (object)softwareTicket.DescricaoNecessidade ?? DBNull.Value),
                new SqlParameter("@DescricaoIntervencao", (object)softwareTicket.DescricaoIntervencao ?? DBNull.Value)
            };

            using (var command = new SqlCommand(query, _context.GetConnection(), transaction))
            {
                command.Parameters.AddRange(parameters);
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Atualiza dados específicos de um ticket de hardware
        /// </summary>
        /// <param name="hardwareTicket">Dados atualizados do ticket de hardware</param>
        /// <param name="transaction">Transação ativa</param>
        private void UpdateHardwareTicket(HardwareTicket hardwareTicket, SqlTransaction transaction)
        {
            const string query = @"
                UPDATE HardwareTickets 
                SET Equipamento = @Equipamento, 
                    Avaria = @Avaria, 
                    DescricaoReparacao = @DescricaoReparacao, 
                    Pecas = @Pecas
                WHERE TicketId = @TicketId";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@TicketId", hardwareTicket.TicketId),
                new SqlParameter("@Equipamento", (object)hardwareTicket.Equipamento ?? DBNull.Value),
                new SqlParameter("@Avaria", (object)hardwareTicket.Avaria ?? DBNull.Value),
                new SqlParameter("@DescricaoReparacao", (object)hardwareTicket.DescricaoReparacao ?? DBNull.Value),
                new SqlParameter("@Pecas", (object)hardwareTicket.Pecas ?? DBNull.Value)
            };

            using (var command = new SqlCommand(query, _context.GetConnection(), transaction))
            {
                command.Parameters.AddRange(parameters);
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Atualiza dados específicos de um ticket de software
        /// </summary>
        /// <param name="softwareTicket">Dados atualizados do ticket de software</param>
        /// <param name="transaction">Transação ativa</param>
        private void UpdateSoftwareTicket(SoftwareTicket softwareTicket, SqlTransaction transaction)
        {
            const string query = @"
                UPDATE SoftwareTickets 
                SET Software = @Software, 
                    DescricaoNecessidade = @DescricaoNecessidade, 
                    DescricaoIntervencao = @DescricaoIntervencao
                WHERE TicketId = @TicketId";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@TicketId", softwareTicket.TicketId),
                new SqlParameter("@Software", (object)softwareTicket.Software ?? DBNull.Value),
                new SqlParameter("@DescricaoNecessidade", (object)softwareTicket.DescricaoNecessidade ?? DBNull.Value),
                new SqlParameter("@DescricaoIntervencao", (object)softwareTicket.DescricaoIntervencao ?? DBNull.Value)
            };

            using (var command = new SqlCommand(query, _context.GetConnection(), transaction))
            {
                command.Parameters.AddRange(parameters);
                command.ExecuteNonQuery();
            }
        }
    }
}


        #endregion