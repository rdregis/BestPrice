using System.Reflection.Metadata.Ecma335;
using System;
using System.Globalization;
using System.Runtime.Intrinsics.X86;
using Newtonsoft.Json.Linq;



public class OrderBookService
{
    public OrderBookService(string asset, JToken data)
    {
        this.orderBookRecord = convertToOrderBookRecord(asset, data);
    }

    public OrderBookRecord convertToOrderBookRecord(string asset, JToken data)
    {
        OrderBookRecord orderBookRecord = new OrderBookRecord();
        orderBookRecord.idBestPrice = 0;
       
        orderBookRecord.orderBookData = new OrderBookData(asset, data);
        
        return(orderBookRecord);
    }
    public OrderBookService(OrderBookRecord orderBookRecord)
    {
        this.orderBookRecord = orderBookRecord;
    }
    public double getMaxPrice(string type)
    {

        switch (type) {
            case "asks":
                return (this.orderBookRecord.orderBookData.bookAsksItems.Max(t => t.price));
            case "bids":
                return (this.orderBookRecord.orderBookData.bookBidsItems.Max(t => t.price));
            default:
                return (0);
        }
    
    }

    public double getMinPrice(string type)
    {
        switch (type) {
            case "asks":
                return (this.orderBookRecord.orderBookData.bookAsksItems.Min(t => t.price));
            case "bids":
                return (this.orderBookRecord.orderBookData.bookBidsItems.Min(t => t.price));
            default:
                return (0);
        }
    }

    public double getAvgPrice(string type)
    {
       
        switch (type) {
            case "asks":
                return (this.orderBookRecord.orderBookData.bookAsksItems.Average(t => t.price));
            case "bids":
                return (this.orderBookRecord.orderBookData.bookBidsItems.Average(t => t.price));
            default:
                return (0);
        }
    }
    public double getAvgQuantity(string type)
    {
       
        switch (type) {
            case "asks":
                return (this.orderBookRecord.orderBookData.bookAsksItems.Average(t => t.quantity));
            case "bids":
                return (this.orderBookRecord.orderBookData.bookBidsItems.Average(t => t.quantity));
            default:
                return (0);
        }
    }

    public BestPriceResult makeBuy(double quantity) 
    {
        List<BookItem> bookList = orderBookRecord.orderBookData.bookAsksItems.OrderBy(o=>o.price).ToList();
        BestPriceResult bestPriceResult = makeOperation(quantity, bookList);

        
        bestPriceResult.Id = orderBookRecord.idBestPrice;
        long timestamp = long.Parse(orderBookRecord.orderBookData.timestamp);
        bestPriceResult.Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).UtcDateTime.ToString();
       
        bestPriceResult.Operation = "buy";
        bestPriceResult.Asset = orderBookRecord.orderBookData.asset;

        return (bestPriceResult);
        
    }

    
public BestPriceResult makeSell(double quantity) 
    {

        List<BookItem> bookList = orderBookRecord.orderBookData.bookBidsItems.OrderByDescending(o=>o.price).ToList();
        BestPriceResult bestPriceResult = makeOperation(quantity, bookList);


        bestPriceResult.Id = orderBookRecord.idBestPrice;
        long timestamp = long.Parse(orderBookRecord.orderBookData.microtimestamp);
        bestPriceResult.Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).UtcDateTime.ToString();
        bestPriceResult.Operation = "sell";
        bestPriceResult.Asset = orderBookRecord.orderBookData.asset;

        return (bestPriceResult);

    }

    private BestPriceResult makeOperation(double quantity, List<BookItem>bookList)
    {
        BestPriceResult bestPriceResult = new BestPriceResult();
        int sumItems = 0;
        double sumPrice = 0;
        double sumQuantity = 0;

       
        foreach (var item in bookList) {
             if (sumQuantity + item.quantity > quantity) {
                break;
            }
            bestPriceResult.BookItems.Add(item);
            sumQuantity += item.quantity;
            sumPrice += item.price;
            ++sumItems;
        }

        if (bestPriceResult.BookItems.Count == 1) {
            bestPriceResult.BookItems.Clear();
        }

        bestPriceResult.Items = sumItems;
        bestPriceResult.Price = sumPrice;
        bestPriceResult.Quantity = sumQuantity;

        return (bestPriceResult);
    }
    // private string channel { get; set; }
    // private JToken data {get; set;}

    private OrderBookRecord orderBookRecord = null!;
    
}
