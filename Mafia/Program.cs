using System;
using System.Threading;
using Mafia.App;

namespace Mafia
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("U alive now");
            var bot = new TgBot();
            Thread.Sleep(-1);
            // var Game = new Game(Settings.Default, bot);
            
            // TODO: change pool to commands (organize state checking)
            // TODO: change settings while recording people
            // TODO: create cool settings presets
            // TODO: split responsibilities of how to write and what to write
        }
    }
}
