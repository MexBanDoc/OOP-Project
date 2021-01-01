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
        private static City city =
            new City(new List<IPerson>(Settings.Default.GeneratePopulation()), Settings.Default.CityName);

        [Test]
        public void TestConstructor()
        {
            var mafia = new MafiaRole();
            var doctor = new HealerRole();
            var citizen = new CitizenRole();
            var city = new City(new List<IPerson>(
                new[]
                {
                    new Person(citizen, doctor, "Bob"),
                    new Person(citizen, mafia, "Alice"),
                    new Person(citizen, null, "Ira"),
                }), Settings.Default.CityName);

            city.Roles.Count.Should().Be(3);
            city.Roles.Should().BeEquivalentTo(citizen, mafia, doctor);
        }
        
        public static IEnumerable<TestCaseData> TestGetPersonByName
        {
            get
            {
                yield return new TestCaseData(null, null);
                yield return new TestCaseData("", null);
                yield return new TestCaseData("Person77", null);
                yield return new TestCaseData("Person0", city.Population.First());
            }
        }

        [TestCaseSource("TestGetPersonByName")]
        public void GetPersonByName(string name, IPerson person)
        {
            city.GetPersonByName(name).Should().BeEquivalentTo(person);
        }

        [Test]
        public void StartDay()
        {
            city.StartDay();
            city.DayTime.Should().Be(DayTime.Day);
            city.LastChanges.Should().BeEmpty();
            foreach (var person in city.Population)
                person.IsImmortal.Should().BeFalse();
        }

        [Test]
        public void StartNight()
        {
            city.StartNight();
            city.DayTime.Should().Be(DayTime.Night);
            city.LastChanges.Should().BeEmpty();
        }

        public static IEnumerable<TestCaseData> TestAddChange
        {
            get
            {
                yield return new TestCaseData(null, null); 
                
            }
        }

        [TestCaseSource("TestAddChange")]
        public void AddChangeSuccess(IPerson target, Role role)
        {
            
        }

        [Test]
        public static void AddChangeError()
        {
            var person = new Person(new CitizenRole(), new HealerRole(), "Bob");
            city.LastChanges.ContainsKey(person).Should().BeFalse();
            city.AddChange(person, new MafiaRole().Interact(person));
            city.LastChanges.Count.Should().Be(1);
            city.LastChanges.Should().Contain(person, PersonState.Killed);
        }
    }
}