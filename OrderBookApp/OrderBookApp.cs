using System;
using System.Threading;
using Websocket.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLitePCL;
using Microsoft.VisualBasic;

public class OrderBookApp
{
    public OrderBookApp(DBBestPrice dbBestPrice)
    {
        this.workerTimer = new WorkerTimer(5000);
        this.dbBestPrice = dbBestPrice;
    }
    public void execute() 
     {
        BitStampMng bitStampMng = new BitStampMng();
        bitStampMng.connect(bitstamp_url);
        bitStampMng.subscribe("order_book_btcusd");
        bitStampMng.subscribe("order_book_ethusd");

        workerTimer.start(OnTimedEvent!);
        bitStampMng.start(HandleMessage);

        
    }

    private void HandleMessage(ResponseMessage message)
    {
        // Console.WriteLine("Message received: " + message);
        JObject jObject  = JObject.Parse(message.ToString());
        if (jObject["event"]!.ToString() != "data") {
            return;
        }

       
        string asset = jObject["channel"]!.ToString().Remove(0, (new string("order_book_")).Length);
        OrderBookData orderBookData = new OrderBookData(asset, jObject["data"]!);
        this.dbBestPrice.insertBestPrice(orderBookData);

        OrderBookRecord orderBookRecord = this.dbBestPrice.selectBestPrice(@"SELECT * FROM BestPrice ORDER BY idBestPrice DESC LIMIT 1");
        OrderBookService orderBookService = new OrderBookService(jObject["channel"]!.ToString(), jObject["data"]!);

        
    }
    public Func<AggregateBestPriceRecord, bool> handleAggregateRecord =  (AggregateBestPriceRecord aggregateBestPriceRecord) => 
    {
        var msg1 = string.Format("\tTime average {0}: Price Asks = {1}, Bids = {2}", 
                aggregateBestPriceRecord.asset, aggregateBestPriceRecord.asksPrice, aggregateBestPriceRecord.bidsPrice);
        var msg2 = string.Format("\t             Quantity Asks = {0}, Bids = {1}", 
                 aggregateBestPriceRecord.asksQuantity, aggregateBestPriceRecord.bidsQuantity);
        Console.WriteLine(msg1);
        Console.WriteLine(msg2);

        return true;
    };
    private void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
    {

        Console.WriteLine("The Elapsed event was raised at {0}", e.SignalTime);
        OrderBookRecord orderBookRecord = this.dbBestPrice.selectBestPrice(@"SELECT * FROM BestPrice ORDER BY idBestPrice DESC LIMIT 1");

        OrderBookService orderBookService = new OrderBookService(orderBookRecord);
        var msg0 = string.Format("\tCurrent asset {0}: Index {1}", orderBookRecord.orderBookData.asset, orderBookRecord.idBestPrice);
        var msg1 = string.Format("\t\tMax: Price Asks = {0}, Bids = {1}", orderBookService.getMaxPrice("asks"), orderBookService.getMaxPrice("bids"));
        var msg2 = string.Format("\t\tMin: Price Asks = {0}, Bids = {1}", orderBookService.getMinPrice("asks"), orderBookService.getMinPrice("bids"));
        var msg3 = string.Format("\t\tAvg: Price Asks = {0}, Bids = {1}", orderBookService.getAvgPrice("asks"), orderBookService.getAvgPrice("bids"));

        Console.WriteLine(msg0);
        Console.WriteLine(msg1);
        Console.WriteLine(msg2);
        Console.WriteLine(msg3);

        double interval = ((System.Timers.Timer)source).Interval / 1000;
        
        this.dbBestPrice.agregateBestPrice(handleAggregateRecord, @$"
                        SELECT  T.asset, avg(A.price) AS asksPrice, avg(b.price) AS bidsPrice, 
                                avg(A.quantity) AS asksQuantity, avg(b.quantity) AS bidsQuantity 
                            FROM BestPrice T
                            JOIN BestPriceAsks A, BestPriceBids B
                            WHERE 
                                T.idBestPrice = A.idBestPrice 
                                AND T.idBestPrice = B.idBestPrice
                                AND datetime(creationTime,'auto') <= datetime('now')
                                AND datetime(creationTime,'auto') >  datetime('now','-{interval} second')
                            GROUP by T.asset
                        "
                        );
    }

    private DBBestPrice dbBestPrice;
    private WorkerTimer workerTimer = null!;
    private string bitstamp_url = "wss://ws.bitstamp.net";
    private  Dictionary<string, Dictionary<string, List<double>>> dictRecord = 
            new  Dictionary<string, Dictionary<string, List<double>>>();
}