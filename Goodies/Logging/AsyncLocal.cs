#if NET452
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Remoting.Messaging;

namespace BusterWood.Logging
{
    public class AsyncLocal<T>
    {
        readonly string _id = Guid.NewGuid().ToString();

        public T Value
        {
            get { return (T)CallContext.LogicalGetData(_id); }
            set { CallContext.LogicalSetData(_id, value); }
        }
    }
}
#endif
