
namespace Ticket2Help.DAL.Models
{
    public class SoftwareTicket : Ticket
    {
        public string SoftwareName { get; set; }
        public string NecessityDescription { get; set; }
        public string InterventionDescription { get; set; }

        public override string GetSummary()
        {
            return $"[SW] {SoftwareName} - {NecessityDescription} | Status: {Status}";
        }
    }
}
