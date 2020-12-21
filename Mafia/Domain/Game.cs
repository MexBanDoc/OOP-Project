using System;
using System.Collections.Generic;
using System.Linq;

namespace Mafia.Domain
{
    public class Game
    {
        private readonly ISettings settings;
        private readonly ICity city;
        private readonly IUserInterface userInterface;

        public Game(ISettings settings, ICity city, IUserInterface userInterface)
        {
            this.settings = settings;
            this.city = city;
            this.userInterface = userInterface;
        }

        public Game(ISettings settings, IUserInterface userInterface)
            : this(settings, new City(new List<IPerson>(settings.GeneratePopulation())), userInterface)
        {
        }

        public void ProcessNight()
        {
            city.StartNight();
            DoInteractions(DayTime.Night);
        }

        public void StartGame()
        {
            while (GetGameStatus() == WinState.InProcess)
            {
                ProcessNight();
                userInterface.TellResults(city, DayTime.Night);
                if (GetGameStatus() != WinState.InProcess)
                {
                    break;
                }
                ProcessDay();
                userInterface.TellResults(city, DayTime.Day);
            }
            
            userInterface.TellGameResult(GetGameStatus(), city);
        }

        public WinState GetGameStatus() => settings.WinCondition(city);
        
        public void ProcessDay()
        {
            city.StartDay();
            DoInteractions(DayTime.Day);
        }

        private void DoInteractions(DayTime dayTime)
        {
            if (dayTime == DayTime.Day)
            {
                UpdateCityChanges(new CitizenRole(), person => true);
            }
            else
            {
                foreach (var role in city.Roles.Where(r => r.DayTime == dayTime))
                {
                    UpdateCityChanges(role, person => role.Equals(person.NightRole));
                }
            }

            foreach (var cityLastChange in 
                city.LastChanges.Where(cityLastChange => cityLastChange.Value == PersonState.Killed))
            {
                cityLastChange.Key.TryKill();
            }
        }

        private void UpdateCityChanges(Role role, Func<IPerson, bool> validate)
        {
            var band = city.Population.Where(p => p.IsAlive).Where(validate);
            var target = userInterface.AskForInteractionTarget(band, role, city);
            if (target == null)
            {
                return;
            }
            role.Interact(target);
            city.AddChange(target, role);
        }
    }
}