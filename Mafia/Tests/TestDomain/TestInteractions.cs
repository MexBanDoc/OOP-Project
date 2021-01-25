using System.Linq.Expressions;
using FluentAssertions;
using Mafia.Domain;
using Moq;
using NUnit.Framework;

namespace Tests.TestDomain
{
    public class TestInteractions
    {
        [TestFixture]
        public class TestCheckInteraction
        {
            [Test]
            public void TestInteract()
            {
                var mock = new Mock<IPerson>();
                new CheckInteraction().Interact(mock.Object).Should().Be(PersonState.Alive);
                mock.Setup(person => person.NightRole).Returns(new MafiaRole());
                new CheckInteraction().Interact(mock.Object).Should().Be(PersonState.Killed);
                mock.Setup(person => person.NightRole).Returns(new PoliсemanRole());
                new CheckInteraction().Interact(mock.Object).Should().Be(PersonState.Alive);
            }
        }

        [TestFixture]
        public class TestCrazyInteraction
        {
            [Test]
            [TestCase("ABCD", "AbCd")]
            [TestCase("АБВГ", "АбВг")]
            [TestCase("1234", "1234")]
            [TestCase("abcd", "AbCd")]
            [TestCase("абвг", "АбВг")]
            [TestCase(" ", " ")]
            [TestCase(",./-=+*/#$^&()[]{}!?", ",./-=+*/#$^&()[]{}!?")]
            public void TestInteract(string name, string result)
            {
                var person = new Person(null, null, name);
                new CrazyInteraction().Interact(person);
                person.Name.Should().Be(result);

            }
        }
    }
}