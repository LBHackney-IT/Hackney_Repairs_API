using HackneyRepairs.Interfaces;
using HackneyRepairs.Models;
using HackneyRepairs.Repository;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace HackneyRepairs.Tests.Repository
{
    [Collection("Universal Housing")]
    public class UhwRepositoryTests
    {
        private UniversalHousingSimulator<UhwDbContext> _simulator;
        private ILoggerAdapter<UhwRepository> _logger;

        public UhwRepositoryTests()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "test");

            _logger = new Mock<ILoggerAdapter<UhwRepository>>().Object;
            _simulator = new UniversalHousingSimulator<UhwDbContext>();

            _simulator.Reset();
        }
    }
}
