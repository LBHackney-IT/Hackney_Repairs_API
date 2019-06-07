using System;
using System.Threading.Tasks;
using HackneyRepairs.Actions;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Models;

namespace HackneyRepairs.Services
{
    public class HackneyNotesService : IHackneyNotesService
    {
        private IUhwRepository _uhwRepository;
        private ILoggerAdapter<NoteActions> _logger;

        public HackneyNotesService(IUhwRepository uhwRepository, ILoggerAdapter<NoteActions> logger)
        {
            _uhwRepository = uhwRepository;
            _logger = logger;
        }

        public async Task AddNote(FullNoteRequest note)
        {
            _logger.LogInformation($"HackneyNoteService/AddNote(): Calling UHWRepository for adding note to {note.ObjectKey} object for : {note.ObjectReference})");
            await _uhwRepository.AddNote(note);
        }

        //public string GetUHUsername(string lbhEmail)
        //{
        //    _logger.LogInformation($"HackneyNoteService/GetUHUsername(): Calling UHWRepository for getting UHUsername from {lbhEmail} object for : {lbhEmail})");
        //    var response = _uhwRepository.GetUHUsernameByEmail(lbhEmail);
        //    return response;
        //}
    }
}
