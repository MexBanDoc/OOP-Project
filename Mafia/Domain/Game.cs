using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task ProcessNight()
        {
            city.StartNight();
            await DoInteractions(DayTime.Night);
        }

        public async Task StartGame()
        {
            while (true)
            {
                if (GetGameStatus() != WinState.InProcess)
                {
                    break;
                }
                await ProcessNight();
                await userInterface.TellResults(city, DayTime.Night);
                
                if (GetGameStatus() != WinState.InProcess)
                {
                    break;
                }
                await ProcessDay();
                await userInterface.TellResults(city, DayTime.Day);
            }
            
            await userInterface.TellGameResult(GetGameStatus(), city);
        }

        public WinState GetGameStatus() => settings.WinCondition(city);
        
        public async Task ProcessDay()
        {
            city.StartDay();
            await DoInteractions(DayTime.Day);
        }

        private async Task DoInteractions(DayTime dayTime)
        {
            var tasks = new List<Task<IPerson>>();
            var roles = new List<Role>();
            
            if (dayTime == DayTime.Day)
            {
                var citizen = new CitizenRole();
                roles.Add(citizen);
                tasks.Add(UpdateCityChanges(citizen, person => true));
            }
            else
            {
                foreach (var role in city.Roles.Where(r => r.DayTime == dayTime))
                {
                    roles.Add(role);
                    tasks.Add(UpdateCityChanges(role, person => role.Equals(person.NightRole)));
                }
            }
            
            await Task.WhenAll(tasks);

            for (var i = 0; i < tasks.Count; i++)
            {
                var target = tasks[i].Result;
                if (target == null)
                {
                    Console.WriteLine("Target is null");
                    continue;
                }

                city.AddChange(target, roles[i].Interact(target));
            }

            foreach (var cityLastChange in 
                city.LastChanges.Where(cityLastChange => cityLastChange.Value == PersonState.Killed))
            {
                cityLastChange.Key.TryKill();
            }
        }

        private async Task<IPerson> UpdateCityChanges(Role role, Func<IPerson, bool> validate)
        {
            var band = city.Population.Where(p => p.IsAlive).Where(validate);
            return await userInterface.AskForInteractionTarget(band, role, city);
        }
    }
}