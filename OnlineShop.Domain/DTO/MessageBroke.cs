namespace OnlineShop.Domain.DTO;

public class MessageBroke<T>
{
    public int Id { get; set; }
    public required T Message { get; set; }
}
