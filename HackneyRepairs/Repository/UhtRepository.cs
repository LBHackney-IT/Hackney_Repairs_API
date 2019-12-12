using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HackneyRepairs.DbContext;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Models;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using Dapper;
using HackneyRepairs.DTOs;

namespace HackneyRepairs.Repository
{
	public class UhtRepository : IUhtRepository
	{
		private UhtDbContext _context;
		private ILoggerAdapter<UhtRepository> _logger;
		public UhtRepository(UhtDbContext context, ILoggerAdapter<UhtRepository> logger)
		{
			_context = context;
			_logger = logger;
		}

        public async Task<PropertyDetails> GetPropertyDetailsByReference(string reference)
        {
            _logger.LogInformation($"Getting details for property {reference}");
            try
            {
                using (SqlConnection connnection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    string query = $@"SELECT 
                            address1 AS 'ShortAddress',
                            post_code AS 'PostCodeValue',
                            ~no_maint AS 'Maintainable', 
                            property.prop_ref AS 'PropertyReference',
                            level_code AS 'LevelCode',
                            lulevel.lu_desc AS 'Description',
                            rent.tenure AS 'TenureCode',
							tenure.ten_desc AS 'TenureDescription',
                            cat_type AS 'PropertyTypeCode',
							LTRIM(RTRIM(pt_prop_desc)) AS 'PropertyTypeDescription',
							LTRIM(RTRIM([NeighbourhoodDescription])) AS 'LettingAreaDescription',
                            tenagree.tag_ref AS 'TenancyAgreementReference'
							FROM 
                            property 
                            LEFT JOIN lulevel ON property.level_code = lulevel.lu_ref
							LEFT join rent on property.prop_ref = rent.prop_ref
							LEFT join tenure on rent.tenure = tenure.ten_type
							left join [vw_pcPropertydesc] on property.prop_ref = [vw_pcPropertydesc].prop_ref
                            LEFT join proptype on property.cat_type = proptype.pt_prop_code
							LEFT JOIN tenagree on tenagree.prop_ref = property.prop_ref AND tenagree.terminated = 0
                            WHERE property.prop_ref = @PropertyReference";
                    var property = connnection.Query<PropertyDetails>(query, new { PropertyReference = reference }).First();
                    return property;
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
			_logger.LogInformation($"Getting the work order details from UHT for {workOrderReference}");
			try
			{
                using (var connection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
				{
                    var query = $@"SET dateformat ymd;
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
                        where created > @CutoffTime AND wo_ref = @WorkOrderReference";

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

                    query = $@"set dateformat ymd;
                        select  rmtask.job_code,
                            convert(varchar(50), task_text) comments,
                            est_cost itemValue,
                            est_units itemqty,
                            u_smv smv,
                            rmjob.trade
                        from rmtask inner join rmjob on rmtask.job_code = rmjob.job_code
                        where created > @CutoffTime AND wo_ref = @WorkOrderReference";

                    drsOrderResult.Tasks = connection.Query<DrsTask>(query, queryParameters).ToList();
                    return drsOrderResult;
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
                throw new UhtRepositoryException();
			}
		}

		public async Task<bool> UpdateRequestStatus(string repairRequestReference)
		{
			bool status = true;
			_logger.LogInformation($"updating the repair request status to 000 for {repairRequestReference}");
			try
			{
				int uCount = _context.Database.ExecuteSqlCommand("update rmreqst set rq_status='000',rq_cancel='',rq_cancel_date='',rq_cancelby='',rq_overall_status_date='',rq_overall_status='' where rq_ref=@p0", repairRequestReference);
				status = uCount > 0;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
			}
			finally
			{
				_context.Database.CloseConnection();
			}

			return status;
		}

		public async Task<int?> UpdateVisitAndBlockTrigger(string workOrderReference, DateTime startDate, DateTime endDate, int orderId, int bookingId, string slotDetail)
		{
			_logger.LogInformation($"Updating visit for order {workOrderReference}");
			/*Method for UpdateVisitAndBlockTrigger
             * Steps to be taken:
             * 1. Use order reference to get order sid (direct database query using "order_sid = db.Database.SqlQuery<int>(@"select rmworder_sid from rmworder where wo_ref='" + order_ref.Trim() + "'").FirstOrDefault()". db entity to be created??)
             * 2. If order sid is not null, run the stored procedure usp_update_uh_with_optiappt(order_sid, ) where r is the visit sid returned.
             *    Create a method in UhtRepository to run the sql command and stored procedure in 2 and 3 above.startTime, endTime, slotdtl
             * 3. Block the uh trigger by inserting the visit into the table u_sentToAppointmentSys
             */
			try
			{
				int? order_sid = null;
				int visit_sid = 0;
				using (var command = _context.Database.GetDbConnection().CreateCommand())
				{
					_context.Database.OpenConnection();
					// Step 1
					command.CommandText = @"select rmworder_sid from rmworder where wo_ref='" + workOrderReference.Trim() + "'";
					command.CommandType = CommandType.Text;
					using (var reader = await command.ExecuteReaderAsync())
					{
						if (reader != null & reader.HasRows)
						{
							while (reader.Read())
							{
								order_sid = reader.GetInt32(0);
							}
						}
					}

					// Step 2
					if (order_sid != null)
					{
						command.CommandText = "usp_update_uh_with_optiappt";
						command.CommandType = CommandType.StoredProcedure;
						var param = new SqlParameter
						{
							DbType = DbType.Int32,
							ParameterName = "@rmworder_sid",
							Value = order_sid
						};
						command.Parameters.Add(param);
						command.Parameters.Add(new SqlParameter
						{
							DbType = DbType.DateTime2,
							ParameterName = "@newappointdatetime",
							Value = startDate
						});
						command.Parameters.Add(new SqlParameter
						{
							DbType = DbType.DateTime2,
							ParameterName = "@enddatetime",
							Value = endDate
						});
						command.Parameters.Add(new SqlParameter
						{
							DbType = DbType.String,
							ParameterName = "@slot_name",
							Value = slotDetail
						});
						SqlParameter returnParam = new SqlParameter
						{
							DbType = DbType.Int32,
							ParameterName = "@visit_sid",
						};
						returnParam.Direction = ParameterDirection.ReturnValue;
						visit_sid = command.Parameters.Add(returnParam);
						await command.ExecuteNonQueryAsync();
						// Step 3
						// update table u_sentToAppointmentSys directly
						if (visit_sid != 0)
						{
							_logger.LogInformation($"Blocking UH triggers for order {workOrderReference}");
							string commandString = @"insert into u_sentToAppointmentSys (wo_ref,orderId,bookingId,dateSent,appointmentStart,appointmentEnd,uhVisitID) ";
							commandString += @"values ('" + workOrderReference + "'," + orderId + "," + bookingId + ", convert(datetime,'" + DateTime.Now + "',101),convert(datetime,'" + startDate + "',101),convert(datetime,'" + endDate + "',101)," + visit_sid + ")";
							_context.Database.ExecuteSqlCommand(commandString);
						}
					}
				}

				return order_sid;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				throw new UhtRepositoryException();
			}
			finally
			{
				_context.Database.CloseConnection();
			}
		}

        public async Task<IEnumerable<UHWorkOrder>> GetTasksForWorkOrder(string workOrderReference)
        {
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
                            wo.wo_ref = @WorkOrderReference";

                    var queryParameters = new
                    {
                        WorkOrderReference = workOrderReference
                    };
                    workOrders = await connection.QueryAsync<UHWorkOrder>(query, queryParameters);

                    return workOrders;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new UhtRepositoryException();
            }
        }

        public async Task<UHWorkOrder> GetWorkOrder(string workOrderReference)
        {
            UHWorkOrder workOrder;
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
                            wo.wo_ref = @WorkOrderReference AND t.task_no = 1";

                    workOrder = connection.Query<UHWorkOrder>(query, new { WorkOrderReference = workOrderReference }).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new UhtRepositoryException();
            }

            return workOrder;
        }

        public async Task<IEnumerable<UHWorkOrder>> GetWorkOrders(string[] workOrderReferences)
        {
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
                            wo.auth_date AS AuthDate,
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
                            wo.wo_ref IN @WorkOrderReferences AND t.task_no = 1";

                    var workOrders = connection.Query<UHWorkOrder>(query, new { WorkOrderReferences = workOrderReferences }).ToArray();
                    return workOrders;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new UhtRepositoryException();
            }
        }

        // unused
		public async Task<IEnumerable<UHWorkOrder>> GetWorkOrderByPropertyReference(string propertyReference)
        {
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
                                       wo.auth_date AS AuthDate,
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
                                       WHERE wo.created > '{GetCutoffTime()}' AND wo.prop_ref = '{propertyReference}' AND t.task_no = 1;";
					workOrders = await connection.QueryAsync<UHWorkOrder>(query);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new UhtRepositoryException();
            }

			return workOrders;
        }

        public async Task<IEnumerable<UHWorkOrder>> GetWorkOrdersByPropertyReferences(string[] propertyReferences, DateTime since, DateTime until)
        {
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
                                       wo.auth_date AS AuthDate,
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
                                       WHERE wo.created > @CutoffTime AND wo.created <= @Until
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
                throw new UhtRepositoryException();
            }

            return workOrders;
        }

        public async Task<IEnumerable<UHWorkOrder>> GetWorkOrderByBlockReference(string[] blockReferences, string trade, DateTime since, DateTime until)
        {
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
                                       wo.auth_date AS AuthDate,
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
                                       WHERE wo.created > @CutoffTime AND wo.created <= @Until
                                       AND wo.created >= @Since
                                       AND (p.u_block IN @BlockReferences OR p.prop_ref IN @BlockReferences)
                                       AND tr.trade_desc = @Trade AND t.task_no = 1;";

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
                throw new UhtRepositoryException();
            }

            return workOrders;
        }

		public async Task<IEnumerable<RepairRequestBase>> GetRepairRequestsByPropertyReference(string propertyReference)
        {
            try
            {
                using (var connection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    string query = @"set dateformat ymd;
                                      select    r.rq_ref as repairRequestReference,
                                                r.rq_problem as problemDescription,
                                                r.rq_priority as priority,
                                                r.prop_ref as propertyReference
                                                FROM rmreqst r
                                                where r.rq_date > @CutoffTime AND r.prop_ref = @PropertyReference";

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
                throw new UhtRepositoryException();
            }
        }

        public async Task<IEnumerable<RepairWithWorkOrderDto>> GetRepairRequest(string repairReference)
        {
            _logger.LogInformation($"Querying UHT db for repair reference: {repairReference}");

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
                                        (request.rq_date > '{GetCutoffTime()}' OR task.created > '{GetCutoffTime()}') AND
                                        request.rq_ref = '{repairReference}'";
                    var repair = connection.Query<RepairWithWorkOrderDto>(query).ToList();
                    return repair;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new UhtRepositoryException();
            }
        }

		public async Task<IEnumerable<DetailedAppointment>> GetAppointmentsByWorkOrderReference(string workOrderReference)
        {
            List<DetailedAppointment> appointments;
            try
            {
                using (var connection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    string query = $@"SELECT 
                                        visit.visit_sid AS Id,
                                        visit.visit_prop_appointment AS BeginDate,
                                        visit.visit_prop_end AS EndDate,
                                        ROW_NUMBER() OVER (ORDER BY visit.visit_sid) AS CreationOrder,
                                        'UH' AS SourceSystem,
                                        'Unknown' AS Status,
                                        supplier.sup_name AS AssignedWorker,
                                        supplier.sup_tel AS Mobilephone,
                                        visit.visit_comment AS Comment
                                    FROM 
                                        visit
                                    RIGHT OUTER JOIN 
                                        rmworder ON rmworder.rmworder_sid = visit.reference_sid
                                    INNER JOIN supplier ON supplier.sup_ref = rmworder.sup_ref
                                    WHERE 
                                        rmworder.wo_ref = @WorkOrderReference
                                    ORDER BY visit.visit_sid";
                    appointments = connection.Query<DetailedAppointment>(query, new { WorkOrderReference = workOrderReference }).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new UhtRepositoryException(ex.Message);
            }

            return appointments;
        }

        public async Task<DetailedAppointment> GetLatestAppointmentByWorkOrderReference(string workOrderReference)
        {
            DetailedAppointment appointment;
            try
            {
                using (var connection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    string query = @"SELECT
                                            Id,
                                            Status,
                                            AssignedWorker,
                                            Mobilephone,
                                            Priority,
                                            SourceSystem,
                                            Comment,
                                            BeginDate,
                                            EndDate,
                                            CreationDate
                                        FROM (
                                            SELECT
                                                visit.visit_sid AS Id,
                                                'Unknown' AS Status,
                                                supplier.sup_name AS AssignedWorker,
                                                supplier.sup_tel AS Mobilephone,
                                                rmworder.u_priority AS Priority,
                                                'UH' AS SourceSystem,
                                                visit.visit_comment AS Comment,
                                                visit.visit_prop_appointment AS BeginDate,
                                                visit.visit_prop_end AS EndDate,
                                                NULL AS CreationDate,
                                                rmworder.expected_completion AS ExpectedOn
                                            FROM
                                                visit
                                            RIGHT OUTER JOIN rmworder ON rmworder.rmworder_sid = visit.reference_sid
                                            INNER JOIN supplier ON supplier.sup_ref = rmworder.sup_ref
                                            WHERE
                                                rmworder.wo_ref = @WorkOrderReference) AS allApps
                                        WHERE
                                            allApps.EndDate = allApps.ExpectedOn OR allApps.ExpectedOn = 'Jan 1 1900 12:00:00:000AM'";

                    appointment = connection.Query<DetailedAppointment>(query, new { WorkOrderReference = workOrderReference }).FirstOrDefault();

                    if (appointment == null)
                    {
                        query = $@"SELECT
                                    Id,
                                    Status,
                                    AssignedWorker,
                                    Mobilephone,
                                    Priority,
                                    SourceSystem,
                                    Comment,
                                    BeginDate,
                                    EndDate,
                                    CreationDate
                                FROM (
                                    SELECT
                                        visit.visit_sid AS Id,
                                        'Unknown' AS Status,
                                        supplier.sup_name AS AssignedWorker,
                                        supplier.sup_tel AS Mobilephone,
                                        rmworder.u_priority AS Priority,
                                        'UH' AS SourceSystem,
                                        visit.visit_comment AS Comment,
                                        visit.visit_prop_appointment AS BeginDate,
                                        visit.visit_prop_end AS EndDate,
                                        NULL AS CreationDate,
                                        rmworder.expected_completion AS ExpectedOn
                                    FROM
                                        visit
                                    RIGHT OUTER JOIN rmworder ON rmworder.rmworder_sid = visit.reference_sid
                                    INNER JOIN supplier ON supplier.sup_ref = rmworder.sup_ref
                                    WHERE
                                        rmworder.wo_ref = @WorkOrderReference) AS allApps
                                ORDER BY Id desc";

                        appointment = connection.Query<DetailedAppointment>(query, new { WorkOrderReference = workOrderReference }).FirstOrDefault();
					}
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new UhtRepositoryException();
            }

            return appointment;
        }

        public async Task<IEnumerable<UHWorkOrderFeed>> GetWorkOrderFeed(string startId, int resultSize)
        {
            try
            {
                using (var connection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
					_logger.LogInformation($"Getting up to {resultSize} work orders with an id > {startId}");

                    string query = $@"set dateformat ymd;
                        SELECT TOP {resultSize}
                            LTRIM(RTRIM(work_order.wo_ref)) AS WorkOrderReference,
                            LTRIM(RTRIM(work_order.prop_ref)) AS PropertyReference,
                            work_order.created AS Created,
                            request.rq_problem AS ProblemDescription
                        FROM 
                            rmworder AS work_order
                        INNER JOIN
                            rmreqst AS request ON work_order.rq_ref = request.rq_ref
                        WHERE 
                            work_order.created > @CutoffTime AND work_order.wo_ref > @StartId
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
                throw new UhtRepositoryException();
            }
        }

        public static string GetCutoffTime()
        {
            DateTime now = DateTime.Now;
            DateTime dtCutoff = new DateTime(now.Year, now.Month, now.Day, 23, 0, 0);

            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (environment.ToLower() != "development" && environment.ToLower() != "local")
            {
                dtCutoff = dtCutoff.AddDays(-1);
            }
            else
            {
                dtCutoff = dtCutoff.AddYears(-10);
            }

            return dtCutoff.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public Task<int?> GetWorkOrderSid(string workOrderReference)
        {
            try
            {
                using (var connection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    string query = $@"
                        SELECT    
                            rmworder_sid
                        FROM 
                            rmworder
                        WHERE 
                            wo_ref = @WorkOrderReference";

                    var workOrderSid = connection.Query<int?>(query, new { WorkOrderReference = workOrderReference }).FirstOrDefault();
                    return Task.FromResult(workOrderSid);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new UhtRepositoryException();
            }
        }
    }

    public static class DateReaderExtensions
	{
		public static List<T> MapToList<T>(this DbDataReader reader) where T : new()
		{
			if (reader != null && reader.HasRows)
			{
				var entity = typeof(T);
				var entities = new List<T>();
				var propDict = new Dictionary<string, PropertyInfo>();
				var props = entity.GetProperties(BindingFlags.Instance | BindingFlags.Public);
				propDict = props.ToDictionary(p => p.Name.ToUpper(), p => p);
				while (reader.Read())
				{
					T newObject = new T();
					for (int index = 0; index < reader.FieldCount; index++)
					{
						if (propDict.ContainsKey(reader.GetName(index).ToUpper()))
						{
							var info = propDict[reader.GetName(index).ToUpper()];
							if (info != null && info.CanWrite)
							{
								var val = reader.GetValue(index);
								info.SetValue(newObject, (val == DBNull.Value) ? null : val, null);
							}
						}
					}

					entities.Add(newObject);
				}

				return entities;
			}

			return null;
		}
    }

	public class UhtRepositoryException : Exception
    {
        public UhtRepositoryException() { }

        public UhtRepositoryException(string message) : base(message)
        { }
    }
}