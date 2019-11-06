using HackneyRepairs.Actions;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Models;
using Moq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace HackneyRepairs.Tests.Actions
{
    public class CautionaryContactActionsTests
    {
        #region Cautionary Contact Data by property references
        [Fact]
        public async Task get_cautionary_contacts_alerts_and_notes_by_uh_property_reference()
        {
            var mockLogger = new Mock<ILoggerAdapter<CautionaryContactActions>>();

            string[] callerNotes =
            {
                "Don't come its not Healthy",
                "Merged Contacts"
            };
            IList<AddressAlert> addressAlerts = new List<AddressAlert>
            {
                new AddressAlert { AlertCode = "VA", AlertDescription = "This is a description" },
                new AddressAlert { AlertCode = "DIS", AlertDescription = "This is a description" }
            };
            IList<ContactAlert> contactAlerts = new List<ContactAlert>
            {
                new ContactAlert { AlertCode = "VA", AlertDescription = "This is a description" }
            };

            var cautionaryContact = new CautionaryContactLevelModel()
            {
                CallerNotes = callerNotes.ToList(),
                AddressAlerts = addressAlerts,
                ContactAlerts = contactAlerts
            };

            var fakeService = new Mock<IHackneyCautionaryContactService>();
            fakeService.Setup(service => service.GetCautionaryContactByRef("00000123"))
               .ReturnsAsync(cautionaryContact);

            var fakeRequestBuilder = new Mock<IHackneyPropertyServiceRequestBuilder>();
            var workOrdersService = new Mock<IHackneyWorkOrdersService>();
            CautionaryContactActions cautionaryContactActions = new CautionaryContactActions(fakeService.Object, mockLogger.Object);

            var result = await cautionaryContactActions.GetCautionaryContactByRef("00000123");
            var outputCautionaryContact = new CautionaryContactLevelModel()
            {
                CallerNotes = callerNotes.ToList(),
                AddressAlerts = addressAlerts,
                ContactAlerts = contactAlerts
            };

            Assert.Equal(JsonConvert.SerializeObject(outputCautionaryContact), JsonConvert.SerializeObject(result));
        }
        #endregion
    }
}
