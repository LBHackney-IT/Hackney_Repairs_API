using System;
using System.Threading.Tasks;
using HackneyRepairs.Entities;

namespace HackneyRepairs.Interfaces
{
    public interface IHackneyWorkOrdersService
    {
		Task<WorkOrderEntity> GetWorkOrderByReference(string reference);
    }
}
