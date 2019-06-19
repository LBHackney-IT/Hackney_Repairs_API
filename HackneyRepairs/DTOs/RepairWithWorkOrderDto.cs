using System;
namespace HackneyRepairs.DTOs
{
    public class RepairWithWorkOrderDto
    {
        public string rq_ref { get; set; }
        public string rq_problem { get; set; }
        public string rq_priority { get; set; }
        public string prop_ref { get; set; }
        public string rq_name { get; set; }
        public string rq_phone { get; set; }
        public string wo_ref { get; set; }
        public string sup_ref { get; set; }
        public string job_code { get; set; }
        public string user_login { get; set; }
        public string username { get; set; }
        public string rq_date { get; set; }
    }
}
