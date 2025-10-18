using FridgeManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace FridgeManagementSystem.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartRepository _cartRepository;

        public CartController(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }
        public async Task<IActionResult> AddItem(int fridgeId, int quantity = 1, int redirect = 0)
        {
            var cartCount = await _cartRepository.AddItem(fridgeId, quantity);
            if (redirect == 0)
                return Ok(cartCount);
            return RedirectToAction("GetUserCart");
        }

        public async Task<IActionResult> RemoveItem(int fridgeId)
        {
            var cartCount = await _cartRepository.RemoveItem(fridgeId);
            return RedirectToAction("GetUserCart");
        }

        public async Task<IActionResult> GetUserCart()
        {
            var cart = await _cartRepository.GetUserCart();
            return View(cart);
        }

        public async Task<IActionResult> GetTotalItemsInCart()
        {
            int cartItem = await _cartRepository.GetCartItemCount();
            return Ok(cartItem);
        }
        public async Task<IActionResult> Checkout()
        {
            bool isCheckout = await _cartRepository.DoCheckout();
            //if (!isCheckout)
            //    throw new Exception("Something went wrong");
            return RedirectToAction("Home", "Customer");
        }
    }
}
