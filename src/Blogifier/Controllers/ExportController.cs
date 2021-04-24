using System.Data;
using System;
using Blogifier.Core.Providers;
using Blogifier.Core.Data;
using Blogifier.Shared;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using ClosedXML;
using iTextSharp;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using ClosedXML.Excel;

namespace Blogifier.Controllers
{
    [Route("api/[controller]")]
	[ApiController]
    public class ExportController : ControllerBase
    {
        private readonly INewsletterProvider _newsletterProvider;
        private readonly IHttpContextAccessor _httpContextAccessor; 
        private readonly string fixedPath = "Downloads/";

		public ExportController(INewsletterProvider newsletterProvider, IHttpContextAccessor contextAccessor)
		{
			_newsletterProvider = newsletterProvider;
            _httpContextAccessor = contextAccessor;
		}

        [Authorize]
		[HttpGet("emails/{fileType}")]
		public async Task<IActionResult> ExportToFileType(string fileType)
		{
			var emails = await _newsletterProvider.GetSubscribers();

            var compare = fileType.ToLower();

            switch (compare) {
                case "pdf":
                    int pdfRowIndex = 1;  
  
                    string filename = "subscribers_export" + DateTime.Now.ToString() + ".pdf";  
                    // string filepath = Path.Combine(_httpContextAccessor.HttpContext.Request.Host.Value + "/" + fixedPath, filename); 
                    // FileStream fs = new FileStream(filepath, FileMode.Create);  
                    using (MemoryStream stream = new MemoryStream()) {
                        Document document = new Document(PageSize.A4, 5f, 5f, 10f, 10f);  
                        PdfWriter writer = PdfWriter.GetInstance(document, stream);  
                        document.Open();  
                
                        Font font1 = FontFactory.GetFont(FontFactory.COURIER_BOLD, 10);  
                        Font font2 = FontFactory.GetFont(FontFactory.COURIER, 8);  
                
                        float[] columnDefinitionSize = { 2F, 5F, 2F, 5F };  
                        PdfPTable table;  
                        PdfPCell cell;  
                
                        table = new PdfPTable(columnDefinitionSize)  
                        {  
                            WidthPercentage = 100  
                        };  
                
                        cell = new PdfPCell  
                        {  
                            BackgroundColor = new BaseColor(0xC0, 0xC0, 0xC0)  
                        };  
                
                        table.AddCell(new Phrase("SubscriberId", font1));  
                        table.AddCell(new Phrase("Email", font1));  
                        table.AddCell(new Phrase("IP", font1));  
                        table.AddCell(new Phrase("Country", font1));
                        table.AddCell(new Phrase("Region", font1));
                        table.AddCell(new Phrase("Blog", font1));  
                        table.HeaderRows = 1;  
                
                        foreach (var data in emails)  
                        {  
                            table.AddCell(new Phrase(data.Id.ToString(), font2));  
                            table.AddCell(new Phrase(data.Email.ToString(), font2));  
                            table.AddCell(new Phrase(data.Ip.ToString(), font2));  
                            table.AddCell(new Phrase(data.Country.ToString(), font2));  
                            table.AddCell(new Phrase(data.Region.ToString(), font2));  
                            table.AddCell(new Phrase(data.Blog.ToString(), font2));  
                            pdfRowIndex++;  
                        }  
                
                        document.Add(table);  
                        document.Close();  
                        document.CloseDocument();  
                        document.Dispose();  
                        writer.Close();  
                        writer.Dispose();  
                        return File(stream.ToArray(), "application/pdf", filename);
                    }
                case "excel":
                    using (XLWorkbook workBook = new XLWorkbook())  
                    { 
                        workBook.AddWorksheet(GetEmails(emails));
                        using (MemoryStream stream = new MemoryStream())  
                        {  
                            workBook.SaveAs(stream);  
                            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Subscribers.xlsx");  
                        }  
                    }  
                default:
                    break;
            }
            return Ok();
		}

        private DataTable GetEmails(List<Subscriber> subscribers)  
        {  
    
            DataTable dtSubs = new DataTable("Subscribers");  
            dtSubs.Columns.AddRange(new DataColumn[6] { new DataColumn("SubscriberID"),  
                                            new DataColumn("Email"),  
                                            new DataColumn("IP"),  
                                            new DataColumn("Country"),
                                            new DataColumn("Region"),
                                            new DataColumn("Blog") });  
            foreach (var s in subscribers)  
            {  
                dtSubs.Rows.Add(s.Id, s.Email.ToString(), s.Ip.ToString(), s.Country.ToString(), s.Region.ToString(), s.Blog.ToString());  
            }  
    
            return dtSubs;  
        }  
    }
}
