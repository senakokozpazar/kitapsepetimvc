using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using kitapsepetimvc.Data;
using kitapsepetimvc.Models;

namespace kitapsepetimvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private const int PageSize = 12;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        //Yazar ya da kitap adına göre arama
        [HttpGet]
        public async Task<JsonResult> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Json(Array.Empty<object>());
            }

            var results = await _context.Books
                .Where(b => b.Title.Contains(query) || b.Author.Contains(query))
                .Select(b => new
                {
                    b.Id,
                    b.Title,
                    b.Author,
                    b.Price,
                    b.ImageUrl,
                    Stock = b.Stock
                })
                .ToListAsync();

            return Json(results);
        }


        public async Task<IActionResult> Index(
            string category = null,
            string sortOrder = null,
            int page = 1)
        {
            // Tüm kategoriler (yuvarlak filtre için)
            var categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();

            // En çok satan 4 kitap (örnek: OrderItems sayısına göre)
            var bestSellers = await _context.Books
                .Include(b => b.Category)
                .OrderByDescending(b => b.OrderItems.Count) //her bir kitap içindeki oi listesinde kaç eleman var onu sayıyor
                .Take(4)
                .ToListAsync();

            // Kitap sorgusu
            var booksQuery = _context.Books.Include(b => b.Category).AsQueryable();

            if (!string.IsNullOrEmpty(category))
            {
                // Kategori adı ile filtreleme
                booksQuery = booksQuery.Where(b => b.Category.Name == category);
            }

            // Sıralama
            booksQuery = sortOrder switch
            {
                "title_asc" => booksQuery.OrderBy(b => b.Title),
                "title_desc" => booksQuery.OrderByDescending(b => b.Title),
                "author_asc" => booksQuery.OrderBy(b => b.Author),
                "author_desc" => booksQuery.OrderByDescending(b => b.Author),
                "price_asc" => booksQuery.OrderBy(b => b.Price),
                "price_desc" => booksQuery.OrderByDescending(b => b.Price),
                _ => booksQuery.OrderBy(b => b.Title),
            };

            var totalBooks = await booksQuery.CountAsync();

            var books = await booksQuery
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.BestSellers = bestSellers;
            ViewBag.CurrentCategory = category;
            ViewBag.SortOrder = sortOrder;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalBooks / (double)PageSize);

            return View(books);
        }
    }
}
