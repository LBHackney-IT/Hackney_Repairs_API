using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HackneyRepairs.Models;
using Microsoft.EntityFrameworkCore;

namespace HackneyRepairs.DbContext
{
    public partial class UhWebDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public UhWebDbContext(DbContextOptions<UhWebDbContext> options) : base(options)
        {
        }
    }
}
