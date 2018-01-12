using System;
using System.Collections.Generic;

namespace BusterWood.Mapping
{
    class ConsoleObserver : IObserver<string>
    {
        public List<string> Values = new List<string>();

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
            Console.Error.WriteLine(error);
        }

        public void OnNext(string value)
        {
            Console.WriteLine(value);
            Values.Add(value);
        }
    }
}
