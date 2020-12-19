using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mafia.Domain;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Game = Mafia.Domain.Game;

namespace Mafia.App
{
    public class TGBot : IUserInterface
    {
        private TelegramBotClient Bot;

        private static readonly ConcurrentDictionary<long, IPlayersPool> PlayersPools = new ConcurrentDictionary<long, IPlayersPool>();
        private static readonly ConcurrentDictionary<IPerson, long> PersonToChat = new ConcurrentDictionary<IPerson, long>();
        private static readonly ConcurrentDictionary<ICity, long> CityToChat = new ConcurrentDictionary<ICity, long>();
        
        public IPerson AskForInteractionTarget(IEnumerable<IPerson> players, Role role, ICity city)
        {
            if (!CityToChat.ContainsKey(city))
            {
                return null;
            }
            var chatId = CityToChat[city];
            if (role.dayTime == DayTime.Night)
            {
                Bot.SendTextMessageAsync(chatId, "Город засыпает").Wait();
            }
            else
            {
                Bot.SendTextMessageAsync(chatId, "Город просыпается").Wait();
            }
            var victims = new List<IPerson>();
            foreach (var player in players)
            {
                // Bot.SendTextMessageAsync(chatId, $"Просыпается {role.Name} {player.Name}").Wait(); // TODO: delete?
                // var name = city.Population
                //     .Where(person => person.NightRole is null || !person.NightRole.Equals(role))
                //     .First(person => person.IsAlive).Name;
                // victims.Add(city.GetPersonByName(name));
                // TODO: implement
            }
            
            return victims[new Random().Next(victims.Count - 1)];
        }

        public async void TellResults(ICity city, DayTime dayTime)
        {
            if (!CityToChat.ContainsKey(city))
            {
                return;
            }
            var chatId = CityToChat[city];
            foreach (var pair in city.LastChanges)
            {
                await Bot.SendTextMessageAsync(chatId, $"{pair.Key.Name} {pair.Value}");
            }
        }
        
        public void TellGameResult(WinState state, ICity city)
        {
            if (!CityToChat.ContainsKey(city))
            {
                return;
            }
            var chatId = CityToChat[city];
            switch (state)
            {
                case WinState.MafiaWins:
                    Bot.SendTextMessageAsync(chatId, "Мафия победила");
                    break;
                case WinState.InProcess:
                    Bot.SendTextMessageAsync(chatId, "Ничья");
                    break;
                case WinState.PeacefulWins:
                    Bot.SendTextMessageAsync(chatId, "Мирные победили");
                    break;
                default:
                    Bot.SendTextMessageAsync(chatId, "Technical problems");
                    break;
            }
        }

        public TGBot()
        {
            var token = Environment.GetEnvironmentVariable("TGBotToken");
            // if (token == null)
            // {
            //     Console.WriteLine("Null");
            // }

            // Console.WriteLine(token);

            Bot = new TelegramBotClient(token);

            Bot.OnMessage += BotOnMessageReceived;
            
            Bot.StartReceiving(Array.Empty<UpdateType>());
        }

        private const string StartGameCommand = "/startGame";
        private const string PlayCommand = "/play";
        private const string EndRecordCommand = "/endRecord";

        private async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;

            var chat = message.Chat;
            var user = message.From;

            await Bot.SendTextMessageAsync(chat.Id, $"Chat.Id: {chat.Id}\nUser: {user.Id}");

            if (message.Text == StartGameCommand)
            {
                if (PlayersPools.ContainsKey(chat.Id))
                {
                    await Bot.SendTextMessageAsync(chat.Id, "Game record already has started!");
                    return;
                }
                
                var pool = new PlayersPool();
                PlayersPools[chat.Id] = pool;
                
                await Bot.SendTextMessageAsync(chat.Id, "Record to game started!");
            }

            if (message.Text == PlayCommand)
            {
                if (!PlayersPools.ContainsKey(chat.Id))
                {
                    await Bot.SendTextMessageAsync(chat.Id, $"There no opened game record\nType {StartGameCommand} to start one");
                }

                var pool = PlayersPools[chat.Id];

                if (!pool.IsOpen)
                {
                    await Bot.SendTextMessageAsync(chat.Id, "Sorry, record is end up");
                    return;
                }

                var added = pool.AddPlayerAsync(user.Id);

                if (added)
                {
                    await Bot.SendTextMessageAsync(chat.Id, "Successfully join game!");
                }
                else
                {
                    await Bot.SendTextMessageAsync(chat.Id, "Already joined game");
                }
            }

            if (message.Text == EndRecordCommand)
            {
                var population = new List<IPerson>();
                if (PlayersPools.TryRemove(chat.Id, out var pool))
                {
                    foreach (var keyValuePair in pool.ExtractPersons())
                    {
                        population.Add(keyValuePair.Value);
                        PersonToChat[keyValuePair.Value] = keyValuePair.Key;
                    }
                }
                else
                {
                    await Bot.SendTextMessageAsync(chat.Id, $"There no opened game record\nType {StartGameCommand} to start one");
                }
                
                var settings = Settings.Default;
                var city = new City(population);
                CityToChat[city] = chat.Id;
                var game = new Game(settings, city, this);

                RunGame(city, game);
            }
            
            if (message.Text == "/help")
            {
               await Bot.SendTextMessageAsync(chat.Id, "Technical problems");
            }
        }

        private static async Task RunGame(ICity city, Game game)
        {
            game.StartGame();
            while (!CityToChat.TryRemove(city, out var chatId))
            {
                Console.WriteLine($"Fail to remove city {city}");
            }

            
            foreach (var person in city.Population)
            {
                while (!PersonToChat.TryRemove(person, out var personChat))
                {
                    Console.WriteLine($"Fail to remove city {person}");
                }
            }
        }
    }

    public interface IPlayersPool // TODO: may be disposable
    {
        bool IsOpen { get; }
        bool AddPlayerAsync(long playerId);
        IEnumerable<KeyValuePair<long, IPerson>> ExtractPersons();
    }

    public class PlayersPool : IPlayersPool
    {
        private static readonly string[] Names = {
            "Liam", "Olivia", "Noah", "Emma",
            "Oliver", "Ava", "William", "Sophia",
            "Elijah", "Isabella", "James", "Charlotte",
            "Benjamin", "Amelia", "Lucas", "Mia",
            "Mason", "Harper", "Ethan", "Evelyn"
        };
        
        public bool IsOpen { get; private set; } = true;
        
        private readonly Random random = new Random();
        private readonly ConcurrentDictionary<long, IPerson> players = new ConcurrentDictionary<long, IPerson>();

        public bool AddPlayerAsync(long playerId)
        {
            if (players.ContainsKey(playerId))
            {
                return false;
            }

            var person = CreatePerson();
            players[playerId] = person;
            return true;
        }

        private IPerson CreatePerson()
        {
            var dayRole = new CitizenRole();
            var index = players.Count % 3; // count of inheritors of Role
            Role nightRole = index switch // TODO: make this cool
            {
                1 => new MafiaRole(),
                2 => new HealerRole(),
                _ => null
            };

            var name = Names[random.Next(0, Names.Length)];
            return new Person(dayRole, nightRole, name);
        }

        public IEnumerable<KeyValuePair<long, IPerson>> ExtractPersons() // TODO: redo
        {
            IsOpen = false;
            return players;
        }
    }
}