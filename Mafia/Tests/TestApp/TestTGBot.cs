using Mafia.App;
using Mafia.Domain;
using Moq;
using NUnit.Framework;

namespace Tests.TestApp
{
    [TestFixture]
    public class TestTGBot
    {
        [Test]
        public void AskForInteractionTarget()
        {
            var mock = new Mock<ICity>();
            var players = new[] {new Person(null, new MafiaRole(), null),
                new Person(), new Person(null, new MafiaRole(), null),
                new Person(null, new MazaiRole(), null) 
            };
            var bot = new TgBot();
            bot.AskForInteractionTarget(players, new DoctorRole(), mock.Object).Wait();
            
        }
    }
}