using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HackneyRepairs.DTOs;
using HackneyRepairs.Models;

namespace HackneyRepairs.Interfaces
{
    public interface IUHWWarehouseRepository
    {
        Task<bool> GetMaintainableFlag(string propertyReference);
        Task<IEnumerable<RepairRequestBase>> GetRepairRequestsByPropertyReference(string propertyReference);
        Task<IEnumerable<RepairWithWorkOrderDto>> GetRepairRequest(string repairReference);
        Task<PropertyLevelModel> GetPropertyLevelInfo(string reference);
        Task<PropertyLevelModel[]> GetPropertyListByPostCode(string post_code, int? maxLevel, int? minLevel);
        Task<PropertyDetails> GetPropertyDetailsByReference(string reference);
        Task<PropertyDetails[]> GetPropertiesDetailsByReference(string[] references);
        Task<PropertyDetails> GetPropertyBlockByReference(string reference);
        Task<PropertyDetails> GetPropertyEstateByReference(string reference);
        Task<PropertyLevelModel[]> GetFacilitiesByPropertyRef(string reference);
        Task<UHWorkOrder> GetWorkOrderByWorkOrderReference(string reference);
        Task<IEnumerable<UHWorkOrder>> GetWorkOrdersByWorkOrderReferences(string[] reference);
        Task<IEnumerable<UHWorkOrder>> GetWorkOrderByPropertyReference(string propertyReference);
        Task<IEnumerable<UHWorkOrder>> GetWorkOrdersByPropertyReferences(string[] propertyReferences, DateTime since, DateTime until);
        Task<IEnumerable<UHWorkOrder>> GetWorkOrderByBlockReference(string[] blockReferences, string trade, DateTime since, DateTime until);
        Task<DrsOrder> GetWorkOrderDetails(string workOrderReference);        
        Task<IEnumerable<Note>> GetNotesByWorkOrderReference(string workOrderReference);
        Task<IEnumerable<UHWorkOrderFeed>> GetWorkOrderFeed(string startID, int size);
        Task<IEnumerable<Note>> GetNoteFeed(int startId, string noteTarget, int size);
        Task<IEnumerable<string>> GetDistinctNoteKeyObjects();
		Task<List<PropertyLevelModel>> GetPropertyLevelInfosForParent(string reference);
        Task<PropertyLevelModel[]> GetPropertyDetailsByFirstLineOfAddress(string firstLineOfAddress, int limit);
    }
}
