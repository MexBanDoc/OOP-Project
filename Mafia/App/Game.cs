using System.Linq;
using Mafia.Domain;

namespace Mafia.App
{
    public class Game
    {
        private ISettings Settings;
        private ICity City;
        private IBot Bot;

        public Game(ISettings settings, ICity city, IBot bot)
        {
            Settings = settings;
            City = city;
            Bot = bot;
        }

        public void ProcessNight()
        {
            City.StartNight();
            var personToHeal = Bot.GetPersonToHeal(City.Population.Where(p => p.Role == Role.Doctor));
            City.Heal(personToHeal);
            var personToInvestigate = Bot.GetPersonToInvestigate(City.Population.Where(p => p.Role == Role.Sheriff));
            City.Investigate(personToInvestigate);
            var personToMurder = Bot.GetPersonToMurder(City.Population.Where(p => p.Role == Role.Mafia));
            City.Murder(personToMurder);
        }

        public void ProcessDay()
        {
            City.StartDay();
        }
    }
}