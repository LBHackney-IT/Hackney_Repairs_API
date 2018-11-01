﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HackneyRepairs.DTOs;
using HackneyRepairs.Models;

namespace HackneyRepairs.Interfaces
{
    public interface IUhtRepository
    {
        Task<DrsOrder> GetWorkOrderDetails(string workOrderReference);
        Task<bool> UpdateRequestStatus(string repairRequestReference);
        Task<int?> UpdateVisitAndBlockTrigger(string workOrderReference, DateTime startDate, DateTime endDate, int orderId, int bookingId, string slotDetail);
        Task<UHWorkOrder> GetWorkOrder(string workOrderReference);
        Task<IEnumerable<UHWorkOrder>> GetWorkOrders(string[] workOrderReferences);
        Task<IEnumerable<UHWorkOrder>> GetWorkOrderByPropertyReference(string propertyReference);
        Task<IEnumerable<UHWorkOrder>> GetWorkOrdersByPropertyReferences(string[] propertyReferences, DateTime since, DateTime until);
        Task<IEnumerable<UHWorkOrder>> GetWorkOrderByBlockReference(string blockReference, string trade, DateTime since, DateTime until);
        Task<IEnumerable<RepairRequestBase>> GetRepairRequestsByPropertyReference(string propertyReference);
        Task<IEnumerable<RepairWithWorkOrderDto>>GetRepairRequest(string repairReference);
        Task<DetailedAppointment> GetLatestAppointmentByWorkOrderReference(string workOrderReference);
        Task<IEnumerable<DetailedAppointment>> GetAppointmentsByWorkOrderReference(string workOrderReference);
        Task<IEnumerable<UHWorkOrderFeed>> GetWorkOrderFeed(string startId, int resultSize);
    }
}
