using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using HackneyRepairs.DbContext;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Models;
using HackneyRepairs.PropertyService;
using Microsoft.EntityFrameworkCore;

namespace HackneyRepairs.Repository
{
    public class UHWWarehouseRepository :IUHWWarehouseRepository
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
                using (var connection  = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    var query = "SELECT [no_maint] FROM [property] where [prop_ref]='" + propertyReference + "'";
                    var result = connection.Query<bool>(query).FirstOrDefault();
                    return Convert.ToBoolean(result);                
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new UHWWarehouseRepositoryException();
            }
        }

        public async Task<IEnumerable<RepairRequestBase>> GetRepairRequests(string propertyReference)
        {
            if (IsDevelopmentEnvironment())
            {
                return new List<RepairRequest>();
            }

            try
            {
                using (var connection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    string query = $@"set dateformat ymd;
                                SELECT   
                                    r.rq_ref as repairRequestReference,
                                    r.rq_problem as problemDescription,
                                    r.rq_priority as priority,
                                    r.prop_ref as propertyReference
                                FROM rmreqst r
                                    WHERE r.rq_date < '{GetCutoffTime()}' AND r.prop_ref = '{propertyReference}'";
                    var repairs = connection.Query<RepairRequestBase>(query).ToList();
                    return repairs;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new UhtRepositoryException();
            }
        }

        public async Task<PropertyLevelModel> GetPropertyLevelInfo(string reference)
        {
            _logger.LogInformation($"Getting propertiy hierarchical info for: {reference}");
            try
            {
                using (var connection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    var query = $@"
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
                            prop_ref = '{reference}'";

                    var result = connection.Query<PropertyLevelModel>(query).FirstOrDefault();
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new UHWWarehouseRepositoryException();
            }
        }

		public async Task<List<PropertyLevelModel>> GetPropertyLevelInfosForParent(string reference)
        {
            _logger.LogInformation($"Getting propertiy hierarchical info for: {reference}");
            try
            {
                using (var connection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    var query = $@"
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
                            major_ref = '{reference}'";

					var result = connection.Query<PropertyLevelModel>(query).AsList();
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new UHWWarehouseRepositoryException();
            }
        }

        public async Task<PropertySummary[]> GetPropertyListByPostCode(string postcode)
        {
            _logger.LogInformation($"Getting properties for postcode {postcode}");
            try
            {
                using (SqlConnection connection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    string query = $@"
                        SELECT 
                            short_address AS 'ShortAddress',
                            post_code AS 'PostCodeValue', 
                            prop_ref AS 'PropertyReference' 
                        FROM 
                            property 
                        WHERE 
                            level_code = 7 AND post_code = '{postcode}'";
                    var properties = connection.Query<PropertySummary>(query).ToArray();
                    return properties;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new UHWWarehouseRepositoryException();
            }
        }

        public async Task<PropertyDetails> GetPropertyDetailsByReference(string reference)
        {
            _logger.LogInformation($"Getting details for property {reference}");
            try
            {
                using (SqlConnection connnection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    string query = $@"
                        SELECT 
                            short_address AS 'ShortAddress',
                            post_code AS 'PostCodeValue',
                            ~no_maint AS 'Maintainable', 
                            prop_ref AS 'PropertyReference' 
                        FROM 
                            property 
                        WHERE 
                            prop_ref = '{reference}'";
                    var property = connnection.Query<PropertyDetails>(query).FirstOrDefault();
                    return property;
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
                    string query = $@"
                        SELECT 
                            short_address AS 'ShortAddress',
                            post_code AS 'PostCodeValue',
                            ~no_maint AS 'Maintainable', 
                            prop_ref AS 'PropertyReference'
                        FROM 
                            property
                        WHERE 
                            prop_ref = (SELECT u_block 
                                        FROM property 
                                        WHERE prop_ref = '{reference}')";
                    var property = connection.Query<PropertyDetails>(query).FirstOrDefault();
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
                    string query = $@"
                        SELECT 
                            short_address AS 'ShortAddress',
                            post_code AS 'PostCodeValue',
                            ~no_maint AS 'Maintainable',
                            prop_ref AS 'PropertyReference'
                        FROM 
                            property 
                        WHERE 
                            prop_ref = (SELECT u_estate 
                                        FROM property 
                            WHERE prop_ref = '{reference}')";
                    var property = connection.Query<PropertyDetails>(query).FirstOrDefault();
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
            if (IsDevelopmentEnvironment())
            {
                return null;
            }
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
                            INNER JOIN rmtask t ON t.wo_ref = wo.wo_ref 
                            INNER JOIN rmtrade tr ON tr.trade = t.trade
                        WHERE 
                            wo.created < '{GetCutoffTime()}' AND wo.wo_ref = '{reference}'AND t.task_no = 1";

                    var workOrder = connection.Query<UHWorkOrder>(query).FirstOrDefault();
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
            if (IsDevelopmentEnvironment())
            {
                return null;
            }
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
                            INNER JOIN rmtask t ON t.wo_ref = wo.wo_ref 
                            INNER JOIN rmtrade tr ON tr.trade = t.trade
                        WHERE 
                            wo.created < '{GetCutoffTime()}' AND wo.wo_ref IN('{String.Join("', '", references)}') AND t.task_no = 1";

                    return connection.Query<UHWorkOrder>(query).ToArray();
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
            if (IsDevelopmentEnvironment())
            {
                return null;
            }

            _logger.LogInformation($"Getting the work order details from Uh Warehouse for {workOrderReference}");
            try
            {
                using (var connection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    var query = $@"set dateformat ymd;
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
                        short_address address1,
                        post_code postcode,
                        convert(varchar(50),rq_problem) comments
                        from rmworder 
                        inner join property on rmworder.prop_ref=property.prop_ref
                        inner join rmreqst on rmworder.rq_ref=rmreqst.rq_ref
                        where created < '{GetCutoffTime()}' AND wo_ref='{workOrderReference}'";
                    var drsOrderResult = connection.Query<DrsOrder>(query).FirstOrDefault();

                    if (drsOrderResult == null)
                    {
                        return drsOrderResult;
                    }

                    query = $@"set dateformat ymd;
                        select  rmtask.job_code,
                            convert(varchar(50), task_text) comments,
                            est_cost itemValue,
                            est_units itemqty,
                            u_smv smv,
                            rmjob.trade
                        from rmtask inner join rmjob on rmtask.job_code = rmjob.job_code
                        where created < '{GetCutoffTime()}' AND wo_ref = '{workOrderReference}'";
                    drsOrderResult.Tasks = connection.Query<DrsTask>(query).ToList();

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
            if (IsDevelopmentEnvironment())
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
                                       WHERE wo.created < '{GetCutoffTime()}' AND wo.prop_ref = '{propertyReference}' AND t.task_no = 1;";
                    workOrders = await connection.QueryAsync<UHWorkOrder>(query);
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
            if (IsDevelopmentEnvironment())
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
                                       WHERE wo.created < '{GetCutoffTime()}' AND wo.created <= '{until.ToString("yyyy-MM-dd HH:mm:ss")}'
                                       AND wo.created >= '{since.ToString("yyyy-MM-dd HH:mm:ss")}'
                                       AND wo.prop_ref IN('{String.Join("','", propertyReferences)}') AND t.task_no = 1";
                    workOrders = await connection.QueryAsync<UHWorkOrder>(query);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new UHWWarehouseRepositoryException();
            }
            return workOrders;
        }

        public async Task<IEnumerable<UHWorkOrder>> GetWorkOrderByBlockReference(string blockReference, string trade, DateTime since, DateTime until)
        {
            if (IsDevelopmentEnvironment())
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
                                       WHERE wo.created < '{GetCutoffTime()}' 
                                       AND wo.created <= '{until.ToString("yyyy - MM - dd HH: mm:ss")}' 
                                       AND wo.created >= '{since.ToString("yyyy-MM-dd HH:mm:ss")}' 
                                       AND p.u_block = '{blockReference}' AND tr.trade_desc = '{trade}' AND t.task_no = 1;";
					workOrders = await connection.QueryAsync<UHWorkOrder>(query);
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
            if (IsDevelopmentEnvironment())
            {
                return new List<Note>();
            }

            try
            {
                using (var connection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    var query = $@"set dateformat ymd;
                            SELECT
                               '{workOrderReference}' AS WorkOrderReference,
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
                                note.NDate < '{GetCutoffTime()}' AND work_order.wo_ref = '{workOrderReference}'";
                    var notes = connection.Query<Note>(query);
                    return notes; 
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new UHWWarehouseRepositoryException();
            }
        }

        public async Task<IEnumerable<UHWorkOrderFeed>> GetWorkOrderFeed(string startID, int size)
        {
            if (IsDevelopmentEnvironment())
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
                            StagedDB.dbo.rmworder AS work_order
                        INNER JOIN
                            StagedDB.dbo.rmreqst AS request ON work_order.rq_ref = request.rq_ref
                        WHERE 
                            work_order.created < '{GetCutoffTime()}' AND work_order.wo_ref > '{startID}'
                            AND work_order.wo_ref NOT LIKE '[A-Z]%'
                        ORDER BY work_order.wo_ref";
                    var workOrders = connection.Query<UHWorkOrderFeed>(query);
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
            if (IsDevelopmentEnvironment())
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
                            note.NDate < '{GetCutoffTime()}' AND NoteID > {noteId}
                            AND KeyObject in ('{noteTarget}')
                        ORDER BY NoteID";
                    var notes = connection.Query<Note>(query);
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

		public static string GetCutoffTime()
        {
            DateTime now = DateTime.Now;
            DateTime dtCutoff = new DateTime(now.Year, now.Month, now.Day, 23, 0, 0);
            dtCutoff = dtCutoff.AddDays(-1);

            return dtCutoff.ToString("yyyy-MM-dd HH:mm:ss");
        }

        bool IsDevelopmentEnvironment()
        {
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (environment.ToLower() != "development" && environment.ToLower() != "local")
            {
                return false;
            }
            return true;
        }
    }

    public class UHWWarehouseRepositoryException : Exception { }
}
