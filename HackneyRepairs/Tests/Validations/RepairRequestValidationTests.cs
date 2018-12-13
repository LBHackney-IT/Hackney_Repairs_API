using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HackneyRepairs.Models;
using HackneyRepairs.Validators;
using Xunit;

namespace HackneyRepairs.Tests.Validations
{
    public class RepairRequestValidationTests
    {
        [Fact]
        public void returns_true_if_repair_request_is_valid()
        {
            var request = new RepairRequest
            {
                Priority = "N",
                ProblemDescription = "tap leaking",
                PropertyReference = "12345678",
                Contact = new RepairRequestContact
                {
                    Name = "Al Smith",
                    TelephoneNumber = "07876543210",
                    EmailAddress = "al.smith@hotmail.com",
                    CallbackTime = "8am - 12pm"
                }
            };
            var repairRequestValidator = new RepairRequestValidator();
            var result = repairRequestValidator.Validate(request);
            Assert.True(result.Valid);
            Assert.Equal(result.ErrorMessages.Count, 0);
        }

        [Fact]
        public void returns_false_and_errormessage_if_repair_request_contains_empty_problem()
        {
            var request = new RepairRequest
            {
                Priority = "N",
                ProblemDescription = "",
                PropertyReference = "12345678"
            };
            var repairRequestValidator = new RepairRequestValidator();
            var result = repairRequestValidator.Validate(request);
            Assert.False(result.Valid);
            Assert.Equal(result.ErrorMessages.Count, 2);
            Assert.Contains("Please provide a valid Problem", result.ErrorMessages);
        }

        [Fact]
        public void returns_false_and_errormessage_if_repair_request_contains_empty_Priority()
        {
            var request = new RepairRequest
            {
                Priority = "",
                ProblemDescription = "tap leaking",
                PropertyReference = "12345678"
            };
            var repairRequestValidator = new RepairRequestValidator();
            var result = repairRequestValidator.Validate(request);
            Assert.False(result.Valid);
            Assert.Equal(result.ErrorMessages.Count, 2);
            Assert.Contains("Please provide a valid Priority", result.ErrorMessages);
        }

        [Fact]
        public void returns_false_and_errormessage_if_repair_request_contains_invalid_Priority()
        {
            var request = new RepairRequest
            {
                Priority = "Q",
                ProblemDescription = "tap leaking",
                PropertyReference = "12345678"
            };
            var repairRequestValidator = new RepairRequestValidator();
            var result = repairRequestValidator.Validate(request);
            Assert.False(result.Valid);
            Assert.Equal(result.ErrorMessages.Count, 2);
            Assert.Contains("Please provide a valid Priority", result.ErrorMessages);
        }

        [Fact]
        public void returns_false_and_errormessage_if_repair_request_contains_empty_Property_reference()
        {
            var request = new RepairRequest
            {
                Priority = "N",
                ProblemDescription = "tap leaking",
                PropertyReference = ""
            };
            var repairRequestValidator = new RepairRequestValidator();
            var result = repairRequestValidator.Validate(request);
            Assert.False(result.Valid);
            Assert.Equal(result.ErrorMessages.Count, 2);
            Assert.Contains("You must provide a Property reference", result.ErrorMessages);
        }

        [Fact]
        public void returns_false_and_errormessage_if_repair_request_does_not_have_a_contact()
        {
            var request = new RepairRequest
            {
                Priority = "N",
                ProblemDescription = "tap leaking",
                PropertyReference = "12345678"
            };
            var repairRequestValidator = new RepairRequestValidator();
            var result = repairRequestValidator.Validate(request);
            Assert.False(result.Valid);
            Assert.Equal(result.ErrorMessages.Count, 1);
            Assert.Contains("Please provide a contact", result.ErrorMessages);
        }

        [Fact]
        public void returns_false_and_errormsg_if_repair_request_contact_contains_empty_name()
        {
            var request = new RepairRequest
            {
                Priority = "N",
                ProblemDescription = "tap leaking",
                PropertyReference = "12345678",
                Contact = new RepairRequestContact
                {
                    Name = "",
                    TelephoneNumber = "07876543210",
                    EmailAddress = "al.smith@hotmail.com",
                    CallbackTime = "8am - 12pm"
                }
            };
            var repairRequestValidator = new RepairRequestValidator();
            var result = repairRequestValidator.Validate(request);
            Assert.False(result.Valid);
            Assert.Equal(result.ErrorMessages.Count, 1);
            Assert.Contains("Contact Name cannot be empty", result.ErrorMessages);
        }

        [Fact]
        public void returns_false_and_errormsg_if_repair_request_contact_contains_invalid_telephone()
        {
            var request = new RepairRequest
            {
                Priority = "N",
                ProblemDescription = "tap leaking",
                PropertyReference = "12345678",
                Contact = new RepairRequestContact
                {
                    Name = "Al Smith",
                    TelephoneNumber = "76543210",
                    EmailAddress = "al.smith@hotmail.com",
                    CallbackTime = "8am - 12pm"
                }
            };
            var repairRequestValidator = new RepairRequestValidator();
            var result = repairRequestValidator.Validate(request);
            Assert.False(result.Valid);
            Assert.Equal(result.ErrorMessages.Count, 1);
            Assert.Contains("Telephone number must contain minimum of 10 and maximum of 11 digits.", result.ErrorMessages);
        }

        [Fact]
        public void returns_false_and_errormsg_if_repair_request_contact_contains_invalid_email()
        {
            var request = new RepairRequest
            {
                Priority = "N",
                ProblemDescription = "tap leaking",
                PropertyReference = "12345678",
                Contact = new RepairRequestContact
                {
                    Name = "Al Smith",
                    TelephoneNumber = "07876543210",
                    EmailAddress = "al.smithhotmail.com",
                    CallbackTime = "8am - 12pm"
                }
            };
            var repairRequestValidator = new RepairRequestValidator();
            var result = repairRequestValidator.Validate(request);
            Assert.False(result.Valid);
            Assert.Equal(result.ErrorMessages.Count, 1);
            Assert.Contains("Please enter valid Email address", result.ErrorMessages);
        }

        [Fact]
        public void returns_true_and_no_error_messages_if_repair_request_contact_contains_no_email()
        {
            var request = new RepairRequest
            {
                Priority = "N",
                ProblemDescription = "tap leaking",
                PropertyReference = "12345678",
                Contact = new RepairRequestContact
                {
                    Name = "Al Smith",
                    TelephoneNumber = "07876543210",
                    CallbackTime = "8am - 12pm"
                }
            };
            var repairRequestValidator = new RepairRequestValidator();
            var result = repairRequestValidator.Validate(request);
            Assert.True(result.Valid);
            Assert.Equal(result.ErrorMessages.Count, 0);
        }

        [Fact]
        public void returns_true_and_no_error_messages_if_repair_request_contact_contains_no_collback()
        {
            var request = new RepairRequest
            {
                Priority = "N",
                ProblemDescription = "tap leaking",
                PropertyReference = "12345678",
                Contact = new RepairRequestContact
                {
                    Name = "Al Smith",
                    TelephoneNumber = "07876543210"
                }
            };
            var repairRequestValidator = new RepairRequestValidator();
            var result = repairRequestValidator.Validate(request);
            Assert.True(result.Valid);
            Assert.Equal(result.ErrorMessages.Count, 0);
        }
    }
}
