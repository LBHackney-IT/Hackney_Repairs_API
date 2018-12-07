using System.Threading.Tasks;
using HackneyRepairs.Models;

namespace HackneyRepairs.Interfaces
{
    public interface IHackneyNotesService
    {
        Task AddNote(FullNoteRequest note);
    }
}