

using FridgeManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace FridgeManagementSystem.Repositories
{
    public class HomeRepository : IHomeRepository
    {
        private readonly FridgeManagementSystemContext _db;
        public HomeRepository(FridgeManagementSystemContext db)
        {
            _db = db;
        }
        public async Task<IEnumerable<FridgeType>> FridgeTypes()
        {
            return await _db.FridgeType.ToListAsync();
        }
        public async Task<IEnumerable<Fridge>> GetFridges(string searchTerm = "", int fridgeTypeId = 0)
        {
            searchTerm = searchTerm.ToLower();
            IEnumerable<Fridge> fridges = await (from fridge in _db.Fridges
                           join fridgeType in _db.FridgeType
                           on fridge.FridgeTypeId equals fridgeType.FridgeTypeId
                           where string.IsNullOrWhiteSpace(searchTerm) || (fridge!=null && fridge.Description.ToLower().StartsWith(searchTerm))
                           select new Fridge
                           {
                               FridgeId = fridge.FridgeId,
                               ImageFile = fridge.ImageFile,
                               SerialNumber = fridge.SerialNumber,
                               Description = fridge.Description,
                               Price = fridge.Price,
                               FridgeTypeId = fridge.FridgeTypeId
                           }
                           ).ToListAsync();
            if (fridgeTypeId > 0)
            {
                fridges = fridges.Where(a => a.FridgeTypeId == fridgeTypeId).ToList();
            }
            return fridges;
        }

    }
}
