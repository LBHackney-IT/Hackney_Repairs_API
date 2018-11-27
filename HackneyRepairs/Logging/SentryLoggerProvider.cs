﻿using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace HackneyRepairs.Logging
{
    public class SentryLoggerProvider : ILoggerProvider
    {
        private readonly string _url;
        private readonly string _environment;
        private readonly ConcurrentDictionary<string, SentryLogger> _loggers = new ConcurrentDictionary<string, SentryLogger>();
        public SentryLoggerProvider(string url, string environment)
        {
            _url = url;
            _environment = environment;
        }
        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, name => new SentryLogger(name, _url, _environment));
        }
        public void Dispose()
        {
            _loggers.Clear();
        }
    }
}