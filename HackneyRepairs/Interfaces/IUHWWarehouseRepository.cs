using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HackneyRepairs.Models;
using HackneyRepairs.PropertyService;

namespace HackneyRepairs.Interfaces
{
    public interface IUHWWarehouseRepository
    {
        Task<bool> GetMaintainableFlag(string propertyReference);
        Task<IEnumerable<RepairRequestBase>> GetRepairRequests(string propertyReference);
        Task<PropertyLevelModel> GetPropertyLevelInfo(string reference);
        Task<PropertySummary[]> GetPropertyListByPostCode(string post_code);
        Task<PropertyDetails> GetPropertyDetailsByReference(string reference);
        Task<PropertyDetails> GetPropertyBlockByReference(string reference);
        Task<PropertyDetails> GetPropertyEstateByReference(string reference);
        Task<UHWorkOrder> GetWorkOrderByWorkOrderReference(string reference);
		Task<IEnumerable<UHWorkOrder>> GetWorkOrderByPropertyReference(string propertyReference);
        Task<IEnumerable<UHWorkOrder>> GetWorkOrdersByPropertyReferences(string[] propertyReferences, DateTime from, DateTime until);
		Task<IEnumerable<UHWorkOrder>> GetWorkOrderByBlockReference(string blockReference, string trade);
        Task<DrsOrder> GetWorkOrderDetails(string workOrderReference);        
        Task<IEnumerable<Note>> GetNotesByWorkOrderReference(string workOrderReference);
        Task<IEnumerable<UHWorkOrderFeed>> GetWorkOrderFeed(string startID, int size);
        Task<IEnumerable<Note>> GetNoteFeed(int startId, string noteTarget, int size);
        Task<IEnumerable<string>> GetDistinctNoteKeyObjects();
		Task<List<PropertyLevelModel>> GetPropertyLevelInfosForParent(string reference);
    }
}
