﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using HackneyRepairs.Models;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Factories;
using HackneyRepairs.Actions;
using HackneyRepairs.Formatters;
using HackneyRepairs.Services;
using HackneyRepairs.Validators;
using System.ComponentModel.DataAnnotations;

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

		public PropertiesController(ILoggerAdapter<PropertyActions> propertyLoggerAdapter, ILoggerAdapter<WorkOrdersActions> workorderLoggerAdapter, IUhtRepository uhtRepository, IUhwRepository uhwRepository, IUHWWarehouseRepository uHWWarehouseRepository)
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
        }

        // GET properties
        /// <summary>
        /// Returns the hierarchy details of a property  
        /// </summary>
        /// <param name="propertyReference">The reference number of the requested property
		/// </param>
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
                var json = Json(await actions.GetPropertyHierarchy(propertyReference));
                json.StatusCode = 200;
                json.ContentType = "application/json";
                return json;
            }
            catch (MissingPropertyException ex)
            {
                var errors = new List<ApiErrorMessage>()
                {
                    new ApiErrorMessage
                    {
                        developerMessage = ex.Message,
                        userMessage = "Property not foundr"
                    }
                };
                var jsonResponse = Json(errors);
                jsonResponse.StatusCode = 404;
                return jsonResponse;
            }
            catch (Exception e)
            {
                var errors = new List<ApiErrorMessage>()
                {
                    new ApiErrorMessage
                    {
                        developerMessage = "Internal Server Error",
                        userMessage = "Internal Error"
                    }
                };
                var jsonResponse = Json(errors);
                jsonResponse.StatusCode = 500;
                return jsonResponse;
            }
        }


        // GET properties
        /// <summary>
        /// Gets a property or properties for a particular postcode
        /// </summary>
        /// <param name="postcode">The post code of the propterty being requested</param>
        /// <param name="max_level">The maximum hierarchy level or the properties requested</param>
        /// <param name="min_level">The minimum hierarchy level of the properties requested</param>
        /// <returns>A list of properties matching the specified post code</returns>
        /// <response code="200">Returns the list of properties</response>
        /// <response code="400">If a post code is not provided</response>   
        /// <response code="500">If any errors are encountered</response>   
        [HttpGet]
        public async Task<JsonResult> Get([FromQuery]string postcode, int? max_level = null, int? min_level = null)
        {
            try
            {
                if (max_level < min_level || max_level > 8 || max_level < 0 || min_level > 8 || min_level < 0)
                {
                    var errors = new List<ApiErrorMessage>
                    {
                        new ApiErrorMessage
                        {
                            developerMessage = "Invalid parameter - invalid level passed",
                            userMessage = "Please provide a valid level"
                        }
                    };
                    var json = Json(errors);
                    json.StatusCode = 400;
                    return json;
                }

                if (_postcodeValidator.Validate(postcode))
                {
					PropertyActions actions = new PropertyActions(_propertyService, _propertyServiceRequestBuilder, _workordersService, _propertyLoggerAdapter);
                    var json = Json(await actions.FindProperty(_propertyServiceRequestBuilder.BuildListByPostCodeRequest(postcode), max_level, min_level));
                    json.StatusCode = 200;
                    json.ContentType = "application/json";
                    return json;
                }
                else
                {
                    var errors = new List<ApiErrorMessage>
                    {
                        new ApiErrorMessage
                        {
                            developerMessage = "Invalid parameter - postcode",
                            userMessage = "Please provide a valid post code"
                        }
                    };
                    var json = Json(errors);
                    json.StatusCode = 400;
                    return json;
                }
            }

            catch (Exception e)
            {
                var errors = new List<ApiErrorMessage>
                {
                    new ApiErrorMessage
                    {
                        developerMessage = e.Message,
                        userMessage = "We had some problems processing your request"
                    }
                };
                var json = Json(errors);
                json.StatusCode = 500;
                return json;
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
                var json = Json(await actions.FindPropertyDetailsByRef(reference));
                json.StatusCode = 200;
                json.ContentType = "application/json";
                return json;
            }
            catch (MissingPropertyException ex)
            {
                var errors = new List<ApiErrorMessage>()
                {
                    new ApiErrorMessage
                    {
                        developerMessage = ex.Message,
                        userMessage = "Resource identification error"
                    }
                };
                var jsonResponse = Json(errors);
                jsonResponse.StatusCode = 404;
                return jsonResponse;
            }
            catch (Exception e)
            {
                var errors = new List<ApiErrorMessage>()
                {
                    new ApiErrorMessage
                    {
                        developerMessage = "Internal Server Error",
                        userMessage = "Internal Error"
                    }
                };
                var jsonResponse = Json(errors);
                jsonResponse.StatusCode = 500;
                return jsonResponse;
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
                var json = Json(result);
                json.StatusCode = 200;
                json.ContentType = "application/json";
                return json;
            }
            catch(MissingPropertyException e)
            {
                var jsonResponse = Json(null);
                jsonResponse.StatusCode = 404;
                return jsonResponse;
            }
            catch(Exception e)
            {
                var errors = new List<ApiErrorMessage>()
                {
                    new ApiErrorMessage
                    {
                        developerMessage = "API Internal Error",
                        userMessage = "API Internal Error"
                    }
                };
                var jsonResponse = Json(errors);
                jsonResponse.StatusCode = 500;
                return jsonResponse;
            }
        }

		// GET work orders raised against a block and all properties in it
        /// <summary>
		/// Gets work orders raised against a block and against any property int he block
        /// </summary>
		/// <param name="propertyReference">Property reference, the level of the property cannot be higher than block.</param>
		/// <param name="trade">Trade of the work order to filter the results (Required).</param>
        /// <returns>Details of the block the requested property belongs to</returns>
		/// <response code="200">Returns work orders raised against a block and all properties in it</response>
        /// <response code="404">If the property was not found</response>   
        /// <response code="500">If any errors are encountered</response> 
        [HttpGet("{propertyReference}/block/work_orders")]
		public async Task<JsonResult> GetWorkOrdersForBlockByPropertyReference(string propertyReference, string trade)
        {
            try
            {
				PropertyActions actions = new PropertyActions(_propertyService, _propertyServiceRequestBuilder, _workordersService, _propertyLoggerAdapter);
				var result = await actions.GetWorkOrdersForBlock(propertyReference, trade);
                var json = Json(result);
                json.StatusCode = 200;
                json.ContentType = "application/json";
                return json;
            }
            catch (MissingPropertyException ex)
            {
				var error = new ApiErrorMessage
                {
                    developerMessage = ex.Message,
                    userMessage = @"Cannot find property."
                };
                var jsonResponse = Json(error);
                jsonResponse.StatusCode = 404;
                return jsonResponse;
            }
			catch (InvalidParameterException ex)
			{
				var error = new ApiErrorMessage
                {
					developerMessage = ex.Message,
					userMessage = "403 Forbidden - Invalid parameter provided."
                };
                var jsonResponse = Json(error);
                jsonResponse.StatusCode = 403;
                return jsonResponse;
			}
            catch (Exception ex)
            {
                var errors = new List<ApiErrorMessage>()
                {
                    new ApiErrorMessage
                    {
						developerMessage = "API Internal Error",
                        userMessage = "API Internal Error"
                    }
                };
                var jsonResponse = Json(errors);
                jsonResponse.StatusCode = 500;
                return jsonResponse;
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
                    var jsonResponse = Json(null);
                    jsonResponse.StatusCode = 404;
                    return jsonResponse;
                }
                else
                {
                    var json = Json(result);
                    json.StatusCode = 200;
                    json.ContentType = "application/json";
                    return json;
                }
            }
            catch (MissingPropertyException e)
            {
                var jsonResponse = Json(null);
                jsonResponse.StatusCode = 404;
                return jsonResponse;
            }
            catch (Exception e)
            {
                var errors = new List<ApiErrorMessage>()
                {
                    new ApiErrorMessage
                    {
                        developerMessage = "API Internal Error",
                        userMessage = "API Internal Error"
                    }
                };
                var jsonResponse = Json(errors);
                jsonResponse.StatusCode = 500;
                return jsonResponse;
            }
        }
    }
}