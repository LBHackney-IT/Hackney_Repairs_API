﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HackneyRepairs.Models;
using RepairsService;

namespace HackneyRepairs.Interfaces
{
    public interface IHackneyRepairsServiceRequestBuilder
    {
        NewRepairRequest BuildNewRepairRequest(RepairRequest request);
        RepairRefRequest BuildRepairRequest(string request);
        NewRepairTasksRequest BuildNewRepairTasksRequest(RepairRequest request);
        WorksOrderRequest BuildWorksOrderRequest(string request);
    }
}
