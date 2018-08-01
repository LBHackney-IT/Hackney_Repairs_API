using System;
using System.Threading.Tasks;
using HackneyRepairs.Entities;
using HackneyRepairs.Interfaces;

namespace HackneyRepairs.Services
{
    public class FakeWorkOrdersService : IHackneyWorkOrdersService
    {      
	public Task<WorkOrderEntity> GetWorkOrderByReference(string reference)
	{
	    if (string.Equals(reference, "9999999999"))
	    {
	        return Task.Run(() => (WorkOrderEntity)null);
	    }
	    var workOrderEntity = new WorkOrderEntity
	    {
	        wo_ref = reference
	    };
	    return Task.Run(() => workOrderEntity);
    	}
    }
}
