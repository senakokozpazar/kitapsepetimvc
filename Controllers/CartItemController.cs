using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kitapsepetimvc.Data;
using kitapsepetimvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace kitapsepetimvc.Controllers
{
    public class CartItemController : Controller
    {
        private readonly AppDbContext _context;

        public CartItemController(AppDbContext context)
        {
            _context = context;
        }

        // Sepetteki tüm ürünleri listele
        public async Task<IActionResult> Index()
        {
            var cartItems = await _context.CartItems.Include(c => c.Book).ToListAsync();
            return View(cartItems);
        }

        // Sepete kitap ekle
        [HttpPost]
        public async Task<IActionResult> AddToCart(int bookId)
        {
            var existingItem = await _context.CartItems.FirstOrDefaultAsync(c => c.BookId == bookId);
            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                _context.CartItems.Add(new CartItem { BookId = bookId, Quantity = 1 });
            }
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Book");
        }

        // Sepetten kitap çıkar (isteğe bağlı)
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var cartItem = await _context.CartItems.FindAsync(id);
            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}

