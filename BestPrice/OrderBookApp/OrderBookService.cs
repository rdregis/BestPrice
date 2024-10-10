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

    public BestPriceTrade makeBuy(double quantity) 
    {
        List<BookItem> bookList = orderBookRecord.orderBookData.bookAsksItems.OrderBy(o=>o.price).ToList();
        BestPriceTrade bestPriceTrade = makeOperation(quantity, bookList);

        
         makePosOperation(bestPriceTrade, "buy");


        return (bestPriceTrade);
        
    }

    
public BestPriceTrade makeSell(double quantity) 
    {

        List<BookItem> bookList = orderBookRecord.orderBookData.bookBidsItems.OrderByDescending(o=>o.price).ToList();
        BestPriceTrade bestPriceTrade = makeOperation(quantity, bookList);

        makePosOperation(bestPriceTrade, "sell");

        return (bestPriceTrade);

    }

    private BestPriceTrade makeOperation(double quantity, List<BookItem>bookList)
    {
        BestPriceTrade bestPriceTrade = new BestPriceTrade();
        int sumItems = 0;
        double sumPrice = 0;
        double sumQuantity = 0;

       
        foreach (var item in bookList) {
             if (sumQuantity + item.quantity > quantity) {
                break;
            }
            bestPriceTrade.BookItems.Add(item);
            sumQuantity += item.quantity;
            sumPrice += item.price;
            ++sumItems;
        }

        if (bestPriceTrade.BookItems.Count == 1) {
            bestPriceTrade.BookItems.Clear();
        }

        bestPriceTrade.Items = sumItems;
        bestPriceTrade.Price = sumPrice;
        bestPriceTrade.Quantity = sumQuantity;

        return (bestPriceTrade);
    }
    
    private void makePosOperation(BestPriceTrade bestPriceTrade, string operation)
    {
        bestPriceTrade.Id = orderBookRecord.idBestPrice;
        long timestamp = long.Parse(orderBookRecord.orderBookData.timestamp);
        bestPriceTrade.Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).UtcDateTime.ToString();
        bestPriceTrade.Timestamp = orderBookRecord.orderBookData.timestamp;
        bestPriceTrade.Operation = operation;
        bestPriceTrade.Asset = orderBookRecord.orderBookData.asset;

        DBBestPrice dbBestPrice = new DBBestPrice("BestPrice.db");

        dbBestPrice.connect();
        dbBestPrice.insertBestPriceTrade(bestPriceTrade);
        dbBestPrice.disconnect(); 
    }
    private OrderBookRecord orderBookRecord = null!;
    
}
