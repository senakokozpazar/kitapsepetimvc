using System.Threading.Tasks;
using kitapsepetimvc.Data;
using kitapsepetimvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace kitapsepetimvc.Controllers
{
    public class OrderItemController : Controller
    {
        private readonly AppDbContext _context;

        public OrderItemController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /OrderItem
        public async Task<IActionResult> Index()
        {
            var orderItems = await _context.OrderItems
                .Include(oi => oi.Order)  // İstersen ilişkili Order bilgisi
                .Include(oi => oi.Book)   // İstersen ilişkili Book bilgisi
                .ToListAsync();
            return View(orderItems);
        }

        // GET: /OrderItem/Create
        public IActionResult Create()
        {
            return View(new OrderItem());
        }

        // POST: /OrderItem/Create
        [HttpPost]
        public async Task<IActionResult> Create(OrderItem orderItem)
        {
            if (ModelState.IsValid)
            {
                _context.Add(orderItem);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(orderItem);
        }

        // GET: /OrderItem/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var orderItem = await _context.OrderItems.FindAsync(id);
            if (orderItem == null) return NotFound();
            return View(orderItem);
        }

        // POST: /OrderItem/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(OrderItem orderItem)
        {
            if (ModelState.IsValid)
            {
                _context.Update(orderItem);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(orderItem);
        }

        // GET: /OrderItem/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var orderItem = await _context.OrderItems.FindAsync(id);
            if (orderItem == null) return NotFound();
            return View(orderItem);
        }

        // POST: /OrderItem/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var orderItem = await _context.OrderItems.FindAsync(id);
            if (orderItem != null)
            {
                _context.OrderItems.Remove(orderItem);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // GET: /OrderItem/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var orderItem = await _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Book)
                .FirstOrDefaultAsync(oi => oi.Id == id);

            if (orderItem == null) return NotFound();
            return View(orderItem);
        }
    }
}
