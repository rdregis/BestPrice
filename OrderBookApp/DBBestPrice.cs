using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System.Globalization;


public class DBBestPrice {
    public DBBestPrice(string dbName)
    {
        this.dbName = dbName;
    }

    public void createTables()  
    {
        DBSqlite dbSqlite = new DBSqlite();
        dbSqlite.dbConnection(this.dbName);

        try {
            dbSqlite.executeCommand(@"CREATE TABLE if not exists BestPrice(
                                        idBestPrice INTEGER NOT NULL, 
                                        asset Varchar(10) NOT NULL,
                                        timestamp Varchar(10) NOT NULL, 
                                        microtimestamp VarChar(10) NOT NULL,
                                        creationTime VarChar(10) NOT NULL,
                                        PRIMARY KEY (idBestPrice)
                                        )"
                                    );
            dbSqlite.executeCommand(@"CREATE TABLE if not exists BestPriceAsks(
                                        idBestPrice INTEGER NOT NULL, 
                                        idAsks INTEGER, 
                                        price REAL NOT NULL,
                                        quantity REAL NOT NULL,
                                        PRIMARY KEY (idBestPrice, idAsks),
                                        FOREIGN KEY (idBestPrice) REFERENCES BestPrice (idBestPrice)
                                        )"
                                    );
            dbSqlite.executeCommand(@"CREATE TABLE if not exists BestPriceBids(
                                        idBestPrice INTEGER NOT NULL, 
                                        idBids INTEGER, 
                                        price REAL NOT NULL,
                                        quantity REAL NOT NULL,
                                        PRIMARY KEY (idBestPrice, idBids),
                                        FOREIGN KEY (idBestPrice) REFERENCES BestPrice (idBestPrice)
                                        )"
                                    );
        }
        catch(Exception ex) {
            Console.WriteLine("" + ex.Message);
            System.Environment.Exit(1);
        }
        dbSqlite.dbDisconnection();
    }

    public void connect()
    {
        this.dbSqlite = new DBSqlite();
        this.dbSqlite.dbConnection(this.dbName);
    }

    public void disconnect()
    {
        this.dbSqlite!.dbDisconnection();
    }

    public void insertBestPrice(OrderBookData orderBookData)
    {
        var transaction = dbSqlite!.createTransaction();
        try {
            var cmd = dbSqlite.createCommand(@"SELECT COUNT(*) as count FROM BestPrice"
                                            );
            //cmd.Parameters.AddWithValue("@asset", orderBookData.asset);
            int id = 0;
            using (var reader = cmd.ExecuteReader()) {
                while (reader.Read()) {
                    id = int.Parse(reader["count"].ToString()!);
                }
            }
            cmd.Parameters.AddWithValue("@asset", orderBookData.asset);
            cmd = dbSqlite.createCommand(@"INSERT INTO BestPrice( asset, idBestPrice,  timestamp, microtimestamp, creationTime) 
                                                values ( @asset, @idBestPrice, @timestamp, @microtimestamp, @creationTime
                                                )"
                                            );
            cmd.Parameters.AddWithValue("@asset", orderBookData.asset);
            cmd.Parameters.AddWithValue("@idBestPrice", ++id );
            cmd.Parameters.AddWithValue("@timestamp", orderBookData.timestamp);
            cmd.Parameters.AddWithValue("@microtimestamp", orderBookData.microtimestamp);
            long xxx = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            cmd.Parameters.AddWithValue("@creationTime", DateTimeOffset.Now.ToUnixTimeSeconds());
            cmd.ExecuteNonQuery();

            foreach (var item in orderBookData.bookAsksItems.Select((value, index) => new { index, value })) {
                cmd = dbSqlite.createCommand(@"INSERT INTO BestPriceAsks( idBestPrice, idAsks, price, quantity) 
                                                    values ( @idBestPrice, @idAsks, @price, @quantity
                                                    )"
                                                );
                cmd.Parameters.AddWithValue("@idBestPrice", id );
                cmd.Parameters.AddWithValue("@idAsks", item.index + 1);
                cmd.Parameters.AddWithValue("@price", item.value.price);
                cmd.Parameters.AddWithValue("@quantity", item.value.quantity);
                cmd.ExecuteNonQuery();
            };
            foreach (var item in orderBookData.bookBidsItems.Select((value, index) => new { index, value })) {
                cmd = dbSqlite.createCommand(@"INSERT INTO BestPriceBids( idBestPrice, idBids, price, quantity) 
                                                    values ( @idBestPrice, @idBids, @price, @quantity
                                                    )"
                                                );
                cmd.Parameters.AddWithValue("@idBestPrice", id );
                cmd.Parameters.AddWithValue("@idBids", item.index + 1);
                cmd.Parameters.AddWithValue("@price", item.value.price);
                cmd.Parameters.AddWithValue("@quantity", item.value.quantity);
                cmd.ExecuteNonQuery();
            };
            transaction.Commit();
            
        }
        catch(Exception ex) {
            transaction.Rollback();
            Console.WriteLine("" + ex.Message);
            System.Environment.Exit(1);
        }
    }

    
    public OrderBookRecord selectBestPrice(string select, string? asset = null )
    {
        var cmd = dbSqlite!.createCommand(select);
        if (asset != null) {
            cmd.Parameters.AddWithValue("@asset",asset);
        }
        using (var reader = cmd.ExecuteReader()) {
            while (reader.Read()) {
               return(createOrderBookRecord(reader));
               
            }

        }
        return null!;
    }
    public void selectBestPrice(Func<OrderBookRecord, bool> handleRecord, string select)
    {
        var cmd = dbSqlite!.createCommand(select);
        using (var reader = cmd.ExecuteReader()) {
            while (reader.Read()) {
               handleRecord(createOrderBookRecord(reader));
            }
        }
    }
    public void agregateBestPrice(Func<AggregateBestPriceRecord, bool> handleAgregateRecord, string select)
    {
        var cmd = dbSqlite!.createCommand(select);
        using (var reader = cmd.ExecuteReader()) {
            while (reader.Read()) {
               handleAgregateRecord(createAgregateRecord(reader));
            }
        }
    }
    private AggregateBestPriceRecord createAgregateRecord(SqliteDataReader reader)
    {
         AggregateBestPriceRecord aggregateBestPriceRecord = new AggregateBestPriceRecord();

         aggregateBestPriceRecord.asset =  reader["asset"].ToString()!;
         aggregateBestPriceRecord.asksPrice =  double.Parse(reader["asksPrice"].ToString()!);
         aggregateBestPriceRecord.bidsPrice =  double.Parse(reader["bidsPrice"].ToString()!);
         aggregateBestPriceRecord.asksQuantity =  double.Parse(reader["asksQuantity"].ToString()!);
         aggregateBestPriceRecord.bidsQuantity =  double.Parse(reader["bidsQuantity"].ToString()!);

         return(aggregateBestPriceRecord);


    }
    private OrderBookRecord createOrderBookRecord(SqliteDataReader reader)
    {

        OrderBookRecord orderBookRecord = new OrderBookRecord();

        orderBookRecord.idBestPrice = int.Parse(reader["idBestPrice"].ToString()!);
        orderBookRecord.orderBookData.asset = reader["asset"].ToString()!;
        orderBookRecord.orderBookData.timestamp = reader["timestamp"].ToString()!;
        orderBookRecord.orderBookData.microtimestamp = reader["microtimestamp"].ToString()!;


        var cmd = dbSqlite!.createCommand(@"Select * from BestPriceAsks WHERE idBestPrice = @idBestPrice");
        cmd.Parameters.AddWithValue("@idBestPrice", orderBookRecord.idBestPrice );
        using (var reader2 = cmd.ExecuteReader()) {
            while (reader2.Read()) {
                BookItem bookItem = new BookItem(
                    double.Parse(reader2["price"].ToString()!),
                    double.Parse(reader2["quantity"].ToString()!));
                orderBookRecord.orderBookData.bookAsksItems.Add(bookItem);
                          }
        }

        cmd = dbSqlite!.createCommand(@"Select * from BestPriceBids WHERE idBestPrice = @idBestPrice");
        cmd.Parameters.AddWithValue("@idBestPrice", orderBookRecord.idBestPrice );
        using (var reader2 = cmd.ExecuteReader()) {
            while (reader2.Read()) {
                BookItem bookItem = new BookItem(
                    double.Parse(reader2["price"].ToString()!),
                    double.Parse(reader2["quantity"].ToString()!));
                orderBookRecord.orderBookData.bookBidsItems.Add(bookItem);
                          }
        }
        
        return(orderBookRecord);

    }
    private string dbName; 
    private DBSqlite? dbSqlite;
}