﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task<PropertySummary[]> GetPropertyListByPostCode(string postcode)
        {
            List<PropertySummary> properties = new List<PropertySummary>();
            _logger.LogInformation($"Getting properties for postcode {postcode}");
            string CS = Environment.GetEnvironmentVariable("UhWarehouseDb");
            if (CS == null)
            {
                CS = ConfigurationManager.ConnectionStrings["UhWarehouseDb"].ConnectionString;
            }
            try
            {
                using (SqlConnection con = new SqlConnection(CS))
                {
                    string sql = "select short_address as 'ShortAddress', post_code as 'PostCodeValue', prop_ref as 'PropertyReference' from property where level_code = 7 and post_code = '" + postcode + "'";
                    SqlCommand cmd = new SqlCommand(sql, con);
                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    if (dr != null & dr.HasRows)
                    {
                        properties = dr.MapToList<PropertySummary>();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return properties.ToArray();
        }

        public async Task<PropertyDetails> GetPropertyDetailsByReference(string reference)
        {
            var property = new PropertyDetails();
            _logger.LogInformation($"Getting details for property {reference}");
            string CS = Environment.GetEnvironmentVariable("UhWarehouseDb");
            if (CS == null)
            {
                CS = ConfigurationManager.ConnectionStrings["UhWarehouseDb"].ConnectionString;
            }
            try
            {
                using (SqlConnection con = new SqlConnection(CS))
                {
                    string sql = "select short_address as 'ShortAddress', post_code as 'PostCodeValue', ~no_maint as 'Maintainable', prop_ref as 'PropertyReference' from property where prop_ref = '" + reference + "'";
                    SqlCommand cmd = new SqlCommand(sql, con);
                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    if (dr != null & dr.HasRows)
                    {
                        property = dr.MapToList<PropertyDetails>().FirstOrDefault();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return property;
        }

        public Task<PropertyDetails> GetPropertyBlockByReference(string reference)
        {
            var property = new PropertyDetails();
            _logger.LogInformation($"Getting details for property {reference}");
            string CS = Environment.GetEnvironmentVariable("UhWarehouseDb");
            if (CS == null)
            {
                CS = ConfigurationManager.ConnectionStrings["UhWarehouseDb"].ConnectionString;
            }
            try
            {
                using (SqlConnection con = new SqlConnection(CS))
                {
                    string sql = "select short_address as 'ShortAddress', post_code as 'PostCodeValue', ~no_maint as 'Maintainable', prop_ref as 'PropertyReference' from property where prop_ref";
                    sql += " = (SELECT [u_block] FROM [property] where prop_ref = '" + reference + "')";
                    SqlCommand cmd = new SqlCommand(sql, con);
                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    if (dr != null & dr.HasRows)
                    {
                        property = dr.MapToList<PropertyDetails>().FirstOrDefault();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return Task.Run(() => property);
        }

        public Task<PropertyDetails> GetPropertyEstateByReference(string reference)
        {
            var property = new PropertyDetails();
            _logger.LogInformation($"Getting details for property {reference}");
            string CS = Environment.GetEnvironmentVariable("UhWarehouseDb");
            if (CS == null)
            {
                CS = ConfigurationManager.ConnectionStrings["UhWarehouseDb"].ConnectionString;
            }
            try
            {
                using (SqlConnection con = new SqlConnection(CS))
                {
                    string sql = "select short_address as 'ShortAddress', post_code as 'PostCodeValue', ~no_maint as 'Maintainable', prop_ref as 'PropertyReference' from property where prop_ref";
                    sql += " = (SELECT [u_estate] FROM [property] where prop_ref = '" + reference + "')";
                    SqlCommand cmd = new SqlCommand(sql, con);
                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    if (dr != null & dr.HasRows)
                    {
                        property = dr.MapToList<PropertyDetails>().FirstOrDefault();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return Task.Run(() => property);
        }
    }

    public class UHWWarehouseRepositoryException : Exception { }
}
