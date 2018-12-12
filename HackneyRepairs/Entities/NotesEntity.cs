using System;
namespace HackneyRepairs.Entities
{
    public class NotesEntity
    {
	    public string KeyObject { get; set; }
		public int KeyNumb { get; set; }
		public string KeyText { get; set; }
		public DateTime NDate { get; set; }
		public string UserID { get; set; }
		public string SecureCategory { get; set; }
		public string NoteType { get; set; }
		public string NoteText { get; set; }
		public int NoteID { get; set; }
		public string AppCode { get; set; }
		public int ClientID { get; set; }
    }
}
