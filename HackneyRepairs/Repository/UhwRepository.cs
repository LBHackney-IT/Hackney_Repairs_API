﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using HackneyRepairs.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using HackneyRepairs.Entities;
using Dapper;
using HackneyRepairs.Models;

namespace HackneyRepairs.Repository
{
    public class UhwRepository : IUhwRepository
    {
        private UhwDbContext _context;
        private string environmentDbWord;

        private ILoggerAdapter<UhwRepository> _logger;
        public UhwRepository(UhwDbContext context, ILoggerAdapter<UhwRepository> logger)
        {
            _context = context;
            _logger = logger;

            switch (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"))
            {
                case "Production":
                    environmentDbWord = "live";
                    break;
                case "Test":
                    environmentDbWord = "live";
                    break;
                default:
                    environmentDbWord = "dev";
                    break;
            }
        }

        public async Task AddOrderDocumentAsync(string documentType, string workOrderReference, int workOrderId, string processComment)
        {
            _logger.LogInformation($"Starting process to add repair request document to UH for work order {workOrderReference}");
            try
            {
                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    _context.Database.OpenConnection();
                    command.CommandText = "usp_StartHackneyProcessV2";
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter
                    {
                        DbType = DbType.String,
                        ParameterName = "@docTypeCode",
                        Value = documentType
                    });
                    command.Parameters.Add(new SqlParameter
                    {
                        DbType = DbType.String,
                        ParameterName = "@WorkOrderRef",
                        Value = workOrderReference
                    });
                    command.Parameters.Add(new SqlParameter
                    {
                        DbType = DbType.Int32,
                        ParameterName = "@WorkOrderId",
                        Value = workOrderId
                    });
                    command.Parameters.Add(new SqlParameter
                    {
                        DbType = DbType.String,
                        ParameterName = "@ProcessComment",
                        Value = processComment
                    });
                    await command.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new UhwRepositoryException();
            }
            finally
            {
                _context.Database.CloseConnection();
            }
        }

        public async Task<IEnumerable<Note>> GetNotesByWorkOrderReference(string workOrderReference)
        {
            _logger.LogInformation($"Getting notes for {workOrderReference}");
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
                                uhw{environmentDbWord}.dbo.W2ObjectNote AS note
                            INNER JOIN 
                                uht{environmentDbWord}.dbo.rmworder AS work_order
                                ON note.KeyNumb = work_order.rmworder_sid
                            WHERE 
                                work_order.wo_ref = '{workOrderReference}'";
                    var notes = connection.Query<Note>(query);
                    return notes;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new UhtRepositoryException();
            }
        }

        public async Task<IEnumerable<Note>> GetNoteFeed(int noteId, string noteTarget, int size)
        {
            try
            {
                using (var connection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                   
                    _logger.LogInformation($"Getting up to {size} notes with an id > {noteId}");

                    var query = $@"set dateformat ymd;
                        SELECT TOP {size}
                            LTRIM(RTRIM(work_order.wo_ref)) AS WorkOrderReference,
                            note.NDate AS LoggedAt,
                            note.UserID AS LoggedBy,
                            note.NoteText AS [Text],
                            note.NoteID AS NoteId
                        FROM 
                            uhw{environmentDbWord}.dbo.W2ObjectNote AS note
                        INNER JOIN
                            uht{environmentDbWord}.dbo.rmworder AS work_order ON note.KeyNumb = work_order.rmworder_sid
                        WHERE
                            note.NDate > '{GetCutoffTime()}' AND note.NoteID > {noteId}
                            AND note.KeyObject IN ('{noteTarget}') 
                        ORDER BY NoteID";
                    var notes = connection.Query<Note>(query);
                    return notes;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new UhwRepositoryException();
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
    }

    public class UhwRepositoryException : Exception {}
}


