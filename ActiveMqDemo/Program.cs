using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActiveMq
{
    class Program
    {
        static void Main(string[] args)
        {
            for (int i = 0; i < 50; i++)
            {
                //Console.WriteLine(i);
               // MqHelper.ProduceMessageToQueue("CbsTestQueue", "你好");
                MqHelper.ConsumeMessageFromQueue("CbsTestQueue");
            }
            
        }
    }
}
