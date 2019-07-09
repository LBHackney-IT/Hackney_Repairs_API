using System;
namespace HackneyRepairs.Models
{
    public class UHWorkOrder : UHWorkOrderBase
    {
        public string SORCode { get; set; }
        public string SORCodeDescription { get; set; }
        public string Trade { get; set; }
        public decimal EstimatedUnits { get; set; }
        public string TaskStatus { get; set; }
        public string UnitType { get; set; }
    }
}
