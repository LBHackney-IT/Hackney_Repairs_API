using HackneyRepairs.Actions;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Models;
using Moq;
using Newtonsoft.Json;
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
            string[] alertCodes = 
            {
                "VA", "PV"
            };

            var cautionaryContact = new CautionaryContactLevelModel()
            {
                CallerNotes = "Don't come its not Healthy",
                AlertCodes = alertCodes.ToList()
            };
           
            var fakeService = new Mock<IHackneyCautionaryContactService>();
            fakeService.Setup(service => service.GetCautionaryContactByRef("00000123"))
               .ReturnsAsync(cautionaryContact);

            var fakeRequestBuilder = new Mock<IHackneyPropertyServiceRequestBuilder>();
            var workOrdersService = new Mock<IHackneyWorkOrdersService>();
            CautionaryContactActions cautionaryContactActions = new CautionaryContactActions(fakeService.Object, mockLogger.Object);

            var results = await cautionaryContactActions.GetCautionaryContactByRef("00000123");
            var outputCautionaryContact = new CautionaryContactLevelModel()
            {
                CallerNotes = "Don't come its not Healthy",
                AlertCodes = alertCodes.ToList()
            };
            var json = new { results = outputCautionaryContact };
            Assert.Equal(JsonConvert.SerializeObject(json), JsonConvert.SerializeObject(results));
        }
        #endregion
    }
}
