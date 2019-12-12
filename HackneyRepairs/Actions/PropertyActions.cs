using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HackneyRepairs.Formatters;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Models;
using HackneyRepairs.PropertyService;
using Microsoft.EntityFrameworkCore.Query;

namespace HackneyRepairs.Actions
{
    public class PropertyActions
    {
        private IHackneyPropertyService _propertyService;
        private IHackneyWorkOrdersService _workordersService;
        private IHackneyPropertyServiceRequestBuilder _requestBuilder;
        private readonly ILoggerAdapter<PropertyActions> _logger;

        public PropertyActions(IHackneyPropertyService propertyService, IHackneyPropertyServiceRequestBuilder requestBuilder, IHackneyWorkOrdersService workOrdersService, ILoggerAdapter<PropertyActions> logger)
        {
            _propertyService = propertyService;
            _requestBuilder = requestBuilder;
            _workordersService = workOrdersService;
            _logger = logger;
        }

        public async Task<IEnumerable<UHWorkOrder>> GetWorkOrdersForBlock(string propertyReference, string trade, DateTime since, DateTime until)
        {
            if (string.IsNullOrEmpty(trade) || string.IsNullOrEmpty(propertyReference))
            {
                throw new InvalidParameterException();
            }

            var propertyInfo = await _propertyService.GetPropertyLevelInfo(propertyReference);
            if (propertyInfo == null)
            {
                throw new MissingPropertyException();
            }

            int propertyLevel;
            int.TryParse(propertyInfo.LevelCode, out propertyLevel);
            if (propertyLevel < 3)
            {
                throw new InvalidParameterException();
            }

            _logger.LogError($"Gathering block or sub-block references for {propertyReference}");
            var blockReferences = await GetBlockReferences(propertyReference);
            if (!blockReferences.Any())
            {
                _logger.LogError($"No block or sub-block identified for {propertyReference}, returning an empty list");
                return new List<UHWorkOrder>();
            }

            _logger.LogInformation($"Finding work order details for block reference (including children): {GenericFormatter.CommaSeparate(blockReferences.ToArray())}, trade: {trade}");
            var blockResult = await _workordersService.GetWorkOrderByBlockReference(blockReferences.ToArray(), trade, since, until);
            _logger.LogInformation($"{blockResult.Count()} work order details returned for block or sub-block references (including children): {GenericFormatter.CommaSeparate(blockReferences.ToArray())}, trade: {trade}");
            return blockResult;
		}

        public async Task<IEnumerable<PropertyLevelModel>> GetPropertyHierarchy(string reference)
        {
            _logger.LogError($"Getting property hierarchy for {reference}");
            try
            {
                var results = new List<PropertyLevelModel>();
                string parent = reference;

                while (!String.IsNullOrWhiteSpace(parent))
                {
                    var response = await _propertyService.GetPropertyLevelInfo(parent);
                    if (response == null && string.Equals(parent, reference))
                    {
                        _logger.LogError($"Property not found for {reference}");
                        throw new MissingPropertyException();
                    }

                    if (response == null)
                    {
                        _logger.LogError($"Property hierarchy appears to be broken, parent property {reference} does not exist. Returning results until this point");
                        break;
                    }

                    results.Add(response);
                    parent = response.MajorReference;
                }

                GenericFormatter.TrimStringAttributesInEnumerable(results);
                return results;
            }
            catch (MissingPropertyException e)
            {
                _logger.LogError($"Finding a property with the property reference: {reference} returned an error: {e.Message}");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError($"Finding a property with the property reference: {reference} returned an error: {e.Message}");
                throw new PropertyServiceException();
            }
        }

        public async Task<object> FindProperty(string postcode, int? maxLevel, int? minLevel)
        {
            _logger.LogInformation($"Finding property by postcode: {postcode}");
            try
            {
                var response = await _propertyService.GetPropertyListByPostCode(postcode, maxLevel, minLevel);
                if (response.Any())
                {
                    GenericFormatter.TrimStringAttributesInEnumerable(response);
                }

                return new
                {
                    results = response
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Finding property by postcode: {postcode} returned an error: {e.Message}");
                throw new PropertyServiceException();
            }
        }

        //Return a list of addresses using the first line of address
        public async Task<object> FindPropertyByFirstLineOfAddress(string firstLineOfAddress, int limit)
        {
            _logger.LogInformation($"Finding property by first line of address: {firstLineOfAddress}");
            try
            {
                var response = await _propertyService.GetPropertyListByFirstLineOfAddress(firstLineOfAddress, limit);
                if (response.Any())
                {
                    GenericFormatter.TrimStringAttributesInEnumerable(response);
                }

                return new
                {
                    results = response
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Finding property by address: {firstLineOfAddress} returned an error: {e.Message}");
                throw new PropertyServiceException();
            }
        }

        public async Task<object> FindPropertyDetailsByRef(string reference)
        {
            _logger.LogInformation($"Finding property by reference: {reference}");
            try
            {
                var response = await _propertyService.GetPropertyByRef(reference);
                if (response == null)
                {
                    throw new MissingPropertyException();
                }
                else
                {
                    return BuildPropertyDetails(response);
                }
            }
            catch (MissingPropertyException e)
            {
                _logger.LogError($"Finding a property with the property reference: {reference} returned an error: {e.Message}");
                throw e;
            }
            catch (Exception e)
            {
                _logger.LogError($"Finding a property with the property reference: {reference} returned an error: {e.Message}");
                throw new PropertyServiceException();
            }
        }

        public async Task<List<NewBuildWarrantyData>> FindNewBuildPropertyWarrantByRefAsync(string reference)
        {
            _logger.LogInformation($"Getting new build property warranty by reference: {reference}");
            try
            {
                var response = await _propertyService.GetNewBuildPropertyWarrantByRefAsync(reference);
                return response;
                //if (response == null)
                //{
                //    throw new MissingPropertyException();
                //}
                //else
                //{
                //    return BuildPropertyDetails(response);
                //}
            }
            catch (Exception e)
            {
                _logger.LogError($"Finding a property with the property reference: {reference} returned an error: {e.Message}");
                throw new PropertyServiceException();
            }

            //catch (MissingPropertyException e)
            //{
            //    _logger.LogError($"Finding a property with the property reference: {reference} returned an error: {e.Message}");
            //    throw e;
            //}
        }

        public async Task<object[]> FindPropertiesDetailsByReferences(string[] references)
        {
            try
            {
                var response = await _propertyService.GetPropertiesByReferences(references);
                if (response.Length != references.Length)
                {
                    throw new MissingPropertyException();
                }
                else
                {
                    return BuildPropertiesDetails(response);
                }
            }
            catch (MissingPropertyException e)
            {
                _logger.LogError($"Finding properties with the property references: {GenericFormatter.CommaSeparate(references)} returned an error: {e.Message}");
                throw e;
            }
            catch (Exception e)
            {
                _logger.LogError($"Finding properties with the property references: {GenericFormatter.CommaSeparate(references)} returned an error: {e.Message}");
                throw new PropertyServiceException();
            }
        }

        public async Task<object> FindPropertyBlockDetailsByRef(string reference)
        {
            _logger.LogInformation($"Finding the block of a property by the property reference: {reference}");
            try
            {
                var response = await _propertyService.GetPropertyBlockByRef(reference);
                if (response == null)
                {
                    throw new MissingPropertyException();
                }
                else
                {
                    return BuildPropertyDetails(response);
                }
            }
            catch (MissingPropertyException e)
            {
                _logger.LogError($"Finding the block of a property by the property reference: {reference} returned an error: {e.Message}");
                throw e;
            }
            catch (Exception e)
            {
                _logger.LogError($"Finding the block of a property by the property reference: {reference} returned an error: {e.Message}");
                throw new PropertyServiceException();
            }
        }

        public async Task<object> FindPropertyEstateDetailsByRef(string reference)
        {
            _logger.LogInformation($"Finding the estate of a property by the property reference: {reference}");
            try
            {
                var response = await _propertyService.GetPropertyEstateByRef(reference);
                if (response == null)
                {
                    throw new MissingPropertyException();
                }
                else
                {
                    return BuildPropertyDetails(response);
                }
            }
            catch (MissingPropertyException e)
            {
                _logger.LogError($"Finding the estate of a property by the property reference: {reference} returned an error: {e.Message}");
                throw e;
            }
            catch (Exception e)
            {
                _logger.LogError($"Finding the estate of a property by the property reference: {reference} returned an error: {e.Message}");
                throw new PropertyServiceException();
            }
        }

        public async Task<object> FindFacilitiesByPropertyRef(string reference)
        {
            _logger.LogInformation($"Finding the facilities of a property by the property reference: {reference}");
            try
            {
                var response = await _propertyService.GetFacilitiesByPropertyRef(reference);
                if (response.Any())
                {
                    GenericFormatter.TrimStringAttributesInEnumerable(response);
                }

                return new
                {
                    results = response
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Finding the facilities of a property by the property reference: {reference} returned an error: {e.Message}");
                throw new PropertyServiceException();
            }
        }

        private async Task<IEnumerable<string>> GetBlockReferences(string propertyReference)
        {
            var hierarchy = await GetPropertyHierarchy(propertyReference);
            var blockReferences = new List<string>();

            foreach (var property in hierarchy)
            {
                if (property.LevelCode == "3" || property.LevelCode == "4")
                {
                    blockReferences.Add(property.PropertyReference);
                }
            }

            return blockReferences;
        }

        private object BuildProperty(PropertySummary property)
        {
            return new
            {
                address = property.ShortAddress.Trim(),
                postcode = property.PostCodeValue.Trim(),
                propertyReference = property.PropertyReference.Trim()
            };
        }

        private object BuildPropertyDetails(PropertyDetails property)
        {
            if (string.IsNullOrEmpty(property.Description))
            {
                return new
                {
                    address = property.ShortAddress.Trim(),
                    postcode = property.PostCodeValue.Trim(),
                    propertyReference = property.PropertyReference.Trim(),
                    maintainable = property.Maintainable,
                    tenureCode = property.TenureCode,
                    tenure = property.TenureDescription,
                    lettingArea = property.LettingAreaDescription,
                    propertyTypeCode = property.PropertyTypeCode,
                    propertyTypeDescription = property.PropertyTypeDescription 
                };
            }

            return new
            {
                address = property.ShortAddress.Trim(),
                postcode = property.PostCodeValue.Trim(),
                propertyReference = property.PropertyReference.Trim(),
                maintainable = property.Maintainable,
                levelCode = property.LevelCode,
                description = property.Description.Trim(),
                tenureCode = (property.TenureCode ?? "").Trim(),
                tenure = (property.TenureDescription ?? "").Trim(),
                lettingArea = property.LettingAreaDescription,
                 propertyTypeCode = property.PropertyTypeCode,
                propertyTypeDescription = property.PropertyTypeDescription
            };
        }

        private object[] BuildPropertiesDetails(PropertyDetails[] property)
        {
            var properties = new object[property.Length];
            for (int i = 0; i < property.Length; i++)
            {
                properties[i] = new
                {
                    address = property[i].ShortAddress.Trim(),
                    postcode = property[i].PostCodeValue.Trim(),
                    propertyReference = property[i].PropertyReference.Trim(),
                    maintainable = property[i].Maintainable,
                    levelCode = property[i].LevelCode,
                    description = property[i].Description.Trim()
                };
            }

            return properties;
        }
    }

    public class MissingPropertyListException : Exception { }
    public class PropertyServiceException : Exception { }
    public class MissingPropertyException : Exception { }
	public class InvalidParameterException : Exception { }
}
