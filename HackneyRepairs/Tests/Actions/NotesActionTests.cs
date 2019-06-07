using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HackneyRepairs.Actions;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Models;
using Moq;
using Xunit;

namespace HackneyRepairs.Tests.Actions
{
    public class NotesActionTests
    {
        Mock<ILoggerAdapter<NoteActions>> _mockLogger;
        readonly int randomReference;

        public NotesActionTests()
        {
            Random rnd = new Random();
            randomReference = rnd.Next(100000000, 999999999);
            _mockLogger = new Mock<ILoggerAdapter<NoteActions>>();
        }

        #region get notes feed
        [Fact]
        public async Task get_note_feed_by_noteID_returns_a_list_of_notes()
        {
            List<Note> fakeResponse = new List<Note>
            {
                new Note()
                {
                    WorkOrderReference = "123"
                }
            };
            Mock<IHackneyWorkOrdersService> _workOrderService = new Mock<IHackneyWorkOrdersService>();
            Mock<IHackneyNotesService> _notesService = new Mock<IHackneyNotesService>();
            Mock<IHackneyRepairsService> _repairsService = new Mock<IHackneyRepairsService>();
            _workOrderService.Setup(service => service.GetNoteFeed(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
                             .Returns(Task.FromResult<IEnumerable<Note>>(fakeResponse));
            NoteActions notesActions = new NoteActions(_workOrderService.Object, _notesService.Object, _repairsService.Object, _mockLogger.Object);
            var response = await notesActions.GetNoteFeed(randomReference, "", randomReference);

            Assert.True(response is List<Note>);
        }

        [Fact]
        public async Task get_note_feed_throws_and_exception_when_response_has_one_null_object()
        {
            List<Note> fakeResponse = new List<Note>
            {
                new Note()
            };
            Mock<IHackneyWorkOrdersService> _workOrderService = new Mock<IHackneyWorkOrdersService>();
            Mock<IHackneyNotesService> _notesService = new Mock<IHackneyNotesService>();
            Mock<IHackneyRepairsService> _repairsService = new Mock<IHackneyRepairsService>();
            _workOrderService.Setup(service => service.GetNoteFeed(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
                             .Returns(Task.FromResult<IEnumerable<Note>>(fakeResponse));
            NoteActions notesActions = new NoteActions(_workOrderService.Object, _notesService.Object, _repairsService.Object, _mockLogger.Object);
            await Assert.ThrowsAsync<MissingNoteTargetException>(async () => await notesActions.GetNoteFeed(randomReference, "notfoundtarget", randomReference));
        }
        #endregion

        #region add note
        [Fact]
        public async Task throws_exception_if_workorder_does_not_exist()
        {
            Mock<IHackneyWorkOrdersService> _workOrderService = new Mock<IHackneyWorkOrdersService>();
            Mock<IHackneyNotesService> _notesService = new Mock<IHackneyNotesService>();
            Mock<IHackneyRepairsService> _repairsService = new Mock<IHackneyRepairsService>();
            int? response = null;
            _workOrderService.Setup(service => service.GetWorkOrderSid(It.IsAny<string>())).ReturnsAsync(response);
            NoteActions notesActions = new NoteActions(_workOrderService.Object, _notesService.Object, _repairsService.Object, _mockLogger.Object);
            await Assert.ThrowsAsync<MissingWorkOrderException>(async () => await notesActions.AddNote(new NoteRequest()));
        }
        #endregion
    }
}
