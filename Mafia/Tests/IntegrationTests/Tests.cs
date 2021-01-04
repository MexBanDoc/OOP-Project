using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            private Dictionary<Role, IPerson> targets;
            
            public Task AskForInteractionTarget(IEnumerable<IPerson> players, Role role, ICity city)
            {
                Console.WriteLine(role.DayTime == DayTime.Night ? "Город засыпает" : "Город просыпается");

                targets = new Dictionary<Role, IPerson>();
                
                var victims = new List<IPerson>();
                foreach (var player in players)
                {
                    Console.WriteLine($"Просыпается {role.Name} {player.Name}");
                    var name = city.Population
                        .Where(person => person.NightRole is null || !person.NightRole.Equals(role))
                        .First(person => person.IsAlive).Name;
                    victims.Add(city.GetPersonByName(name));
                }
                
                targets[role] = victims[new Random().Next(victims.Count - 1)];
                return Task.CompletedTask;
            }

            public Task<IPerson> GetInteractionTarget(Role role, ICity city)
            {
                return Task.FromResult(targets.ContainsKey(role) ? targets[role] : null);
            }

            public Task TellResults(ICity city, DayTime dayTime)
            {
                foreach (var pair in city.LastChanges)
                    Console.Write($"{pair.Key.Name} {pair.Value}");
                Console.WriteLine();
                Console.WriteLine();
                return Task.CompletedTask;
            }

            public Task TellGameResult(WinState state, ICity city)
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
                return Task.CompletedTask;
            }
        }
        
        [Test]
        public void PeacefulWins()
        {
            var city = new City(Settings.Default.GeneratePopulation(new[]
                        {
                            "Timofey",
                            "ЛЕНА",
                            "Android",
                            "Дыня",
                            "God"
                        },
                        new Random())
                    .ToHashSet(),
                Settings.Default.CityName);
            var game = new Game(Settings.Default, city, new ConsoleInterface());
            game.StartGame().Wait();
            game.GetGameStatus().Should().Be(WinState.PeacefulWins);
        }

        [Test]
        public void MafiaWins()
        {
            var settings = new Settings(
                Settings.DefaultWinCondition,
                new Dictionary<Role, int>
                {
                    [new MafiaRole()] = 50
                }, 0);
            var city = new City(settings.GeneratePopulation(new[]
                        {
                            "Timofey",
                            "ЛЕНА",
                            "Android",
                            "Дыня",
                            "God"
                        },
                        new Random())
                    .ToHashSet(),
                settings.CityName);
            var game = new Game(settings, city, new ConsoleInterface());
            game.StartGame().Wait();
            game.GetGameStatus().Should().Be(WinState.MafiaWins);
        }
    }
}