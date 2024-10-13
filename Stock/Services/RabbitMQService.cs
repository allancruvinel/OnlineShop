using Microsoft.AspNetCore.Connections;
using OnlineShop.Domain.DTO;
using Order;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Stock.Services.Interfaces;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace Stock.Services;

public class RabbitMQService : IRabbitMQService
{

    private readonly ILogger<RabbitMQService> _logger;
    private IConnection _connection;
    private IModel _channel;
    private IServiceProvider _serviceProvider;

    public RabbitMQService(ILogger<RabbitMQService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        IniciarConexaoRabbitMQ();
        _serviceProvider = serviceProvider; 
    }
    private void IniciarConexaoRabbitMQ()
    {
        var factory = new ConnectionFactory { HostName = "rabbitmq" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }
    public void EnviarMensagemParaRoutingKey<T>(T mensagem, string routingKey = "order_queue")
    {
        _channel.ExchangeDeclare(exchange: "orders", type: ExchangeType.Topic);
        MessageBroke<T> messageBroke = new MessageBroke<T>
        {
            Id = 1,
            Message = mensagem
        };
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(messageBroke));

        _channel.BasicPublish(exchange: "orders", routingKey: routingKey, basicProperties: null, body: body);
    }



    public void RecebedorDeMensagem()
    {
        _channel.ExchangeDeclare(exchange: "orders", type: ExchangeType.Topic);

        _channel.QueueDeclare(queue: "update_stock",
            durable: true,
            exclusive: false,
        autoDelete: false,
            arguments: null);

        _channel.QueueBind(queue: "update_stock",
                      exchange: "orders",
                      routingKey: "*.stock_update");
        Console.WriteLine(" [*] Waiting for messages.");

        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var routingKey = ea.RoutingKey;
                _logger.LogInformation($" [x] Received '{routingKey}':'{message}'");
                MessageBroke<Order.Order> messageBroke;
                var mensagem = JsonSerializer.Deserialize<MessageBroke<Order.Order>>(message);
                using (var scope = _serviceProvider.CreateScope())
                {
                    var _orderData = scope.ServiceProvider.GetRequiredService<ItemData>();
                    var item = _orderData.Item.Where(it => it.ItemId == mensagem.Message.ItemId).First();
                    item.Quantity--;
                    _orderData.Item.Update(item);
                    _orderData.SaveChanges();
                }
                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                _logger.LogInformation($" [x] Reduzido quantidade do item ID '{mensagem.Message.ItemId}': Em 1");

            }
            catch (Exception ex)
            {
                _logger.LogError($" Error on receiving message {ex.ToString()}");
            }

        };

        _channel.BasicConsume(queue: "update_stock",
                     autoAck: false,
                     consumer: consumer);



    }
}
