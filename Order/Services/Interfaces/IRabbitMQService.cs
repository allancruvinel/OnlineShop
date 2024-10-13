namespace Order.Services.Interfaces;

public interface IRabbitMQService
{
    void EnviarMensagemParaFila<T>(T mensagem, string routingKey);
    void RecebedorDeMensagem();
}
