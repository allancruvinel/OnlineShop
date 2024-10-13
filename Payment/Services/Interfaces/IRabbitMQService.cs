namespace Payment.Services.Interfaces;

public interface IRabbitMQService
{
    public void RecebedorDeMensagem<T>();
}
