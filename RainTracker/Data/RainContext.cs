using Microsoft.EntityFrameworkCore;
using RainTracker.Models;
using System.Collections.Generic;

namespace RainTracker.Data
{
    public class RainContext : DbContext
    {
        public RainContext(DbContextOptions<RainContext> options) : base(options) { }

        public DbSet<RainRecord> RainRecords => Set<RainRecord>();
    }
}

