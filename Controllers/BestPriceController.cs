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
    public IEnumerable<BestPriceResult> Get()
    {
        List<BestPriceResult> bestPriceList =  new List<BestPriceResult>();


        BestPriceResult bestPriceItemBuy = new BestPriceResult();
        bestPriceItemBuy.Timestamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss.ffffff");
        bestPriceItemBuy.Operation = "buy";
        bestPriceList.Add(bestPriceItemBuy);
        
        BestPriceResult bestPriceItemSell = new BestPriceResult();
        bestPriceItemSell.Id = 1;
        bestPriceItemSell.Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(DateTimeOffset.Now.ToUnixTimeMilliseconds()).UtcDateTime.ToString();
        bestPriceItemSell.Operation = "sell";
        bestPriceList.Add(bestPriceItemSell);

        return ((IEnumerable<BestPriceResult>)bestPriceList).ToArray();
       

    }
     [HttpGet("{operation}/{asset}/{quantity}")]
    public ActionResult<IEnumerable<BestPriceResult>> Get(string operation, string asset, double quantity)
    {

        DBBestPrice dbBestPrice = new DBBestPrice("BestPrice.db");
        dbBestPrice.connect();      
        OrderBookRecord orderBookRecord = 
                dbBestPrice.selectBestPrice(@"SELECT * FROM BestPrice WHERE asset = @asset ORDER BY idBestPrice DESC LIMIT 1", asset);

        if (orderBookRecord == null) {
            return (StatusCode(500, $"No Data to execute {operation} on asset: {asset} "));
        }

        OrderBookService orderBookService = new OrderBookService(orderBookRecord);


       List<BestPriceResult> bestPriceList =  new List<BestPriceResult>();

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
        
        return ((IEnumerable<BestPriceResult>)bestPriceList).ToArray();
       }

   
}
