using System;
using System.Linq;

namespace Mafia.Domain
{
    public class Game
    {
        private ISettings Settings;
        private ICity City;
        private IUserInterface userInterface;

        public Game(ISettings settings, ICity city, IUserInterface userInterface)
        {
            Settings = settings;
            City = city;
            this.userInterface = userInterface;
        }

        public void ProcessNight()
        {
            City.StartNight();
            //var personToHeal = Bot.GetPersonToHeal(City.Population.Where(p => p.RoleEnum == RoleEnum.Doctor));
            //City.Heal(personToHeal);
            //var personToInvestigate = Bot.GetPersonToInvestigate(City.Population.Where(p => p.RoleEnum == RoleEnum.Sheriff));
            //City.Investigate(personToInvestigate);
            //var personToMurder = userInterface.GetPersonToMurder(City.Population.Where(p => p.Role is Domain.Mafia));
            var personToMurder = userInterface.AskForTarget(
                City.Population.Where(p => p.Role is Domain.Mafia),
                new Mafia());
            var mafia = City.Roles.First(r => r is Domain.Mafia);
            mafia.Interact(personToMurder);
            //обобщить
        }

        public void StartGame()
        {
            throw new NotImplementedException();
        }

        public void ProcessDay()
        {
            City.StartDay();
        }
    }
}