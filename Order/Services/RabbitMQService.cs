using Microsoft.AspNetCore.Connections;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using OnlineShop.Domain.DTO;
using Order.Services.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace Order.Services;

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
    public void EnviarMensagemParaFila<T>(T mensagem, string routingKey)
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

        _channel.QueueDeclare(queue: "update_orders",
            durable: true,
            exclusive: false,
        autoDelete: false,
            arguments: null);

        _channel.QueueBind(queue: "update_orders",
                      exchange: "orders",
                      routingKey: "*.order_status_update");
        Console.WriteLine(" [*] Waiting for messages.");

        var consumerUpdateOrders = new EventingBasicConsumer(_channel);

        consumerUpdateOrders.Received += (model, ea) =>
        {
            try
            {
                

                    var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var routingKey = ea.RoutingKey;
                _logger.LogInformation($" [x] Received '{routingKey}':'{message}'");
                MessageBroke<Order> messageBroke;
                var mensagem = JsonSerializer.Deserialize<MessageBroke<Order>>(message);
                using (var scope = _serviceProvider.CreateScope())
                {
                    var _orderData = scope.ServiceProvider.GetRequiredService<OrderData>();
                    _orderData.Orders.Update(mensagem.Message);
                    _orderData.SaveChanges();
                }
                
                _logger.LogInformation($" [x] pedido '{mensagem.Message.OrderId}':Atualizado");
                //message recieved successfully, sending basic ack for commiting the receiving
                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                // logica para criar pagamento e validar
                //pagamento feito com sucesso? Atualizar estoque e status do Pedido, estoque e enviar ordem de faturamento
                

            }
            catch (Exception ex)
            {
                _logger.LogError($" Error on receiving message {ex.ToString()}");
            }

        };

        _channel.BasicConsume(queue: "update_orders",
                     autoAck: false,
                     consumer: consumerUpdateOrders);



    }
}
