using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HackneyRepairs.Actions;
using HackneyRepairs.Formatters;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Models;
using HackneyRepairs.PropertyService;

namespace HackneyRepairs.Services
{
    public class HackneyPropertyService : IHackneyPropertyService
    {
        private PropertyServiceClient _client;
        private IUhtRepository _uhtRepository;
        private IUHWWarehouseRepository _uhWarehouseRepository;
        private ILoggerAdapter<PropertyActions> _logger;
        public HackneyPropertyService(IUhtRepository uhtRepository, IUHWWarehouseRepository uHWWarehouseRepository, ILoggerAdapter<PropertyActions> logger)
        {
            _client = new PropertyServiceClient();
            _uhtRepository = uhtRepository;
            _uhWarehouseRepository = uHWWarehouseRepository;
            _logger = logger;
        }

        public Task<PropertyLevelModel> GetPropertyLevelInfo(string reference)
        {
			_logger.LogInformation($"HackneyPropertyService/GetPropertyLevelInfo(): Sent request to warehouse repository (property reference: {reference})");
            var response = _uhWarehouseRepository.GetPropertyLevelInfo(reference);
			_logger.LogInformation($"HackneyPropertyService/GetPropertyLevelInfo(): Received level info from warehouse repository (property reference: {reference})");
            return response;
        }

        public Task<PropertyInfoResponse> GetPropertyListByPostCodeAsync(ListByPostCodeRequest request)
        {
            //_client = new PropertyServiceClient();
            _logger.LogInformation($"HackneyPropertyService/GetPropertyListByPostCodeAsync(): Sent request to upstream PropertyServiceClient (Postcode: {request.PostCode})");
            var response = _client.GetPropertyListByPostCodeAsync(request);
            _logger.LogInformation($"HackneyPropertyService/GetPropertyListByPostCodeAsync(): Received response from upstream PropertyServiceClient (Postcode: {request.PostCode})");
            return response;
        }

        public Task<PropertyDetails> GetPropertyByRef(string reference)
        {
            _logger.LogInformation($"HackneyPropertyService/GetPropertyByRefAsync(): Sent request to upstream PropertyServiceClient (Property reference: {reference})");
            var response = _uhtRepository.GetPropertyDetailsByReference(reference);
            _logger.LogInformation($"HackneyPropertyService/GetPropertyByRefAsync(): Received response from upstream PropertyServiceClient (Property reference: {reference})");
            return response;
        }

        public Task<NewBuildWarrantyData> GetNewBuildPropertyWarrantByRefAsync(string reference)
        {
            _logger.LogInformation($"HackneyPropertyService/GetNewBuildPropertyWarrantByRefAsync: Sent request to upstream PropertyServiceClient (Property reference: {reference})");
            var response = _uhtRepository.GetNewBuildWarrantDetailsAsync(reference);
            _logger.LogInformation($"HackneyPropertyService/GetNewBuildPropertyWarrantByRefAsync: Received response from upstream PropertyServiceClient (Property reference: {reference})");
            return response;
        }

        public async Task<PropertyDetails[]> GetPropertiesByReferences(string[] references)
        {
            _logger.LogInformation($"HackneyPropertyService/GetPropertyByRefAsync(): Sent request to upstream PropertyServiceClient (Property references: {GenericFormatter.CommaSeparate(references)})");
            var response = await _uhWarehouseRepository.GetPropertiesDetailsByReference(references);
            _logger.LogInformation($"HackneyPropertyService/GetPropertyByRefAsync(): Received response from upstream PropertyServiceClient (Property references: {GenericFormatter.CommaSeparate(references)})");
            return response;
        }

        public async Task<bool> GetMaintainable(string reference)
        {
            _logger.LogInformation($"HackneyPropertyService/GetMaintainable(): Sent request to upstream PropertyServiceClient (Reference: {reference})");
            var response = await _uhWarehouseRepository.GetMaintainableFlag(reference);
            _logger.LogInformation($"HackneyPropertyService/GetMaintainable(): Received response from upstream PropertyServiceClient (Reference: {reference})");
            return response;
        }

        public async Task<PropertyLevelModel[]> GetPropertyListByPostCode(string post_code, int? maxLevel, int? minLevel)
        {
            _logger.LogInformation($"HackneyPropertyService/GetPropertyListByPostCode(): Sent request to upstream data warehouse (Postcode: {post_code})");
            var response = await _uhWarehouseRepository.GetPropertyListByPostCode(post_code, maxLevel, minLevel);
            _logger.LogInformation($"HackneyPropertyService/GetPropertyListByPostCode(): Received response from upstream data warehouse (Postcode: {post_code})");
            return response;
        }

        public async Task<PropertyLevelModel[]> GetPropertyListByFirstLineOfAddress(string firstLineOfAddress, int limit)
        {
            _logger.LogInformation($"HackneyPropertyService/GetPropertyListByPostCode(): Sent request to upstream data warehouse (Postcode: {firstLineOfAddress})");
            var response = await _uhWarehouseRepository.GetPropertyDetailsByFirstLineOfAddress(firstLineOfAddress, limit);
            _logger.LogInformation($"HackneyPropertyService/GetPropertyListByPostCode(): Received response from upstream data warehouse (Postcode: {firstLineOfAddress})");
            return response;
        }

        public async Task<PropertyDetails> GetPropertyBlockByRef(string reference)
        {
            PropertyDetails property = new PropertyDetails();
            _logger.LogInformation($"HackneyPropertyService/GetPropertyBlockByRef(): Sent request to upstream data warehouse (Property reference: {reference})");
            property = await _uhWarehouseRepository.GetPropertyBlockByReference(reference);
            _logger.LogInformation($"HackneyPropertyService/GetPropertyBlockByRef(): Received response from upstream data warehouse (Property reference: {reference})");
            return property;
        }

        public async Task<PropertyDetails> GetPropertyEstateByRef(string reference)
        {
            PropertyDetails property = new PropertyDetails();
            _logger.LogInformation($"HackneyPropertyService/GetPropertyEstateByRef(): Sent request to upstream data warehouse (Property reference: {reference})");
            property = await _uhWarehouseRepository.GetPropertyEstateByReference(reference);
            _logger.LogInformation($"HackneyPropertyService/GetPropertyEstateByRef(): Received response from upstream data warehouse (Property reference: {reference})");
            return property;
        }

        public async Task<PropertyLevelModel[]> GetFacilitiesByPropertyRef(string reference)
        {
            //PropertyDetails property = new PropertyDetails();
            _logger.LogInformation($"HackneyPropertyService/GetPropertyEstateByRef(): Sent request to upstream data warehouse (Property reference: {reference})");
            var response = await _uhWarehouseRepository.GetFacilitiesByPropertyRef(reference);
            _logger.LogInformation($"HackneyPropertyService/GetPropertyEstateByRef(): Received response from upstream data warehouse (Property reference: {reference})");
            return response;
        }

        public Task<List<PropertyLevelModel>> GetPropertyLevelInfosForParent(string parentReference)
		{
			_logger.LogInformation($"HackneyPropertyService/GetPropertyLevelInfosForParent(): Sent request to warehouse repository (property reference: {parentReference})");
			var response = _uhWarehouseRepository.GetPropertyLevelInfosForParent(parentReference);
			_logger.LogInformation($"HackneyPropertyService/GetPropertyLevelInfosForParent(): Received level infos from warehouse repository (property reference: {parentReference})");
            return response;
		}
	}
}