using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Models;
using System.Text.RegularExpressions;
using System.Collections.Specialized;

namespace HackneyRepairs.Validators
{
    public class RepairRequestValidator : IRepairRequestValidator
    {
        private NameValueCollection _configuration;

        public RepairRequestValidator()
        { }
        public RepairRequestValidator(NameValueCollection configuration)
        {
            _configuration = configuration;
        }

        public RepairRequestValidationResult Validate(RepairRequest request)
        {
            var validationResult = new RepairRequestValidationResult(request);

            if (request == null)
            {
                validationResult.Valid = false;
                validationResult.ErrorMessages.Add("Please provide  a valid repair request");
                validationResult.RepairApiError.Add(new JsonApiErrorMessage
                {
                        Code = 400,
                        DeveloperMessage = "Please provide  a valid repair request",
                        UserMessage = "Please provide a valid repair request",
                        Source = @"/"
                });
                
                return validationResult;
            }

            if (string.IsNullOrWhiteSpace(request.ProblemDescription))
            {
                validationResult.Valid = false;
                validationResult.ErrorMessages.Add("Please provide a valid Problem");
                validationResult.RepairApiError.Add(new JsonApiErrorMessage
                {
                    Code = 400,
                    DeveloperMessage = "Problem description cannot be null or empty",
                    UserMessage = "Please provide a valid Problem",
                    Source = @"/problemDescription"
                });
            }

            if (!string.IsNullOrWhiteSpace(request.PropertyReference))
            {
                var propRefPattern = "^[0-9]{8}$";
                if (!Regex.IsMatch(request.PropertyReference, propRefPattern))
                {
                    validationResult.Valid = false;
                    validationResult.ErrorMessages.Add("Please provide a valid Property reference");
                    validationResult.RepairApiError.Add(new JsonApiErrorMessage
                    {
                        Code = 400,
                        DeveloperMessage = "Property reference is invalid",
                        UserMessage = "Please provide a valid Property reference",
                        Source = @"/propertyReference"
                    });
                }
            }
            else
            {
                validationResult.Valid = false;
                validationResult.ErrorMessages.Add("You must provide a Property reference");
                validationResult.RepairApiError.Add(new JsonApiErrorMessage
                {
                    Code = 400,
                    DeveloperMessage = "Property reference cannot be null or empty",
                    UserMessage = "You must provide a Property reference",
                    Source = @"/propertyReference"
                });
            }

            var priorityPattern = "^[UGINEZVMuginezvm]{1}$";
            if (!Regex.IsMatch(request.Priority, priorityPattern))
            {
                validationResult.Valid = false;
                validationResult.ErrorMessages.Add("Please provide a valid Priority");
                validationResult.RepairApiError.Add(new JsonApiErrorMessage
                {
                    Code = 400,
                    DeveloperMessage = "Priority is invalid",
                    UserMessage = "Please provide a valid Priority",
                    Source = @"priority"
                });
            }

            if (request.WorkOrders != null)
            {
                int _count = 0;

                if (request.WorkOrders.Count > 0)
                {
                    foreach (WorkOrder or in request.WorkOrders)
                    {
                        if (String.IsNullOrEmpty(or.SorCode))
                        {
                            validationResult.Valid = false;
                            validationResult.ErrorMessages.Add("If Repair request has workOrders you must provide a sorCode");
                            validationResult.RepairApiError.Add(new JsonApiErrorMessage
                            {
                                Code = 400,
                                DeveloperMessage = "sorCode is invalid",
                                UserMessage = "SOR code is missing, please provide a valid SOR code",
                                Source = $@"/workOrders/{_count}/sorCode"
                            });
                        }
                        else
                        {
                            var sorPattern = "^[A-Za-z0-9]{7,8}$";
                            if (!Regex.IsMatch(or.SorCode, sorPattern))
                            {
                                validationResult.Valid = false;
                                validationResult.ErrorMessages.Add("If Repair request has workOrders you must provide a valid sorCode");
                                validationResult.RepairApiError.Add(new JsonApiErrorMessage
                                {
                                    Code = 400,
                                    DeveloperMessage = "sorCode is invalid",
                                    UserMessage = "Please provide a valid SOR code",
                                    Source = $@"/workOrders/{_count}/sorCode"
                                });
                            }
                            else
                            {
                                //Check provided sorCode is a listed sorcode
                                if (!this.getContractorForSOR(or.SorCode))
                                {
                                    validationResult.Valid = false;
                                    validationResult.RepairApiError.Add(new JsonApiErrorMessage
                                    {
                                        Code = 400,
                                        DeveloperMessage = "sorCode provided is prohibited",
                                        UserMessage = "If Repair request has workOrders you must provide a valid sorCode",
                                        Source = $@"/workOrders/{_count}/sorCode"
                                    });
                                }
                            }
                        }

                        //increment count in loop
                        _count++;
                    }
                }
                else
                {
                    validationResult.Valid = false;
                    validationResult.ErrorMessages.Add("If Repair request has workOrders you must provide a valid sorCode");
                    validationResult.RepairApiError.Add(new JsonApiErrorMessage
                    {
                        Code = 400,
                        DeveloperMessage = "sorCode is invalid",
                        UserMessage = "If Repair request has workOrders you must provide a valid sorCode",
                        Source = $@"/workOrders/{_count}/sorCode"
                    });
                }
            }

            if (request.Contact != null)
            {
                if (request.Contact.Name == null || request.Contact.Name.Length < 1)
                {
                    validationResult.Valid = false;
                    validationResult.ErrorMessages.Add("Contact Name cannot be empty");
                    validationResult.RepairApiError.Add(new JsonApiErrorMessage
                    {
                        Code = 400,
                        DeveloperMessage = "Contact Name cannot be empty",
                        UserMessage = "Please provide a name for the contact",
                        Source = $@"/contact/name"
                    });
                }

                var telephonePattern = "^[0-9]{10,11}$";
                var telephone = request.Contact.TelephoneNumber.Replace(" ", "");
                if (!Regex.IsMatch(telephone, telephonePattern))
                {
                    validationResult.Valid = false;
                    validationResult.ErrorMessages.Add("Telephone number must contain minimum of 10 and maximum of 11 digits.");
                    validationResult.RepairApiError.Add(new JsonApiErrorMessage
                    {
                        Code = 400,
                        DeveloperMessage = "Contact Telephone number is invalid",
                        UserMessage = "Telephone number must contain minimum of 10 and maximum of 11 digits",
                        Source = $@"/contact/telephoneNumber"
                    });
                }

                if (request.Contact.EmailAddress != null && request.Contact.EmailAddress != string.Empty)
                {
                    var emailPattern = @"[A-Za-z][A-Za-z0-9._%-]+[A-Za-z_\-0-9]@[A-Za-z0-9._%-]+(\.[A-Za-z]{2,4}|\.[A-Za-z]{2,3}\.[A-Za-z]{2,3})([,;]?\s*[A-Za-z_-][A-Za-z0-9._%-]+[A-Za-z_\-0-9]@[A-Za-z0-9._%-]+(\.[A-Za-z]{2,4}|\.[A-Za-z]{2,3}\.[A-Za-z]{2,3}))*";
                    var email = request.Contact.EmailAddress.Replace(" ", "");
                    if (!Regex.IsMatch(email, emailPattern))
                    {
                        validationResult.Valid = false;
                        validationResult.ErrorMessages.Add("Please enter valid Email address");
                        validationResult.RepairApiError.Add(new JsonApiErrorMessage
                        {
                            Code = 400,
                            DeveloperMessage = "Email address is invalid",
                            UserMessage = "Please enter valid Email address",
                            Source = $@"/contact/emailAddress"
                        });
                    }
                }
            }
            else
            {
                validationResult.Valid = false;
                validationResult.ErrorMessages.Add("Please provide a contact");
                validationResult.RepairApiError.Add(new JsonApiErrorMessage
                {
                    Code = 400,
                    DeveloperMessage = "Contact cannot be null",
                    UserMessage = "Please provide a contact",
                    Source = $@"/contact"
                });
            }

            return validationResult;
        }

        //Used to validate sorcode to return JSONAPI source error format
        //Method duplicated from HackneyRepairsServiceRequestBuilder class
        private bool getContractorForSOR(string sorCode)
        {
            string[] sorLookupOptions = _configuration.Get("UhSorSupplierMapping").Split('|');
            Dictionary<string, string> sorDictionary = new Dictionary<string, string>();

            foreach (string s in sorLookupOptions)
            {
                sorDictionary.Add(s.Split(',')[0], s.Split(',')[1]);
            }

            return sorDictionary.ContainsKey(sorCode) ? true : false;
        }
    }

    public class RepairRequestValidationResult : ValidationResult
    {
        public RepairRequestValidationResult(RepairRequest request)
        {
            ErrorMessages = new List<string>();
            RepairApiError = new List<JsonApiErrorMessage>();
            Valid = true;
            RepairRequest = request;
        }

        public RepairRequest RepairRequest { get; set; }
        public List<JsonApiErrorMessage> RepairApiError { get; }
    }
}
