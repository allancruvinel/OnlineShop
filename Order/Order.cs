namespace Order;

public class Order
{
    public DateTime Date { get; set; }

    public int ItemId { get; set; }

    public int userId { get; set; }

    public int OrderId { get; set; }

    public string? OrderStatus {get;set;}
}
