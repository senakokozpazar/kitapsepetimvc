using System.Linq;
using System.Threading.Tasks;
using kitapsepetimvc.Data;
using kitapsepetimvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace kitapsepetimvc.Controllers
{
    
    public class CartItemController : Controller
    {
        private readonly AppDbContext _context;

        public CartItemController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var cartItems = await _context.CartItems
                .Include(c => c.Book)
                .ToListAsync();

            return View(cartItems);
        }

        // Cart Controller'ına bu metodu ekleyin
        [HttpGet]
        public async Task<IActionResult> GetCartItems()
        {
            var cartItems = await _context.CartItems
                .Include(ci => ci.Book)
                .Select(ci => new {
                    quantity = ci.Quantity,
                    book = new
                    {
                        title = ci.Book.Title,
                        price = ci.Book.Price
                    }
                })
                .ToListAsync();

            return Json(cartItems);
        }

        // Sepetin tüm detaylarını JSON olarak döner
        [HttpGet]
        public async Task<JsonResult> GetCart()
        {
            var items = await _context.CartItems
                .Include(c => c.Book)
                .Select(c => new
                {
                    c.Id,
                    BookId = c.Book.Id,
                    c.Book.Title,
                    c.Book.Price,
                    c.Quantity,
                    TotalPrice = c.Quantity * c.Book.Price
                })
                .ToListAsync();

            var totalPrice = items.Sum(i => i.TotalPrice);
            var totalQuantity = items.Sum(i => i.Quantity);

            return Json(new { items, totalPrice, totalQuantity });
        }

        // Sepete kitap ekle - AJAX
        [HttpPost]
        public async Task<JsonResult> AddToCartAjax(int bookId)
        {
            var existingItem = await _context.CartItems.Include(c => c.Book).FirstOrDefaultAsync(c => c.BookId == bookId);
            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                var book = await _context.Books.FindAsync(bookId);
                if (book == null)
                    return Json(new { success = false, message = "Kitap bulunamadı." });

                _context.CartItems.Add(new CartItem { BookId = bookId, Quantity = 1 });
            }

            await _context.SaveChangesAsync();

            var totalQuantity = await _context.CartItems.SumAsync(c => c.Quantity);
            return Json(new { success = true, totalQuantity });
        }

        // Sepette ürün adet azalt - AJAX
        [HttpPost]
        public async Task<JsonResult> DecreaseQuantity(int cartItemId)
        {
            var item = await _context.CartItems.FindAsync(cartItemId);
            if (item == null)
                return Json(new { success = false, message = "Sepet ürünü bulunamadı." });

            if (item.Quantity > 1)
                item.Quantity--;
            else
                _context.CartItems.Remove(item);

            await _context.SaveChangesAsync();

            var totalQuantity = await _context.CartItems.SumAsync(c => c.Quantity);
            return Json(new { success = true, totalQuantity });
        }

        // Sepette ürün adet artır - AJAX
        [HttpPost]
        public async Task<JsonResult> IncreaseQuantity(int cartItemId)
        {
            var item = await _context.CartItems.FindAsync(cartItemId);
            if (item == null)
                return Json(new { success = false, message = "Sepet ürünü bulunamadı." });

            item.Quantity++;
            await _context.SaveChangesAsync();

            var totalQuantity = await _context.CartItems.SumAsync(c => c.Quantity);
            return Json(new { success = true, totalQuantity });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            var item = await _context.CartItems.FindAsync(cartItemId);
            if (item != null && quantity > 0)
            {
                item.Quantity = quantity;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // Sepetten ürün çıkar - AJAX
        [HttpPost]
        public async Task<JsonResult> RemoveFromCartAjax(int cartItemId)
        {
            var cartItem = await _context.CartItems.FindAsync(cartItemId);
            if (cartItem == null)
                return Json(new { success = false, message = "Sepet ürünü bulunamadı." });

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            var totalQuantity = await _context.CartItems.SumAsync(c => c.Quantity);
            return Json(new { success = true, totalQuantity });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var item = await _context.CartItems.FindAsync(cartItemId);
            if (item != null)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
