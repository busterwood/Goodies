# BusterWood.Mapping

Sort of a replacement for Automapper but *much simpler and smaller*.

Performance is "good" as `BusterWood.Mapping` uses the DLR to create and JIT compile methods to do the mapping, and these methods are cached.

## Copying an object

`BusterWood.Mapping` contains an extension method for all objects called `Copy<T>()` which returns a *shallow* copy of the original object. The type being cloned *must* have a parameterless contructor, then all public properties and fields are copied.

`BusterWood.Mapping` can also clone sequences of object via the `CopyAll<T>()` extension which takes a `IEnumerable<T>` and returns an `IEnumerable<T>`.

To allow for customized copying the following overloads of `CopyAll<T>()` take an extra action to be performed on each copy:
* `CopyAll<T>(Func<T, T>)` calls the supplied function for each copied object 
* `CopyAll<T>(Func<T, T, int>)` calls the supplied function for each mapped object passing the zero based index of the object 

## Copying between different types

You can copy an object of one type to another type using the `Copy<TFrom,TTo>()` extension method.  The type being mapped *to* **must** have a parameterless contructor, then all readable public properties (and fields) of the source type are copied to properties (or fields) of the target type.  

`BusterWood.Mapping` can also copy sequences of objects via the `CopyAll<TFrom,TTo>()` extension which takes a `IEnumerable<TFrom>` and returns an `IEnumerable<TTo>`.  `CopyAll<TFrom,TTo>()` has overloads that allow the mapping to be customized:

* `CopyAll<TFrom,TTo>(Func<TFrom,TTo>)` calls the supplied function for each mapped object
* `CopyAll<TFrom,TTo>(Func<TFrom,TTo, int>)` calls the supplied function for each mapped object passing the zero based index of the object 

## Type compatibility

When copying data types must be compatible in *some sense*, the following lists the type compatibility rules:

| Source Type                                       | Target Type                                                                                                              |
|---------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------|
| Any numeric type or enum                          | Any numeric type or any enum                                                                                             |
| `Nullable<T>` where T is any numeric type or enum | any numeric type or any enum. `default(T)` is used as the value if value is null                                         |
| `Nullable<T>` where T is any numeric type or enum | `Nullable<T>` where T is any numeric type or enum                                                                        |
| any type other                                    | type must match or be [assignable](https://msdn.microsoft.com/en-us/library/system.type.isassignablefrom(v=vs.110).aspx) |

## Name compatibility

For `Copy`, `CopyAll` and all the data mappings, the following rules apply when looking for the destination field or property to map to:

1. the source name (case insensitive)
2. if the name ends with 'ID' then try the name without 'ID'  (case insensitive)
3. if the name does *not* end with 'ID' then try the name with 'Id' suffix added (case insensitive)
4. the above names with underscores removed  (case insensitive)
5. the above names with the target class name prefix removed (case insensitive)

Note that the rules are following in the above sequence, and that rules 2 & 3 only apply when the data type of the field being mapped is a primative type, and enum, or a nullable<T> of those types.

For example, if the source name is `ORDER_ID` then the following names would be considered  (shown in perference order):

1. ORDER_ID
2. ORDER_
3. ORDERID
4. ORDER
5. ID     (* this will be considered when mapping from a DbDataReader to a type called `Order`)

Note: name comparison is *case insensitive*.
