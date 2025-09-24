
using FridgeManagementSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace FridgeManagementSystem.Areas.Identity.Data
{
    public interface IEmployeeNumberService
    {
        Task<string> GenerateEmployeeNumberAsync();
        //Task InitializeSequenceAsync();
    }

    public class EmployeeNumberService : IEmployeeNumberService
    {
        private readonly FridgeManagementSystemContext _context;

        public EmployeeNumberService(FridgeManagementSystemContext context)
        {
            _context = context;
        }

        public async Task<string> GenerateEmployeeNumberAsync()
        {
            // Get the highest existing employee number
            var lastEmployee = await _context.Employees
                .Where(e => e.EmployeeNo != null && e.EmployeeNo.StartsWith("EMP"))
                .OrderByDescending(e => e.EmployeeNo)
                .FirstOrDefaultAsync();

            if (lastEmployee == null)
            {
                return "EMP001";
            }

            // Extract the numeric part and increment
            var numberPart = lastEmployee.EmployeeNo.Substring(3); // Remove "EMP"
            if (int.TryParse(numberPart, out int lastNumber))
            {
                return $"EMP{(lastNumber + 1):D3}"; // Format as EMP001, EMP002, etc.
            }

            // If parsing fails, start from 1
            return "EMP001";
        }
        //public async Task InitializeSequenceAsync()
        //{
        //    // Example: create a default EMP001 if no employee exists
        //    var exists = await _context.Employees.AnyAsync(e => e.EmployeeNo.StartsWith("EMP"));
        //    if (!exists)
        //    {
        //        // You might insert a dummy or first record here
        //        // But if unnecessary, leave this empty or log
        //    }
        //}
    }
}