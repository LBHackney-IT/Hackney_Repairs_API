using System;
namespace HackneyRepairs.Models
{
    public class DetailedAppointment : Appointment
    {
        public string Id { get; set; }
        public string Status { get; set; }
        public string Outcome { get; set; }
        public string AssignedWorker { get; set; }
        public string Phonenumber { get; set; }
        public string Priority { get; set; }
        public string SourceSystem { get; set; }
        public string Comment { get; set; }
        public DateTime? CreationDate { get; set; }
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
