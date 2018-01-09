# Money

The `Money` struct prevents you accidently adding numbers together that repesents *different currencies*.

The `Money` constructor requires a `decimal` amount and an [ISO currency code](https://en.wikipedia.org/wiki/ISO_4217) `string`.

Basic operators include:

* `-` for negation
* `*` for multiplication
* `/` for division
* `==` for equality
* `!=` for inequality

If different currencies are passed to any of the following operators then an `InvalidOperationException` will be thrown:

* `+` for addition
* `-` for substraction
* `>` for greater than comparision
* `<` for less than comparison
* `>=` for greater than or equal to comparision
* `<=` for less than or equal to comparison

There also extension methods add to `decimal` for conversion to some common currencies, i.e. `1m.GBP()`, `2m.USD()` and `3m.EUR()`

## MoneyBag

The `MoneyBag` class is a collection of money in one or more currencies, and has the following methods:

* `Add(Money m)`
* `Substract(Money m)`
* `Contains(string currency)`
* `Money Remove(string currency)`
* `GetEnumerator` to enumerate the totals of each currency

`MoneyBag` also has an `Money this[string currency]` getter property that will return the amount of a currency in the bag, or zero if the bag does not contain the currency.

