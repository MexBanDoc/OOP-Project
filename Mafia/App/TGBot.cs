using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mafia.Domain;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Exceptions;
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
        
        public IPerson AskForInteractionTarget(IEnumerable<IPerson> players, Role role, ICity city)
        {
            if (!cityToChat.ContainsKey(city)) return null;

            var chatId = cityToChat[city];

            var choosers = players.ToHashSet();
            if (choosers.Count == 0) return null;

            return role.DayTime == DayTime.Night
                ? AskRoleForInteractionTarget(role, city, chatId, choosers)
                : AskJudgedPerson(city, chatId, choosers);
        }

        private IPerson AskRoleForInteractionTarget(Role role, ICity city, long chatId, HashSet<IPerson> choosers)
        {
            if (!isCityAsleep)
            {
                bot.SendTextMessageAsync(chatId, "Город засыпает").Wait();
                isCityAsleep = true;
            }

            bot.SendTextMessageAsync(chatId, $"Просыпается {role.Name}");

            var targets = city.Population
                .Where(p => p.IsAlive && !choosers.Contains(p))
                .Select(p => p.Name)
                .ToArray();

            switch (targets.Length)
            {
                case 0:
                    return null;
                case 1:
                    return city.GetPersonByName(targets[0]);
            }

            var pollMessages = choosers
                .Select(chooser => personToChat[chooser])
                .Select(userId => bot.SendPollAsync(userId, "Who will be your target?", targets).Result)
                .ToList();

            Thread.Sleep(30 * 1000);

            var votedTargets = new List<IPerson>();
            foreach (var poll in pollMessages.Select(message =>
                bot.StopPollAsync(message.Chat.Id, message.MessageId).Result))
            {
                foreach (var option in poll.Options)
                    bot.SendTextMessageAsync(chatId, $"{option.Text} chosen by {option.VoterCount}").Wait();
                bot.SendTextMessageAsync(chatId, $"Total votes {poll.TotalVoterCount}").Wait();

                var winner = poll.Options.OrderBy(option => option.VoterCount).Last().Text;

                votedTargets.Add(city.GetPersonByName(winner));
            }

            if (votedTargets.Count == 0) return null;

            var result = votedTargets[Math.Max(0, random.Next(votedTargets.Count) - 1)];
            
            bot.SendTextMessageAsync(chatId, $"{role.Name} chosen {result.Name}").Wait();

            return result;
        }

        private IPerson AskJudgedPerson(ICity city, long chatId, HashSet<IPerson> choosers)
        {
            isCityAsleep = false;
            bot.SendTextMessageAsync(chatId, "Город просыпается").Wait();

            bot.SendTextMessageAsync(chatId, $"Choosers {choosers.Count}").Wait();

            if (choosers.Count < 2) return null;

            var message = bot.SendPollAsync(chatId, "Who will you judge?", choosers.Select(p => p.Name), isAnonymous: false).Result;
            
            Thread.Sleep(60 * 1000);
            
            var poll = bot.StopPollAsync(chatId, message.MessageId).Result;

            foreach (var option in poll.Options)
                bot.SendTextMessageAsync(chatId, $"{option.Text} chosen by {option.VoterCount}").Wait();
            bot.SendTextMessageAsync(chatId, $"Total votes {poll.TotalVoterCount}").Wait();

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
        }

        public TgBot()
        {
            var token = Environment.GetEnvironmentVariable("TGBotToken");
            bot = new TelegramBotClient(token);
            bot.OnMessage += BotOnMessageReceived;
            bot.StartReceiving(Array.Empty<UpdateType>());
        }

        private const string StartGameCommand = "/startGame";
        private const string PlayCommand = "/play";
        private const string EndRecordCommand = "/endRecord";

        private async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;
            var chatId = message.Chat.Id;
            var userId = message.From.Id;

            // await Bot.SendTextMessageAsync(chat.Id, $"Chat.Id: {chat.Id}\nUser: {user.Id}");
            
            // TODO: join /play and /start game

            switch (message.Text)
            {
                case StartGameCommand:
                    await StartGameMethod(chatId);
                    await PlayMethod(chatId, userId);
                    break;
                case PlayCommand:
                    await PlayMethod(chatId, userId);
                    break;
                case EndRecordCommand:
                    await EndRecordMethod(chatId);
                    break;
                case "/help":
                case "/start":
                    await bot.SendTextMessageAsync(chatId,
                        $"{StartGameCommand} - press to start a game\n{PlayCommand} - add self to game\n{EndRecordCommand} - end recording players and start game already\n/help - this message");
                    break;
            }
        }

        private async Task StartGameMethod(long chatId)
        {
            if (playersPools.ContainsKey(chatId))
            {
                await bot.SendTextMessageAsync(chatId, "Game record already has started!");
                return;
            }
                
            var pool = new PlayersPool();
            playersPools[chatId] = pool;
                
            await bot.SendTextMessageAsync(chatId, "Record to game started!");
        }

        private async Task PlayMethod(long chatId, long userId)
        {
            if (!playersPools.ContainsKey(chatId))
            {
                await bot.SendTextMessageAsync(chatId, $"There no opened game record\nType {StartGameCommand} to start one");
                return;
            }

            var pool = playersPools[chatId];

            if (!pool.IsOpen)
            {
                await bot.SendTextMessageAsync(chatId, "Sorry, record is end up");
                return;
            }

            var added = pool.AddPlayerAsync(userId);

            try
            {
                // TODO: send user from what chat came message
                if (added)
                {
                    await bot.SendTextMessageAsync(chatId, "Successfully join game!");
                    await bot.SendTextMessageAsync(userId, "Successfully join game!");
                }
                else
                {
                    await bot.SendTextMessageAsync(chatId, "Already joined game");
                    await bot.SendTextMessageAsync(userId, "Already joined game");
                }
            }
            catch (ApiRequestException)
            {
                // Console.WriteLine(e);
            }
        }

        private async Task EndRecordMethod(long chatId)
        {
            var population = new List<IPerson>();
            if (!playersPools.ContainsKey(chatId))
            {
                await bot.SendTextMessageAsync(chatId,
                    $"There no opened game record\nType {StartGameCommand} to start one");
                return;
            }

            var pool = playersPools[chatId];
            foreach (var keyValuePair in pool.ExtractPersons())
            {
                var userChatId = keyValuePair.Key;
                var person = keyValuePair.Value;
                population.Add(person);
                personToChat[person] = userChatId;
                try
                {
                    if (person.NightRole == null)
                        await bot.SendTextMessageAsync(userChatId,
                            $"Your name is {person.Name} and you are Peaceful");
                    else
                        await bot.SendTextMessageAsync(userChatId,
                            $"Your name is {person.Name} and you are {person.NightRole.Name}");
                }
                catch (ChatNotInitiatedException e)
                {
                    var badUserChat = await bot.GetChatAsync(userChatId);

                    await bot.SendTextMessageAsync(chatId,
                        $"User @{badUserChat.Username}, please, start me!\n@mafiaprojectbot");
                    return;
                }
            }

            playersPools.TryRemove(chatId, out pool); // TODO: handle when fails to remove
            var settings = Settings.Default;
            var city = new City(population);
            cityToChat[city] = chatId;
            var game = new Game(settings, city, this);

            RunGame(city, game);
        }

        private void RunGame(ICity city, Game game)
        {
            game.StartGame();
            cityToChat.TryRemove(city, out var chatId);
            Console.WriteLine($"Successfully removed city {city} with chat.Id {chatId}");

            foreach (var person in city.Population)
            {
                personToChat.TryRemove(person, out var personChat);
                Console.WriteLine($"Successfully removed city {person} with chat.Id {personChat}");
            }
        }
    }
}