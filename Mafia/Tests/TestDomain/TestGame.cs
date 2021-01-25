using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Mafia.Domain;
using Moq;
using NUnit.Framework;

namespace Tests.TestDomain
{
    [TestFixture]
    public class TestGame
    {
        [Test]
        public void DoInteractions()
        {
            var mock = new Mock<IUserInterface>();
            var settings = new Settings(Settings.DefaultWinCondition, Settings.Deadly.PlayerDistribution,
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
            mock.Setup(inter => inter.AskForInteractionTarget(city.Population, new MafiaRole(), city))
                .Returns(Task.CompletedTask);
            mock.Setup(inter => inter.GetInteractionTarget(new MafiaRole(), city))
                .Returns(Task.FromResult(city.Population.FirstOrDefault()));
            var game = new Game(settings, city, mock.Object);
            game.DoInteractions(DayTime.Night).Wait();
            city.LastChanges.Count.Should().Be(1);
            city.LastChanges.First().Value.Should().Be(PersonState.Killed);
            game.DoInteractions(DayTime.Day).Wait();
            city.LastChanges.Count.Should().Be(1);
            city.LastChanges.First().Value.Should().Be(PersonState.Killed);
        }
        
        
    }
}