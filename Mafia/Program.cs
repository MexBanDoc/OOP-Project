using System;
using System.Threading;
using Mafia.App;
using Mafia.Domain;

namespace Mafia
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("U alive now");
            var bot = new TGBot();
            Thread.Sleep(-1);
            var Game = new Game(Settings.Default, bot);
        }
    }
}
