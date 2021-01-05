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
            var votingList = new List<IVoting>();
            
            if (dayTime == DayTime.Day)
            {
                var voting = new Voting(new CitizenRole(), city, userInterface);
                votingList.Add(voting);
            }
            else
            {
                var collection = city.Roles.Where(r => r.DayTime == dayTime)
                    .Select(role => new Voting(role, city, userInterface));
                votingList.AddRange(collection);
            }

            await Task.WhenAll(votingList.Select(v => v.Start()));

            await Task.Delay(TimeSpan.FromSeconds(settings.VoteDelay));

            await Task.WhenAll(votingList.Select(v => v.End()));

            foreach (var voting in votingList)
            {
                var target = voting.Result;
                
                if (target == null)
                {
                    // Console.WriteLine("Target is null");
                    continue;
                }
                
                city.AddChange(target, voting.Role.Interact(target));
            }

            foreach (var cityLastChange in 
                city.LastChanges.Where(cityLastChange => cityLastChange.Value == PersonState.Killed))
            {
                cityLastChange.Key.TryKill();
            }
        }
    }
}