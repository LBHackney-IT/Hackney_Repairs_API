using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Models;
using System.Text.RegularExpressions;

namespace HackneyRepairs.Validators
{
    public class RepairRequestValidator : IRepairRequestValidator
    {
        public RepairRequestValidationResult Validate(RepairRequest request)
        {
            var validationResult = new RepairRequestValidationResult(request);

            if (request == null)
            {
                validationResult.Valid = false;
                validationResult.ErrorMessages.Add("Please provide  a valid repair request");
                return validationResult;
            }

            if (string.IsNullOrWhiteSpace(request.ProblemDescription))
            {
                validationResult.Valid = false;
                validationResult.ErrorMessages.Add("Please provide a valid Problem");
            }

            if (!string.IsNullOrWhiteSpace(request.PropertyReference))
            {
                var propRefPattern = "^[0-9]{8}$";
                if (!Regex.IsMatch(request.PropertyReference, propRefPattern))
                {
                    validationResult.Valid = false;
                    validationResult.ErrorMessages.Add("Please provide a valid Property reference");
                }
            }
            else
            {
                validationResult.Valid = false;
                validationResult.ErrorMessages.Add("You must provide a Property reference");
            }

            var priorityPattern = "^[UGINEZVMuginezvm]{1}$";
            if (!Regex.IsMatch(request.Priority, priorityPattern))
            {
                validationResult.Valid = false;
                validationResult.ErrorMessages.Add("Please provide a valid Priority");
            }

            if (request.WorkOrders != null)
            {
                if (!(request.WorkOrders.Count == 0))
                {
                    foreach (WorkOrder or in request.WorkOrders)
                    {
                        if (!(or.SorCode == null || or.SorCode.Length == 0))
                        {
                            var sorPattern = "^[A-Za-z0-9]{7,8}$";
                            if (!Regex.IsMatch(or.SorCode, sorPattern))
                            {
                                validationResult.Valid = false;
                                validationResult.ErrorMessages.Add("If Repair request has workOrders you must provide a valid sorCode");
                            }
                        }
                        else
                        {
                            validationResult.Valid = false;
                            validationResult.ErrorMessages.Add("If Repair request has workOrders you must provide a sorCode");
                        }
                    }
                }
                else
                {
                    validationResult.Valid = false;
                    validationResult.ErrorMessages.Add("If Repair request has workOrders you must provide a valid sorCode");
                }
            }

            if (request.Contact != null)
            {
                if (request.Contact.Name == null || request.Contact.Name.Length < 1)
                {
                    validationResult.Valid = false;
                    validationResult.ErrorMessages.Add("Contact Name cannot be empty");
                }

                var telephonePattern = "^[0-9]{10,11}$";
                var telephone = request.Contact.TelephoneNumber.Replace(" ", "");
                if (!Regex.IsMatch(telephone, telephonePattern))
                {
                    validationResult.Valid = false;
                    validationResult.ErrorMessages.Add("Telephone number must contain minimum of 10 and maximum of 11 digits.");
                }

                if (request.Contact.EmailAddress != null && request.Contact.EmailAddress != string.Empty)
                {
                    var emailPattern = @"[A-Za-z][A-Za-z0-9._%-]+[A-Za-z_\-0-9]@[A-Za-z0-9._%-]+(\.[A-Za-z]{2,4}|\.[A-Za-z]{2,3}\.[A-Za-z]{2,3})([,;]?\s*[A-Za-z_-][A-Za-z0-9._%-]+[A-Za-z_\-0-9]@[A-Za-z0-9._%-]+(\.[A-Za-z]{2,4}|\.[A-Za-z]{2,3}\.[A-Za-z]{2,3}))*";
                    var email = request.Contact.EmailAddress.Replace(" ", "");
                    if (!Regex.IsMatch(email, emailPattern))
                    {
                        validationResult.Valid = false;
                        validationResult.ErrorMessages.Add("Please enter valid Email address");
                    }
                }
            }
            else
            {
                validationResult.Valid = false;
                validationResult.ErrorMessages.Add("Please provide a contact");
            }

            return validationResult;
        }
    }

    public class RepairRequestValidationResult : ValidationResult
    {
        public RepairRequestValidationResult(RepairRequest request)
        {
            ErrorMessages = new List<string>();
            Valid = true;
            RepairRequest = request;
        }

        public RepairRequest RepairRequest { get; set; }
    }
}
