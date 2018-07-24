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
using Microsoft.EntityFrameworkCore.Query.Internal;
using System.Data.SqlClient;
using System.Configuration;
using HackneyRepairs.Entities;
using Dapper;

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

		public async Task<bool> GetMaintainableFlag(string propertyReference)
		{
			bool? notMaintainableFlag = null;
			_logger.LogInformation($"Getting the maintainable flag from UHT for {propertyReference}");
			string CS = Environment.GetEnvironmentVariable("UhtDb");
			if (CS == null)
			{
				CS = ConfigurationManager.ConnectionStrings["UhtDb"].ConnectionString;
			}
			try
			{
				using (SqlConnection con = new SqlConnection(CS))
				{
					string sql = "SELECT [no_maint] FROM [property] where [prop_ref]='" + propertyReference + "'";
					SqlCommand cmd = new SqlCommand(sql, con);
					con.Open();
					SqlDataReader dr = cmd.ExecuteReader();
					while (dr.Read())
					{
						notMaintainableFlag = dr.GetBoolean(0);
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
			}
			return !notMaintainableFlag.Value;
		}

		public async Task<DrsOrder> GetWorkOrderDetails(string workOrderReference)
		{
			DrsOrder drsOrder = new DrsOrder();
			_logger.LogInformation($"Getting the work order details from UHT for {workOrderReference}");
			try
			{
				using (var command = _context.Database.GetDbConnection().CreateCommand())
				{
					_context.Database.OpenConnection();
					command.CommandText = @"select	created createdDate,
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
                    where wo_ref='" + workOrderReference + "'";

					command.CommandType = CommandType.Text;

					using (var reader = await command.ExecuteReaderAsync())
					{
						if (reader != null & reader.HasRows)
						{
							drsOrder = reader.MapToList<DrsOrder>().FirstOrDefault();
						}
					}

					command.CommandText = @"select  rmtask.job_code,
                convert(varchar(50), task_text) comments,
                est_cost itemValue,
                    est_units itemqty,
                u_smv smv,
                    rmjob.trade
                    from rmtask inner join rmjob on rmtask.job_code = rmjob.job_code

                where wo_ref = '" + workOrderReference + "'";
					command.CommandType = CommandType.Text;

					using (var reader = await command.ExecuteReaderAsync())
					{
						drsOrder.Tasks = reader.MapToList<DrsTask>();
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
			}
			return drsOrder;
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
					//Step 1
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
					//Step 2
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
						//update table u_sentToAppointmentSys directly
						if (visit_sid != 0)
						{
							_logger.LogInformation($"Blocking UH triggers for order {workOrderReference}");
							string commandString = @"insert into u_sentToAppointmentSys (wo_ref,orderId,bookingId,dateSent,appointmentStart,appointmentEnd,uhVisitID) ";
							commandString += @"values ('" + workOrderReference + "'," + orderId + "," + bookingId + ", convert(datetime,'" + DateTime.Now + "',105),convert(datetime,'" + startDate + "',105),convert(datetime,'" + endDate + "',105)," + visit_sid + ")";
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


		public async Task<WorkOrderEntity> GetWorkOrder(string workOrderReference)
		{
			WorkOrderEntity workOrder = new WorkOrderEntity();
			try
			{
				using (var connection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
				{
					string query = "SELECT * FROM rmworder WHERE wo_ref = '" + workOrderReference + "'";

					workOrder = connection.Query<WorkOrderEntity>(query).FirstOrDefault();
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
                throw new UhtRepositoryException();
			}
			return workOrder;
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
	}
}


