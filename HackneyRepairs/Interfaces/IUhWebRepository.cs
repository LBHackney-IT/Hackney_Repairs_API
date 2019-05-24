using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HackneyRepairs.DTOs;
using HackneyRepairs.Models;

namespace HackneyRepairs.Interfaces
{
    public interface IUhWebRepository
    {       
        string GenerateUHSession(string UHUsername);    
    }
}
