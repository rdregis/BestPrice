public class BestPriceTrade
{
    public long Id { get; set; }
    public string Timestamp { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public string Asset { get; set; } = string.Empty ;
    
    public double Price { get; set; }
    public double Quantity { get; set; }
    public int Items { get; set; }
    public List<BookItem> BookItems { get; set; } = new List<BookItem>();

}