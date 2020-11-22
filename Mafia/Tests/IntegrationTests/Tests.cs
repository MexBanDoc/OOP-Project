using System;
using System.Collections.Generic;
using System.Linq;
using Mafia.Domain;
using NUnit.Framework;

namespace TestMafia
{
    [TestFixture]
    public class Tests
    {
        private class ConsoleInterface : IUserInterface
        {
            public IPerson AskForInteractionTarget(IEnumerable<IPerson> players, Role role, ICity city)
            {
                Console.WriteLine("Город засыпает");
                var victims = new List<IPerson>();
                foreach (var player in players)
                {
                    Console.WriteLine($"Просыпается {role.Name} {player.Name}");
                    var name = city.Population
                        .Where(person => person.NightRole is null || !person.NightRole.Equals(role))
                        .First(person => person.IsAlive).Name;
                    victims.Add(city.GetPersonByName(name));
                }
                
                return victims[new Random().Next(victims.Count - 1)];
            }

            public void StartGame()
            {
                Console.WriteLine("U alive now");
                Console.WriteLine();
            }

            public void TellResults(ICity city, DayTime dayTime)
            {
                foreach (var pair in city.LastChanges)
                    Console.Write($"{pair.Key.Name} {pair.Value}");
                Console.WriteLine();
            }

            public void TellGameResult(WinState state)
            {
                switch (state)
                {
                    case WinState.MafiaWins:
                        Console.WriteLine("Мафия победила");
                        break;
                    case WinState.InProcess:
                        Console.WriteLine("Ничья");
                        break;
                    case WinState.PeacefulWins:
                        Console.WriteLine("Мирные победили");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(state), state, null);
                }
            }
        }
        
        [Test]
        public void Test1()
        {
            var Game = new Game(Settings.Default, new ConsoleInterface());
            Game.StartGame();
            
        }
    }
}