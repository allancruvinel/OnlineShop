using Microsoft.EntityFrameworkCore;
using Order;
using Order.Services;
using Order.Services.Interfaces;


public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configurar a string de conexão
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        // Adicionar o DbContext como um serviço
        builder.Services.AddDbContext<OrderData>(options => options.UseInMemoryDatabase("Order"));

        builder.Services.AddSingleton<IRabbitMQService,RabbitMQService>();
        builder.Services.AddScoped<IStockService,StockService>();

        builder.Services.AddControllers();

        var app = builder.Build();

        var rabbitMQService = app.Services.GetRequiredService<IRabbitMQService>();
        rabbitMQService.RecebedorDeMensagem();

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();

    }
}
