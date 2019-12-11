using HackneyRepairs.Actions;
using HackneyRepairs.Builders;
using HackneyRepairs.Factories;
using HackneyRepairs.Formatters;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Repository;
using HackneyRepairs.Services;
using HackneyRepairs.Validators;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using System.Configuration;
using System.Globalization;
using System.Threading.Tasks;

namespace HackneyRepairs.Controllers
{
    [Produces("application/json")]
    [Route("v1/properties")]
    public class PropertiesController : Controller
    {
        private IHackneyPropertyService _propertyService;
        private IHackneyWorkOrdersService _workordersService;
        private IHackneyPropertyServiceRequestBuilder _propertyServiceRequestBuilder;
        private IPostcodeValidator _postcodeValidator;
        private ILoggerAdapter<PropertyActions> _propertyLoggerAdapter;
        private ILoggerAdapter<WorkOrdersActions> _workorderLoggerAdapter;
        private HackneyConfigurationBuilder _configBuilder;
        private readonly IExceptionLogger _exceptionLogger;

        public PropertiesController(ILoggerAdapter<PropertyActions> propertyLoggerAdapter, ILoggerAdapter<WorkOrdersActions> workorderLoggerAdapter, IUhtRepository uhtRepository, IUhwRepository uhwRepository, IUHWWarehouseRepository uHWWarehouseRepository, IExceptionLogger exceptionLogger)
        {
            HackneyPropertyServiceFactory propertyFactory = new HackneyPropertyServiceFactory();
            _configBuilder = new HackneyConfigurationBuilder((Hashtable)Environment.GetEnvironmentVariables(), ConfigurationManager.AppSettings);
            _propertyService = propertyFactory.build(uhtRepository, uHWWarehouseRepository, propertyLoggerAdapter);
            _propertyServiceRequestBuilder = new HackneyPropertyServiceRequestBuilder(_configBuilder.getConfiguration(), new PostcodeFormatter());
            _postcodeValidator = new PostcodeValidator();
            _propertyLoggerAdapter = propertyLoggerAdapter;
            HackneyWorkOrdersServiceFactory workOrdersServiceFactory = new HackneyWorkOrdersServiceFactory();
            _workordersService = workOrdersServiceFactory.build(uhtRepository, uhwRepository, uHWWarehouseRepository, workorderLoggerAdapter);
            _workorderLoggerAdapter = workorderLoggerAdapter;
            _exceptionLogger = exceptionLogger;
        }

        // GET properties
        /// <summary>
        /// Returns the hierarchy details of a property  
        /// </summary>
        /// <param name="propertyReference">The reference number of the requested property
        /// <returns>A list of property details and its parent properties</returns>
        /// <response code="200">Returns a list of property details</response>
        /// <response code="404">If the property is not found</response>   
        /// <response code="500">If any errors are encountered</response> 
        [HttpGet("{propertyReference}/hierarchy")]
        public async Task<JsonResult> GetPropertyHierarchy(string propertyReference)
        {
            try
            {
                PropertyActions actions = new PropertyActions(_propertyService, _propertyServiceRequestBuilder, _workordersService, _propertyLoggerAdapter);
                var result = await actions.GetPropertyHierarchy(propertyReference);
                return ResponseBuilder.Ok(result);
            }
            catch (MissingPropertyException ex)
            {
                _exceptionLogger.CaptureException(ex);
                return ResponseBuilder.Error(404, "Property not found", ex.Message);
            }
            catch (UHWWarehouseRepositoryException e)
            {
                _exceptionLogger.CaptureException(e);
                return ResponseBuilder.Error(500, "We could not contact Universal Housing (UHW) to retrieve property details for " +
                    "this reference. Please raise a ticket at https://support.hackney.gov.uk including the details of this error, " +
                    "the repair or property and a screenshot.", e.Message);
            }
            catch (Exception ex)
            {
                _exceptionLogger.CaptureException(ex);
                return ResponseBuilder.Error(500, "We had some issues processing your request", ex.Message);
            }
        }

        // GET properties
        /// <summary>
        /// Gets a property or properties for a particular postcode
        /// </summary>
        /// <param name="postcode">The post code of the propterty being requested</param>
        /// <param name="max_level">The highest hierarchy level or the properties requested. Higest is 0 (Owner, Hackney Council)</param>
        /// <param name="min_level">The lowest hierarchy level of the properties requested. Lowest is 8 (Non-Dwell)</param>
        /// <returns>A list of properties matching the specified post code</returns>
        /// <response code="200">Returns the list of properties</response>
        /// <response code="400">If a post code is not provided</response>   
        /// <response code="500">If any errors are encountered</response>   
        [HttpGet]
        public async Task<JsonResult> Get([FromQuery]string postcode, int? max_level = null, int? min_level = null)
        {
            try
            {
                if (min_level < max_level || max_level > 8 || max_level < 0 || min_level > 8 || min_level < 0)
                {
                    return ResponseBuilder.Error(400, "Invalid parameter - level is not valid", "Invalid parameter - level is not valid");
                }

                if (!_postcodeValidator.Validate(postcode))
                {
                    return ResponseBuilder.Error(400, "Please provide a valid post code", "Invalid parameter - postcode");
                }

                PropertyActions actions = new PropertyActions(_propertyService, _propertyServiceRequestBuilder, _workordersService, _propertyLoggerAdapter);
                var result = await actions.FindProperty(_propertyServiceRequestBuilder.BuildListByPostCodeRequest(postcode), max_level, min_level);
                return ResponseBuilder.Ok(result);
            }
            catch (UHWWarehouseRepositoryException e)
            {
                _exceptionLogger.CaptureException(e);
                return ResponseBuilder.Error(500, "We could not contact Universal Housing (UHW) to retrieve properties matching your query. " +
                    "Please raise a ticket at https://support.hackney.gov.uk including the details of this error, the repair or property " +
                    "and a screenshot.", e.Message);
            }
            catch (Exception ex)
            {
                _exceptionLogger.CaptureException(ex);
                return ResponseBuilder.Error(500, "We had some problems processing your request", ex.Message);
            }
        }

        // GET properties
        /// <summary>
        /// Gets a property or properties for a particular postcode
        /// </summary>
        /// <param name="address">First line of the propterty address being requested</param>
        /// <param name="limit">Maximum number of results to return. Default value of 100</param>
        /// <returns>A list of properties matching the specified first line of address</returns>
        /// <response code="200">Returns the list of properties</response>
        /// <response code="404">If the property is not found</response>   
        /// <response code="500">If any errors are encountered</response>   
        [HttpGet("fladdress")]
        public async Task<JsonResult> GetByFirstLineOfAddress(string address, int limit = 100)
        {
            try
            { 
                PropertyActions actions = new PropertyActions(_propertyService, _propertyServiceRequestBuilder, _workordersService, _propertyLoggerAdapter);
                var result = await actions.FindPropertyByFirstLineOfAddress(address, limit);
                return ResponseBuilder.Ok(result);
            }//Doesn't get thrown
            catch (MissingPropertyException ex)
            {
                _exceptionLogger.CaptureException(ex);
                return ResponseBuilder.Error(404, "Property not found", ex.Message);
            }
            catch (UHWWarehouseRepositoryException ex)
            {
                _exceptionLogger.CaptureException(ex);
                return ResponseBuilder.Error(500, "We could not contact Universal Housing (UHW) to " +
                    "retrieve properties matching your query. Please raise a ticket at " +
                    "https://support.hackney.gov.uk including the details of this error, the repair " +
                    "or property and a screenshot.", ex.Message);
            }
            catch (Exception ex)
            {
                _exceptionLogger.CaptureException(ex);
                return ResponseBuilder.Error(500, "We had some problems processing your request", ex.Message);
            }
        }

        // GET property details by reference
        /// <summary>
        /// Gets the details of a property by a given reference number
        /// </summary>
        /// <param name="reference">The reference number of the requested property</param>
        /// <returns>Details of the requested property</returns>
        /// <response code="200">Returns the property</response>
        /// <response code="404">If the property is not found</response>   
        /// <response code="500">If any errors are encountered</response> 
        [HttpGet("{reference}")]
        public async Task<JsonResult> GetByReference(string reference)
        {
            try
            {
				PropertyActions actions = new PropertyActions(_propertyService, _propertyServiceRequestBuilder, _workordersService, _propertyLoggerAdapter);
                var response = await actions.FindPropertyDetailsByRef(reference);
                return ResponseBuilder.Ok(response);
            }
            catch (MissingPropertyException ex)
            {
                _exceptionLogger.CaptureException(ex);
                return ResponseBuilder.Error(404, "Property not found or incorrect reference number", ex.Message);
            }
            catch (UHWWarehouseRepositoryException ex)
            {
                _exceptionLogger.CaptureException(ex);
                return ResponseBuilder.Error(500, "We could not contact Universal Housing (UHW) to " +
                    "retrieve the property matching your query. Please raise a ticket at " +
                    "https://support.hackney.gov.uk including the details of this error, the repair " +
                    "or property and a screenshot.", ex.Message);
            }
            catch (Exception ex)
            {
                _exceptionLogger.CaptureException(ex);
                return ResponseBuilder.Error(500, "We had some problems processing your request", ex.Message);
            }
        }

        // GET property details by reference
        /// <summary>
        /// Gets the warranty details of a new build property by a given property reference number
        /// </summary>
        /// <param name="reference">The reference number of the new build property</param>
        /// <returns>Details of the requested property</returns>
        /// <response code="200">Returns the new build warranty</response>
        /// <response code="404">If the warranty is not found</response>   
        /// <response code="500">If any errors are encountered</response> 
        [HttpGet("{reference}/new_build/warranty")]
        public async Task<JsonResult> GetNewBuildWarrantyAsync(string reference)
        {
            try
            {
                PropertyActions actions = new PropertyActions(_propertyService, _propertyServiceRequestBuilder, _workordersService, _propertyLoggerAdapter);
                var response = await actions.FindNewBuildPropertyWarrantByRefAsync(reference);
                return ResponseBuilder.Ok(response);
            }
            catch (MissingPropertyException ex)
            {
                _exceptionLogger.CaptureException(ex);
                return ResponseBuilder.Error(404, "Property not found or incorrect reference number", ex.Message);
            }
            catch (UHWWarehouseRepositoryException ex)
            {
                _exceptionLogger.CaptureException(ex);
                return ResponseBuilder.Error(500, "We could not contact Universal Housing (UHW) to " +
                    "retrieve the property matching your query. Please raise a ticket at " +
                    "https://support.hackney.gov.uk including the details of this error, the repair " +
                    "or property and a screenshot.", ex.Message);
            }
            catch (Exception ex)
            {
                _exceptionLogger.CaptureException(ex);
                return ResponseBuilder.Error(500, "We had some problems processing your request", ex.Message);
            }
        }

        // GET properties details by references
        /// <summary>
        /// Gets the details for properties by given references
        /// </summary>
        //// <param name="reference">A reference number of the requested property</param>
        /// <returns>A list with details of the requested properties</returns>
        /// <response code="200">Returns a list of properties</response>
        /// <response code="404">If any of the properties requested is not found</response>   
        /// <response code="500">If any errors are encountered</response> 
        [HttpGet("by_references")]
        public async Task<JsonResult> GetByReferences(string[] reference)
        {
            try
            {
                PropertyActions actions = new PropertyActions(_propertyService, _propertyServiceRequestBuilder, _workordersService, _propertyLoggerAdapter);
                var response = await actions.FindPropertiesDetailsByReferences(reference);
                return ResponseBuilder.Ok(response);
            }
            catch (MissingPropertyException ex)
            {
                _exceptionLogger.CaptureException(ex);
                return ResponseBuilder.Error(404, "One or more property references could not be found", ex.Message);
            }
            catch (UHWWarehouseRepositoryException ex)
            {
                _exceptionLogger.CaptureException(ex);
                return ResponseBuilder.Error(500, "We could not contact Universal Housing (UHW) to " +
                    "retrieve properties matching your query. Please raise a ticket at " +
                    "https://support.hackney.gov.uk including the details of this error, the " +
                    "repair or property and a screenshot.", ex.Message);
            }
            catch (Exception ex)
            {
                _exceptionLogger.CaptureException(ex);
                return ResponseBuilder.Error(500, "We had some problems processing your request", ex.Message);
            }
        }

        // GET details of a property block by property by reference
        /// <summary>
        /// Gets the details of a block of a property by a given property reference number
        /// </summary>
        /// <param name="reference">The reference number of the property</param>
        /// <returns>Details of the block the requested property belongs to</returns>
        /// <response code="200">Returns the block of the property</response>
        /// <response code="404">If the property is not found</response>   
        /// <response code="500">If any errors are encountered</response> 
        [HttpGet("{reference}/block")]
        public async Task<JsonResult> GetBlockByReference(string reference)
        {
            try
            {
                PropertyActions actions = new PropertyActions(_propertyService, _propertyServiceRequestBuilder, _workordersService, _propertyLoggerAdapter);
                var result = await actions.FindPropertyBlockDetailsByRef(reference);
                return ResponseBuilder.Ok(result);
            }
            catch (MissingPropertyException ex)
            {
                _exceptionLogger.CaptureException(ex);
                return ResponseBuilder.Error(404, "Resource identification error", ex.Message);
            }
            catch (UHWWarehouseRepositoryException ex)
            {
                _exceptionLogger.CaptureException(ex);
                return ResponseBuilder.Error(500, "We could not contact Universal Housing (UHW) to " +
                    "retrieve the block matching your query. Please raise a ticket at " +
                    "https://support.hackney.gov.uk including the details of this error, the repair " +
                    "or property and a screenshot.", ex.Message);
            }
            catch (Exception ex)
            {
                _exceptionLogger.CaptureException(ex);
                return ResponseBuilder.Error(500, "API Internal Error", ex.Message);
            }
        }

        // GET work orders raised against a block and all properties in it
        /// <summary>
        /// Gets work orders raised against a block and against any property int he block
        /// </summary>
        /// <param name="propertyReference">Property reference, the level of the property cannot be higher than block.</param> 
        /// <param name="trade">Trade of the work order to filter the results (Required).</param>
        /// <param name="since">A string with the format dd-MM-yyyy (Optional).</param>
        /// <param name="until">A string with the format dd-MM-yyyy (Optional).</param>
        /// <returns>Details of the block the requested property belongs to</returns>
        /// <response code="200">Returns work orders raised against a block and all properties in it</response>
        /// <response code="400">If trade parameter is missing or since or until do not have the right datetime format</response>   
        /// <response code="404">If the property was not found</response>   
        /// <response code="500">If any errors are encountered</response> 
        [HttpGet("{propertyReference}/block/work_orders")]
        public async Task<JsonResult> GetWorkOrdersForBlockByPropertyReference(string propertyReference, string trade, string since, string until)
        {
            try
            {
                DateTime validSince = DateTime.Now.AddYears(-2);
                if (since != null)
                {
                    if (!DateTime.TryParseExact(since, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out validSince))
                    {
                        return ResponseBuilder.Error(400, "Invalid parameter value - since", "Parameter is not a valid DateTime");
                    }
                }

                DateTime validUntil = DateTime.Now;
                if (until != null)
                {
                    if (!DateTime.TryParseExact(until, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out validUntil))
                    {
                        return ResponseBuilder.Error(400, "Invalid parameter value - until", "Parameter is not a valid DateTime");
                    }

                    validUntil = validUntil.AddDays(1).AddSeconds(-1);
                }

                PropertyActions actions = new PropertyActions(_propertyService, _propertyServiceRequestBuilder, _workordersService, _propertyLoggerAdapter);
                var result = await actions.GetWorkOrdersForBlock(propertyReference, trade, validSince, validUntil);
                return ResponseBuilder.Ok(result);
            }
            catch (MissingPropertyException ex)
            {
                _exceptionLogger.CaptureException(ex);
                return ResponseBuilder.Error(404, "Block not found or incorrect reference number.", ex.Message);
            }
            catch (UHWWarehouseRepositoryException ex)
            {
                _exceptionLogger.CaptureException(ex);
                return ResponseBuilder.Error(500, "We could not contact Universal Housing (UHW) to retrieve the block matching your query. " +
                    "Please raise a ticket at https://support.hackney.gov.uk including the details of this error, the repair or property " +
                    "and a screenshot. ", ex.Message);
            }
            catch (UhtRepositoryException ex)
            {
                _exceptionLogger.CaptureException(ex);
                return ResponseBuilder.Error(500, "We could not contact Universal Housing (UHT) to retrieve the block matching your query. " +
                    "Please raise a ticket at https://support.hackney.gov.uk including the details of this error, the repair or property " +
                    "and a screenshot. ", ex.Message);
            }
            catch (InvalidParameterException ex)
            {
                _exceptionLogger.CaptureException(ex);
                return ResponseBuilder.Error(403, "Reference is not for a block. Please enter a block " +
                    "reference number", ex.Message);
            }
            catch (Exception ex)
            {
                _exceptionLogger.CaptureException(ex);
                return ResponseBuilder.Error(500, "API Internal Error", ex.Message);
            }
        }

        // GET details of a property's estate by property by reference
        /// <summary>
        /// Gets the details of an estate of a property by a given property reference number
        /// </summary>
        /// <param name="reference">The reference number of the property</param>
        /// <returns>Details of the estate the requested property belongs to</returns>
        /// <response code="200">Returns the estate of the property</response>
        /// <response code="404">If the property is not found</response>   
        /// <response code="500">If any errors are encountered</response> 
        [HttpGet("{reference}/estate")]
        public async Task<JsonResult> GetEstateByReference(string reference)
        {
            try
            {
                PropertyActions actions = new PropertyActions(_propertyService, _propertyServiceRequestBuilder, _workordersService, _propertyLoggerAdapter);
                var result = await actions.FindPropertyEstateDetailsByRef(reference);
                if (result == null)
                {
                    return ResponseBuilder.Error(404, "No estate identified for the property requested", "No estate identified for the property requested");
                }

                return ResponseBuilder.Ok(result);
            }
            catch (MissingPropertyException ex)
            {
                _exceptionLogger.CaptureException(ex);
                return ResponseBuilder.Error(404, "Estate not found or incorrect reference number", ex.Message);
            }
            catch (UHWWarehouseRepositoryException ex)
            {
                _exceptionLogger.CaptureException(ex);
                return ResponseBuilder.Error(500, "We could not contact Universal Housing (UHW) to " +
                    "retrieve the estate matching your query. Please raise a ticket at " +
                    "https://support.hackney.gov.uk including the details of this error, the repair " +
                    "or property and a screenshot.", ex.Message);
            }
            catch (Exception ex)
            {
                _exceptionLogger.CaptureException(ex);
                return ResponseBuilder.Error(500, "API Internal Error", ex.Message);
            }
        }

        // GET details of a property's facilities by property by reference
        /// <summary>
        /// Gets the details of the facilities linked to a property by a given property reference number
        /// </summary>
        /// <param name="reference">The reference number of the property</param>
        /// <returns>Details of the estate the requested property belongs to</returns>
        /// <response code="200">Returns an array of the facilities linked to a property</response>
        /// <response code="404">If the facilities are not found</response>   
        /// <response code="500">If any errors are encountered</response> 
        [HttpGet("{reference}/facilities")]
        public async Task<JsonResult> GetFacilitiesByPropertyReference(string reference)
        {
            try
            {
                PropertyActions actions = new PropertyActions(_propertyService, _propertyServiceRequestBuilder, _workordersService, _propertyLoggerAdapter);
                var result = await actions.FindFacilitiesByPropertyRef(reference);
                if (result == null)
                {
                    return ResponseBuilder.Error(404, "No facilities identified for the property requested", "No facilities identified for the property requested");
                }

                return ResponseBuilder.Ok(result);
            }
            catch (MissingPropertyException ex)
            {
                _exceptionLogger.CaptureException(ex);
                return ResponseBuilder.Error(404, "Facilities not found or incorrect reference " +
                    "number", ex.Message);
            }
            catch (UHWWarehouseRepositoryException ex)
            {
                _exceptionLogger.CaptureException(ex);
                return ResponseBuilder.Error(500, "We could not contact Universal Housing (UHW) to " +
                    "retrieve the facilities matching your query. Please raise a ticket at " +
                    "https://support.hackney.gov.uk including the details of this error, the repair " +
                    "or property and a screenshot.", ex.Message);
            }
            catch (Exception ex)
            {
                _exceptionLogger.CaptureException(ex);
                return ResponseBuilder.Error(500, "API Internal Error", ex.Message);
            }
        }
    }
}
