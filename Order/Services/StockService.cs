using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using OnlineShop.Domain.DTO;
using Order.Services.Interfaces;

namespace Order.Services
{
    public class StockService : IStockService
    {

        public StockService()
        {
        }

        public async Task<List<Item>> GetItemsAsync()
        {
            using HttpClient _httpClient = new HttpClient();
            var response = await _httpClient.GetAsync("http://stock:8080/item");
            response.EnsureSuccessStatusCode(); // Garante que a requisição teve sucesso

            var json = await response.Content.ReadAsStringAsync();
            var items = JsonSerializer.Deserialize<List<Item>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return items ?? new List<Item>();
        }

        public async Task<bool> IsItemInStock(int itemId)
        {
            var items = await GetItemsAsync();
            var item = items.FirstOrDefault(i => i.ItemId == itemId);
            return item != null && item.Quantity > 0;
        }
    }
}
