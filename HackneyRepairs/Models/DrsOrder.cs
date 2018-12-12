using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HackneyRepairs.Models
{
    public class DrsOrder
    {
        public DrsOrder()
        {
            Tasks = new List<DrsTask>();
        }

        public int Id { get; set; }
        public DateTime createdDate { get; set; }
        public DateTime dueDate { get; set; }
        public string wo_ref { get; set; }
        public string contactName { get; set; }
        public string contract { get; set; }
        public string prop_ref { get; set; }
        public bool txtMessage { get; set; }
        public string phone { get; set; }
        public string priority { get; set; }

        public string userid { get; set; }

        public List<DrsTask> Tasks { get; set; }
        public string propname { get; set; }

        public string address1 { get; set; }
        public string postcode { get; set; }
        public string comments { get; set; }
    }

    public class DrsTask
    {
        public string job_code { get; set; }
        public string comments { get; set; }
        public decimal itemValue { get; set; }
        public decimal itemqty { get; set; }
        public int smv { get; set; }
        public string trade { get; set; }

    }
}
