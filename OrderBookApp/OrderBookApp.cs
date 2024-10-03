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
    public Func<OrderBookRecord, bool> handleTimeRecord =  (OrderBookRecord orderBookRecord) => 
    {
        Console.WriteLine(@$"TimeRecord {orderBookRecord.idBestPrice}: {orderBookRecord.orderBookData.asset}: 
                                        {orderBookRecord.orderBookData.timestamp}: {orderBookRecord.orderBookData.microtimestamp}");
        
        OrderBookService orderBookService = new OrderBookService(orderBookRecord);
       
        foreach (var item in this.dictRecord) {
            if (item.Key != orderBookRecord.orderBookData.asset) {
                continue;
            }
            Dictionary<string, List<double>> dictType = item.Value;
            foreach (var itemType in dictType) { 
                if (itemType.Key == "bids") {
                    itemType.Value.Add(orderBookService.getAvgPrice("bids"));
                }
                else {
                    itemType.Value.Add(orderBookService.getAvgPrice("asks"));
                }
            }
        }

        // Dictionary<string, List<double>> dictType = new Dictionary<string, List<double>>();

        // List<double> asksItems = new List<double>();
        // asksItems.Add(orderBookService.getAvgPrice("asks"));
        // dictType.Add("asks", asksItems);


        // List<double> bidsItems = new List<double>();
        // bidsItems.Add(orderBookService.getAvgPrice("bids"));
        // dictType.Add("bids", bidsItems);


        // dictRecord.Add(orderBookRecord.orderBookData.asset, dictType);
        
        
        return true;
    };
    private void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
    {

        Console.WriteLine("The Elapsed event was raised at {0}", e.SignalTime);
        OrderBookRecord orderBookRecord = this.dbBestPrice.selectBestPrice(@"SELECT * FROM BestPrice ORDER BY idBestPrice DESC LIMIT 1");

        OrderBookService orderBookService = new OrderBookService(orderBookRecord);
        Console.WriteLine("\tMax bids price: " + orderBookService.getMaxPrice("bids"));
        Console.WriteLine("\tMin bids price: " + orderBookService.getMinPrice("bids"));
        Console.WriteLine("\tAvg bids price: " + orderBookService.getAvgPrice("bids"));
        Console.WriteLine("\tMax asks price: " + orderBookService.getMaxPrice("asks"));
        Console.WriteLine("\tMin asks price: " + orderBookService.getMinPrice("asks"));
        Console.WriteLine("\tAvg asks price: " + orderBookService.getAvgPrice("asks"));

        double interval = ((System.Timers.Timer)source).Interval / 1000;
        
        this.dbBestPrice.selectBestPrice(handleTimeRecord, @$"
                        SELECT * from BestPrice 
                        where  datetime(creationTime,'auto') <= datetime('now')
                        and datetime(creationTime,'auto') >  datetime('now','-{interval} second')"
                        );
    }

    private DBBestPrice dbBestPrice;
    private WorkerTimer workerTimer = null!;
    private string bitstamp_url = "wss://ws.bitstamp.net";
    private  Dictionary<string, Dictionary<string, List<double>>> dictRecord = 
            new  Dictionary<string, Dictionary<string, List<double>>>();
}