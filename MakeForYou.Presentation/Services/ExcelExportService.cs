using ClosedXML.Excel;
using FUNews.BusinessLogic.Entities;

namespace FUNews.Presentation.Services
{
    public class ExcelExportService
    {
        public byte[] ExportChangeAudit(
            IEnumerable<NewsArticle> articles,
            List<SystemAccount> accounts)
        {
            using var workbook = new XLWorkbook();

            CreateAuditSheet(workbook, articles, accounts);
            CreateStatisticsSheet(workbook, articles);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        private void CreateAuditSheet(
            XLWorkbook workbook,
            IEnumerable<NewsArticle> articles,
            List<SystemAccount> accounts)
        {
            var ws = workbook.Worksheets.Add("Change Audit");

            // Header
            ws.Cell(1, 1).Value = "Title";
            ws.Cell(1, 2).Value = "Created By";
            ws.Cell(1, 3).Value = "Created Date";
            ws.Cell(1, 4).Value = "Last Edited By";
            ws.Cell(1, 5).Value = "Last Modified";
            ws.Cell(1, 6).Value = "Status";

            ws.Range(1, 1, 1, 6).Style
                .Font.SetBold()
                .Fill.SetBackgroundColor(XLColor.LightGray);

            int row = 2;

            foreach (var a in articles)
            {
                ws.Cell(row, 1).Value = a.NewsTitle;
                ws.Cell(row, 2).Value = a.CreatedBy?.AccountName;
                ws.Cell(row, 3).Value = a.CreatedDate;
                ws.Cell(row, 4).Value =
                    a.UpdatedById != null
                        ? accounts.FirstOrDefault(x => x.AccountId == a.UpdatedById)?.AccountName
                        : null;
                ws.Cell(row, 5).Value = a.ModifiedDate;
                ws.Cell(row, 6).Value = a.NewsStatus == true ? "Active" : "Inactive";

                row++;
            }

            ws.Columns().AdjustToContents();
        }

        private void CreateStatisticsSheet(
            XLWorkbook workbook,
            IEnumerable<NewsArticle> articles)
        {
            var ws = workbook.Worksheets.Add("Statistics");

            int total = articles.Count();
            int active = articles.Count(a => a.NewsStatus == true);
            int inactive = total - active;
            int modified = articles.Count(a => a.ModifiedDate != null);

            ws.Cell(1, 1).Value = "Metric";
            ws.Cell(1, 2).Value = "Value";

            ws.Range(1, 1, 1, 2).Style
                .Font.SetBold()
                .Fill.SetBackgroundColor(XLColor.LightGray);

            ws.Cell(2, 1).Value = "Total Articles";
            ws.Cell(2, 2).Value = total;

            ws.Cell(3, 1).Value = "Active Articles";
            ws.Cell(3, 2).Value = active;

            ws.Cell(4, 1).Value = "Inactive Articles";
            ws.Cell(4, 2).Value = inactive;

            ws.Cell(5, 1).Value = "Modified Articles";
            ws.Cell(5, 2).Value = modified;

            ws.Columns().AdjustToContents();
        }
    }
}
