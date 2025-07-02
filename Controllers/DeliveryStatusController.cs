using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using kitapsepetimvc.Models;

namespace kitapsepetimvc.Controllers
{
    public class DeliveryStatusController : Controller
    {
        private readonly HttpClient _client;

        public DeliveryStatusController()
        {
            _client = new HttpClient();
        }

        public async Task<IActionResult> Index()
        {
            var response = await _client.GetAsync("https://localhost:7098/api/DeliveryStatus/GetDeliveryStatuses");
            var json = await response.Content.ReadAsStringAsync();
            var deliveryStatuses = JsonConvert.DeserializeObject<List<DeliveryStatus>>(json);
            return View(deliveryStatuses);
        }

        public IActionResult Create()
        {
            return View(new DeliveryStatus());
        }

        [HttpPost]
        public async Task<IActionResult> Create(DeliveryStatus delivery)
        {
            var content = new StringContent(JsonConvert.SerializeObject(delivery), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("https://localhost:7098/api/DeliveryStatus/AddDeliveryStatus", content);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var response = await _client.GetAsync($"https://localhost:7098/api/DeliveryStatus/GetDeliveryStatusById/{id}");
            var json = await response.Content.ReadAsStringAsync();
            var deliveryStatus = JsonConvert.DeserializeObject<DeliveryStatus>(json);
            return View(deliveryStatus);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(DeliveryStatus deliveryStatus)
        {
            var content = new StringContent(JsonConvert.SerializeObject(deliveryStatus), Encoding.UTF8, "application/json");
            var response = await _client.PutAsync($"https://localhost:7098/api/DeliveryStatus/UpdateDeliveryStatus/{deliveryStatus.Id}", content);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            var response = await _client.DeleteAsync($"https://localhost:7098/api/DeliveryStatus/DeleteDeliveryStatus/{id}");
            return RedirectToAction("Index");
        }
    }
}
