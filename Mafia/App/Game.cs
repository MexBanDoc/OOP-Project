using Mafia.Domain;

namespace Mafia.App
{
    public class Game
    {
        private ISettings Settings;
        private ICity City;

        public Game(ISettings settings, ICity city)
        {
            Settings = settings;
            City = city;
        }
    }
}