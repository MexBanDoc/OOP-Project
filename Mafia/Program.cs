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
            // var Game = new Game(Settings.Default, bot);

            // TODO: change poll to commands (organize state checking) (use cityName)
            // TODO: create cool settings presets
            // TODO: change settings while recording people
            // TODO: split responsibilities of how to write and what to write
            // TODO: set wait time in settings
            
            // /Krutovsky_Danya_(@peace)@CumCockCity -> (string: person)
        }
    }
}
