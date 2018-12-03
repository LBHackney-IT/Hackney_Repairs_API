using System;

namespace HackneyRepairs.Interfaces
{
    public interface IExceptionLogger
    {
        void CaptureException(Exception exception);
    }
}
