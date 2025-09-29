// Controllers/EmployeeTypesController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FridgeManagementSystem.Data;
using FridgeManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace FridgeManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EmployeeTypeController : Controller
    {
        private readonly FridgeManagementSystemContext _context;

        public EmployeeTypeController(FridgeManagementSystemContext context)
        {
            _context = context;
        }

        // GET: EmployeeTypes
        public async Task<IActionResult> Index()
        {
            var employeeTypes = await _context.EmployeeTypes

                .Where(et => et.IsActive)
                .OrderBy(et => et.Name)
                .ToListAsync();

            return View(employeeTypes);
        }

        // GET: EmployeeTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: EmployeeTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name")] EmployeeType employeeType)
        {
            if (ModelState.IsValid)
            {
               
                employeeType.CreatedAt = DateTime.UtcNow;
                employeeType.IsActive = true;

                _context.Add(employeeType);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Employee type created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(employeeType);
        }

        // GET: EmployeeTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employeeType = await _context.EmployeeTypes.FindAsync(id);
            if (employeeType == null || !employeeType.IsActive)
            {
                return NotFound();
            }
            return View(employeeType);
        }

        // POST: EmployeeTypes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,IsActive")] EmployeeType employeeType)
        {
            if (id != employeeType.Id)
            {
                return NotFound();
            }

            // Check if trying to deactivate an employee type that's in use
            if (!employeeType.IsActive)
            {
                bool isEmployeeTypeInUse = await IsEmployeeTypeInUse(id);
                if (isEmployeeTypeInUse)
                {
                    ModelState.AddModelError("IsActive", "Cannot deactivate this employee type because it is currently assigned to active employee.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingType = await _context.EmployeeTypes.FindAsync(id);

                    if (existingType == null)
                    {
                        return NotFound();
                    }

                    existingType.Name = employeeType.Name;
                    
                    existingType.IsActive = employeeType.IsActive;

                    _context.Update(existingType);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Employee type updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeTypeExists(employeeType.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(employeeType);
        }

        // GET: EmployeeTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employeeType = await _context.EmployeeTypes
               .FirstOrDefaultAsync(m => m.Id == id && m.IsActive);
            if (employeeType == null)
            {
                return NotFound();
            }

            // Check if employee type is in use
            var employeesUsingType = await _context.Employees
                .AnyAsync(e => e.EmployeeTypeId == id && e.User.IsActive);

            if (employeesUsingType)
            {
                TempData["ErrorMessage"] = "Can not delete this employee type because it is currently assigned to active employees.";
                return RedirectToAction(nameof(Index));
            }

            return View(employeeType);
        }

        // POST: EmployeeTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employeeType = await _context.EmployeeTypes.FindAsync(id);
            if (employeeType != null)
            {
                // Soft delete
                employeeType.IsActive = false;
                _context.Update(employeeType);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Employee type deleted successfully.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeTypeExists(int id)
        {
            return _context.EmployeeTypes.Any(e => e.Id == id && e.IsActive);
        }

        // Helper method to check if employee type is in use
        private async Task<bool> IsEmployeeTypeInUse(int employeeTypeId)
        {
            // Check if any active employees are using this employee type
            return await _context.Employees
                .AnyAsync(e => e.EmployeeTypeId == employeeTypeId && e.User.IsActive);
        }

    }
}