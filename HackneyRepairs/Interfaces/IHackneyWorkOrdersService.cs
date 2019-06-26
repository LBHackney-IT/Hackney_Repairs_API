using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HackneyRepairs.Models;

namespace HackneyRepairs.Interfaces
{
    public interface IHackneyWorkOrdersService
    {
        Task<UHWorkOrder> GetWorkOrder(string workOrderReference);
        Task<IEnumerable<UHWorkOrder>> GetWorkOrders(string[] workOrderReferences);
        Task<IEnumerable<MobileReport>> GetMobileReports(string servitorReference);
        Task<IEnumerable<UHWorkOrder>> GetWorkOrderByPropertyReference(string propertyReference);
        Task<IEnumerable<UHWorkOrder>> GetWorkOrdersByPropertyReferences(string[] propertyReferences, DateTime since, DateTime until);
        Task<IEnumerable<UHWorkOrder>> GetWorkOrderByBlockReference(string[] blockReferences, string trade, DateTime since, DateTime until);
        Task<IEnumerable<Note>> GetNotesByWorkOrderReference(string workOrderReference);
        Task<int?> GetWorkOrderSid(string workOrderReference);
        Task<IEnumerable<Note>> GetNoteFeed(int startId, string noteTarget, int size);
        Task<IEnumerable<UHWorkOrderFeed>> GetWorkOrderFeed(string startId, int resultSize);
        Task<IEnumerable<UHWorkOrder>> GetTasksForWorkOrder(string workOrderReference);
    }
}
