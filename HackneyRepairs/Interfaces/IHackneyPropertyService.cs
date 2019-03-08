using System.Threading.Tasks;
using HackneyRepairs.PropertyService;
using HackneyRepairs.Models;
using System.Collections.Generic;

namespace HackneyRepairs.Interfaces
{
    public interface IHackneyPropertyService
    {
        Task<PropertyLevelModel> GetPropertyLevelInfo(string reference);
        Task<PropertyInfoResponse> GetPropertyListByPostCodeAsync(ListByPostCodeRequest request);
        Task<PropertyLevelModel[]> GetPropertyListByPostCode(string post_code, int? maxLevel, int? minLevel);
        Task<PropertyLevelModel[]> GetPropertyListByAddress(string firstlineofaddress);
        Task<PropertyDetails> GetPropertyByRefAsync(string reference);
        Task<PropertyDetails[]> GetPropertiesByReferences(string[] references);
        Task<PropertyDetails> GetPropertyBlockByRef(string reference);
        Task<PropertyDetails> GetPropertyEstateByRef(string reference);
        Task<bool> GetMaintainable(string reference);
		Task<List<PropertyLevelModel>> GetPropertyLevelInfosForParent(string parentReference);
    }
}