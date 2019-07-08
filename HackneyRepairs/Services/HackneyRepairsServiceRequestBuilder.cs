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

        public List<RepairTaskInfo> GetTaskList(RepairRequest request)
        {
            var taskList = new List<RepairTaskInfo>();
            foreach (var workorder in request.WorkOrders)
            {
                taskList.Add(new RepairTaskInfo
                {
                    PropertyReference = request.PropertyReference,
                    PriorityCode = request.Priority.ToUpper(),
                    JobCode = workorder.SorCode,
                    SupplierReference = getContractorForSOR(workorder.SorCode),
                    EstimatedUnits = decimal.Parse(workorder.EstimatedUnits)
                });
            }

            return taskList;
        }

        public NewRepairTasksRequest BuildNewRepairTasksRequestAsUser(RepairRequest request, string sessionToken)
        {
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
                SessionToken = sessionToken,
                CompanyCode = "001",
                SourceSystem = GetUhSourceSystem(),
                TaskList = GetTaskList(request).ToArray()
            };
        }

        public NewRepairTasksRequest BuildNewRepairTasksRequest(RepairRequest request)
        {                        
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
                TaskList = GetTaskList(request).ToArray()
            };                            
        }

        private string GetUHSessionToken(string uHUsername)
        {
            throw new NotImplementedException();
        }

        public WorksOrderRequest BuildWorksOrderRequestWithSession(string request, string sessionToken)
        {
            return new WorksOrderRequest
            {
                OrderReference = request,
                SessionToken = sessionToken,
                SourceSystem = GetUhSourceSystem(),
                CompanyCode = "001"
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
