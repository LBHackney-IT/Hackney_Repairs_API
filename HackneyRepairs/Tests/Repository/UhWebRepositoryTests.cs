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
    public class UhWebRepositoryTests
    {
        private UniversalHousingSimulator<UhwDbContext> _simulator;
        private ILoggerAdapter<UhWebRepository> _logger;

        public UhWebRepositoryTests()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "test");

            _logger = new Mock<ILoggerAdapter<UhWebRepository>>().Object;
            //_simulator = new UniversalHousingSimulator<UhWebDbContext>();

            //_simulator.Reset();
        }
    }
}
