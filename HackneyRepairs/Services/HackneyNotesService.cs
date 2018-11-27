﻿using System;
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

        public async Task AddNote(NoteRequest note)
        {
            _logger.LogInformation($"HackneyNoteService/AddNote(): Calling UHWRepository for adding note to {note.ObjectKey} object for : {note.ObjectReference})");
            await _uhwRepository.AddNote(note);
        } 
    }
}
