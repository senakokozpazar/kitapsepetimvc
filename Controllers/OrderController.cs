using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using kitapsepetimvc.Data;
using kitapsepetimvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace kitapsepetimvc.Controllers
{
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;
        private readonly HttpClient _httpClient;

        public OrderController(AppDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient();
        }

        // GET: /Order
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Book)
                .ToListAsync();

            // API'den teslimat durumları çekiliyor
            List<DeliveryStatus> deliveryStatuses = new List<DeliveryStatus>();
            var deliveryResponse = await _httpClient.GetAsync("https://localhost:7098/api/DeliveryStatus/GetDeliveryStatuses");
            if (deliveryResponse.IsSuccessStatusCode)
            {
                var json = await deliveryResponse.Content.ReadAsStringAsync();
                deliveryStatuses = JsonSerializer.Deserialize<List<DeliveryStatus>>(json);
            }

            // API'den ödeme detayları çekiliyor
            List<PaymentDetail> paymentDetails = new List<PaymentDetail>();
            var paymentResponse = await _httpClient.GetAsync("https://localhost:7098/api/PaymentDetail/GetPaymentDetails");
            if (paymentResponse.IsSuccessStatusCode)
            {
                var json = await paymentResponse.Content.ReadAsStringAsync();
                paymentDetails = JsonSerializer.Deserialize<List<PaymentDetail>>(json);
            }

            // OrderViewModel listesi oluşturuluyor
            var orderViewModels = orders.Select(order =>
            {
                var delivery = deliveryStatuses.FirstOrDefault(d => d.OrderId == order.Id.ToString());
                var payment = paymentDetails.FirstOrDefault(p => p.OrderId == order.Id.ToString());

                return new OrderViewModel
                {
                    Id = order.Id,
                    UserId = order.UserId,
                    OrderDate = order.OrderDate,
                    Status = order.Status,
                    OrderItems = order.OrderItems.ToList(),

                    DeliveryStatus = delivery?.Status,
                    DeliveryLastUpdated = delivery?.LastUpdated,

                    PaymentAmount = payment?.Amount,
                    PaymentMethod = payment?.PaymentMethod,
                    IsPaid = payment?.IsPaid,
                    PaymentDate = payment?.PaymentDate
                };
            }).ToList();

            return View(orderViewModels);
        }

        // GET: /Order/Create
        public IActionResult Create()
        {
            return View(new Order());
        }

        // POST: /Order/Create
        [HttpPost]
        public async Task<IActionResult> Create(Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(order);
        }

        // GET: /Order/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();
            return View(order);
        }

        // POST: /Order/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Update(order);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(order);
        }

        // GET: /Order/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();
            return View(order);
        }

        // POST: /Order/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // GET: /Order/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Book)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }

        // GET: /Order/Checkout
        public async Task<IActionResult> Checkout()
        {
            // Burada kullanıcıya ait sepet verisi ve diğer gerekli bilgiler getirilebilir.
            // Şimdilik boş bir View döndür.
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(Order order)
        {
            try
            {
                // 1. Sepet öğelerini al
                var cartItems = await _context.CartItems
                    .Include(ci => ci.Book)
                    .ToListAsync();

                if (!cartItems.Any())
                {
                    TempData["ErrorMessage"] = "Sepetiniz boş. Sipariş verilemez.";
                    return View(order);
                }

                // 2. Model validation
                if (!ModelState.IsValid)
                {
                    return View(order);
                }

                // 3. Siparişi oluştur
                order.OrderDate = DateTime.Now;
                order.Status = "Yeni";

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // 4. Sipariş öğelerini ekle
                var orderItems = new List<OrderItem>();
                foreach (var item in cartItems)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        BookId = item.BookId,
                        Quantity = item.Quantity,
                        UnitPrice = item.Book.Price
                    };
                    orderItems.Add(orderItem);
                }

                _context.OrderItems.AddRange(orderItems);
                await _context.SaveChangesAsync();

                // 5. Toplam tutarı hesapla
                var totalAmount = cartItems.Sum(ci => ci.Quantity * ci.Book.Price);

                // 6. PaymentDetail ve DeliveryStatus API'ye gönder
                try
                {
                    var paymentDetail = new PaymentDetail
                    {
                        OrderId = order.Id.ToString(),
                        Amount = totalAmount,
                        PaymentMethod = "Kredi Kartı",
                        IsPaid = true,
                        PaymentDate = DateTime.Now
                    };

                    var deliveryStatus = new DeliveryStatus
                    {
                        OrderId = order.Id.ToString(),
                        Status = "Hazırlanıyor",
                        LastUpdated = DateTime.Now
                    };

                    // JSON serialization options for camelCase
                    var options = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };

                    var paymentJson = new StringContent(
                        JsonSerializer.Serialize(paymentDetail, options),
                        System.Text.Encoding.UTF8,
                        "application/json");

                    var deliveryJson = new StringContent(
                        JsonSerializer.Serialize(deliveryStatus, options),
                        System.Text.Encoding.UTF8,
                        "application/json");

                    // API çağrıları
                    var paymentResponse = await _httpClient.PostAsync("https://localhost:7098/api/PaymentDetail/Add", paymentJson);
                    var deliveryResponse = await _httpClient.PostAsync("https://localhost:7098/api/DeliveryStatus/Add", deliveryJson);

                    // API hata kontrolü (isteğe bağlı)
                    if (!paymentResponse.IsSuccessStatusCode || !deliveryResponse.IsSuccessStatusCode)
                    {
                        // Log the error but don't fail the order
                        // You might want to add logging here
                    }
                }
                catch (Exception ex)
                {
                    // API hatası durumunda sipariş devam etsin ama hata loglansın
                    // Log the exception
                    // You might want to add logging here
                }

                // 7. Sepeti temizle
                _context.CartItems.RemoveRange(cartItems);
                await _context.SaveChangesAsync();

                // 8. Başarı mesajı ve yönlendirme
                TempData["SuccessMessage"] = $"Sipariş başarıyla tamamlandı! Sipariş No: {order.Id}";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Genel hata durumu
                TempData["ErrorMessage"] = "Sipariş işlemi sırasında bir hata oluştu. Lütfen tekrar deneyin.";
                return View(order);
            }
        }


    }
}
