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

namespace Mafia.App
{
    public class TGBot : IUserInterface
    {
        private static TelegramBotClient Bot;

        private static readonly ConcurrentDictionary<long, IPlayersPool> PlayersPools = new ConcurrentDictionary<long, IPlayersPool>();
        
        public IPerson AskForInteractionTarget(IEnumerable<IPerson> players, Role role, ICity city)
        {
            var choosers = players.ToHashSet();
            var targets = city.Population
                .Where(p => !choosers.Contains(p))
                .Select(p => p.Name).ToArray();
            
                // теперь бы каждому выбирателью отправить пулл
                // подождать !!сколько-то!!
                // завершить пуллы и отдать популярную цель
            
            throw new NotImplementedException();
        }

        public void StartGame()
        {
            // запомнить по игре участников и их роли
            throw new NotImplementedException();
        }

        public void TellResults(ICity city, DayTime dayTime)
        {
            // wha?
            throw new NotImplementedException();
        }
        
        public void TellGameResult(WinState state)
        {
            throw new NotImplementedException();
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
            
            var me = Bot.GetMeAsync().Result;

            Bot.OnMessage += BotOnMessageReceived;
            
            Bot.StartReceiving(Array.Empty<UpdateType>());
        }
        
        // /startgame -> settings [defaultCondition, people]
        // take condition (default), take roles and their count, total count!?
            // all players must type /play, (randomly choose role)
            // if player is peaceful [nightRole is null]
        // finally create game and start it already
        
        // more shit code ahead

        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;

            var chat = message.Chat;
            var user = message.From;

            await Bot.SendTextMessageAsync(chat.Id, $"Chat.Id: {chat.Id}\nUser: {user.Id}");

            if (message.Text == "/startgame")
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

            if (message.Text == "/play")
            {
                if (!PlayersPools.ContainsKey(chat.Id))
                {
                    await Bot.SendTextMessageAsync(chat.Id, "There no opened game record\nType /startgame to start one");
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
            
            // TODO: /endRecord -> form game -> start it
            
            // TODO: /help

            var msg = Bot.SendTextMessageAsync(chat.Id, "Technical problems");
        }
    }

    public interface IPlayersPool // TODO: may be disposable
    {
        bool IsOpen { get; }
        bool AddPlayerAsync(long playerId);
        IEnumerable<(IPerson, long)> ExtractPersons();
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

        public IEnumerable<(IPerson, long)> ExtractPersons() // TODO: redo
        {
            IsOpen = false;
            foreach (var keyValue in players)
            {
                yield return (keyValue.Value, keyValue.Key);
            }
        }
    }
}