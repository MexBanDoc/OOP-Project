using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Mafia.Domain;
using NUnit.Framework;

namespace Tests.IntegrationTests
{
    [TestFixture]
    public class Tests
    {
        private class ConsoleInterface : IUserInterface
        {
            public IPerson AskForInteractionTarget(IEnumerable<IPerson> players, Role role, ICity city)
            {
                if (role.dayTime == DayTime.Night)
                    Console.WriteLine("Город засыпает");
                else 
                    Console.WriteLine("Город просыпается");
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

            public void TellResults(ICity city, DayTime dayTime)
            {
                foreach (var pair in city.LastChanges)
                    Console.Write($"{pair.Key.Name} {pair.Value}");
                Console.WriteLine();
                Console.WriteLine();
            }

            public void TellGameResult(WinState state, ICity city)
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
        public void PeacefulWins()
        {
            var game = new Game(Settings.Default, new ConsoleInterface());
            game.StartGame();
            game.GetGameStatus().Should().Be(WinState.PeacefulWins);
        }

        [Test]
        public void MafiaWins()
        {
            var game = new Game(new Settings(
                Settings.DefaultWinCondition,
                new List<Tuple<Role, int>>{
                    Tuple.Create((Role)new CitizenRole(), 6), Tuple.Create((Role)new MafiaRole(), 3)},
                6), new ConsoleInterface());
            game.StartGame();
            game.GetGameStatus().Should().Be(WinState.MafiaWins);
        }
    }
}