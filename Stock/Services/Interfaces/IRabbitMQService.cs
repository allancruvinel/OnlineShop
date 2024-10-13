namespace Stock.Services.Interfaces;

public interface IRabbitMQService
{
    void EnviarMensagemParaRoutingKey<T>(T mensagem, string routingKey = "create");

    public void RecebedorDeMensagem();
}
