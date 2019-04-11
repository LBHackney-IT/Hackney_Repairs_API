using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HackneyRepairs.Actions;
using HackneyRepairs.Interfaces;

namespace HackneyRepairs.Factories
{
    public class HackneyCautionaryContactServiceFactory
    {
        internal IHackneyCautionaryContactService build(IUhtRepository uhtRepository, IUHWWarehouseRepository uHWWarehouseRepository, ILoggerAdapter<CautionaryContactActions> cautionaryContactLoggerAdapter)
        {
            throw new NotImplementedException();
        }
    }
}
