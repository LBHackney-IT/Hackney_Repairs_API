using System;
using Microsoft.EntityFrameworkCore;

namespace HackneyRepairs.DbContext
{
    public partial class DRSDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public DRSDbContext(DbContextOptions<DRSDbContext> options) : base(options)
        {
        }
    }   
}
