
namespace Ticket2Help.BLL.Models
{
    public class SoftwareTicket : Ticket
    {
        public string NomeSoftware { get; set; }
        public string DescricaoNecessidade { get; set; }
        public string DescricaoIntervencao { get; set; }

        public override string GetResumo()
        {
            return $"[SW] {NomeSoftware} - {DescricaoNecessidade} | Estado: {Estado}";
        }
    }
}
