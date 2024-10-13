using Microsoft.EntityFrameworkCore;
using Stock;
using Stock.Services;
using Stock.Services.Interfaces;


public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<ItemData>(options => options.UseInMemoryDatabase("Stock"));
        builder.Services.AddSingleton<IRabbitMQService, RabbitMQService>();


        builder.Services.AddControllers();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ItemData>();
            context.SeedData(); 
        }

        var rabbitMQService = app.Services.GetRequiredService<IRabbitMQService>();
        rabbitMQService.RecebedorDeMensagem();

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();

    }
}
