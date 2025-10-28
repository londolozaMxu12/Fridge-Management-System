using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using FridgeManagementSystem.Services;
using System;

namespace FridgeManagementSystem.Controllers
{
    public class RFQsController : Controller
    {
        private readonly FridgeManagementSystemContext _context;
        private readonly IEmailService _emailService;

        public RFQsController(FridgeManagementSystemContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }
        // Example usage
        public async Task<IActionResult> NotifySupplier(string email)
        {
            await _emailService.SendEmailAsync(email, "RFQ Notification", "You have a new RFQ request.");
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Index()
        {
            var rfqs = await _context.RFQs
                .Include(r => r.PurchaseRequest)
                .Include(r => r.RFQSuppliers)
                    .ThenInclude(rs => rs.Supplier)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
            return View(rfqs);
        }

        public async Task<IActionResult> Create(int? purchaseRequestId)
        {
            var viewModel = new CreateRFQViewModel();

            if (purchaseRequestId.HasValue)
            {
                var purchaseRequest = await _context.PurchaseRequests.FindAsync(purchaseRequestId.Value);
                if (purchaseRequest != null)
                {
                    viewModel.PurchaseRequestId = purchaseRequest.PurchaseRequestId;
                    viewModel.Title = $"RFQ for {purchaseRequest.FridgeName}";
                    viewModel.Description = $"Request for {purchaseRequest.Quantity} units of {purchaseRequest.FridgeName}";
                }
            }

            viewModel.Suppliers = await _context.Suppliers
                .Where(s => s.IsActive)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.CompanyName
                })
                .ToListAsync();

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRFQViewModel model)
        {
            if (ModelState.IsValid)
            {
                var rfq = new RFQ
                {
                    RFQNumber = model.Title,
                    ItemDescription = model.Description,
                    PurchaseRequestId = model.PurchaseRequestId,
                    Deadline = model.Deadline,
                    Status = model.RFQStatus
                };

                _context.Add(rfq);
                await _context.SaveChangesAsync();

                // Add selected suppliers
                foreach (var supplierId in model.SelectedSupplierIds)
                {
                    var rfqSupplier = new RFQSupplier
                    {
                        RFQId = rfq.RFQId,
                        SupplierId = supplierId,
                        SentDate = DateTime.UtcNow
                    };
                    _context.Add(rfqSupplier);

                    // Send email to supplier
                    var supplier = await _context.Suppliers.FindAsync(supplierId);
                    if (supplier != null)
                    {
                        await _emailService.SendRFQToSupplier(supplier, rfq);
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            model.Suppliers = await _context.Suppliers
                .Where(s => s.IsActive)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.CompanyName
                })
                .ToListAsync();

            return View(model);
        }
    }

    public class CreateRFQViewModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int PurchaseRequestId { get; set; }
        public DateTime Deadline { get; set; } = DateTime.UtcNow.AddDays(14);
        public List<int> SelectedSupplierIds { get; set; } = new();
        public List<SelectListItem> Suppliers { get; set; } = new();
        public RFQStatus RFQStatus { get; internal set; }
    }
        //public IActionResult Index()
        //{
        //    return View();
        //}
    }
