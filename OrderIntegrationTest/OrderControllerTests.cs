using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Order;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;

public class OrderControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public OrderControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_ReturnsOrders_Success()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/Order/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var orders = await response.Content.ReadFromJsonAsync<List<Order.Order>>();
        orders.Should().NotBeNull();
        orders.Count.Should().Be(0); // Garantir que o banco de dados de teste tem 2 pedidos
    }

    [Fact]
    public async Task Post_ReturnsOrders_Success()
    {
        // Arrange
        var client = _factory.CreateClient();
        // Cria o objeto Order que será enviado no POST
        var newOrder = new Order.Order
        {
            Date = DateTime.Now,
            ItemId = 1,
            userId = 123,
            OrderId = 456
        };

        StringContent jsonContent = new(
        JsonSerializer.Serialize(newOrder),
        Encoding.UTF8,
        "application/json");


        // Act
        var response = await client.PostAsync("/Order", jsonContent);

        // Assert
        var createdOrder = await response.Content.ReadFromJsonAsync<Order.Order>();
        createdOrder.Should().NotBeNull();
        createdOrder.OrderId.Should().Be(newOrder.OrderId);
    }
}
