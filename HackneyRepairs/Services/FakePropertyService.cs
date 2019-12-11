using System;
using System.Threading.Tasks;
using HackneyRepairs.Interfaces;
using HackneyRepairs.PropertyService;
using HackneyRepairs.Models;
using HackneyRepairs.Actions;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace HackneyRepairs.Services
{
    public class FakePropertyService : IHackneyPropertyService
    {
        public Task<PropertyInfoResponse> GetPropertyListByPostCodeAsync(ListByPostCodeRequest request)
        {
            var response = new PropertyInfoResponse()
            {
                PropertyList = new PropertySummary[2],
                Success = true
            };
            var property1 = new PropertySummary()
            {
                ShortAddress = "Back Office, Robert House, 6 - 15 Florfield Road",
                PostCodeValue = "E8 1DT",
                PropertyReference = "1/525252525"
            };
            var property2 = new PropertySummary()
            {
                ShortAddress = "Meeting room, Maurice Bishop House, 17 Reading Lane",
                PostCodeValue = "E8 1DT",
                PropertyReference = "6/32453245   "
            };
            response.PropertyList[0] = property1;
            response.PropertyList[1] = property2;
            switch (request.PostCode)
            {
                case "E8 1DT":
                    return Task.Run(() => response);
                case "E8 2LT":
                    return Task.Run(() => new PropertyInfoResponse
                                            {
                                                Success = false,
                                                ErrorCode = 9903,
                                                ErrorMessage = "Master Password is Invalid.",
                                                PropertyList = null
                                            });
                default:
                    return Task.Run(() => new PropertyInfoResponse
                                            {
                                                PropertyList = new PropertySummary[0],
                                                Success = true
                                            });
            }
        }

        public Task<PropertyDetails> GetPropertyByRef(string reference)
        {
            switch (reference)
            {
                case "52525252":
                    return Task.Run(() => new PropertyDetails()
                    {
                        ShortAddress = "Back Office, Robert House, 6 - 15 Florfield Road    ",
                        PostCodeValue = "E8 1DT",
                        PropertyReference = "52525252",
                        Maintainable = true,
                        TenureCode = "SEC",
                        TenureDescription = "Secure",
                        LettingAreaDescription = "Lordship South TMO (SN) H2556",
                        PropertyTypeCode = "NBD",
                        PropertyTypeDescription = "New Build Dwellings"
                    });
                case "5252":
                    throw new PropertyServiceException();
                default:
                    return Task.Run(() => (PropertyDetails)null);
            }
        }

        public Task<List<NewBuildWarrantyData>> GetNewBuildPropertyWarrantByRefAsync(string reference)
        {
            var datas = new NewBuildWarrantyData[]
            {
                new NewBuildWarrantyData
                {
                     ComponentName = "Roof Covering Warranty",
                     CompletionDate = DateTime.Parse("2037-06-30 00:00:00"),
                     Status = "Warranty Period",
                     ContactDetails = "0330 123 1234",
                     Manufacturer = "LBH Manufacture"
                },
                new NewBuildWarrantyData
                {
                     ComponentName = "Lift Warranty",
                     CompletionDate = DateTime.Parse("2037-06-30T00:00:00"),
                     Status = "Warranty Period",
                     ContactDetails = "0330 123 1234",
                     Manufacturer = "LBH Manufacture"
                }
            };

            var emptydata = new NewBuildWarrantyData()
            {
                ComponentName = "XXXXXXXXXX",
                CompletionDate = DateTime.Parse("0001-01-01T00:00:00"),
                Status = null,
                ContactDetails = null,
                Manufacturer = null
            };

            List<NewBuildWarrantyData> emptylist = new List<NewBuildWarrantyData>();
            emptylist.Add(emptydata);

            switch (reference)
            {
                case "00088888":
                    return Task.Run(() => datas.ToList());
                case "5252":
                    return Task.Run(() => emptylist);
                default:
                    return Task.Run(() => emptylist);
            }
        }

        public Task<PropertyDetails[]> GetPropertiesByReferences(string[] references)
        {
            var fakeProperty = new PropertyDetails
            {
                ShortAddress = "Back Office, Robert House, 6 - 15 Florfield Road    ",
                PostCodeValue = "E8 1DT",
                PropertyReference = "random",
                Maintainable = true,
                LevelCode = 7,
                Description = "none"
            };

            if (references.Contains("5454545454") && references.Contains("123435234"))
            {
                return Task.Run(() => new PropertyDetails[]
                {
                    fakeProperty,
                    fakeProperty
                });
            }

            if (references.Contains("5454545454"))
            {
                return Task.Run(() => new PropertyDetails[]
                {
                    fakeProperty
                });
            }

            if (references.Contains("5252"))
            {
                throw new PropertyServiceException();
            }

            return Task.Run(() => new PropertyDetails[0]);
        }

        public Task<bool> GetMaintainable(string reference)
        {
            return Task.Run(() => reference == "525252525");
        }

        public Task<PropertyLevelModel[]> GetPropertyListByPostCode(string post_code, int? maxLevel, int? minLevel)
        {
            var PropertyList = new PropertyLevelModel[2];
            PropertyLevelModel[] emptyPropertyList;
            var property1 = new PropertyLevelModel()
            {
                Address = "Back Office, Robert House, 6 - 15 Florfield Road",
                Postcode = "E8 1DT",
                PropertyReference = "1/525252525"
            };
            var property2 = new PropertyLevelModel()
            {
                Address = "Meeting room, Maurice Bishop House, 17 Reading Lane",
                Postcode = "E8 1DT",
                PropertyReference = "6/32453245   "
            };
            PropertyList[0] = property1;
            PropertyList[1] = property2;
            switch (post_code)
            {
                case "E8 1DT":
                    return Task.Run(() => PropertyList);
                case "E8 2LN":
                    emptyPropertyList = null;
                    return Task.Run(() => emptyPropertyList);
                 default:
                    emptyPropertyList = new PropertyLevelModel[0];
                    return Task.Run(() => emptyPropertyList);
            }
        }
       
        public Task<PropertyLevelModel[]> GetPropertyListByFirstLineOfAddress(string address, int limit = 2)
        {
            var PropertyList = new PropertyLevelModel[2];
            PropertyLevelModel[] emptyPropertyList;
            var property1 = new PropertyLevelModel()
            {
                Address = "2 Acacia House  Lordship Road",
                Postcode = "N16 0PX",
                PropertyReference = "1/43453543"
            };
            var property2 = new PropertyLevelModel()
            {
                Address = "4 Acacia House  Lordship Road",
                Postcode = "N16 0PX",
                PropertyReference = "2/32453245"
            };
            PropertyList[0] = property1;
            PropertyList[1] = property2;
            switch (address)
            {
                case "Acacia":
                    return Task.Run(() => PropertyList);
                case "Elmbridge":
                    emptyPropertyList = null;
                    return Task.Run(() => emptyPropertyList);
                default:
                    emptyPropertyList = new PropertyLevelModel[0];
                    return Task.Run(() => emptyPropertyList);
            }
        }

        public Task<PropertyLevelModel[]> GetFacilitiesByPropertyRef(string reference)
        {
            var PropertyList = new PropertyLevelModel[2];
            PropertyLevelModel[] emptyPropertyList;
            var property1 = new PropertyLevelModel()
            {
                Address = "Lift 1296 1-16 Oak House  Lordship Road",
                Postcode = "N16 0PX",
                PropertyReference = "1/43453543"
            };
            var property2 = new PropertyLevelModel()
            {
                Address = "Cleaners Room, Lobby Laburnum Court  Laburnum Street",
                Postcode = "N16 0PX",
                PropertyReference = "2/32453245"
            };
            PropertyList[0] = property1;
            PropertyList[1] = property2;
            switch (reference)
            {
                case "00000038":
                    return Task.Run(() => PropertyList);
                case "038":
                    emptyPropertyList = null;
                    return Task.Run(() => emptyPropertyList);
                //throw new PropertyServiceException();
                default:
                    throw new PropertyServiceException();
                    //return Task.Run(() => (PropertyLevelModel[])null);
                    //emptyPropertyList = new PropertyLevelModel[0];
                    //return Task.Run(() => emptyPropertyList);
            }
        }

        public Task<PropertyDetails> GetPropertyBlockByRef(string reference)
        {
            switch (reference)
            {
                case "52525252":
                    return Task.Run(() => new PropertyDetails()
                    {
                        ShortAddress = "Back Office Block, Robert House, 6 - 15 Florfield Road    ",
                        PostCodeValue = "E8 1DT",
                        PropertyReference = "525252527",
                        Maintainable = true,
                        TenureCode = "",
                        TenureDescription = ""
                    });
                case "5252":
                    throw new PropertyServiceException();
                default:
                    return Task.Run(() => (PropertyDetails)null);
            }
        }

        public Task<PropertyDetails> GetPropertyEstateByRef(string reference)
        {
            switch (reference)
            {
                case "52525252":
                    return Task.Run(() => new PropertyDetails()
                    {
                        ShortAddress = "Back Office Estate, Robert House, 6 - 15 Florfield Road    ",
                        PostCodeValue = "E8 1DT",
                        PropertyReference = "525252527",
                        Maintainable = true,
                        TenureCode = "",
                        TenureDescription = ""
                    });
                case "5252":
                    throw new PropertyServiceException();
                default:
                    return Task.Run(() => (PropertyDetails)null);
            }
        }

        public Task<List<PropertyLevelModel>> GetPropertyLevelInfosForParent(string parentReference)
		{ 
			if (string.Equals(parentReference, "99999999"))
            {
				List<PropertyLevelModel> emptyList = new List<PropertyLevelModel>();
				return Task.Run(() => (List<PropertyLevelModel>)emptyList);
            }

			List<PropertyLevelModel> levelInfos = new List<PropertyLevelModel>()
			{
				new PropertyLevelModel()
				{
					PropertyReference = "12345678",
					Description = "Dwelling"
				},
				new PropertyLevelModel()
                {
					PropertyReference = "12345677",
					Description = "Dwelling"
                },
				new PropertyLevelModel()
                {
					PropertyReference = "12345676",
					Description = "Garage"
                }
			};
			return Task.Run(() => (levelInfos));
		}

        public Task<PropertyLevelModel> GetPropertyLevelInfo(string reference)
        {
			switch (reference)
			{
				case "00079999":
					return Task.Run(() => (new PropertyLevelModel()
					{
						MajorReference = "00074866",
						PropertyReference = "00079999",
						Address = "St Thomass Square 1 Pitcarin House",
						LevelCode = "7"
					}));

				case "00074866":
                    return Task.Run(() => (new PropertyLevelModel()
                    {
						MajorReference = "00078556",
                        PropertyReference = "00074866",
                        Address = "St Thomass Square 1-93 Pitcarin House",
                        LevelCode = "3"
                    }));

				case "00078556":
                    return Task.Run(() => (new PropertyLevelModel()
                    {
						MajorReference = "00087086",
                        PropertyReference = "00078556",
                        Address = "Frampton Park Road Frampton Park Estate",
                        LevelCode = "2"
                    }));

				case "00087086":
                    return Task.Run(() => (new PropertyLevelModel()
                    {
						MajorReference = "",
                        PropertyReference = "00087086",
                        Address = "Wilton Way Hackney Homes Limited",
                        LevelCode = "0"
                    }));

				case "99999999":
					return Task.Run(() => ((PropertyLevelModel)null));

				case "66666666":
					throw new PropertyServiceException();

				default:
					return Task.Run(() => (new PropertyLevelModel()
					{
						PropertyReference = "12345678",
						Description = "Dwelling"
					}));
            }
        }
    }
}