using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using HackneyRepairs.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using Dapper;
using HackneyRepairs.Models;
using System.Linq;

namespace HackneyRepairs.Repository
{
    public class UhwRepository : IUhwRepository
    {
        private UhwDbContext _context;
        private string environmentDbWord;
        private ILoggerAdapter<UhwRepository> _logger;
        private const string DefaultNoteType = "GLO_GEN";

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
                                @WorkOrderReference AS WorkOrderReference,
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
                                work_order.wo_ref = @WorkOrderReference";
                    var notes = connection.Query<Note>(query, new { WorkOrderReference = workOrderReference });
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
                            note.NDate > @CutoffTime AND note.NoteID > @NoteId
                            AND note.KeyObject IN (@NoteTarget) 
                        ORDER BY NoteID";

                    var queryParameters = new
                    {
                        NoteId = noteId,
                        CutoffTime = GetCutoffTime(),
                        NoteTarget = noteTarget
                    };
                    var notes = connection.Query<Note>(query, queryParameters);
                    return notes;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new UhwRepositoryException();
            }
        }

        public string GetUHUsernameByEmail(string lbhEmail)
        {
            string uhusername = string.Empty;
            _logger.LogInformation($"Retrieving UHUsername for {lbhEmail}");
            try
            {
                using (SqlConnection connection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    connection.Open();
                    //_context.Database.ExecuteSqlCommand(commandString, new { uhusername = UHUsername });
                    string query = "SELECT [User_ID] FROM[uhwdev].[dbo].[W2User] where [EMail] = @LBHEmail";
                    uhusername = connection.QuerySingle<string>(query, new { LBHEmail = lbhEmail });
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

            return uhusername;
        }    

        public async Task AddNote(FullNoteRequest note)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@NoteType", DefaultNoteType);
            parameters.Add("@NoteText", note.Text);
            parameters.Add("@rmworder_sid", note.WorkOrderSid);
            parameters.Add("@UserId", Environment.GetEnvironmentVariable("UHUsername"));
            try
            {
                using (var connection = new SqlConnection(_context.Database.GetDbConnection().ConnectionString))
                {
                    connection.Query("ws_AddNoteToOrder", parameters, commandType: CommandType.StoredProcedure); 
                }
            }
            catch (Exception ex)
            {
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

    public class UhwRepositoryException : Exception { }
}