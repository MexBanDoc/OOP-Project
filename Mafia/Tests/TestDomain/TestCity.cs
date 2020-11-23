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
            new City(new List<IPerson>(Settings.Default.GeneratePopulation()));

        [Test]
        public void TestConstructor()
        {
            
        }
        
        public static IEnumerable<TestCaseData> TestGetPersonByName
        {
            get
            {
                yield return new TestCaseData(null, null);
                yield return new TestCaseData("", null);
                yield return new TestCaseData("Person77", null);
                yield return new TestCaseData("Person", city.Population.First());
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
        public void AddChange(IPerson target, Role role, object result)
        {
            
        }
    }
}