# BusterWood.Batching

Batches multiple async calls into one or more batched calls, which is useful for batching together database reads.

For example, say you have a data access layer for _orders_ that gets an order via an integer id:
```csharp
...
using BusterWood.Mapper; // easy reading and mapping from the database

public class OrderDataAccess 
{
  private readonly _connectionString;
  
  public OrderDataAccess(string connectionString)
  {
    _connectionString = connectionString;
  }
  
  public async Task<Order> FindByIdAsync(int orderId) 
  {
    using (var cnn = new SqlConnection(_connectionString))
    {
      await cnn.OpenAsync();
      return await cnn.QueryAsync("select * from [order] where order_id=@orderId", new {orderId}).SingleOrDefaultAsync<Order>();
    }
  }
}
```

The `FindByIdAsync` is fine for one-at-a-time order loading, but what if you had hunderds or thousands of asynchonous tasks all reading at the same time?  This is where async batching comes in, grouping to together calls for efficency based on batching time, e.g. group toegther calls for 100ms (the default).

Using `SingleResultAsyncBatcher` or `MultiResultAsyncBatcher` you can reduce thousands of database calls into tens of calls.

Here is an example of batching the above OrderDataAccess call:

```csharp
...
using BusterWood.Mapper; // easy reading and mapping from the database

public class OrderDataAccess 
{
  private readonly _connectionString;
  private readonly SingleResultAsyncBatcher<int, Order> _orderBatcher;
  
  public OrderDataAccess(string connectionString)
  {
    _connectionString = connectionString;
    _orderBatcher = new SingleResultAsyncBatcher<int, Order>(ids => FindByManyIdsAsync(ids));
  }
  
  public async Task<Order> FindByIdAsync(int orderId) 
  {
    return await _orderBatcher.QueryAsync(orderId);
  }

  private async Task<Dictionary<int, Order>> FindByManyIdsAsync(IReadOnlyCollection<int> orderIds) 
  {
    // use a SQL Server Table-Valued-Parameter
    var idsTable = orderIds.ToTableType(new [] { new SqlMetaData("ID", SqlType.Int) }, "IdType");
    using (var cnn = new SqlConnection(_connectionString))
    {
      await cnn.OpenAsync();
      return await cnn.QueryAsync("select o.* from [order] o join @idsTable ids on ids.id = o.id", new {idsTable}).ToDictionaryAsync<int, Order>(ord => ord.Id);
    }
  }
}
```
