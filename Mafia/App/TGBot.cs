using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mafia.Domain;
using Mafia.Infrastructure;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Game = Mafia.Domain.Game;

namespace Mafia.App
{
    // TODO: polls -> /name@city
    
    // TODO: separate how to talk and what to talk
    
    public class TgBot : IUserInterface
    {
        private readonly TelegramBotClient bot;
        private readonly User me;
        
        private readonly Random random = new Random();
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        private readonly ConcurrentDictionary<long, IPlayersPool> playersPools =
            new ConcurrentDictionary<long, IPlayersPool>();
        private readonly ConcurrentDictionary<IPerson, long> personToChat = new ConcurrentDictionary<IPerson, long>();
        private readonly ConcurrentDictionary<ICity, long> cityToChat = new ConcurrentDictionary<ICity, long>();
        private readonly ConcurrentDictionary<ICity, bool> citiToAwake = new ConcurrentDictionary<ICity, bool>();
        
        private const int VoteDelay = 30;
        
        public async Task AskForInteractionTarget(IEnumerable<IPerson> players, Role role, ICity city)
        {
            if (!cityToChat.ContainsKey(city)) return;

            var choosers = players.ToHashSet();
            if (choosers.Count == 0) return;
            
            await TellGreetingsToRole(role, city);

            if (role.DayTime == DayTime.Day)
            {
                await AskJudgedPerson(city, choosers);
            }
            else
            {
                await AskRoleForInteractionTarget(city, choosers);
            }
        }

        public Task<IPerson> GetInteractionTarget(Role role, string cityName)
        {
            throw new NotImplementedException();
        }

        private async Task TellGreetingsToRole(Role role, ICity city)
        {
            var chatId = cityToChat[city];
            var lockTaken = false;
            
            try
            {
                await semaphore.WaitAsync();
                lockTaken = true;
                
                if (role.DayTime == DayTime.Day)
                {
                    citiToAwake[city] = true;
                    await bot.SendTextMessageAsync(chatId, "Город просыпается");
                }
                else
                {
                    if (citiToAwake[city])
                    {
                        await bot.SendTextMessageAsync(chatId, "Город засыпает");
                        citiToAwake[city] = false;
                    }
                
                    await bot.SendTextMessageAsync(chatId, $"Просыпается {role.Name}");
                }
            }
            finally
            {
                if (lockTaken)
                {
                    semaphore.Release();
                }
            }
        }

        private async Task<IPerson> AskRoleForInteractionTarget(ICity city, IReadOnlyCollection<IPerson> choosers)
        {
            var targets = city.Population
                .Where(p => p.IsAlive && !choosers.Contains(p))
                .Select(p => p.Name).ToArray();

            if (targets.Length == 1)
            {
                // bot.SendTextMessageAsync(chatId, $"{role.Name} automatically chosen {targets[0]}").Wait();
                return city.GetPersonByName(targets[0]);
            }

            var pollMessages = choosers
                .Select(chooser => personToChat[chooser])
                .Select(userId => bot.SendPollAsync(userId, "Who will be your target?", targets).Result)
                .ToList();

            await Task.Delay(TimeSpan.FromSeconds(VoteDelay));

            var votedTargets = new List<IPerson>();
            foreach (var poll in pollMessages.Select(message =>
                bot.StopPollAsync(message.Chat.Id, message.MessageId).Result))
            {
                // foreach (var option in poll.Options)
                //     bot.SendTextMessageAsync(chatId, $"{option.Text} chosen by {option.VoterCount}").Wait();
                // bot.SendTextMessageAsync(chatId, $"Total votes {poll.TotalVoterCount}").Wait();

                var winner = poll.Options.OrderBy(option => option.VoterCount).Last().Text;

                votedTargets.Add(city.GetPersonByName(winner));
            }

            if (votedTargets.Count == 0) return null;

            var result = votedTargets[Math.Max(0, random.Next(votedTargets.Count) - 1)];
            
            // await bot.SendTextMessageAsync(cityToChat[city], $"{choosers.FirstOrDefault()?.NightRole.Name} chosen {result?.Name}");

            return result;
        }

        private async Task<IPerson> AskJudgedPerson(ICity city, IReadOnlyCollection<IPerson> choosers)
        {
            var chatId = cityToChat[city];
            // bot.SendTextMessageAsync(chatId, $"Choosers {choosers.Count}").Wait();

            if (choosers.Count < 2) return null;

            var message = await bot.SendPollAsync(chatId, "Who will you judge?", choosers.Select(p => p.Name), isAnonymous: false);
            
            await Task.Delay(TimeSpan.FromSeconds(2 * VoteDelay));

            var poll = await bot.StopPollAsync(chatId, message.MessageId);

            // foreach (var option in poll.Options)
            //     bot.SendTextMessageAsync(chatId, $"{option.Text} chosen by {option.VoterCount}").Wait();
            // bot.SendTextMessageAsync(chatId, $"Total votes {poll.TotalVoterCount}").Wait();

            var winner = poll.Options.OrderBy(option => option.VoterCount).Last().Text;

            return city.GetPersonByName(winner);
        }

        public async Task TellResults(ICity city, DayTime dayTime)
        {
            if (!cityToChat.ContainsKey(city)) return;
            
            var chatId = cityToChat[city];
            foreach (var pair in city.LastChanges.Where(pair => pair.Value != PersonState.Immortal))
            {
                await bot.SendTextMessageAsync(chatId, $"{pair.Key.Name} {pair.Value}");
            }
        }
        
        public async Task TellGameResult(WinState state, ICity city)
        {
            if (!cityToChat.ContainsKey(city)) return;
            
            var chatId = cityToChat[city];
            switch (state)
            {
                case WinState.MafiaWins:
                    await bot.SendTextMessageAsync(chatId, "Мафия победила");
                    break;
                case WinState.InProcess:
                    await bot.SendTextMessageAsync(chatId, "Ничья");
                    break;
                case WinState.PeacefulWins:
                    await bot.SendTextMessageAsync(chatId, "Мирные победили");
                    break;
                default:
                    await bot.SendTextMessageAsync(chatId, "Technical problems");
                    break;
            }
            
            var mapMessage = new StringBuilder();
            foreach (var person in city.Population)
            {
                mapMessage.Append($"{person.Name} was {(person.NightRole == null ? "Peaceful" : person.NightRole.Name)}\n");
            }

            mapMessage.Append("\nType /play if you want restart\nLike this game? Share this bot!");
            
            bot.SendTextMessageAsync(chatId,mapMessage.ToString()).Wait();
            
            bot.SendTextMessageAsync(chatId, "🍑").Wait();
        }

        public TgBot()
        {
            var token = Environment.GetEnvironmentVariable("TGBotToken");
            bot = new TelegramBotClient(token);
            bot.OnMessage += BotOnMessageReceived;
            bot.StartReceiving(Array.Empty<UpdateType>());
            me = bot.GetMeAsync().Result;
        }
        
        private const string PlayCommand = "/play";
        private const string EndRecordCommand = "/endRecord";
        
        private async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;
            var chat = message.Chat;
            var user = message.From;
            
            // Console.WriteLine($"{user.Username}: {user.Id}");
            if (message.Text == null)
            {
                return;
            }

            switch (message.Text.Replace($"@{me.Username}", ""))
            {
               case PlayCommand:
                   await PlayMethod(chat, user);
                   break;
               case EndRecordCommand: 
                   await EndRecordMethod(chat.Id); 
                   break;
               case "/help":
               case "/start": 
                   await bot.SendTextMessageAsync(chat.Id, Resources.HelpMessage); 
                   break;
               case "/guide":
                   await bot.SendTextMessageAsync(chat.Id, Resources.GuideMessage); 
                   break;
            }
        }

        private readonly SemaphoreSlim playSemaphoreSlim = new SemaphoreSlim(1, 1);
        
        private async Task PlayMethod(Chat chat, User user)
        {
            var lockTaken = false;

            try
            {
                await playSemaphoreSlim.WaitAsync();
                lockTaken = true;
                
                if (!playersPools.ContainsKey(chat.Id))
                {
                    playersPools[chat.Id] = new PlayersPool(random);
                    await bot.SendTextMessageAsync(chat.Id, "Record to game started!");
                }
            }
            finally
            {
                if (lockTaken)
                {
                    playSemaphoreSlim.Release();
                }
            }

            var pool = playersPools[chat.Id];

            var userName = $"{user.FirstName} {user.LastName} (@{user.Username})";
            var message = pool.AddPlayer(user.Id, userName)
                ? $"Successfully join game in {chat.Title} chat!"
                : $"Already joined game in {chat.Title} chat!";
            await bot.SendTextMessageAsync(chat.Id, message);
            await SendUsersSaveMessage(message, user.Id, chat.Id);
        }

        private async Task SendUsersSaveMessage(string message, long userId, long chatId)
        {
            try
            {
                await bot.SendTextMessageAsync(userId, message);
            }
            catch (ChatNotInitiatedException)
            {
                var badUserChat = await bot.GetChatAsync(userId);

                await bot.SendTextMessageAsync(chatId,
                    $"User @{badUserChat.Username}, please, start me!\n@mafiaprojectbot");
            }
        }

        private async Task EndRecordMethod(long chatId)
        {
            IPlayersPool pool;

            var lockTaken = false;

            try
            {
                await playSemaphoreSlim.WaitAsync();
                lockTaken = true;
                
                if (!playersPools.TryRemove(chatId, out pool))
                {
                    await bot.SendTextMessageAsync(chatId, $"There no opened game record\nType {PlayCommand} to start one");
                }
            }
            finally
            {
                if (lockTaken)
                {
                    playSemaphoreSlim.Release();
                }
            }

            if (pool == null)
            {
                return;
            }
            
            // TODO: while everyone types /play someone creates settings
            // TODO: save chatId -> settings

            var population = new List<IPerson>();

            var settings = Settings.Various;

            foreach (var (userId, person) in pool.ExtractPersons(settings))
            {
                population.Add(person);
                personToChat[person] = userId;
                var message = person.NightRole == null
                    ? $"Your name is {person.Name} and you are Peaceful"
                    : $"Your name is {person.Name} and you are {person.NightRole.Name}";
                await SendUsersSaveMessage(message, userId, chatId);
            }

            var city = new City(population, settings.CityName);
            cityToChat[city] = chatId;
            citiToAwake[city] = true;
            var game = new Game(settings, city, this);

            await RunGame(city, game);
        }

        private async Task RunGame(ICity city, Game game)
        {
            await game.StartGame();
            cityToChat.TryRemove(city, out var chatId);
            Console.WriteLine($"Successfully removed city with chat.Id {chatId}");

            foreach (var person in city.Population)
            {
                personToChat.TryRemove(person, out var personChat);
                Console.WriteLine($"Successfully removed person with chat.Id {personChat}");
            }
        }
    }
}