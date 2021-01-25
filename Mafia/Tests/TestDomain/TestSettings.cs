using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Mafia.Domain;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Tests.TestDomain
{
    [TestFixture]
    public class TestSettings
    {
        public static IEnumerable<TestCaseData> TestCases
        {
            get
            {
                    yield return new TestCaseData(new[] {"Tim", "Helen", "Даня"},
                        new[]
                        {
                            new Person(new PeacefulRole(), null, "Tim"),
                            new Person(new PeacefulRole(), null, "Helen"),
                            new Person(new PeacefulRole(), new MafiaRole(), "Даня")
                        });
                    yield return new TestCaseData(new string[0], new Person[0]);
            }
        }
        
        [TestCaseSource("TestCases")]
        public static void GeneratePopulation(string[] names, Person[] people)
        {
            Settings.Deadly.GeneratePopulation(names, new Random(4)).Should().BeEquivalentTo(people);
        }

        [Test]
        public static void TestDefaultWinCondition()
        {
            var settings = new Settings(Settings.DefaultWinCondition, new Dictionary<Role, int>(),
                0);
            Settings.DefaultWinCondition(new City(Settings.Detective.GeneratePopulation(new[]
                        {
                            "Timofey",
                            "ЛенОЧКА",
                            "Android",
                            "Дыня"
                        },
                        new Random())
                    .ToHashSet(),
                Settings.Detective.CityName)).Should().Be(WinState.InProcess);
            Settings.DefaultWinCondition(new City(Settings.Detective.GeneratePopulation(new[]
                        {
                            "Timofey",
                            "ЛенОЧКА"
                        },
                        new Random())
                    .ToHashSet(),
                Settings.Detective.CityName)).Should().Be(WinState.MafiaWins);
            Settings.DefaultWinCondition(new City(settings.GeneratePopulation(new[]
                        {
                            "Timofey",
                            "ЛенОЧКА",
                            "Android",
                            "Дыня",
                            "Степан",
                            "Николай",
                            "Персик"
                        },
                        new Random())
                    .ToHashSet(),
                settings.CityName)).Should().Be(WinState.PeacefulWins);
        }
        
        [Test]
        public static void TestSaneWinCondition()
        {
            Settings.SaneWinCondition(new City(Settings.CrazyNosyBizarreTown.GeneratePopulation(new[]
                        {
                            "Timofey",
                            "ЛенОЧКА",
                            "Android",
                            "Дыня"
                        },
                        new Random())
                    .ToHashSet(),
                Settings.CrazyNosyBizarreTown.CityName)).Should().Be(WinState.InProcess);
            Settings.SaneWinCondition(new City(Settings.CrazyNosyBizarreTown.GeneratePopulation(new[]
                        {
                            "Timofey",
                            "ЛенОЧКА",
                            "Android"
                        },
                        new Random())
                    .ToHashSet(),
                Settings.CrazyNosyBizarreTown.CityName)).Should().Be(WinState.PsychoWins);
            var settings = new Settings(Settings.SaneWinCondition, new Dictionary<Role, int>(),
                0);
            Settings.SaneWinCondition(new City(settings.GeneratePopulation(new[]
                        {
                            "Timofey",
                            "ЛенОЧКА",
                            "Android",
                            "Дыня",
                            "Степан",
                            "Николай",
                            "Персик"
                        },
                        new Random())
                    .ToHashSet(),
                settings.CityName)).Should().Be(WinState.PeacefulWins);
        }
    }
}