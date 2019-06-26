using System;
namespace HackneyRepairs.Models
{
	public class UHWorkOrder : UHWorkOrderBase
    {
		public string SORCode { get; set; }
        public string SORCodeDescription { get; set; }
        public string Trade { get; set; }
    }
}
