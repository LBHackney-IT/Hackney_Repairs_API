using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HackneyRepairs.Models;

namespace HackneyRepairs.Repository
{
    public static class MobileReportsRepository
    {
        private static readonly string ServerName = Environment.GetEnvironmentVariable("MobileReportsServerName");
        private static readonly string MountedPath = Environment.GetEnvironmentVariable("MobileReportsMountedPath");

        public static IEnumerable<MobileReport> GetReports(string servitorReference)
        {
            EnsureResourceAccess();

            // If there are mobile reports, the first one will be in the Processed directory. 
            // Subsequent reports will be stored in the Unprocessed directory
            var processedReport = new FileInfo(MountedPath + "Processed/" + $"Works Order_{servitorReference}.pdf");
            if (!processedReport.Exists)
            {
                return new List<MobileReport>();
            }

            var results = new List<MobileReport> { BuildMobileReportResponse(processedReport) };
           
            var unprocessedReports = Directory.GetFiles(MountedPath + "Unprocessed/", $"*{servitorReference}*").ToList();
            if (unprocessedReports.Any())
            {
                foreach (string reportUri in unprocessedReports)
                {
                    var report = new FileInfo(reportUri);
                    if (report.Exists)
                    {
                        results.Add(BuildMobileReportResponse(report));
                    }
                }
            }

            return results;
        }

        static void EnsureResourceAccess()
        {
            var directory = new DirectoryInfo(MountedPath);
            if (!directory.Exists)
            {
                throw new MobileReportsConnectionException();
            }
        }

        static MobileReport BuildMobileReportResponse(FileInfo report)
        {
            var mountpoint = (MountedPath.Split('/').Where(portion => !string.IsNullOrWhiteSpace(portion))).FirstOrDefault();
            var reportFormattedUri = report.FullName.Replace(mountpoint, "\\" + ServerName).Replace("/", @"\");
            return new MobileReport
            {
                ReportUri = reportFormattedUri,
                Date = report.CreationTime
            };
        }
    }

    public class MobileReportsConnectionException : Exception { }
}
