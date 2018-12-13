using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Models;

namespace HackneyRepairs.Services
{
	public class FakeWorkOrdersService : IHackneyWorkOrdersService
    {      
        public Task<UHWorkOrder> GetWorkOrder(string workOrderReference)
		{
			if (string.Equals(workOrderReference, "0"))
			{
                return Task.Run(() => (UHWorkOrder)null);
			}

            var workOrder = new UHWorkOrder
			{
                WorkOrderReference = workOrderReference,
                ServitorReference = "44444444"
			};
			return Task.Run(() => workOrder);
		}

        public Task<IEnumerable<UHWorkOrder>> GetWorkOrders(string[] workOrderReferences)
        {
            IEnumerable<UHWorkOrder> workOrders = workOrderReferences
                .Where(reference => reference != "MISSING")
                .Select(reference => 
                {
                    return new UHWorkOrder
                    {
                        WorkOrderReference = reference,
                        ServitorReference = "44444444"
                    };
                });

            return Task.Run(() => workOrders);   
        }

        public Task<IEnumerable<MobileReport>> GetMobileReports(string servitorReference)
        {
            var fakeResponse = new List<MobileReport>
            {
                new MobileReport
                {
                    ReportUri = "Mobile report path"
                }
            };
            return Task.Run(() => (IEnumerable<MobileReport>)fakeResponse);
        }
        
		public Task<IEnumerable<UHWorkOrder>> GetWorkOrderByPropertyReference(string propertyReference)
        {
            if (string.Equals(propertyReference, "9999999999"))
            {
				return Task.Run(() => (IEnumerable<UHWorkOrder>)new List<UHWorkOrder>());
            }

			if (string.Equals(propertyReference, "0"))
            {
                return Task.Run(() => (IEnumerable<UHWorkOrder>)null);
            }

			var workOrder = new List<UHWorkOrder>
            {
				new UHWorkOrder
                {
					PropertyReference = propertyReference
                }
            };
			return Task.Run(() => (IEnumerable<UHWorkOrder>)workOrder);
        }

        public Task<IEnumerable<UHWorkOrder>> GetWorkOrdersByPropertyReferences(string[] propertyReferences, DateTime since, DateTime until)
        {
            if (Array.Exists(propertyReferences, v => v == "9999999999"))
            {
                return Task.Run(() => (IEnumerable<UHWorkOrder>)new List<UHWorkOrder>() { new UHWorkOrder() });
            }

            if (Array.Exists(propertyReferences, v => v == "0"))
            {
                return Task.Run(() => (IEnumerable<UHWorkOrder>)new List<UHWorkOrder>());
            }

            IEnumerable<UHWorkOrder> workOrders = new List<UHWorkOrder>();

            return Task.Run(() => workOrders);
        }

        public Task<IEnumerable<UHWorkOrder>> GetWorkOrderByBlockReference(string[] blockReferences, string trade, DateTime since, DateTime until)
        {
            if (string.Equals(blockReferences.FirstOrDefault(), "9999999999"))
            {
                return Task.Run(() => (IEnumerable<UHWorkOrder>)new List<UHWorkOrder>());
            }

            var workOrder = new List<UHWorkOrder>
            {
                new UHWorkOrder
                {
                    PropertyReference = blockReferences.FirstOrDefault(),
                    Trade = trade
                }
            };
            return Task.Run(() => (IEnumerable<UHWorkOrder>)workOrder);
        }

		public Task<IEnumerable<Note>> GetNotesByWorkOrderReference(string workOrderReference)
        {
			if (string.Equals(workOrderReference, "9999999999"))
            {
				return Task.Run(() => (IEnumerable<Note>)new List<Note>());
            }

			if (string.Equals(workOrderReference, "00"))
            {
				return Task.Run(() => (IEnumerable<Note>)null);
            }

            var noteEntities = new List<Note>
            {
                new Note
                {
                    Text = "Some note",
                    LoggedBy = "UHOrder"
				}    
            };
			return Task.Run(() => (IEnumerable<Note>)noteEntities);
        }

        public Task<IEnumerable<Note>> GetNoteFeed(int startId, string noteTarget, int resultSize)
        {
            if (startId == 99999999)
            {
                return Task.Run(() => (IEnumerable<Note>)new List<Note>());
            }

            if (startId == 11550853)
            {
                throw new FakeWorkOrdersServiceException();
            }

            var fakeNoteResponse = new List<Note>
            {
                new Note
                {
                    WorkOrderReference = "123"
                }
            };
            return Task.Run(() => (IEnumerable<Note>)fakeNoteResponse);
        }

        public Task<IEnumerable<UHWorkOrderFeed>> GetWorkOrderFeed(string startId, int resultSize)
        {
            if (string.Equals(startId, "99999999"))
            {
                return Task.Run(() => (IEnumerable<UHWorkOrderFeed>)new List<UHWorkOrderFeed>());
            }

            if (string.Equals(startId, "11550853"))
            {
                throw new FakeWorkOrdersServiceException();
            }

            var fakeResponse = new List<UHWorkOrderFeed>
            {
                new UHWorkOrderFeed
                {
                    WorkOrderReference = "123456"
                }
            };
            return Task.Run(() => (IEnumerable<UHWorkOrderFeed>)fakeResponse);
        }

        public Task<int?> GetWorkOrderSid(string workOrderReference)
        {
            int? response;
            if (string.Equals(workOrderReference, "0"))
            {
                response = null;
                return Task.Run(() => response);
            }

            response = 44444444;
            return Task.Run(() => response);
        }
    }

    class FakeWorkOrdersServiceException : Exception { }
}
