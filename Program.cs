using Microsoft.Data.Sqlite;

DBBestPrice dbBestPrice = new DBBestPrice("BestPrice.db");
dbBestPrice.createTables();
dbBestPrice.connect();



// for Debug
// Func<OrderBookRecord, bool> handleRecord =  (OrderBookRecord bookRecord) => 
// {
//     Console.WriteLine($"Blog {bookRecord.idBestPrice}: {bookRecord.orderBookData.timestamp}: {bookRecord.orderBookData.microtimestamp}");
//     return true;
// };


// OrderBookData bookData = new OrderBookData();
// bookData.asset = "btc";
// bookData.timestamp = "1234567890";
// bookData.microtimestamp = "33445566" ;
// bookData.bookAsksItems.Add(new BookItem(416.0f, 0.13f));
// bookData.bookAsksItems.Add(new BookItem(417.0f, 0.14f));
// bookData.bookBidsItems.Add(new BookItem(427.0f, 0.24f));



// //dbBestPrice.insertBestPrice(bookData);
// //dbBestPrice.insertBestPrice(bookData);

// DBBestPrice dbBestPrice2 = new DBBestPrice("BestPrice.db");
// dbBestPrice2.connect();
// dbBestPrice2.selectBestPrice(handleRecord, @"Select * from BestPrice") ;

OrderBookApp orderBookApp = new OrderBookApp(dbBestPrice);

Thread thread = new Thread(() => orderBookApp.execute());
thread.Start();
Console.WriteLine("Main thread does some work, then waits.");



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
thread.Join();
