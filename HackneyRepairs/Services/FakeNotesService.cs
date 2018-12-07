using System;
using System.Threading.Tasks;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Models;

namespace HackneyRepairs.Services
{
    public class FakeNotesService : IHackneyNotesService
    {
        public async Task AddNote(FullNoteRequest note) {}
    }
}
