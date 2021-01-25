using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ApprovalTests;
using ApprovalTests.Reporters;
using FluentAssertions;
using Mafia.Domain;
using NUnit.Framework;

namespace Tests.IntegrationTests
{
    [TestFixture]
    public class Tests
    {
        private class StringInterface : IUserInterface
        {
            private Dictionary<Role, IPerson> targets;
            private StringBuilder result = new StringBuilder();
            
            public Task AskForInteractionTarget(IEnumerable<IPerson> players, Role role, ICity city)
            {
                result.Append(role.DayTime == DayTime.Night ? "Город засыпает" : "Город просыпается");
                result.Append("\n");

                targets = new Dictionary<Role, IPerson>();
                
                var victims = new List<IPerson>();
                foreach (var player in players)
                {
                    result.Append($"Просыпается {role.Name} {player.Name}");
                    result.Append("\n");
                    var name = city.Population
                        .Where(person => person.NightRole is null || !person.NightRole.Equals(role))
                        .First(person => person.IsAlive).Name;
                    victims.Add(city.GetPersonByName(name));
                }

                if (victims.Count == 0)
                    targets[role] = null;
                else
                    targets[role] = victims[0];
                
                return Task.CompletedTask;
            }

            public Task<IPerson> GetInteractionTarget(Role role, ICity city)
            {
                return Task.FromResult(targets.ContainsKey(role) ? targets[role] : null);
            }

            public Task TellResults(ICity city, DayTime dayTime)
            {
                foreach (var pair in city.LastChanges)
                    result.Append($"{pair.Key.Name} {pair.Value}");
                result.Append("\n");
                result.Append("\n");
                return Task.CompletedTask;
            }

            public Task TellGameResult(WinState state, ICity city)
            {
                switch (state)
                {
                    case WinState.MafiaWins:
                        result.Append("Мафия победила");
                        break;
                    case WinState.InProcess:
                        result.Append("Ничья");
                        break;
                    case WinState.PeacefulWins:
                        result.Append("Мирные победили");
                        break;
                    case WinState.PsychoWins:
                        result.Append("Психи захватили мир!1!");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(state), state, null);
                }
                result.Append("\n");
                return Task.CompletedTask;
            }

            public string GetGameProcess() => result.ToString();
        }
        
        
        [Test]
        [MethodImpl(MethodImplOptions.NoInlining)]
        [UseReporter(typeof(DiffReporter))]
        public void PeacefulWins()
        {
            var settings = new Settings(Settings.DefaultWinCondition,
                new Dictionary<Role, int> {[new MafiaRole()] = 10}, 0);
            var city = new City(settings.GeneratePopulation(new[]
                        {
                            "Timofey",
                            "ЛенОЧКА",
                            "Android",
                            "Дыня",
                            "God",
                            "Степан",
                            "Николай"
                        },
                        new Random(156))
                    .ToHashSet(),
               settings.CityName);
            var inter = new StringInterface();
            var game = new Game(settings, city, inter);
            game.StartGame().Wait();
            game.GetGameStatus().Should().Be(WinState.PeacefulWins);
            Approvals.Verify(inter.GetGameProcess());
        }
        
        [Test]
        [MethodImpl(MethodImplOptions.NoInlining)]
        [UseReporter(typeof(DiffReporter))]
        public void MafiaWins()
        {
            var settings = new Settings(Settings.DefaultWinCondition,
                new Dictionary<Role, int> {[new MafiaRole()] = 60}, 0);
            var city = new City(settings.GeneratePopulation(new[]
                        {
                            "Timofey",
                            "Леночка",
                            "Android",
                            "Дыня",
                            "God",
                            "Степан"
                        },
                        new Random(4))
                    .ToHashSet(),
                settings.CityName);
            var inter = new StringInterface();
            var game = new Game(settings, city, inter);
            game.StartGame().Wait();
            game.GetGameStatus().Should().Be(WinState.MafiaWins);
            Approvals.Verify(inter.GetGameProcess());
        }
        
        [Test]
        [MethodImpl(MethodImplOptions.NoInlining)]
        [UseReporter(typeof(DiffReporter))]
        public void PsychoWins()
        {
            var settings = new Settings(Settings.SaneWinCondition, Settings.CrazyNosyBizarreTown.PlayerDistribution,
                0);
            var city = new City(settings.GeneratePopulation(new[]
                        {
                            "Timofey",
                            "Леночка",
                            "Android",
                            "Дыня",
                            "Степан"
                        },
                        new Random(4))
                    .ToHashSet(),
                settings.CityName);
            var inter = new StringInterface();
            var game = new Game(settings, city, inter);
            game.StartGame().Wait();
            game.GetGameStatus().Should().Be(WinState.PsychoWins);
            Approvals.Verify(inter.GetGameProcess());
        }
    }
}