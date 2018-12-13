using System;
using HackneyRepairs.DbContext;
using HackneyRepairs.Tests;
using Xunit;

namespace Fact
{
    class WhenUniversalHousingIsRunningAttribute : FactAttribute
    {
        public WhenUniversalHousingIsRunningAttribute()
        {
            var running = UniversalHousingSimulator<UhtDbContext>.IsRunning();
            if (!running) 
            {
                Skip = "Universal Housing Simulator must be running to run repository tests";
            }
        }
    }
}