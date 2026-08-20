using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BMW_Special_Tools.Models;
using BMW_Special_Tools.Data;
using QRCoder;
using System.Text.Json;
using BMW_Special_Tools.ViewModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
namespace BMW_Special_Tools.Controllers;

public class HomeController : Controller
{

    private readonly AppDbContext _context;

    // Inject the database context 
    public HomeController(AppDbContext context)
    {
        _context = context;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public IActionResult Index()
    {
        // Fetch all tools from the database
        var toolsList = _context.Tools.ToList();

        // Pass the data to the view
        return View(toolsList);
    }
    [HttpGet]
    public IActionResult Printing(string? searchTerm)
    {
        // 1. Start with the base query
        var query = _context.Tools.AsQueryable();

        // 2. Apply search filter if a term exists
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(t =>
                t.EnglishName.Contains(searchTerm) ||
                t.Number.Contains(searchTerm) ||
                t.Bmwnumber.Contains(searchTerm));
        }

        var toolsList = query.ToList();
        List<ToolDetailsViewModel> result = new List<ToolDetailsViewModel>();

        foreach (var tool in toolsList)
        {
            string qrPayload = $" Number:{tool.Number} \n\n BMWNumber: {tool.Bmwnumber} \n\n KitNumber: {tool.KitNumber}\n\n BMWKitNumber: {tool.BmwkitNumber}\n\n EName: {tool.EnglishName}\n\n AName: {tool.ArabicName}";
            result.Add(new ToolDetailsViewModel
            {
                ToolEName = tool.EnglishName,
                QrCodeImage = GenerateQrCodeBase64(qrPayload)
            });
        }

        // 3. Pass the search term back to the view so the input box doesn't clear
        ViewData["SearchTerm"] = searchTerm;
        return View(result);
    }

    [HttpPost]
    public IActionResult DownloadStickersPdf([FromQuery] string? searchTerm)
    {
        // 1. Apply the EXACT same filtering logic for the PDF
        var query = _context.Tools.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(t =>
                t.EnglishName.Contains(searchTerm) ||
                t.Number.Contains(searchTerm) ||
                t.Bmwnumber.Contains(searchTerm));
        }

        var toolsList = query.ToList();

        if (!toolsList.Any())
        {
            return BadRequest("No tools found to print.");
        }

        // 2. Generate PDF based ONLY on the filtered toolsList
        var pdfBytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Content().Grid(grid =>
                {
                    grid.Columns(4);
                    grid.Spacing(15);

                    foreach (var tool in toolsList)
                    {
                        string qrPayload = $" Number:{tool.Number} \n\n BMWNumber: {tool.Bmwnumber} \n\n KitNumber: {tool.KitNumber}\n\n BMWKitNumber: {tool.BmwkitNumber}\n\n EName: {tool.EnglishName}\n\n AName: {tool.ArabicName}";
                        byte[] qrBytes = GenerateQrCodeBytes(qrPayload);

                        grid.Item().Column(col =>
                        {
                            col.Item().AlignCenter().Width(100).Image(qrBytes);
                            col.Item().AlignCenter().Text(tool.Number).SemiBold();
                        });
                    }
                });
            });
        }).GeneratePdf();

        return File(pdfBytes, "application/pdf", "Tool_Stickers.pdf");
    }
    private byte[] GenerateQrCodeBytes(string payload)
    {
        using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
        {
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
            using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
            {
                return qrCode.GetGraphic(20);
            }
        }
    }
    [Route("Tools/Details/{qrTag}")]
    public IActionResult Details(string qrTag)
    {
        // 1. Find the tool using the unique QrTag
        var tool = _context.Tools.FirstOrDefault(t => t.QrTag == qrTag);
        if (tool == null)
        {
            return NotFound();
        }

        string payloadUrl = $" Number:{tool.Number} \n\n BMWNumber: {tool.Bmwnumber} \n\n KitNumber: {tool.KitNumber}\n\n BMWKitNumber: {tool.BmwkitNumber}\n\n EName: {tool.EnglishName}\n\n AName: {tool.ArabicName}";

        // 3. Generate the QR Code entirely in memory (No saving to disk!)
        using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
        {
            // ECCLevel.Q gives a good balance of error correction so it scans easily even if slightly damaged
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(payloadUrl, QRCodeGenerator.ECCLevel.Q);

            // PngByteQRCode is cross-platform and works perfectly on Linux
            using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
            {
                // 20 is the pixel size per module (block)
                byte[] qrCodeImage = qrCode.GetGraphic(20);

                // Convert the byte array to a Base64 string so HTML can render it directly
                string base64 = Convert.ToBase64String(qrCodeImage);
                ViewBag.QrCodeImage = $"data:image/png;base64,{base64}";
            }
        }

        return View(tool);
    }

    // --- NEW HELPER FUNCTION ---
    private string GenerateQrCodeBase64(string payload)
    {
        using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
        {
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);

            using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
            {
                byte[] qrCodeImage = qrCode.GetGraphic(20);
                string base64 = Convert.ToBase64String(qrCodeImage);
                return $"data:image/png;base64,{base64}";
            }
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
