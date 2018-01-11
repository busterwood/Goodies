namespace BusterWood.Ducks
{
    enum MissingMethods
    {
        /// <summary>Calling Duck.Cast(...) will throw a <see cref="InvalidCastException"/> if the target object does not fully implement the interface</summary>
        /// <remarks>This is the default behaviour of all Duck.Cast() methods</remarks>
        InvalidCast,

        /// <summary>Calling Mock.Cast(...) always return a proxy, but an <see cref="NotImplementedException"/> will be thrown by any interface methods not present on the target object</summary>
        /// <remarks>Useful for mocking interfaces for unit testing</remarks>
        NotImplemented
    }
}