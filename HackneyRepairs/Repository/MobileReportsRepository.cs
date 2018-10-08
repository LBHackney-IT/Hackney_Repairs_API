using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HackneyRepairs.Repository
{
    public static class MobileReportsRepository
    {
        private static string ServerName = Environment.GetEnvironmentVariable("MobileReportsServerName");
        private static string MountedPath = Environment.GetEnvironmentVariable("MobileReportsMountedPath");

        public static IEnumerable<string> GetReports(string servitorReference)
        {
            EnsureResourceAccess();
            
            // If there are mobile reports, the first one will be in the Processed directory. 
            // Subsequent reports will be stored in the Unprocessed directory
            var processedReport = new FileInfo(MountedPath + "Processed/" + $"Works Order_{servitorReference}.pdf");
            if (!processedReport.Exists)
            {
                return new List<string>();
            }

            var unprocessedReports = Directory.GetFiles(MountedPath + "Unprocessed/", $"*{servitorReference}*").ToList();
            if (unprocessedReports.Any())
            {
                unprocessedReports.Add(processedReport.FullName);
                return FormatReportStrings(unprocessedReports);
            }

            var singleProcessedReportInList = new List<string> { processedReport.FullName };
            return FormatReportStrings(singleProcessedReportInList);
        }

        static IEnumerable<string> FormatReportStrings(IEnumerable<string> mobileReports)
        {
            var mountpoint = Environment.GetEnvironmentVariable("SystemMountpointName");
            var formattedMobileReportStrings = new List<string>();
            foreach (string report in mobileReports.ToList())
            {
                formattedMobileReportStrings.Add(report.Replace("/", @"\").Replace(mountpoint, "\\" + ServerName));
            }
            return formattedMobileReportStrings;
        }

        static void EnsureResourceAccess()
        {
            var directory = new DirectoryInfo(MountedPath);
            if (!directory.Exists)
            {
                throw new MobileReportsConnectionException();
            }
        }
    }

    public class MobileReportsConnectionException : Exception { }
}
