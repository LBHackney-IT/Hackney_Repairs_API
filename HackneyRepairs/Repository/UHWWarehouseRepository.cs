﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using HackneyRepairs.DbContext;
using HackneyRepairs.DTOs;
using HackneyRepairs.Formatters;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Models;
using HackneyRepairs.PropertyService;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace HackneyRepairs.Repository
{
    public class UHWWarehouseRepository : IUHWWarehouseRepository
    {
        private UHWWarehouseDbContext _context;
        private ILoggerAdapter<UHWWarehouseRepository> _logger;

        public UHWWarehouseRepository(UHWWarehouseDbContext context, ILoggerAdapter<UHWWarehouseRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> GetMaintainableFlag(string propertyReference)
        {
            _logger.LogInformation($"Getting the maintainable flag from UHWarehouse for {propertyReference}");
            try
            {
                using (var connection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    var query = "SELECT [no_maint] FROM [property] where [prop_ref]= @PropertyReference";
                    var result = connection.Query<bool>(query, new { PropertyReference = propertyReference }).FirstOrDefault();
                    return Convert.ToBoolean(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new UHWWarehouseRepositoryException();
            }
        }

        public async Task<IEnumerable<RepairRequestBase>> GetRepairRequestsByPropertyReference(string propertyReference)
        {
            if (!IsProduction())
            {
                return new List<RepairRequestBase>();
            }

            try
            {
                using (var connection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    string query = @"set dateformat ymd;
                                SELECT   
                                    r.rq_ref as repairRequestReference,
                                    r.rq_problem as problemDescription,
                                    r.rq_priority as priority,
                                    r.prop_ref as propertyReference
                                FROM rmreqst r
                                    WHERE r.rq_date < @CutoffTime AND r.prop_ref = @PropertyReference";

                    var queryParameters = new
                    {
                        CutoffTime = GetCutoffTime(),
                        PropertyReference = propertyReference
                    };
                    var repairs = connection.Query<RepairRequestBase>(query, queryParameters).ToList();
                    return repairs;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new UhwRepositoryException();
            }
        }

        public async Task<IEnumerable<RepairWithWorkOrderDto>> GetRepairRequest(string repairReference)
        {
            if (!IsProduction())
            {
                return new List<RepairWithWorkOrderDto>();
            }

            try
            {
                using (var connection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    string query = $@"set dateformat ymd;
                                    SELECT
                                        request.rq_ref,
                                        rq_problem,
                                        rq_priority,
                                        request.prop_ref,
                                        rq_name,
                                        rq_phone,
                                        worder.wo_ref,
                                        task.sup_ref,
                                        task.job_code,
                                        auser.user_login,
										auser.username,
										request.rq_date
                                    FROM
                                        rmreqst AS request
                                        LEFT OUTER JOIN rmworder AS worder ON request.rq_ref = worder.rq_ref
                                        LEFT OUTER JOIN rmtask AS task ON task.wo_ref = worder.wo_ref
										LEFT OUTER JOIN auser AS auser ON auser.user_code = request.rq_user
                                    WHERE
                                        request.rq_date < '{GetCutoffTime()}' AND
                                        request.rq_ref = '{repairReference}'";
                    var repair = connection.Query<RepairWithWorkOrderDto>(query).ToList();
                    return repair;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new UhwRepositoryException();
            }
        }

        public async Task<PropertyLevelModel> GetPropertyLevelInfo(string reference)
        {
            _logger.LogInformation($"Getting propertiy hierarchical info for: {reference}");
            try
            {
                using (var connection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    var query = @"
                        SELECT 
                            property.prop_ref AS 'PropertyReference',
                            property.level_code AS 'LevelCode',
                            property.major_ref AS 'MajorReference',
                            lulevel.lu_desc AS 'Description', 
                            property.address1 AS 'Address',
                            property.post_code AS 'PostCode'
                        FROM 
                            StagedDB.dbo.property
                        INNER 
                            JOIN lulevel ON property.level_code = lulevel.lu_ref 
                        WHERE 
                            prop_ref = @PropertyReference";

                    var result = connection.Query<PropertyLevelModel>(query, new { PropertyReference = reference }).FirstOrDefault();
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new UHWWarehouseRepositoryException();
            }
        }

        //Improved query to use for refactoring   
        // public async Task<List<PropertyLevelModel>> GetPropertyLevelInfo(string reference)
        // {
        //     _logger.LogInformation($"Getting propertiy hierarchical info for: {reference}");
        //     try
        //     {
        //         using (var connection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
        //         {
        //             var query = @"
        //                     with property_tree as (
        //                     select prop_ref, level_code, major_ref, address1, post_code
        //                     from property
        //                     where prop_ref = @PropertyReference
        //                     union all
        //                     select p2.prop_ref, p2.level_code, p2.major_ref, p2.address1, p2.post_code
        //                     from property p2
        //                     join property_tree p on p.major_ref = p2.prop_ref 
        //                      )
        //                     select 
        //                     property_tree.prop_ref AS 'PropertyReference',
        //                     property_tree.level_code AS 'LevelCode',
        //                     property_tree.major_ref AS 'MajorReference',
        //                     lulevel.lu_desc AS 'Description', 
        //                     property_tree.address1 AS 'Address',
        //                     property_tree.post_code AS 'PostCode'
        //                     from property_tree
        //                     inner JOIN lulevel on property_tree.level_code = lulevel.lu_ref
        //where level_code <> 0
        //order by level_code desc";

        //             var result = await connection.QueryAsync<PropertyLevelModel>(query, new { PropertyReference = reference });
        //             return result;
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex.Message);
        //         throw new UHWWarehouseRepositoryException();
        //     }
        // }
        public async Task<List<PropertyLevelModel>> GetPropertyLevelInfosForParent(string reference)
        {
            _logger.LogInformation($"Getting propertiy hierarchical info for: {reference}");
            try
            {
                using (var connection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    var query = @"
                        SELECT 
                            property.prop_ref AS 'PropertyReference',
                            property.level_code AS 'LevelCode',
                            property.major_ref AS 'MajorReference',
                            lulevel.lu_desc AS 'Description', 
                            property.address1 AS 'Address',
                            property.post_code AS 'PostCode'
                        FROM 
                            StagedDB.dbo.property
                        INNER 
                            JOIN lulevel ON property.level_code = lulevel.lu_ref 
                        WHERE 
                            major_ref = @PropertyReference";

                    var result = connection.Query<PropertyLevelModel>(query, new { PropertyReference = reference }).AsList();
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new UHWWarehouseRepositoryException();
            }
        }

        public async Task<PropertyLevelModel[]> GetPropertyListByPostCode(string postcode, int? maxLevel, int? minLevel)
        {
            if (maxLevel == null && minLevel == null)
            {
                maxLevel = 7;
                minLevel = 7;
            }

            _logger.LogInformation($"Getting properties for postcode {postcode}");
            try
            {
                using (SqlConnection connection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    string query = $@"
                        SELECT 
                            property.prop_ref AS 'PropertyReference',
                            property.level_code AS 'LevelCode',
                            property.major_ref AS 'MajorReference',
                            lulevel.lu_desc AS 'Description', 
                            property.address1 AS 'Address',
                            property.post_code AS 'PostCode'
                        FROM 
                            property
                        INNER 
                            JOIN lulevel ON property.level_code = lulevel.lu_ref 
                        WHERE 
                            post_code = @Postcode {SqlLevelCondition(minLevel, maxLevel)}
                        ORDER BY property.prop_ref";
                    var properties = connection.Query<PropertyLevelModel>(query, new { Postcode = postcode }).ToArray();
                    return properties;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new UHWWarehouseRepositoryException();
            }
        }

        public async Task<PropertyDetails[]> GetPropertiesDetailsByReference(string[] references)
        {
            _logger.LogInformation($"Getting details for properties {GenericFormatter.CommaSeparate(references)}");
            try
            {
                using (SqlConnection connnection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    string query = @"
                        SELECT 
                            address1 AS 'ShortAddress',
                            post_code AS 'PostCodeValue',
                            ~no_maint AS 'Maintainable', 
                            prop_ref AS 'PropertyReference',
                            level_code AS 'LevelCode',
                            lulevel.lu_desc AS 'Description'
                        FROM 
                            property 
                            INNER JOIN lulevel ON property.level_code = lulevel.lu_ref
                        WHERE 
                            prop_ref IN @PropertyReference";
                    var properties = connnection.Query<PropertyDetails>(query, new { PropertyReference = references }).ToArray();
                    return properties;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new UHWWarehouseRepositoryException();
            }
        }

        public async Task<PropertyLevelModel[]> GetPropertyDetailsByFirstLineOfAddress(string firstLineOfAddress, int limit)
        {
            _logger.LogInformation($"Getting details for properties using first line of address {firstLineOfAddress}");
            string strLimit = limit.ToString();

            //Make sure non-empty strings are returned
            string[] words = firstLineOfAddress.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            StringBuilder _sb = new StringBuilder("(");
            //build where clause string for search
            //and parameters for dapper
            // eg. (charindex(@p, lower(address1)) > 0 and charindex(@pp, lower(address1)) > 0)
            var queryparams = new DynamicParameters();
            int length = words.Length;
            int count = 0;
            StringBuilder _sbparams = new StringBuilder();
            foreach (var word in words)
            {
                _sb.Append($@"charindex(@{_sbparams.Append("p").ToString()}, lower(address1)) > 0");
                //Lower each word used for query 
                queryparams.Add($@"@{_sbparams.ToString()}", word.ToLower());
                if (++count < length)
                    _sb.Append(@" and ");
            }

            _sb.Append(")");
            firstLineOfAddress = _sb.ToString();
            try
            {
                using (SqlConnection connection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    string query = $@"
                        SELECT TOP {strLimit}
                            property.prop_ref AS 'PropertyReference',
                            property.level_code AS 'LevelCode',
                            property.major_ref AS 'MajorReference',
                            lulevel.lu_desc AS 'Description', 
                            property.address1 AS 'Address',
                            property.post_code AS 'PostCode'
                        FROM 
                            property
                        INNER 
                            JOIN lulevel ON property.level_code = lulevel.lu_ref 
                        WHERE 
                            {firstLineOfAddress}
                        ORDER BY property.prop_ref";
                    var properties = connection.Query<PropertyLevelModel>(query, queryparams).ToArray();
                    return properties;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new UHWWarehouseRepositoryException();
            }
        }

        public async Task<PropertyDetails> GetPropertyBlockByReference(string reference)
        {
            _logger.LogInformation($"Getting details for property {reference}");
            try
            {
                using (SqlConnection connection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    string query = @"
                        SELECT 
                            address1 AS 'ShortAddress',
                            post_code AS 'PostCodeValue',
                            ~no_maint AS 'Maintainable', 
                            prop_ref AS 'PropertyReference'
                        FROM 
                            property
                        WHERE 
                            prop_ref = (SELECT u_block 
                                        FROM property 
                                        WHERE prop_ref = @PropertyReference)";

                    var property = connection.Query<PropertyDetails>(query, new { PropertyReference = reference }).FirstOrDefault();
                    return property;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new UHWWarehouseRepositoryException();
            }
        }

        public async Task<PropertyDetails> GetPropertyEstateByReference(string reference)
        {
            _logger.LogInformation($"Getting details for property {reference}");
            try
            {
                using (SqlConnection connection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    string query = @"
                        SELECT 
                            address1 AS 'ShortAddress',
                            post_code AS 'PostCodeValue',
                            ~no_maint AS 'Maintainable',
                            prop_ref AS 'PropertyReference'
                        FROM 
                            property 
                        WHERE 
                            prop_ref = (SELECT u_estate 
                                        FROM property 
                            WHERE prop_ref = @PropertyReference)";
                    var property = connection.Query<PropertyDetails>(query, new { PropertyReference = reference }).FirstOrDefault();
                    return property;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new UHWWarehouseRepositoryException();
            }
        }

        public async Task<PropertyLevelModel[]> GetFacilitiesByPropertyRef(string reference)
        {
            _logger.LogInformation($"Getting  facilities for property {reference}");
            try
            {
                using (SqlConnection connection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    string query = @"with property_tree as (
                            select prop_ref, level_code, major_ref
                            from property
                            where prop_ref = @EstateReference 
                            union all
                            select p2.prop_ref, p2.level_code, p2.major_ref
                            from property p2
                            join property_tree p on p.major_ref = p2.prop_ref 
                             )
                            select 
                            property.prop_ref AS 'PropertyReference',
                            property.level_code AS 'LevelCode',
                            property.major_ref AS 'MajorReference',
                            lulevel.lu_desc AS 'Description', 
                            property.address1 AS 'Address',
                            property.post_code AS 'PostCode'
                            from property
                            inner JOIN
                            lulevel on property.level_code = lulevel.lu_ref
                            where level_code = '6' and major_ref in 
                            (select p.prop_ref from property_tree p where p.level_code <> '0')
                            or  ((level_code = '4' or level_code = '3' or level_code = '2') and prop_ref in 
                            (select p.prop_ref from property_tree p where p.level_code <> '0'))
                            order by level_code asc";
                    var property = connection.Query<PropertyLevelModel>(query, new { EstateReference = reference }).ToArray();
                    return property;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new UHWWarehouseRepositoryException();
            }
        }

        public async Task<UHWorkOrder> GetWorkOrderByWorkOrderReference(string reference)
        {
            if (!IsProduction())
            {
                return null;
            }

            try
            {
                using (var connection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    string query = @"set dateformat ymd;
                        SELECT
                           LTRIM(RTRIM(wo.wo_ref)) AS WorkOrderReference,
                           LTRIM(RTRIM(r.rq_ref)) AS RepairRequestReference,
                           r.rq_problem AS ProblemDescription,
                           wo.created AS Created,
                           wo.est_cost AS EstimatedCost,
                           wo.act_cost AS ActualCost,
                           wo.completed AS CompletedOn,
                           wo.date_due AS DateDue,
                           wo.auth_date AS AuthDate,
                           LTRIM(RTRIM(wo.wo_status)) AS WorkOrderStatus,
                           LTRIM(RTRIM(wo.u_dlo_status)) AS DLOStatus,
                           LTRIM(RTRIM(wo.u_servitor_ref)) AS ServitorReference,
                           LTRIM(RTRIM(wo.prop_ref)) AS PropertyReference,
                           LTRIM(RTRIM(t.job_code)) AS SORCode,
                           LTRIM(RTRIM(tr.trade_desc)) AS Trade,
                           LTRIM(RTRIM(wo.sup_ref)) AS SupplierRef,
						   LTRIM(RTRIM(auser.user_login)) as UserLogin,
        				       LTRIM(RTRIM(auser.username)) as Username,
                           LTRIM(RTRIM(authuser.username)) as 'AuthorisedBy'
                        FROM
                           rmworder wo
                            INNER JOIN rmreqst r ON wo.rq_ref = r.rq_ref
                            INNER JOIN rmtask t ON t.wo_ref = wo.wo_ref 
                            INNER JOIN rmtrade tr ON tr.trade = t.trade
							LEFT OUTER JOIN auser AS auser ON auser.user_code = wo.user_code
                            LEFT OUTER JOIN auser AS authuser ON authuser.user_code = wo.auth_by
                        WHERE 
                            wo.created < @CutoffTime AND wo.wo_ref = @PropertyReference AND t.task_no = 1";

                    var queryParameters = new
                    {
                        CutoffTime = GetCutoffTime(),
                        PropertyReference = reference
                    };
                    var workOrder = connection.Query<UHWorkOrder>(query, queryParameters).FirstOrDefault();
                    return workOrder;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new UHWWarehouseRepositoryException();
            }
        }

        public async Task<IEnumerable<UHWorkOrder>> GetWorkOrdersByWorkOrderReferences(string[] references)
        {
            if (!IsProduction())
            {
                return new List<UHWorkOrder>();
            }

            try
            {
                using (var connection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    string query = @"set dateformat ymd;
                        SELECT
                           LTRIM(RTRIM(wo.wo_ref)) AS WorkOrderReference,
                           LTRIM(RTRIM(r.rq_ref)) AS RepairRequestReference,
                           r.rq_problem AS ProblemDescription,
                           wo.created AS Created,
                           wo.est_cost AS EstimatedCost,
                           wo.act_cost AS ActualCost,
                           wo.completed AS CompletedOn,
                           wo.date_due AS DateDue,
                           LTRIM(RTRIM(wo.wo_status)) AS WorkOrderStatus,
                           LTRIM(RTRIM(wo.u_dlo_status)) AS DLOStatus,
                           LTRIM(RTRIM(wo.u_servitor_ref)) AS ServitorReference,
                           LTRIM(RTRIM(wo.prop_ref)) AS PropertyReference,
                           LTRIM(RTRIM(t.job_code)) AS SORCode,
                           LTRIM(RTRIM(tr.trade_desc)) AS Trade,
						   auser.user_login,
        				   auser.username
                        FROM
                           rmworder wo
                            INNER JOIN rmreqst r ON wo.rq_ref = r.rq_ref
                            INNER JOIN rmtask t ON t.wo_ref = wo.wo_ref 
                            INNER JOIN rmtrade tr ON tr.trade = t.trade
							LEFT OUTER JOIN auser AS auser ON auser.user_code = wo.user_code
                        WHERE 
                            wo.created < @CutoffTime AND wo.wo_ref IN @PropertyReferences AND t.task_no = 1";

                    var queryParameters = new
                    {
                        CutoffTime = GetCutoffTime(),
                        PropertyReferences = references
                    };
                    return connection.Query<UHWorkOrder>(query, queryParameters).ToArray();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new UHWWarehouseRepositoryException();
            }
        }

        public async Task<DrsOrder> GetWorkOrderDetails(string workOrderReference)
        {
            if (!IsProduction())
            {
                return null;
            }

            _logger.LogInformation($"Getting the work order details from Uh Warehouse for {workOrderReference}");
            try
            {
                using (var connection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    var query = @"set dateformat ymd;
                    select created createdDate,
                        date_due dueDate,
                        rtrim(wo_ref) wo_ref,
                        rtrim(rq_name) contactName,
                        rtrim(sup_ref) contract,
                        rmworder.prop_ref,
                        case when (convert(varchar(50),rq_phone))>'' then convert(bit,1)
                        else
                        convert(bit,0)
                        end  txtMessage,
                        rq_phone phone,
                        rq_priority priority,
                        rtrim(rmreqst.user_code) userid,
                        null tasks,
                        rtrim(short_address) propname,
                        address1,
                        post_code postcode,
                        convert(varchar(50),rq_problem) comments
                        from rmworder 
                        inner join property on rmworder.prop_ref=property.prop_ref
                        inner join rmreqst on rmworder.rq_ref=rmreqst.rq_ref
                        where created < @CutoffTime AND wo_ref = @WorkOrderReference";

                    var queryParameters = new
                    {
                        CutoffTime = GetCutoffTime(),
                        WorkOrderReference = workOrderReference
                    };
                    var drsOrderResult = connection.Query<DrsOrder>(query, queryParameters).FirstOrDefault();

                    if (drsOrderResult == null)
                    {
                        return drsOrderResult;
                    }

                    query = @"set dateformat ymd;
                        select  rmtask.job_code,
                            convert(varchar(50), task_text) comments,
                            est_cost itemValue,
                            est_units itemqty,
                            u_smv smv,
                            rmjob.trade
                        from rmtask inner join rmjob on rmtask.job_code = rmjob.job_code
                        where created < @CutoffTime AND wo_ref = @WorkOrderReference";

                    drsOrderResult.Tasks = connection.Query<DrsTask>(query, queryParameters).ToList();
                    return drsOrderResult;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new UHWWarehouseRepositoryException();
            }
        }

        public async Task<IEnumerable<UHWorkOrder>> GetWorkOrderByPropertyReference(string propertyReference)
        {
            if (!IsProduction())
            {
                return new List<UHWorkOrder>();
            }

            IEnumerable<UHWorkOrder> workOrders;
            try
            {
                using (var connection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    string query = $@"set dateformat ymd;
                                    SELECT
                                       LTRIM(RTRIM(wo.wo_ref)) AS WorkOrderReference,
                                       LTRIM(RTRIM(r.rq_ref)) AS RepairRequestReference,
                                       r.rq_problem AS ProblemDescription,
                                       wo.created AS Created,
                                       wo.est_cost AS EstimatedCost,
                                       wo.act_cost AS ActualCost,
                                       wo.completed AS CompletedOn,
                                       wo.date_due AS DateDue,
                                       LTRIM(RTRIM(wo.wo_status)) AS WorkOrderStatus,
                                       LTRIM(RTRIM(wo.u_dlo_status)) AS DLOStatus,
                                       LTRIM(RTRIM(wo.u_servitor_ref)) AS ServitorReference,
                                       LTRIM(RTRIM(wo.prop_ref)) AS PropertyReference,
                                       LTRIM(RTRIM(t.job_code)) AS SORCode,
                                       LTRIM(RTRIM(tr.trade_desc)) AS Trade

                                    FROM
                                       rmworder wo
                                       INNER JOIN rmreqst r ON wo.rq_ref = r.rq_ref
                                       INNER JOIN rmtask t ON wo.rq_ref = t.rq_ref
                                       INNER JOIN rmtrade tr ON t.trade = tr.trade
                                       WHERE wo.created < @CutoffTime AND wo.prop_ref = @PropertyReference AND t.task_no = 1;";

                    var queryParameters = new
                    {
                        CutoffTime = GetCutoffTime(),
                        PropertyReference = propertyReference
                    };
                    workOrders = await connection.QueryAsync<UHWorkOrder>(query, queryParameters);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new UHWWarehouseRepositoryException();
            }

            return workOrders;
        }

        public async Task<IEnumerable<UHWorkOrder>> GetWorkOrdersByPropertyReferences(string[] propertyReferences, DateTime since, DateTime until)
        {
            if (!IsProduction())
            {
                return new List<UHWorkOrder>();
            }

            IEnumerable<UHWorkOrder> workOrders;

            try
            {
                using (var connection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    string query = @"set dateformat ymd;
                                    SELECT
                                       LTRIM(RTRIM(wo.wo_ref)) AS WorkOrderReference,
                                       LTRIM(RTRIM(r.rq_ref)) AS RepairRequestReference,
                                       r.rq_problem AS ProblemDescription,
                                       wo.created AS Created,
                                       wo.est_cost AS EstimatedCost,
                                       wo.act_cost AS ActualCost,
                                       wo.completed AS CompletedOn,
                                       wo.date_due AS DateDue,
                                       LTRIM(RTRIM(wo.wo_status)) AS WorkOrderStatus,
                                       LTRIM(RTRIM(wo.u_dlo_status)) AS DLOStatus,
                                       LTRIM(RTRIM(wo.u_servitor_ref)) AS ServitorReference,
                                       LTRIM(RTRIM(wo.prop_ref)) AS PropertyReference,
                                       LTRIM(RTRIM(t.job_code)) AS SORCode,
                                       LTRIM(RTRIM(tr.trade_desc)) AS Trade

                                    FROM
                                       rmworder wo
                                       INNER JOIN rmreqst r ON wo.rq_ref = r.rq_ref
                                       INNER JOIN rmtask t ON wo.rq_ref = t.rq_ref
                                       INNER JOIN rmtrade tr ON t.trade = tr.trade
                                       WHERE wo.created < @CutoffTime AND wo.created <= @Until
                                       AND wo.created >= @Since
                                       AND wo.prop_ref IN @PropertyReferences AND t.task_no = 1";

                    var queryParameters = new
                    {
                        CutoffTime = GetCutoffTime(),
                        Since = since.ToString("yyyy-MM-dd HH:mm:ss"),
                        Until = until.ToString("yyyy-MM-dd HH:mm:ss"),
                        PropertyReferences = propertyReferences
                    };
                    workOrders = await connection.QueryAsync<UHWorkOrder>(query, queryParameters);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new UHWWarehouseRepositoryException(ex.Message);
            }

            return workOrders;
        }

        public async Task<IEnumerable<UHWorkOrder>> GetWorkOrderByBlockReference(string[] blockReferences, string trade, DateTime since, DateTime until)
        {
            if (!IsProduction())
            {
                return new List<UHWorkOrder>();
            }

            IEnumerable<UHWorkOrder> workOrders;
            try
            {
                using (var connection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    string query = $@"set dateformat ymd;
                                    SELECT
                                       LTRIM(RTRIM(wo.wo_ref)) AS WorkOrderReference,
                                       LTRIM(RTRIM(r.rq_ref)) AS RepairRequestReference,
                                       r.rq_problem AS ProblemDescription,
                                       wo.created AS Created,
                                       wo.est_cost AS EstimatedCost,
                                       wo.act_cost AS ActualCost,
                                       wo.completed AS CompletedOn,
                                       wo.date_due AS DateDue,
                                       LTRIM(RTRIM(wo.wo_status)) AS WorkOrderStatus,
                                       LTRIM(RTRIM(wo.u_dlo_status)) AS DLOStatus,
                                       LTRIM(RTRIM(wo.u_servitor_ref)) AS ServitorReference,
                                       LTRIM(RTRIM(wo.prop_ref)) AS PropertyReference,
                                       LTRIM(RTRIM(t.job_code)) AS SORCode,
                                       LTRIM(RTRIM(tr.trade_desc)) AS Trade

                                    FROM
                                       rmworder wo
                                       INNER JOIN rmreqst r ON wo.rq_ref = r.rq_ref
                                       INNER JOIN rmtask t ON wo.rq_ref = t.rq_ref
                                       INNER JOIN rmtrade tr ON t.trade = tr.trade
                                       INNER JOIN property p ON p.prop_ref = wo.prop_ref
                                       WHERE wo.created < @CutoffTime
                                       AND wo.created <= @Until
                                       AND wo.created >= @Since
                                       AND (p.u_block IN @BlockReferences OR p.prop_ref IN @BlockReferences) 
                                       AND tr.trade_desc = @Trade AND t.task_no = 1";

                    var queryParameters = new
                    {
                        CutoffTime = GetCutoffTime(),
                        Since = since.ToString("yyyy-MM-dd HH:mm:ss"),
                        Until = until.ToString("yyyy-MM-dd HH:mm:ss"),
                        BlockReferences = blockReferences,
                        Trade = trade
                    };
                    workOrders = await connection.QueryAsync<UHWorkOrder>(query, queryParameters);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new UHWWarehouseRepositoryException();
            }

            return workOrders;
        }

        public async Task<IEnumerable<Note>> GetNotesByWorkOrderReference(string workOrderReference)
        {
            if (!IsProduction())
            {
                return new List<Note>();
            }

            try
            {
                using (var connection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    var query = @"set dateformat ymd;
                            SELECT
                                @WorkOrderReference AS WorkOrderReference,
                                note.NoteID AS NoteId,
                                note.NoteText AS Text,
                                note.NDate AS LoggedAt,
                                note.UserID AS LoggedBy
                            FROM
                                StagedDBW2.dbo.W2ObjectNote AS note
                            INNER JOIN 
                                StagedDB.dbo.rmworder AS work_order
                                ON note.KeyNumb = work_order.rmworder_sid
                            WHERE 
                                note.NDate < @CutoffTime AND work_order.wo_ref = @WorkOrderReference";

                    var queryParameters = new
                    {
                        CutoffTime = GetCutoffTime(),
                        WorkOrderReference = workOrderReference
                    };
                    var notes = connection.Query<Note>(query, queryParameters);
                    return notes;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new UHWWarehouseRepositoryException();
            }
        }

        public async Task<IEnumerable<UHWorkOrder>> GetTasksForWorkOrder(string workOrderReference)
        {
            if (!IsProduction())
            {
                return new List<UHWorkOrder>();
            }

            IEnumerable<UHWorkOrder> workOrders;
            try
            {
                using (var connection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    string query = @"set dateformat ymd;
                         SELECT
                           LTRIM(RTRIM(wo.wo_ref)) AS WorkOrderReference,
                           LTRIM(RTRIM(r.rq_ref)) AS RepairRequestReference,
                           r.rq_problem AS ProblemDescription,
                           t.created AS Created,
                           t.est_cost AS EstimatedCost,
                           t.est_units AS EstimatedUnits,
                           LTRIM(RTRIM(t.unit_narr)) AS UnitType,
                           t.completed AS CompletedOn,
                           t.date_due AS DateDue,
                           LTRIM(RTRIM(t.task_status)) AS TaskStatus,
                           LTRIM(RTRIM(t.prop_ref)) AS PropertyReference,
                           LTRIM(RTRIM(t.job_code)) AS SORCode,
                           LTRIM(RTRIM(tr.trade_desc)) AS Trade,
                           LTRIM(RTRIM(t.sup_ref)) AS SupplierRef,
						   LTRIM(RTRIM(auser.user_login)) as UserLogin,
        				   LTRIM(RTRIM(auser.username)) as Username,
                           LTRIM(RTRIM(rj.short_desc)) AS SORCodeDescription
                        FROM
                           rmworder wo
                            INNER JOIN rmreqst r ON wo.rq_ref = r.rq_ref
                            INNER JOIN rmtask t ON t.wo_ref = wo.wo_ref 
                            LEFT JOIN rmjob rj ON  rj.job_code = t.job_code 
                            INNER JOIN rmtrade tr ON tr.trade = t.trade
							LEFT OUTER JOIN auser AS auser ON auser.user_code = wo.user_code
                        WHERE 
                            wo.created < @CutoffTime AND wo.wo_ref = @WorkOrderReference";

                    var queryParameters = new
                    {
                        CutoffTime = GetCutoffTime(),
                        WorkOrderReference = workOrderReference
                    };
                    workOrders = await connection.QueryAsync<UHWorkOrder>(query, queryParameters);
                   
                    return workOrders;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new UHWWarehouseRepositoryException();
            }
        }

        public async Task<IEnumerable<UHWorkOrderFeed>> GetWorkOrderFeed(string startId, int size)
        {
            if (!IsProduction())
            {
                return new List<UHWorkOrderFeed>();
            }

            try
            {
                using (var connection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    string query = $@"set dateformat ymd;
                        SELECT TOP {size} 
                            LTRIM(RTRIM(work_order.wo_ref)) AS WorkOrderReference,
                            LTRIM(RTRIM(work_order.prop_ref)) AS PropertyReference,
                            work_order.created AS Created,
                            request.rq_problem AS ProblemDescription
                        FROM 
                            rmworder AS work_order
                        INNER JOIN
                            rmreqst AS request ON work_order.rq_ref = request.rq_ref
                        WHERE 
                            work_order.created < @CutoffTime AND work_order.wo_ref > @StartId
                            AND work_order.wo_ref NOT LIKE '[A-Z]%'
                        ORDER BY work_order.wo_ref";

                    var queryParameters = new
                    {
                        CutoffTime = GetCutoffTime(),
                        StartId = startId
                    };
                    var workOrders = connection.Query<UHWorkOrderFeed>(query, queryParameters);
                    return workOrders;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new UHWWarehouseRepositoryException();
            }
        }

        public async Task<IEnumerable<Note>> GetNoteFeed(int noteId, string noteTarget, int size)
        {
            if (!IsProduction())
            {
                return new List<Note>();
            }

            try
            {
                using (var connection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    string query = $@"set dateformat ymd;
                        SELECT TOP {size}
                            LTRIM(RTRIM(work_order.wo_ref)) AS WorkOrderReference,
                            note.NDate AS LoggedAt,
                            note.UserID AS LoggedBy,
                            note.NoteText As [Text],
                            note.NoteID AS NoteId
                        FROM 
                            StagedDBW2.dbo.W2ObjectNote AS note
                        INNER JOIN
                            StagedDB.dbo.rmworder AS work_order ON note.KeyNumb = work_order.rmworder_sid
                        WHERE 
                            note.NDate < @CutoffTime AND NoteID > @NoteId
                            AND KeyObject in (@NoteTarget)
                        ORDER BY NoteID";

                    var queryParameters = new
                    {
                        CutoffTime = GetCutoffTime(),
                        NoteId = noteId,
                        NoteTarget = noteTarget
                    };
                    var notes = connection.Query<Note>(query, queryParameters);
                    return notes;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new UHWWarehouseRepositoryException();
            }
        }

        public async Task<IEnumerable<string>> GetDistinctNoteKeyObjects()
        {
            try
            {
                using (var connection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    string query = @"
                        SELECT DISTINCT LOWER([KeyObject])
                        FROM StagedDBW2.dbo.[W2ObjectNote]";
                    var keyObjets = connection.Query<string>(query);
                    return keyObjets;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new UHWWarehouseRepositoryException();
            }
        }

        private static string GetCutoffTime()
        {
            DateTime now = DateTime.Now;
            DateTime dtCutoff = new DateTime(now.Year, now.Month, now.Day, 23, 0, 0);
            dtCutoff = dtCutoff.AddDays(-1);

            return dtCutoff.ToString("yyyy-MM-dd HH:mm:ss");
        }

        private string SqlLevelCondition(int? minLevel, int? maxLevel)
        {
            string levelConditionString;
            if (maxLevel == minLevel)
            {
                levelConditionString = $"AND level_code = {maxLevel}";
            }
            else if (maxLevel == null && minLevel != null)
            {
                levelConditionString = $"AND level_code <= {minLevel}";
            }
            else if (minLevel == null && maxLevel != null)
            {
                levelConditionString = $"AND level_code >= {maxLevel}";
            }
            else
            {
                levelConditionString = $"AND level_code <= {minLevel} AND level_code >= {maxLevel}";
            }

            return levelConditionString;
        }

        private bool IsProduction()
        {
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (environment.ToLower() == "production")
            {
                return true;
            }

            return false;
        }
    }

    public class UHWWarehouseRepositoryException : Exception
    {
      public UHWWarehouseRepositoryException() : base() { }
      public UHWWarehouseRepositoryException(string message) : base(message) { }
    }
}
