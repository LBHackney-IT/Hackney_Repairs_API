using System;
namespace HackneyRepairs.Entities
{
    public class RepairRequestEntity
    {
        public string rq_ref { get; set; }
        public DateTime? rq_date { get; set; }
        public string rq_loc { get; set; }
        public string user_code { get; set; }
        public string rq_user { get; set; }
        public string prop_ref { get; set; }
        public string house_ref { get; set; }
        public int? person_no { get; set; }
        public string rq_name { get; set; }
        public string rq_phone { get; set; }
        public string rq_problem { get; set; }
        public int? rq_open { get; set; }
        public int? task_ref { get; set; }
        public string rq_access { get; set; }
        public int? rq_monam { get; set; }
        public int? rq_monpm { get; set; }
        public int? rq_tueam { get; set; }
        public int? rq_tuepm { get; set; }
        public int? rq_wedam { get; set; }
        public int? rq_wedpm { get; set; }
        public int? rq_thuam { get; set; }
        public int? rq_thupm { get; set; }
        public int? rq_friam { get; set; }
        public int? rq_fripm { get; set; }
        public int? rq_satam { get; set; }
        public int? rq_satpm { get; set; }
        public int? rq_sunam { get; set; }
        public int? rq_sunpm { get; set; }
        public string rq_comment { get; set; }
        public string rq_cancel { get; set; }
        public string rq_officer { get; set; }
        public DateTime? appointment { get; set; }
        public string apoint_time { get; set; }
        public int rmreqst_sid { get; set; }
        public int rq_sys_preinsp { get; set; }
        public string confsup_ref { get; set; }
        public string supref_reason { get; set; }
        public int? h_preinsp { get; set; }
        public DateTime? h_verbal_date { get; set; }
        public string h_callertype { get; set; }
        public string rq_type { get; set; }
        public int? h_deferrals { get; set; }
        public DateTime? rq_deferred_to { get; set; }
        public int? rq_linked { get; set; }
        public DateTime? rq_date_due { get; set; }
        public string rq_priority { get; set; }
        public int? preinspect { get; set; }
        public string rq_status { get; set; }
        public int? preinsp_task { get; set; }
        public string warden_ref { get; set; }
        public string rq_location { get; set; }
        public string rq_locdetail { get; set; }
        public int? rq_monsr { get; set; }
        public int? rq_tuesr { get; set; }
        public int? rq_wedsr { get; set; }
        public int? rq_thusr { get; set; }
        public int? rq_frisr { get; set; }
        public byte[] tstamp { get; set; }
        public string rq_overall_status { get; set; }
        public DateTime? rq_overall_status_date { get; set; }
        public DateTime? rq_cancel_date { get; set; }
        public string rq_cancelby { get; set; }
        public string hh_ref { get; set; }
        public int? sus_haz { get; set; }
        public DateTime? repnotify_dt { get; set; }
        public string vm_propref { get; set; }
        public string u_saffron_job_number { get; set; }
    }
}
