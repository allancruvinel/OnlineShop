using OnlineShop.Domain.DTO;
using Payment.Services.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Payment.Services;

public class RabbitMQService : IRabbitMQService
{

    private readonly ILogger<RabbitMQService> _logger;
    private IConnection _connection;
    private IModel _channel;

    public RabbitMQService(ILogger<RabbitMQService> logger)
    {
        _logger = logger;
        IniciarConexaoRabbitMQ();
    }
    private void IniciarConexaoRabbitMQ()
    {
        var factory = new ConnectionFactory { HostName = "rabbitmq" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    public void EnviarMensagemParaRoutingKey<T>(T mensagem, string routingKey)
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
    public void RecebedorDeMensagem<T>()
    {
        _channel.ExchangeDeclare(exchange: "orders", type: ExchangeType.Topic);

        _channel.QueueDeclare(queue: "new_orders",
            durable:true,
            exclusive:false,
            autoDelete:false,
            arguments:null);

        _channel.QueueBind(queue: "new_orders",
                      exchange: "orders",
                      routingKey: "order_queue");

        Console.WriteLine(" [*] Waiting for messages.");

        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += (model, ea) =>
        {
            try {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var routingKey = ea.RoutingKey;
                    _logger.LogInformation($" [x] Received '{routingKey}':'{message}'");
                MessageBroke<Order.Order> messageBroke;
                var mensagem = JsonSerializer.Deserialize<MessageBroke<Order.Order>>(message);

                //message recieved successfully, sending basic ack for commiting the receiving
                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                // logica para criar pagamento e validar
                //pagamento feito com sucesso? Atualizar estoque e status do Pedido, estoque e enviar ordem de faturamento
                mensagem.Message.OrderStatus = "Pago";
                EnviarMensagemParaRoutingKey(mensagem.Message, "paid_orders.stock_update");
                EnviarMensagemParaRoutingKey(mensagem.Message, "paid_orders.order_status_update");

            }
            catch(Exception ex)
            {
                _logger.LogError($" Error on receiving message {ex.ToString()}");
            }

        };

        _channel.BasicConsume(queue: "new_orders",
                     autoAck: false,
                     consumer: consumer);

        

    }
}
