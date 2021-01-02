using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Mafia.Domain;
using NUnit.Framework;

namespace Tests.TestDomain
{
    [TestFixture]
    public class TestCity
    {
        private static readonly City City =
            new City(new List<IPerson>(Settings.Default.GeneratePopulation(new[]
                    {
                        "Mafia",
                        "Peaceful1",
                        "Peaceful2",
                        "Peaceful3"
                    },
                    new Random(1984))),
                Settings.Default.CityName);

        [Test]
        public void TestConstructor()
        {
            var mafia = new MafiaRole();
            var doctor = new HealerRole();
            var citizen = new CitizenRole();
            var localCity = new City(new List<IPerson>(
                new[]
                {
                    new Person(citizen, doctor, "Bob"),
                    new Person(citizen, mafia, "Alice"),
                    new Person(citizen, null, "Ira"),
                }), Settings.Default.CityName);

            localCity.Roles.Count.Should().Be(3);
            localCity.Roles.Should().BeEquivalentTo(citizen, mafia, doctor);
        }
        
        public static IEnumerable<TestCaseData> TestGetPersonByName
        {
            get
            {
                yield return new TestCaseData(null, null);
                yield return new TestCaseData("", null);
                yield return new TestCaseData("Person77", null);
                yield return new TestCaseData("Mafia", City.Population.First());
            }
        }

        [TestCaseSource(nameof(TestGetPersonByName))]
        public void GetPersonByName(string name, IPerson person)
        {
            City.GetPersonByName(name).Should().BeEquivalentTo(person);
        }

        [Test]
        public void StartDay()
        {
            City.StartDay();
            City.DayTime.Should().Be(DayTime.Day);
            City.LastChanges.Should().BeEmpty();
            foreach (var person in City.Population)
                person.IsImmortal.Should().BeFalse();
        }

        [Test]
        public void StartNight()
        {
            City.StartNight();
            City.DayTime.Should().Be(DayTime.Night);
            City.LastChanges.Should().BeEmpty();
        }

        public static IEnumerable<TestCaseData> TestAddChange
        {
            get
            {
                yield return new TestCaseData(null, null); 
                
            }
        }

        [TestCaseSource(nameof(TestAddChange))]
        public void AddChangeSuccess(IPerson target, Role role)
        {
            
        }

        [Test]
        public static void AddChangeError()
        {
            var person = new Person(new CitizenRole(), new HealerRole(), "Bob");
            City.LastChanges.ContainsKey(person).Should().BeFalse();
            City.AddChange(person, new MafiaRole().Interact(person));
            City.LastChanges.Count.Should().Be(1);
            City.LastChanges.Should().Contain(person, PersonState.Killed);
        }
    }
}