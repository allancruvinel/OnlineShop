using Payment;
using Payment.Services;
using Payment.Services.Interfaces;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSingleton<IRabbitMQService,RabbitMQService>();


builder.Services.AddHostedService<PaymentWorker>();

var host = builder.Build();
host.Run();
