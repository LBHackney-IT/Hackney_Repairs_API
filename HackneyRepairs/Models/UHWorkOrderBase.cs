using System;
namespace HackneyRepairs.Models
{
	public class UHWorkOrderBase
	{
		public string WorkOrderReference { get; set; }
        public string RepairRequestReference { get; set; }
        public string ProblemDescription { get; set; }
        public DateTime Created { get; set; }
		public DateTime AuthDate { get; set; }
        public float EstimatedCost { get; set; }
        public float ActualCost { get; set; }
        public DateTime CompletedOn { get; set; }
        public DateTime DateDue { get; set; }
        public string WorkOrderStatus { get; set; }
        public string DLOStatus { get; set; }
        public string ServitorReference { get; set; }
		public string PropertyReference { get; set; }

        public string SupplierRef { get; set; }

        public string UserLogin { get; set; }
        public string Username { get; set; }
        public string AuthorisedBy { get; set; }
    }
}
