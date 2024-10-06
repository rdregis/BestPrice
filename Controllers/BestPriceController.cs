using System.Runtime;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace BestPrice.Controllers;

[ApiController]
[Route("[controller]")]
public class BestPriceController : ControllerBase
{
    
    private readonly ILogger<BestPriceController> _logger;

    public BestPriceController(ILogger<BestPriceController> logger)
    {
        _logger = logger;

        
    }

    [HttpGet(Name = "GetBestPrice")]
    public IEnumerable<BestPriceTrade> Get()
    {
        List<BestPriceTrade> bestPriceList =  new List<BestPriceTrade>();


        BestPriceTrade bestPriceItemBuy = new BestPriceTrade();
        bestPriceItemBuy.Timestamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        bestPriceItemBuy.Operation = "buy";
        bestPriceList.Add(bestPriceItemBuy);
        
        BestPriceTrade bestPriceItemSell = new BestPriceTrade();
        bestPriceItemSell.Id = 1;
        bestPriceItemSell.Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(DateTimeOffset.Now.ToUnixTimeMilliseconds()).UtcDateTime.ToString();
        bestPriceItemSell.Operation = "sell";
        bestPriceList.Add(bestPriceItemSell);

        return ((IEnumerable<BestPriceTrade>)bestPriceList).ToArray();
       

    }
     [HttpGet("{operation}/{asset}/{quantity}")]
    public ActionResult<IEnumerable<BestPriceTrade>> Get(string operation, string asset, double quantity)
    {

        DBBestPrice dbBestPrice = new DBBestPrice("BestPrice.db");
        dbBestPrice.connect();      
        OrderBookRecord orderBookRecord = 
                dbBestPrice.selectBestPrice(@"SELECT * FROM BestPrice WHERE asset = @asset ORDER BY idBestPrice DESC LIMIT 1", asset);

        if (orderBookRecord == null) {
            return (StatusCode(500, $"No Data to execute {operation} on asset: {asset} "));
        }

        OrderBookService orderBookService = new OrderBookService(orderBookRecord);


       List<BestPriceTrade> bestPriceList =  new List<BestPriceTrade>();

        switch (operation) {
            case "buy":
                bestPriceList.Add(orderBookService.makeBuy(quantity));
                break;
            case "sell":
                bestPriceList.Add(orderBookService.makeSell(quantity));
                break;
            default:
                return (StatusCode(500, "Invalid operation. Use 'sell' or 'buy'"));
        }
        
        return ((IEnumerable<BestPriceTrade>)bestPriceList).ToArray();
       }

    // [HttpPost]
    // public void Post([FromBody] string value)
    // { 
    //     Console.WriteLine(value);
    // }
    [HttpPost]
    // [ProducesResponseType(StatusCodes.Status201Created)]
    // [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<BestPriceTrade> Create(string operation, string asset, double quantity)
    {
       DBBestPrice dbBestPrice = new DBBestPrice("BestPrice.db");
        dbBestPrice.connect();      
        OrderBookRecord orderBookRecord = 
                dbBestPrice.selectBestPrice(@"SELECT * FROM BestPrice WHERE asset = @asset ORDER BY idBestPrice DESC LIMIT 1", asset);

        if (orderBookRecord == null) {
            return (StatusCode(500, $"No Data to execute {operation} on asset: {asset} "));
        }

        OrderBookService orderBookService = new OrderBookService(orderBookRecord);


        BestPriceTrade bestPriceTrade = new BestPriceTrade();

       

        switch (operation) {
            case "buy":
                bestPriceTrade = orderBookService.makeBuy(quantity);
                break;
            case "sell":
                bestPriceTrade = orderBookService.makeSell(quantity);
                break;
            default:
                return (StatusCode(500, "Invalid operation. Use 'sell' or 'buy'"));
        }
        
        return CreatedAtAction(nameof(Create), new {  }, bestPriceTrade);
    }
   
}
