using System;
using System.Linq;
using System.Threading;
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
            if (dayTime == DayTime.Day)
            {
                await UpdateCityChanges(new CitizenRole(), person => true);
            }
            else
            {
                var tasks = city.Roles
                    .Where(r => r.DayTime == dayTime)
                    .Select(role => UpdateCityChanges(role, person => role.Equals(person.NightRole)));
                await Task.WhenAll(tasks);
            }

            foreach (var cityLastChange in 
                city.LastChanges.Where(cityLastChange => cityLastChange.Value == PersonState.Killed))
            {
                cityLastChange.Key.TryKill();
            }
        }
        
        private readonly SemaphoreSlim updateCitySemaphore = new SemaphoreSlim(1, 1);

        private async Task UpdateCityChanges(Role role, Func<IPerson, bool> validate)
        {
            var band = city.Population.Where(p => p.IsAlive).Where(validate);
            var target = await userInterface.AskForInteractionTarget(band, role, city);
            if (target == null) return;

            var lockTaken = false;
            
            try
            {
                await updateCitySemaphore.WaitAsync();
                lockTaken = true;
                
                city.AddChange(target, role.Interact(target));
            }
            finally
            {
                if (lockTaken)
                {
                    updateCitySemaphore.Release();
                }
            }
        }
    }
}