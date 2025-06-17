
namespace Ticket2Help.BLL.Models
{
    public abstract class Ticket
    {
        public int Id { get; set; }
        public DateTime DataCriacao { get; set; } = DateTime.Now;
        public int CodigoColaborador { get; set; }
        public EstadoTicket Estado { get; set; } = EstadoTicket.PorAtender;
        public DateTime? DataAtendimento { get; set; }
        public EstadoAtendimento? EstadoAtendimento { get; set; }

        public abstract string GetResumo();
    }
}