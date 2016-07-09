using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1.RiotAPI.Exceptions
{
    public class RateLimitException : Exception
    {
        public int millisToWait { get; }

        public RateLimitException(string message, int millisToWait) : base(message)
        {
            this.millisToWait = millisToWait;
        }
    }
}
