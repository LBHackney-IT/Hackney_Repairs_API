﻿using System;
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
	public class UhWebRepository : IUhWebRepository
    {
		private UhWebDbContext _context;
		private ILoggerAdapter<UhWebRepository> _logger;
		public UhWebRepository(UhWebDbContext context, ILoggerAdapter<UhWebRepository> logger)
		{
			_context = context;
			_logger = logger;
		}  

		public string GenerateUHSession(string UHUsername)
		{
            string sessionToken = string.Empty;
			_logger.LogInformation($"creating session token for {UHUsername}");
			try
			{
                using (SqlConnection connection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    string commandString = $@"if exists (SELECT ws_userid FROM ws_user where ws_username = @uhusername) 
                                        begin
                                            update ws_user set current_session_token=NEWID(), last_activity_date=getdate() where ws_username = @uhusername
                                        end
                                    else
                                        begin
                                            insert into ws_user (ws_username, ws_access_status, uh_user_login, current_session_token, last_activity_date) 
                                            values (@uhusername, 'E', @uhusername, NEWID(), getdate())
                                        end";
                    _context.Database.ExecuteSqlCommand(commandString, new { uhusername = UHUsername });

                    string query = "SELECT current_session_token FROM ws_user where ws_username = @uhusername";
                    sessionToken = connection.Query<string>(query, new { uhusername = UHUsername }).FirstOrDefault();                   
                }
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
			}
			finally
			{
				_context.Database.CloseConnection();
			}

			return sessionToken;
		}	
    }

    public class UhWebRepositoryException : Exception { }
}