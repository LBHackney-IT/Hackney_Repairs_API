using HackneyRepairs.Actions;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Models;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace HackneyRepairs.Tests.Actions
{
    public class CautionaryContactActionsTests
    {
        #region Cautionary Contact Data by property references
        public async Task get_cautionary_contacts_alerts_by_uh_property_reference()
        {
            var mockLogger = new Mock<ILoggerAdapter<CautionaryContactActions>>();
            var property1 = new CautionaryContactLevelModel()
            {
                PropertyReference = "00000123",
                ContactNo = 111111,
                Title = "MRS",
                Forenames = "BLIN",
                Surename = "",
                CallerNotes = "Don't come its not Healthy",
                alertCode = "CX"
            };
            var property2 = new CautionaryContactLevelModel()
            {
                PropertyReference = "00000123",
                ContactNo = 111111,
                Title = "MRS",
                Forenames = "BLIN",
                Surename = "",
                CallerNotes = "Don't come its not Healthy",
                alertCode = "CX"
            };

            var PropertyList = new CautionaryContactLevelModel[2];
            PropertyList[0] = property1;
            PropertyList[1] = property2;
            var fakeService = new Mock<IHackneyCautionaryContactService>();
            fakeService.Setup(service => service.GetCautionaryContactByRef("00000123"))
               .ReturnsAsync(PropertyList);

            var fakeRequestBuilder = new Mock<IHackneyPropertyServiceRequestBuilder>();
            var workOrdersService = new Mock<IHackneyWorkOrdersService>();
            CautionaryContactActions cautionaryContactActions = new CautionaryContactActions(fakeService.Object, mockLogger.Object);

            var results = await cautionaryContactActions.GetCautionaryContactByRef("Acacia");
            var outputCautionaryContact1 = new CautionaryContactLevelModel
            {
                PropertyReference = "00000123",
                ContactNo = 111111,
                Title = "MRS",
                Forenames = "BLIN",
                Surename = "",
                CallerNotes = "Don't come its not Healthy",
                alertCode = "CX"
            };
            var outputCautionaryContact2 = new CautionaryContactLevelModel
            {
                PropertyReference = "00000123",
                ContactNo = 111111,
                Title = "MRS",
                Forenames = "BLIN",
                Surename = "",
                CallerNotes = "Don't come its not Healthy",
                alertCode = "CX"
            };
            var properties = new CautionaryContactLevelModel[2];
            properties[0] = outputCautionaryContact1;
            properties[1] = outputCautionaryContact2;
            var json = new { results = properties };
            Assert.Equal(JsonConvert.SerializeObject(json), JsonConvert.SerializeObject(results));
        }
        #endregion
    }
}
