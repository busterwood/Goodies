# BusterWood.Goddies namespace

## Generic primative types

I have recently fixed production bugs where integer Ids were being mixed up when accessing a cache, e.g. client and order ID being mixed up.
To prevent this kind of problem the `BusterWood.Goodies` namespace contains several typed primative structs:

* `Int32<T>` - e.g.prevent customer and order IDs being mixed up
* `Int64<T>` 
* `Guid<T>` 
* `Decimal<T>` - e.g. prevent quantity and price being mixed up
* `String<T>` - e.g. prevent customer code and customer name being mixed up
* `Id<TEntity, TKey>` for the fully generic version (but more typing)

These types provide compile time error checking to make sure the domain type `T` matches.

For example:

```
class Customer {
	public Int32<Customer> Id { get; set; }
}

class Order {
	public Int32<Order> Id { get; set; }
	public Int32<Customer> CustomerId { get; set; }
}

static Dictionary<Int32<Order>, Order> orderCache = new Dictionary<Int32<Order>, Order>();

...
var order = orderCache[customer.Id]; // compile time error, expects an Int32<Order>
```

Other feature of these generic types include:
* no per-instance memory overhead, i.e. `Int32<T>` is same size as `int`
* implements `IEquatable<>`
* implements `IXmlSerializable` to transparently serialize and deserialize to and from XML, i.e. `Int32<T>` is read and written as `int`
* `==` and `!=` operators
* explicit casts to and from the base type
* debugger display of the value and domain type