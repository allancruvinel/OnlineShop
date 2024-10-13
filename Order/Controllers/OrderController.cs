using Microsoft.AspNetCore.Mvc;
using Order.Services;
using Order.Services.Interfaces;
using System.Collections.Generic;

namespace Order.Controllers;
[ApiController]
[Route("[controller]")]
public class OrderController(OrderData orderData, 
    ILogger<OrderController> logger,
    IRabbitMQService _rabbitMQService,IStockService stockService) : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };


    [HttpGet]
    public List<Order> Get()
    {
        var dataFromBase = orderData.Orders.ToList();
        return dataFromBase;
    }

    [HttpPost]
    public async Task<ActionResult<Order>> Post([FromBody] Order order)
    {
        if(!await stockService.IsItemInStock(order.ItemId))
        {
            return BadRequest(new { message = "sem itens no estoque" });
        }
        order.OrderStatus = "Aguardando Pagamento";
        orderData.Orders.Add(order);
        var result = await orderData.SaveChangesAsync();
        _rabbitMQService.EnviarMensagemParaFila(order,"order_queue");
        return Ok(new {result = result, objeto = order });
    }
}
