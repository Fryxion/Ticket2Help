
namespace Ticket2Help.DAL.Models
{
    public class HardwareTicket : Ticket
    {
        public string Equipment { get; set; }
        public string Malfunction { get; set; }
        public string RepairDescription { get; set; }
        public string ReplacementParts { get; set; }

        public override string GetSummary()
        {
            return $"[HW] {Equipment} - {Malfunction} | Status: {Status}";
        }
    }
}