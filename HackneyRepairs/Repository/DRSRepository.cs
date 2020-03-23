using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using HackneyRepairs.DbContext;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Models;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;

namespace HackneyRepairs.Repository
{
	public class DRSRepository : IDRSRepository
	{
		private DRSDbContext _context;
		private ILoggerAdapter<DRSRepository> _logger;

		public DRSRepository(DRSDbContext context, ILoggerAdapter<DRSRepository> logger)
		{
			_context = context;
			_logger = logger;
		}

		public async Task<IEnumerable<DetailedAppointment>> GetAppointmentsByWorkOrderReference(string workOrderReference)
		{
			List<DetailedAppointment> appointments;
			_logger.LogInformation($"Getting appointment details from DRS for {workOrderReference}");

			try
			{
				using (var connection = new MySqlConnection(_context.Database.GetDbConnection().ConnectionString))
				{
			    	var query = @"SELECT
                                    jobs.Id,
                                    jobs.BeginDate,
                                    jobs.EndDate,
                                    jobs.Status,
                                    jobs.Outcome,
                                    jobs.AssignedWorker,
                                    s_worker.mobilephone AS Phonenumber,
                                    jobs.Priority,
                                    jobs.CreationDate,
                                    Comment,
                                    'DRS' AS SourceSystem
                                FROM ((
                                    SELECT
                                        s_job.NAME AS Id,
                                        s_job.GLOBALCURRENTTIMEWINDOW_START AS BeginDate,
                                        s_job.GLOBALCURRENTTIMEWINDOW_END AS EndDate,
                                        s_job.status AS Status,
                                        s_job.BD_TASK_COMPLETION_STAT AS Outcome,
                                        s_job.ASSIGNEDWORKERS AS AssignedWorker,
                                        s_job.priority AS Priority,
                                        s_job.CREATIONDATE AS CreationDate,
                                        s_job.BD_APPOINTMENT_REASON AS Comment
                                    FROM
                                        s_job
                                    WHERE
                                        s_job.NAME = @WorkOrderReference)
                                UNION (
                                    SELECT
                                        p_job.NAME AS Id,
                                        p_job.GLOBALCURRENTTIMEWINDOW_START AS BeginDate,
                                        p_job.GLOBALCURRENTTIMEWINDOW_END AS EndDate,
                                        p_job.status AS Status,
                                        p_job.BD_TASK_COMPLETION_STAT AS Outcome,
                                        p_job.ASSIGNEDWORKERS AS AssignedWorker,
                                        p_job.priority AS Priority,
                                        p_job.CREATIONDATE AS CreationDate,
                                        p_job.BD_APPOINTMENT_REASON AS Comment
                                    FROM
                                        p_job
                                    WHERE
                                        p_job.NAME = @WorkOrderReference)) AS jobs
                                        
                                left JOIN s_worker ON jobs.AssignedWorker = s_worker.name";
                    appointments = connection.Query<DetailedAppointment>(query, new { WorkOrderReference = workOrderReference }).ToList();
				}

				return appointments;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				throw new DrsRepositoryException(ex.Message);
			}
		}

		public async Task<DetailedAppointment> GetLatestAppointmentByWorkOrderReference(string workOrderReference)
		{
			IEnumerable<DetailedAppointment> lAppointments;
			try
            {
                _logger.LogInformation($"Getting current appointment details from DRS for {workOrderReference}");
                lAppointments = await GetAppointmentsByWorkOrderReference(workOrderReference);
                DetailedAppointment app = lAppointments.OrderByDescending(a => a.CreationDate).FirstOrDefault();
                return app;
            } 
            catch (Exception ex) 
            {
                _logger.LogError(ex.Message);
                throw new DrsRepositoryException(ex.Message); 
			}
		}
	}

    public class DrsRepositoryException : Exception
    {
        public DrsRepositoryException() { }
        public DrsRepositoryException(string message) : base(message)
        { }
    }  
}
