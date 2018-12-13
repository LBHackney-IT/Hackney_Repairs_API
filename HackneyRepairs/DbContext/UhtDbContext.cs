using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HackneyRepairs.Models;
using Microsoft.EntityFrameworkCore;

namespace HackneyRepairs.DbContext
{
    public partial class UhtDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public UhtDbContext(DbContextOptions<UhtDbContext> options) : base(options)
        {
        }
    }
}
