﻿using System;
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
    // TODO: separate how to talk and what to talk
    
    public class TgBot : IUserInterface
    {
        private readonly TelegramBotClient bot;
        private readonly User me;
        
        private readonly Random random = new Random();

        private readonly SemaphoreSlim playSemaphore = new SemaphoreSlim(1, 1);

        private readonly ConcurrentDictionary<long, IGameRecord> gameRecords =
            new ConcurrentDictionary<long, IGameRecord>();

        private readonly ConcurrentDictionary<IPerson, long> personToChat = new ConcurrentDictionary<IPerson, long>();
        
        private readonly ConcurrentDictionary<ICity, long> cityToChat = new ConcurrentDictionary<ICity, long>();
        
        private readonly SemaphoreSlim cityAwakeSemaphore = new SemaphoreSlim(1, 1);
        private readonly ConcurrentDictionary<ICity, bool> citiToAwake = new ConcurrentDictionary<ICity, bool>();
        
        private readonly SemaphoreSlim cityVotingSemaphore = new SemaphoreSlim(1, 1);
        private readonly ConcurrentDictionary<string, CityVotingPool> cityVotingPools =
            new ConcurrentDictionary<string, CityVotingPool>();


        public async Task AskForInteractionTarget(IEnumerable<IPerson> players, Role role, ICity city)
        {
            if (!cityToChat.ContainsKey(city)) return;

            var choosers = players.ToHashSet();
            if (choosers.Count == 0) return;
            
            await TellGreetingsToRole(role, city);

            if (role.DayTime == DayTime.Day)
            {
                await AskJudgedPerson(city, role, choosers);
            }
            else
            {
                await AskRoleForInteractionTarget(city, role, choosers);
            }
        }

        public async Task<IPerson> GetInteractionTarget(Role role, ICity city)
        {
            if (!cityVotingPools.ContainsKey(city.Name))
            {
                return null;
            }

            var votingPool = cityVotingPools[city.Name];

            var result = votingPool.ExtractRoleVoteWinner(role);

            if (!votingPool.IsEmpty) return city.GetPersonByName(result);
            
            var lockTaken = false;

            try
            {
                await cityVotingSemaphore.WaitAsync();
                lockTaken = true;

                cityVotingPools.TryRemove(city.Name, out votingPool);
            }
            finally
            {
                if (lockTaken)
                {
                    cityVotingSemaphore.Release();
                }
            }

            return city.GetPersonByName(result);
        }

        private async Task TellGreetingsToRole(Role role, ICity city)
        {
            var chatId = cityToChat[city];
            var lockTaken = false;
            
            try
            {
                await cityAwakeSemaphore.WaitAsync();
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
                    cityAwakeSemaphore.Release();
                }
            }
        }

        private async Task AskRoleForInteractionTarget(ICity city, Role role, IReadOnlyCollection<IPerson> choosers)
        {
            var names = city.Population
                .Where(p => p.IsAlive && !choosers.Contains(p))
                .Select(p => p.Name).ToArray();

            await AddCityVoting(city.Name);

            var messageText = CreateMessageText("<b>Who will be your target?</b>\n\n", city.Name, names);
            
            var cityVotingPool = cityVotingPools[city.Name];

            foreach (var userId in choosers.Select(person => personToChat[person]))
            {
                cityVotingPool.AddChatId(userId);
                cityVotingPool.AddRole(userId, role);
            }
            
            var roleVoting = new RoleVotingPool(names);
            cityVotingPool.AddRoleVoting(role, roleVoting);
            
            if (names.Length == 1)
            {
                // bot.SendTextMessageAsync(chatId, $"{role.Name} automatically chosen {targets[0]}").Wait();
                roleVoting.Vote(0, 0); // illegal move
                return;
            }
            
            await Task.WhenAll(choosers.Select(person => bot.SendTextMessageAsync(personToChat[person], messageText, ParseMode.Html)));
        }

        private async Task AskJudgedPerson(ICity city, Role role, IReadOnlyCollection<IPerson> choosers)
        {
            if (choosers.Count < 2) return;
            
            var chatId = cityToChat[city];

            await AddCityVoting(city.Name);
            
            var names = choosers.Select(p => p.Name).ToArray();

            var messageText = CreateMessageText("<b>Who will you blame?</b>\n\n", city.Name, names);

            var cityVotingPool = cityVotingPools[city.Name];
            
            cityVotingPool.AddChatId(chatId);
            foreach (var userId in choosers.Select(c => personToChat[c]))
            {
                cityVotingPool.AddRole(userId, role);
            }

            var roleVoting = new RoleVotingPool(names);
            cityVotingPool.AddRoleVoting(role, roleVoting);

            await bot.SendTextMessageAsync(chatId, messageText, ParseMode.Html);
        }

        private async Task AddCityVoting(string cityName)
        {
            var lockTaken = false;

            try
            {
                await cityVotingSemaphore.WaitAsync();
                lockTaken = true;

                if (!cityVotingPools.ContainsKey(cityName))
                {
                    cityVotingPools[cityName] = new CityVotingPool();
                }
            }
            finally
            {
                if (lockTaken)
                {
                    cityVotingSemaphore.Release();
                }
            }
        }

        private static string CreateMessageText(string title, string cityName, string[] names)
        {
            var messageText = new StringBuilder(title);
            for (var i = 0; i < names.Length; i++)
            {
                messageText.Append(names[i]);
                messageText.Append($"\n/{i}_{cityName}\n\n");
            }

            return messageText.ToString();
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
                case WinState.PsychoWins:
                    await bot.SendTextMessageAsync(chatId, "Все сошли с ума");
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
            
            await bot.SendTextMessageAsync(chatId,mapMessage.ToString());
            
            await bot.SendTextMessageAsync(chatId, "🍑");
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
            
            Console.WriteLine($"{user.Username}: {user.Id}");
            if (message.Text == null)
            {
                return;
            }

            var messageText = message.Text.Replace($"@{me.Username}", "");

            switch (messageText)
            {
               case PlayCommand:
                   await PlayMethod(chat, user);
                   return;
               case EndRecordCommand: 
                   await EndRecordMethod(chat.Id); 
                   return;
               case "/help":
               case "/start": 
                   await bot.SendTextMessageAsync(chat.Id, Resources.HelpMessage);
                   return;
               case "/guide":
                   await bot.SendTextMessageAsync(chat.Id, Resources.GuideMessage); 
                   return;
               case "/settings":
                   await bot.SendTextMessageAsync(chat.Id, Resources.SettingsMessage);
                   return;
            }

            var parts = messageText.Split('_');
            if (parts.Length == 2 && parts[0].Length > 1)
            {
                var index = parts[0].Substring(1);
                await ConsiderVote(index, parts[1], chat.Id, user.Id);
            }

            if (parts.Length == 1)
            {
                await RememberSettings(chat.Id, parts[0].Replace("/", ""));
            }
        }

        private async Task PlayMethod(Chat chat, User user)
        {
            var lockTaken = false;

            try
            {
                await playSemaphore.WaitAsync();
                lockTaken = true;
                
                if (!gameRecords.ContainsKey(chat.Id))
                {
                    gameRecords[chat.Id] = new GameRecord(random);
                    await bot.SendTextMessageAsync(chat.Id, "Record to game started!");
                }
            }
            finally
            {
                if (lockTaken)
                {
                    playSemaphore.Release();
                }
            }

            var pool = gameRecords[chat.Id];

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
            IGameRecord record;

            var lockTaken = false;

            try
            {
                await playSemaphore.WaitAsync();
                lockTaken = true;
                
                if (!gameRecords.TryRemove(chatId, out record))
                {
                    await bot.SendTextMessageAsync(chatId, $"There no opened game record\nType {PlayCommand} to start one");
                }
            }
            finally
            {
                if (lockTaken)
                {
                    playSemaphore.Release();
                }
            }

            if (record == null)
            {
                return;
            }

            var population = new List<IPerson>();

            foreach (var (userId, person) in record.ExtractPersons())
            {
                population.Add(person);
                personToChat[person] = userId;
                var message = person.NightRole == null
                    ? $"Your name is {person.Name} and you are Peaceful"
                    : $"Your name is {person.Name} and you are {person.NightRole.Name}";
                await SendUsersSaveMessage(message, userId, chatId);
            }

            var settings = record.Settings;
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

        private async Task ConsiderVote(string index, string cityName, long chatId, long userId)
        {
            if (!index.All(char.IsDigit))
            {
                return;
            }

            var name = int.Parse(index);

            var lockTaken = false;
            try
            {
                await cityVotingSemaphore.WaitAsync();
                lockTaken = true;

                if (cityVotingPools.ContainsKey(cityName))
                {
                    await bot.SendTextMessageAsync(chatId, cityVotingPools[cityName].Vote(chatId, userId, name));
                }
            }
            finally
            {
                if (lockTaken)
                {
                    cityVotingSemaphore.Release();
                }
            }
        }

        private async Task RememberSettings(long chatId, string settingsName)
        {
            if (!gameRecords.ContainsKey(chatId))
            {
                return;
            }

            var previousSettings = gameRecords[chatId].Settings;

            gameRecords[chatId].Settings = settingsName switch
            {
                nameof(Settings.Classic) => Settings.Classic,
                nameof(Settings.Deadly) => Settings.Deadly,
                nameof(Settings.Detective) => Settings.Detective,
                nameof(Settings.Various) => Settings.Various,
                nameof(Settings.CrazyNosyBizarreTown) => Settings.CrazyNosyBizarreTown,
                nameof(Settings.GoodForHealthBedForEducation) => Settings.GoodForHealthBedForEducation,
                _ => gameRecords[chatId].Settings
            };

            if (gameRecords[chatId].Settings != previousSettings)
            {
                await bot.SendTextMessageAsync(chatId, $"Successfully changed game settings to {settingsName} ⚙️");
            }
        }
    }
}