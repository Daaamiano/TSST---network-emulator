using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Router
{
    class Program
    {
        static void Main(string[] args)
        {
            Router router = new Router();
            router.Start(); 
         }

    }
}
