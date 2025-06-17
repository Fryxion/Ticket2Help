
namespace Ticket2Help.BLL.Models
{
    public class HardwareTicket : Ticket
    {
        public string Equipamento { get; set; }
        public string Avaria { get; set; }
        public string DescricaoReparacao { get; set; }
        public string PecasSubstituidas { get; set; }

        public override string GetResumo()
        {
            return $"[HW] {Equipamento} - {Avaria} | Estado: {Estado}";
        }
    }
}