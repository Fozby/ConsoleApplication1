using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            Mongo mongo = new Mongo();

            while (true)
            {
                String input = Console.ReadLine();

                if (input == "get")
                {
                    Task.Run(async () => await mongo.getRec());
                }
                if (input == "insert")
                {
                    Task.Run(async () => await mongo.insertRec());
                }
                if (input == "count")
                {
                    Task.Run(async () => await mongo.getCount());
                }
            }
        }
    }

}
