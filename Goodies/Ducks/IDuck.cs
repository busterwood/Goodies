using System;

namespace BusterWood.Ducks
{
    /// <summary>Implemented by all proxies returned by <see cref="Duck.Cast(object, System.Type)"/> or <see cref="Duck.Cast{T}(object)"/></summary>
    public interface IDuck
    {
        /// <summary>Returns the object that has been wrapped via <see cref="Duck.Cast(object, System.Type)"/> or <see cref="Duck.Cast{T}(object)"/></summary>
        object Unwrap();
    }

    public interface IDuckInstanceFactory
    {
        object Create(object duck);
    }

    public interface IDuckDelegateFactory
    {
        object Create(Delegate duck);
    }
}