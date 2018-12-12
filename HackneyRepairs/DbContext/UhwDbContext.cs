using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HackneyRepairs.Models;
using Microsoft.EntityFrameworkCore;

namespace HackneyRepairs
{
    public partial class UhwDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public UhwDbContext(DbContextOptions<UhwDbContext> options) : base(options)
        {
        }
    }
}