﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KeyFaxService;

namespace HackneyRepairs.Interfaces
{
    public interface IHackneyKeyFaxServiceRequestBuilder
    {
        string GetStartUpXML(string returnURL);
        //StartupRequest BuildNewStartupRequest();   
    }
}
