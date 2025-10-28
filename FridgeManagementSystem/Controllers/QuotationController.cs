using Microsoft.AspNetCore.Mvc;
using System;

namespace FridgeManagementSystem.Controllers
{
    public class QuotationController : Controller
    {
        private readonly FridgeManagementSystemContext _context;

        public QuotationController(FridgeManagementSystemContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var quotations = await _context.Quotations
                .Include(q => q.RFQ)
                .Include(q => q.Supplier)
                .OrderByDescending(q => q.QuoteDate)
                .ToListAsync();
            return View(quotations);
        }

        public async Task<IActionResult> Review(int id)
        {
            var quotation = await _context.Quotations
                .Include(q => q.RFQ)
                .Include(q => q.Supplier)
                .FirstOrDefaultAsync(q => q.QuotationId == id);

            if (quotation == null)
            {
                return NotFound();
            }

            return View(quotation);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var quotation = await _context.Quotations.FindAsync(id);
            if (quotation == null)
            {
                return NotFound();
            }

            // Convert string to enum
            if (Enum.TryParse<QuoteStatus>(status, true, out var parsedStatus))
            {
                quotation.Status = parsedStatus;
                await _context.SaveChangesAsync();
            }
            else
            {
                // Optional: handle invalid status string
                ModelState.AddModelError("", "Invalid status value.");
                return View(quotation);
            }

            return RedirectToAction(nameof(Index));
        }

        //public async Task<IActionResult> UpdateStatus(int id, string status)
        //{
        //    var quotation = await _context.Quotations.FindAsync(id);
        //    if (quotation == null)
        //    {
        //        return NotFound();
        //    }

        //    quotation.Status = status;
        //    await _context.SaveChangesAsync();

        //    return RedirectToAction(nameof(Index));
        //}
        //public IActionResult Index()
        //{
        //    return View();
        //}
    }
}
