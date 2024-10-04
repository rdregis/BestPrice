public class OrderBookRecord 
{
    public int idBestPrice;
    public OrderBookData orderBookData = new OrderBookData();

}

public class AggregateBestPriceRecord
{
    public string asset;
    public double asksPrice;
    public double bidsPrice;

    public double asksQuantity;
    public double bidsQuantity;
}