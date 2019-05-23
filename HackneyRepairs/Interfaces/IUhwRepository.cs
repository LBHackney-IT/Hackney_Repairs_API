using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HackneyRepairs.Models;

namespace HackneyRepairs.Interfaces
{
    public interface IUhwRepository
    {
        Task AddOrderDocumentAsync(string documentType, string workOrderReference, int workOrderId, string processComment);
		Task<IEnumerable<Note>> GetNotesByWorkOrderReference(string workOrderReference);
        Task<IEnumerable<Note>> GetNoteFeed(int noteId, string noteTarget, int size);
        Task AddNote(FullNoteRequest note);
        string GetUHUsernameByEmail(string lbhEmail);
    }
}
