using System;
using Mafia.App;
using Mafia.Domain;

namespace Mafia
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("U alive now");
            var Game = new Game(Settings.Default, new TGBot());
        }
    }
}
