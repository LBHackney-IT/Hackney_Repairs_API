namespace HackneyRepairs.Models
{
    public class HackneyKeyfaxDataResponse
    {
        public string FaultText { get; set; }
        public string RepairCode { get; set; }
        public string RepairCodeDesc { get; set; }
        public string Priority { get; set; }

        public HackneyKeyfaxDataResponse()
        {
            this.RepairCode = "0";
        }
    }
}
