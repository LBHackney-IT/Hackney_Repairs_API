using System;
namespace HackneyRepairs.Entities
{
    public class WorkOrderEntity
    {
		public string wo_ref { get; set; }
		public string sup_ref  { get; set; }
		public string prop_ref  { get; set; }
		public DateTime? created { get; set; }
		public string user_code  { get; set; }
		public DateTime? issued { get; set; }
		public string rep_type  { get; set; }
		public string rep_class  { get; set; }
		public bool? confirmation_order { get; set; }
		public string wo_type  { get; set; }
		public string plan_ref  { get; set; }
		public float est_cost { get; set; }
		public bool? fc { get; set; }
		public DateTime? completed { get; set; }
		public int rmworder_sid { get; set; }
		public DateTime?  date_due { get; set; }
		public string auth_by  { get; set; }
		public DateTime?  auth_date { get; set; }
		public int authorised { get; set; }
		public int wo_version { get; set; }
		public DateTime?  h_verbal_date { get; set; }
		public string h_comments  { get; set; }
		public DateTime? expected_completion { get; set; }
		public DateTime? cancelled_date { get; set; }
		public bool? work_complete { get; set; }
		public string wo_status  { get; set; }
		public bool? ok_to_statement { get; set; }
		public string statement_approver  { get; set; }
		public bool? statemented { get; set; }
		public string statement_no  { get; set; }
		public DateTime? statement_date { get; set; }
		public string insp_outcome  { get; set; }
		public bool? post_insp { get; set; }
		public string posti_sys  { get; set; }
		public string rq_ref  { get; set; }
		public float est_cost_ori { get; set; }
		public DateTime? datecomp { get; set; }
		public bool? satisfied { get; set; }
		public bool? courtious { get; set; }
		public bool? appointkept { get; set; }
		public bool? punctual { get; set; }
		public bool? proced { get; set; }
		public bool? advice { get; set; }
		public bool? caller { get; set; }
		public bool? satisfied_n { get; set; }
		public bool? courtious_n { get; set; }
		public bool? appointkept_n { get; set; }
		public bool? punctual_n { get; set; }
		public bool? proced_n { get; set; }
		public bool? advice_n { get; set; }
		public bool? caller_n { get; set; }
		public string tensatcomments { get; set; }
		public bool? tstamp { get; set; }
		public bool? hasfeedback { get; set; }
		public DateTime? fbcardsentdate { get; set; }
		public int fbcardsissued { get; set; }
		public string reason_late { get; set; }
		public DateTime? datecomp_user { get; set; }
		public string change_reason  { get; set; }
		public int change_no { get; set; }
		public string statement_old  { get; set; }
		public DateTime? statremove_dt { get; set; }
		public string composite_sor { get; set; }
		public string vm_propref { get; set; }
		public bool? lettability { get; set; }
		public DateTime? attend_date { get; set; }
		public string u_interface_status  { get; set; }
		public DateTime? u_interface_date { get; set; }
		public string u_saffron_job_number  { get; set; }
		public string u_priority { get; set; }
		public string u_dlo_status { get; set; }
		public DateTime? u_dlo_status_date { get; set; }
		public string u_servitor_ref  { get; set; }
		public string u_status_desc  { get; set; }
		public string u_allocated_resource  { get; set; }
		public string u_servitor_user  { get; set; }
		public int act_cost { get; set; }
       
    }
}
