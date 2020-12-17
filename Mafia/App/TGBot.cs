using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mafia.Domain;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;

namespace Mafia.App
{
    public class TGBot : IUserInterface
    {
        private static TelegramBotClient Bot;
        
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

        static TGBot()
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
        }
        
        // /startgame -> settings [defaultCondition, people]
        // take condition (default), take roles and their count, total count!?
            // all players must type /play, (randomly choose role)
            // if player is peaceful [nightRole is null]
        // finally create game and start it already
        // more shit code ahead

        private static void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;

            var chat = message.Chat;
            var user = message.From;
            

            // chat.id => game record
            
            if (message.Text == "//startgame")
            {
                // if game on record, pass
                
                // take chat.Id
                // when in this chat someone types /play
                // save person, chat id; give roles
            }

            if (message.Text == "//play")
            {
                // if in chat.Id opened game record
                    // if person not in this game, add to game
            }

            var msg = Bot.SendTextMessageAsync(chat.Id, "Cum");
        }
    }

    public interface IPlayersPool
    {
        bool IsOpen { get; set; }
        Task AddPlayerAsync();
        IEnumerable<Person> ExtractPersons();
    }

    public class PlayersPool : IPlayersPool
    {
        public bool IsOpen { get; set; }

        public async Task AddPlayerAsync()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Person> ExtractPersons()
        {
            throw new NotImplementedException();
        }
    }
}