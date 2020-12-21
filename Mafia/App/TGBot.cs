using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mafia.Domain;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Game = Mafia.Domain.Game;

namespace Mafia.App
{
    public class TgBot : IUserInterface
    {
        private readonly TelegramBotClient bot;
        
        private readonly Random random = new Random();

        private readonly ConcurrentDictionary<long, IPlayersPool> playersPools =
            new ConcurrentDictionary<long, IPlayersPool>();
        private readonly ConcurrentDictionary<IPerson, long> personToChat = new ConcurrentDictionary<IPerson, long>();
        private readonly ConcurrentDictionary<ICity, long> cityToChat = new ConcurrentDictionary<ICity, long>();

        private bool isCityAsleep;
        private const int VoteDelay = 30;
        
        public IPerson AskForInteractionTarget(IEnumerable<IPerson> players, Role role, ICity city)
        {
            if (!cityToChat.ContainsKey(city)) return null;

            var chatId = cityToChat[city];

            var choosers = players.ToHashSet();
            if (choosers.Count == 0) return null;
            
            TellGreetingsToRole(role, chatId);

            return role.DayTime == DayTime.Night
                ? AskRoleForInteractionTarget(city, chatId, choosers)
                : AskJudgedPerson(city, chatId, choosers);
        }

        private void TellGreetingsToRole(Role role, long chatId)
        {
            if (role.DayTime == DayTime.Day)
            {
                isCityAsleep = false;
                bot.SendTextMessageAsync(chatId, "Город просыпается").Wait();
                return;
            }
            
            if (!isCityAsleep)
            {
                bot.SendTextMessageAsync(chatId, "Город засыпает").Wait();
                isCityAsleep = true;
            }

            bot.SendTextMessageAsync(chatId, $"Просыпается {role.Name}");
        }

        private IPerson AskRoleForInteractionTarget(ICity city, long chatId, IReadOnlyCollection<IPerson> choosers)
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

            Task.Delay(TimeSpan.FromSeconds(VoteDelay)).Wait();

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
            
            // bot.SendTextMessageAsync(chatId, $"{role.Name} chosen {result.Name}").Wait();

            return result;
        }

        private IPerson AskJudgedPerson(ICity city, long chatId, IReadOnlyCollection<IPerson> choosers)
        {
            // bot.SendTextMessageAsync(chatId, $"Choosers {choosers.Count}").Wait();

            if (choosers.Count < 2) return null;

            var message = bot.SendPollAsync(chatId, "Who will you judge?", choosers.Select(p => p.Name), isAnonymous: false).Result;
            
            Task.Delay(TimeSpan.FromSeconds(2 * VoteDelay)).Wait();

            var poll = bot.StopPollAsync(chatId, message.MessageId).Result;

            // foreach (var option in poll.Options)
            //     bot.SendTextMessageAsync(chatId, $"{option.Text} chosen by {option.VoterCount}").Wait();
            // bot.SendTextMessageAsync(chatId, $"Total votes {poll.TotalVoterCount}").Wait();

            var winner = poll.Options.OrderBy(option => option.VoterCount).Last().Text;

            return city.GetPersonByName(winner);
        }

        public async void TellResults(ICity city, DayTime dayTime)
        {
            if (!cityToChat.ContainsKey(city)) return;
            
            var chatId = cityToChat[city];
            foreach (var pair in city.LastChanges)
            {
                await bot.SendTextMessageAsync(chatId, $"{pair.Key.Name} {pair.Value}");
            }
        }
        
        public void TellGameResult(WinState state, ICity city)
        {
            if (!cityToChat.ContainsKey(city)) return;
            
            var chatId = cityToChat[city];
            switch (state)
            {
                case WinState.MafiaWins:
                    bot.SendTextMessageAsync(chatId, "Мафия победила");
                    break;
                case WinState.InProcess:
                    bot.SendTextMessageAsync(chatId, "Ничья");
                    break;
                case WinState.PeacefulWins:
                    bot.SendTextMessageAsync(chatId, "Мирные победили");
                    break;
                default:
                    bot.SendTextMessageAsync(chatId, "Technical problems");
                    break;
            }
            
            bot.SendTextMessageAsync(chatId, "/play if you want restart");
        }

        public TgBot()
        {
            var token = Environment.GetEnvironmentVariable("TGBotToken");
            bot = new TelegramBotClient(token);
            bot.OnMessage += BotOnMessageReceived;
            bot.StartReceiving(Array.Empty<UpdateType>());
        }
        
        private const string PlayCommand = "/play";
        private const string EndRecordCommand = "/endRecord";

        private async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;
            var chat = message.Chat;
            var user = message.From;
            if (message.Text == null)
            {
                return;
            }

            switch (message.Text.Replace("@mafiaprojectbot", ""))
            {
               case PlayCommand:
                   await PlayMethod(chat, user);
                   break;
               case EndRecordCommand: 
                   await EndRecordMethod(chat.Id); 
                   break;
               case "/help":
               case "/start": 
                   await bot.SendTextMessageAsync(chat.Id,
                    $"{PlayCommand} - press to start a game or join to existing game\n{EndRecordCommand} - end recording players and start game already\n/help - this message"); 
                   break;
            }
        }

        private async Task PlayMethod(Chat chat, User user)
        {
            if (!playersPools.ContainsKey(chat.Id))
            {
                playersPools[chat.Id] = new PlayersPool();
                await bot.SendTextMessageAsync(chat.Id, "Record to game started!");
            }

            var pool = playersPools[chat.Id];

            if (!pool.IsOpen)
            {
                await bot.SendTextMessageAsync(chat.Id, "Sorry, record is end up");
                return;
            }

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
            if (!playersPools.ContainsKey(chatId))
            {
                await bot.SendTextMessageAsync(chatId, $"There no opened game record\nType {PlayCommand} to start one");
                return;
            }

            var population = new List<IPerson>();
            var pool = playersPools[chatId];
            
            foreach (var (userId, person) in pool.ExtractPersons())
            {
                population.Add(person);
                personToChat[person] = userId;
                var message = person.NightRole == null
                    ? $"Your name is {person.Name} and you are Peaceful"
                    : $"Your name is {person.Name} and you are {person.NightRole.Name}";
                await SendUsersSaveMessage(message, userId, chatId);
            }

            playersPools.TryRemove(chatId, out pool); // TODO: handle when fails to remove
            var city = new City(population);
            cityToChat[city] = chatId;
            var game = new Game(Settings.Default, city, this);

            RunGame(city, game);
        }

        private void RunGame(ICity city, Game game)
        {
            game.StartGame();
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