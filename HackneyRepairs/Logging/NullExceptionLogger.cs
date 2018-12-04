using System;
using HackneyRepairs.Interfaces;

namespace HackneyRepairs.Logging
{
    public class NullExceptionLogger : IExceptionLogger
    {
        public void CaptureException(Exception exception = null)
        {
        }
    }
}