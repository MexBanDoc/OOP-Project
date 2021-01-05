using System;
using Mafia.App;

namespace Mafia
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("U alive now");
            var bot = new TgBot();
            Console.ReadKey();
            // var Game = new Game(Settings.Deadly, bot);
            
            // TODO: split responsibilities of how to write and what to write
            
            // TODO: write semaphore method
        }
    }
}
