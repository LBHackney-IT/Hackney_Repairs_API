﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HackneyRepairs.Actions;
using HackneyRepairs.Interfaces;
using HackneyRepairs.Tests;

namespace HackneyRepairs.Factories
{
    public class HackneyCautionaryContactServiceFactory
    {
        internal IHackneyCautionaryContactService build(IUHWWarehouseRepository uHWWarehouseRepository, ILoggerAdapter<CautionaryContactActions> logger)
        {
            if (TestStatus.IsRunningInTests == false)
            {
                return new Services.HackneyCautionaryContactService(uHWWarehouseRepository, logger);
            }
            else
            {
                return new Services.FakeCautionaryContactService();
            }
        }
    }
}
