using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MPB_PORTAL_WEB_APP.Data;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using OfficeOpenXml;
using CsvHelper;
using CsvHelper.Configuration;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Globalization;

namespace MPB_PORTAL_WEB_APP.Controllers
{
    [Authorize]
    public class StatisticsController : Controller
    {
        private readonly AppDbContext _context;

        public StatisticsController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var activitiesPerMonth = await _context.Activities
                .GroupBy(a => new { a.StartDate.Year, a.StartDate.Month })
                .Select(g => new { Month = g.Key.Month, Year = g.Key.Year, Total = g.Count() })
                .OrderBy(g => g.Year).ThenBy(g => g.Month)
                .ToListAsync();

            var caseStatuses = await _context.Cases
                .GroupBy(c => c.Status)
                .Select(g => new { Status = g.Key, Total = g.Count() })
                .ToListAsync();

            var mostActiveOrganizations = await _context.Reports
                .GroupBy(r => r.Report_Made_By)
                .Select(g => new { Organization = g.Key, Total = g.Count() })
                .OrderByDescending(g => g.Total)
                .Take(5)
                .ToListAsync();

            ViewBag.ActivitiesPerMonth = activitiesPerMonth;
            ViewBag.CaseStatuses = caseStatuses;
            ViewBag.MostActiveOrganizations = mostActiveOrganizations;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DownloadReport(string type)
        {
            var data = await _context.Reports.ToListAsync();

            if (type == "csv")
            {
                var stream = new MemoryStream();
                using (var writer = new StreamWriter(stream, leaveOpen: true))
                using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
                {
                    csv.WriteRecords(data);
                }
                stream.Position = 0;
                return File(stream, "text/csv", "reports.csv");
            }
            else if (type == "excel")
            {
                var stream = new MemoryStream();
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets.Add("Reports");
                    worksheet.Cells.LoadFromCollection(data, true);
                    package.Save();
                }
                stream.Position = 0;
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "reports.xlsx");
            }
            else if (type == "pdf")
            {
                var stream = new MemoryStream();
                Document doc = new Document();
                PdfWriter writer = PdfWriter.GetInstance(doc, stream);
                writer.CloseStream = false;

                doc.Open();
                doc.Add(new Paragraph("Reports Data"));
                doc.Add(new Paragraph(" ")); // Add spacing

                foreach (var report in data)
                {
                    doc.Add(new Paragraph($"Title: {report.Title}"));
                    doc.Add(new Paragraph($"Date: {report.DateOfReport}"));
                    doc.Add(new Paragraph($"Made By: {report.Report_Made_By}"));
                    doc.Add(new Paragraph(" ")); // Add spacing
                }

                doc.Close();
                stream.Position = 0;
                return File(stream, "application/pdf", "reports.pdf");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
