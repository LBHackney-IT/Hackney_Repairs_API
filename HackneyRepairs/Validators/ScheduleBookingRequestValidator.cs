using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Models;
using HackneyRepairs.Models;

namespace HackneyRepairs.Validators
{
    public class ScheduleBookingRequestValidator : IScheduleBookingRequestValidator
    {
        private IHackneyRepairsService _hackneyRepairsService;
        public ScheduleBookingRequestValidator(IHackneyRepairsService hackneyRepairsService)
        {
            _hackneyRepairsService = hackneyRepairsService;
        }

        public ValidationResult Validate(string workOrderReference, ScheduleAppointmentRequest request)
        {
            var validationResult = new ValidationResult();
            if (string.IsNullOrEmpty(workOrderReference))
            {
                validationResult.Valid = false;
                validationResult.ErrorMessages.Add("You must provide a work order reference");
            }
            else
            {
                var workOrder = Task.Run(() => _hackneyRepairsService.GetWorkOrderDetails(workOrderReference)).Result;
                if (workOrder == null || string.IsNullOrEmpty(workOrder.wo_ref))
                {
                    validationResult.Valid = false;
                    validationResult.ErrorMessages.Add("Please provide a valid work order reference");
                }
            }

            if (string.IsNullOrEmpty(request.BeginDate))
            {
                validationResult.Valid = false;
                validationResult.ErrorMessages.Add("You must provide a begin Date");
            }
            else
            {
                DateTime beginDateTime;
                if (request.BeginDate.ToLower().EndsWith($"z"))
                {
                    request.BeginDate = request.BeginDate.Remove(request.BeginDate.Length - 1, 1);
                }

                if (!DateTime.TryParse(request.BeginDate, out beginDateTime))
                {
                    validationResult.Valid = false;
                    validationResult.ErrorMessages.Add("Please provide a valid begin date in the following format. Eg:2017-11-10T10:00:00Z");
                }
            }

            if (string.IsNullOrEmpty(request.EndDate))
            {
                validationResult.Valid = false;
                validationResult.ErrorMessages.Add("You must provide a end Date");
            }
            else
            {
                DateTime endDateTime;
                if (request.EndDate.ToLower().EndsWith($"z"))
                {
                    request.EndDate = request.EndDate.Remove(request.EndDate.Length - 1, 1);
                }

                if (!DateTime.TryParse(request.EndDate, out endDateTime))
                {
                    validationResult.Valid = false;
                    validationResult.ErrorMessages.Add("Please provide a valid end date in the following format. Eg:2017-11-10T10:00:00Z");
                }
            }

            return validationResult;
        }
    }
}