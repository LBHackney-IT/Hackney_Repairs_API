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

            var hierarchy = await GetPropertyHierarchy(propertyReference);

            string blockReference = (from prop in hierarchy
                                    where prop.LevelCode == "3"
                                     select prop.PropertyReference).FirstOrDefault();
   
			_logger.LogInformation($"Finding work order details for block reference (including children): {blockReference}, trade: {trade}");
            var blockResult = await _workordersService.GetWorkOrderByBlockReference(blockReference, trade, since, until);
			if ((blockResult.ToList()).Count == 0)
			{
				_logger.LogError($"Work orders not found for block reference (including children): {blockReference}, trade: {trade}");
				return blockResult;
			}
			_logger.LogInformation($"Work order details returned for block reference (including children): {blockReference}, trade: {trade}");
            return blockResult;
		}

        public async Task<IEnumerable<PropertyLevelModel>> GetPropertyHierarchy(string reference)
        {
            try
            {
                var results = new List<PropertyLevelModel>();
                string parent = reference;

                while (!String.IsNullOrWhiteSpace(parent))
                {
					var response = await _propertyService.GetPropertyLevelInfo(parent);
                    if (response == null)
                    {
                        throw new MissingPropertyException();
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

        public async Task<object> FindProperty(string postcode)
        {
            _logger.LogInformation($"Finding property by postcode: {postcode}");
            var request = _requestBuilder.BuildListByPostCodeRequest(postcode);
            try
            {
				var response = await _propertyService.GetPropertyListByPostCode(request);
                if(response == null)
                {
                    _logger.LogError($"Finding property by postcode: {request} response not set to an instance of PropertySummary[].");
                    throw new MissingPropertyListException();                    
                }
                return new
                {
                    results = response.Select(BuildProperty).ToArray()
                };
            }
            catch(Exception e)
            {
                _logger.LogError($"Finding property by postcode: {request} returned an error: {e.Message}");
                throw new PropertyServiceException();
            }
        }

        public async Task<object> FindPropertyDetailsByRef(string reference)
        {
            _logger.LogInformation($"Finding property by reference: {reference}");
            try
            {
				var response = await _propertyService.GetPropertyByRefAsync(reference);
                if (response == null)
                {
                    throw new MissingPropertyException();
                }
                else
                {
                    return BuildPropertyDetails(response);
                }
            }
            catch(MissingPropertyException e)
            {
                _logger.LogError($"Finding a property with the property reference: {reference} returned an error: {e.Message}");
                throw e;
            }
            catch(Exception e)
            {
                _logger.LogError($"Finding a property with the property reference: {reference} returned an error: {e.Message}");
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
            catch(Exception e)
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
            return new
            {
                address = property.ShortAddress.Trim(),
                postcode = property.PostCodeValue.Trim(),
                propertyReference = property.PropertyReference.Trim(),
                maintainable = property.Maintainable
            };
        }
    }

    public class MissingPropertyListException : System.Exception{}
    public class PropertyServiceException : System.Exception { }
    public class MissingPropertyException : System.Exception { }
	public class InvalidParameterException : Exception { }

}
