using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1.Database.Exceptions
{
    public class StaleCacheException : Exception
    {
        public StaleCacheException(string message) : base(message)
        {

        }
    }
}
