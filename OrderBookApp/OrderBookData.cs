using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Globalization;

public class OrderBookData 
{
    public OrderBookData()
    {
    }
    public OrderBookData(string asset, JToken jObjectData)
    {
        this.asset = asset;
        
        this.timestamp = jObjectData["timestamp"]!.ToString();
        this.microtimestamp = jObjectData["microtimestamp"]!.ToString();
        this.bookBidsItems = getListBookItem(jObjectData, "bids");
        this.bookAsksItems = getListBookItem(jObjectData, "asks");

    }

    public List<BookItem> getListBookItem(JToken jObjectData, string type)
    {
        List<BookItem> bookList = new List<BookItem>();
        foreach (var item in jObjectData[type]!) {
            BookItem bookItem = new BookItem();
            bookItem.price = double.Parse(item[0]!.ToString(), CultureInfo.InvariantCulture.NumberFormat); 
            bookItem.quantity = double.Parse(item[1]!.ToString(), CultureInfo.InvariantCulture.NumberFormat);
            bookList.Add(bookItem);
        }

        return bookList;
    }
    public string asset = string.Empty;
    public string timestamp = string.Empty;
    public string microtimestamp = string.Empty;
    public List<BookItem> bookAsksItems = new List<BookItem>();
    public List<BookItem> bookBidsItems = new List<BookItem>();


}