public class BookItem { 

    public BookItem ()
    {
    }


    public BookItem (double price,double quantity )
    {
        this.price = price;
        this.quantity = quantity;   
    }

    public double price { get; set; }
    public double quantity { get; set; }
}