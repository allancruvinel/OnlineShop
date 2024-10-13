namespace Order.Services.Interfaces;

public interface IStockService
{
    Task<bool> IsItemInStock(int itemId);
}
