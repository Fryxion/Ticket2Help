namespace Ticket2Help.DAL.Models
{
    /// <summary>
    /// Enumeração para os estados do ticket
    /// </summary>
    public enum EstadoTicket
    {
        PorAtender = 0,
        EmAtendimento = 1,
        Atendido = 2
    }

    /// <summary>
    /// Enumeração para os estados de atendimento
    /// </summary>
    public enum EstadoAtendimento
    {
        Aberto = 0,
        Resolvido = 1,
        NaoResolvido = 2
    }

    /// <summary>
    /// Enumeração para os tipos de ticket
    /// </summary>
    public enum TipoTicket
    {
        Hardware = 0,
        Software = 1
    }

    /// <summary>
    /// Enumeração para tipos de utilizador
    /// </summary>
    public enum TipoUtilizador
    {
        Colaborador = 0,
        Tecnico = 1,
        Administrador = 2
    }
}