using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Models;
using RepairsService;
using UserCredential = HackneyRepairs.PropertyService.UserCredential;

namespace HackneyRepairs.Services
{
    public class HackneyRepairsServiceRequestBuilder : IHackneyRepairsServiceRequestBuilder
    {
        private NameValueCollection _configuration;
        private IHackneyRepairsService _repairsService;

        public HackneyRepairsServiceRequestBuilder(NameValueCollection configuration)
        {
            _configuration = configuration;
        }

        public NewRepairRequest BuildNewRepairRequest(RepairRequest request)
        {
            return new NewRepairRequest
            {
                RepairRequest = new RepairRequestInfo
                {
                    Problem = request.ProblemDescription,
                    Priority = request.Priority?.ToUpper(),
                    PropertyRef = request.PropertyReference,
                    Name = request.Contact.Name,
                    Phone = request.Contact.TelephoneNumber
                },
                DirectUser = GetUserCredentials(),
                SourceSystem = GetUhSourceSystem()
            };
        }

        private RepairsService.UserCredential GetUserCredentials()
        {
            return new RepairsService.UserCredential
            {
                UserName = _configuration.Get("UHUsername"),
                UserPassword = _configuration.Get("UHPassword")
            };
        }

        private string GetUhSourceSystem()
        {
            return _configuration.Get("UHSourceSystem");
        }

        public RepairRefRequest BuildRepairRequest(string request)
        {
            return new RepairRefRequest
            {
                RequestReference = request,
                DirectUser = GetUserCredentials(),
                SourceSystem = GetUhSourceSystem()
            };
        }

        public NewRepairTasksRequest BuildNewRepairTasksRequest(RepairRequest request)
        {
            var taskList = new List<RepairTaskInfo>();
            foreach (var workorder in request.WorkOrders)
            {
                taskList.Add(new RepairTaskInfo
                {
                    PropertyReference = request.PropertyReference,
                    JobCode = workorder.SorCode,
                    SupplierReference = getContractorForSOR(workorder.SorCode)
                });
            }

            return new NewRepairTasksRequest
            {
                RepairRequest = new RepairRequestInfo
                {
                    Problem = request.ProblemDescription,
                    Priority = request.Priority.ToUpper(),
                    PropertyRef = request.PropertyReference,
                    Name = request.Contact.Name,
                    Phone = request.Contact.TelephoneNumber
                },
                DirectUser = GetUserCredentials(),
                SourceSystem = GetUhSourceSystem(),
                TaskList = taskList.ToArray()
            };
        }

        public WorksOrderRequest BuildWorksOrderRequest(string request)
        {
            return new WorksOrderRequest
            {
                OrderReference = request,
                DirectUser = GetUserCredentials(),
                SourceSystem = GetUhSourceSystem()
            };
        }

        public string getContractorForSOR(string sorCode)
        {
            string[] sorLookupOptions = _configuration.Get("UhSorSupplierMapping").Split('|');
            Dictionary<string, string> sorDictionary = new Dictionary<string, string>();
            for (int a = 0; a < sorLookupOptions.Length; a++)
            {
                sorDictionary.Add(sorLookupOptions[a].Split(',')[0], sorLookupOptions[a].Split(',')[1]);
            }

            if (sorDictionary.ContainsKey(sorCode))
            {
                return sorDictionary[sorCode];
            }
            else
            {
                throw new InvalidSORCodeException();
            }
        }
    }

    public class InvalidSORCodeException : Exception
    {
    }
}
