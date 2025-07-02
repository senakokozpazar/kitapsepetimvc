using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using kitapsepetimvc.Models;

namespace kitapsepetimvc.Controllers
{
    public class PaymentDetailController : Controller
    {
        private readonly HttpClient _client;

        public PaymentDetailController()
        {
            _client = new HttpClient();
        }

        public async Task<IActionResult> Index()
        {
            var response = await _client.GetAsync("https://localhost:7098/api/PaymentDetail/GetPaymentDetails");
            var json = await response.Content.ReadAsStringAsync();
            var paymentDetails = JsonConvert.DeserializeObject<List<PaymentDetail>>(json);
            return View(paymentDetails);
        }

        public IActionResult Create()
        {
            return View(new PaymentDetail());
        }

        [HttpPost]
        public async Task<IActionResult> Create(PaymentDetail detail)
        {
            var content = new StringContent(JsonConvert.SerializeObject(detail), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("https://localhost:7098/api/PaymentDetail/AddPaymentDetails", content);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var response = await _client.GetAsync($"https://localhost:7098/api/PaymentDetail/GetPaymentDetailsById/{id}");
            var json = await response.Content.ReadAsStringAsync();
            var detail = JsonConvert.DeserializeObject<PaymentDetail>(json);
            return View(detail);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(PaymentDetail detail)
        {
            var content = new StringContent(JsonConvert.SerializeObject(detail), Encoding.UTF8, "application/json");
            var response = await _client.PutAsync($"https://localhost:7098/api/PaymentDetail/UpdatePaymentDetails/{detail.Id}", content);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            var response = await _client.DeleteAsync($"https://localhost:7098/api/PaymentDetail/DeletePaymentDetails/{id}");
            return RedirectToAction("Index");
        }
    }
}
