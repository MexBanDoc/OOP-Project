﻿using System;
using System.Collections.Generic;
using System.Linq;
using Mafia.Infrastructure;

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

        public Game(ISettings settings, IUserInterface userInterface)
            : this(settings, new City(new List<IPerson>(settings.GeneratePopulation())), userInterface)
        {
        }

        public void ProcessNight()
        {
            City.StartNight();
            DoInteractions(DayTime.Night);
        }

        public void StartGame()
        {
            userInterface.StartGame();
            while (Settings.WinCondition(City) == WinState.InProcess)
            {
                ProcessNight();
                userInterface.TellResults(City, DayTime.Night);
                ProcessDay();
                userInterface.TellResults(City, DayTime.Day);
            }
            userInterface.TellGameResult(Settings.WinCondition(City));
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
                var target = userInterface.AskForInteractionTarget(
                    City.Population.Where(p => (dayTime==DayTime.Day?p.DayRole:p.NightRole) == role)
                        .Where(person => person.IsAlive), 
                    role, City);
                role.Interact(target);
                City.AddChange(target, role);
            }
        }
    }
}