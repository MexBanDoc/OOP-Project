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
            DoInteractions(DayTime.Night);
        }

        public void StartGame()
        {
            throw new NotImplementedException();
        }

        public void ProcessDay()
        {
            City.StartDay();
            DoInteractions(DayTime.Day);
        }

        private void DoInteractions(DayTime dayTime)
        {
            foreach (var role in City.Roles.Where(r => r.dayTime == dayTime)) 
            {
                var target = userInterface.AskForInteractionTarget(City.Population.Where(p => p.Role == role), role);
                role.Interact(target);
            }
        }
    }
}