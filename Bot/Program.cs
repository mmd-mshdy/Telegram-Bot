using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using static Crypto.Cryptoo;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TelegramBotDemo
{
    class Program
    {
        // Replace with your bot token
        private static readonly string botToken = "6319188455:AAFuyf5scrX3TCMQjQviS2Ji5LZ4hglMDM";

        // Create a bot client instance
        private static readonly TelegramBotClient botClient = new(botToken);

        static Dictionary<long, string> userStates = new Dictionary<long, string>();

        public static string botName = "@CS_SaintBot";

        static async Task Main()
        {


            // Subscribe to the OnMessage event
            botClient.OnMessage += Bot_OnMessage;

            botClient.OnMessageEdited += Bot_OnMessage;



            // Start receiving updates from Telegram
            botClient.StartReceiving();

            Console.WriteLine("Bot is running. Press any key to exit.");
            Console.ReadKey();

            botClient.StopReceiving();
        }
        private static async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            Telegram.Bot.Types.Message message = e.Message;
            Console.WriteLine(message.Chat.Id);
            Console.WriteLine(message.MessageId);
            List<string> CussWords = new List<string>();
            int LastId = 0;
            HttpClient httpClient = new HttpClient();
            string stringAPI = "https://api.wallex.ir/v1/currencies/stats";
            if ((message.Chat.Id > 0 && message.Type == MessageType.Text && message.Text.ToLower() == "/define") || (message.Chat.Id < 0 && message.Type == MessageType.Text && message.Text == $"/define{botName}"))
            {
                await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Please enter the word you want to define:");
                userStates[e.Message.Chat.Id] = "word";
                LastId = message.MessageId;
            }
            else if ((message.Chat.Id > 0 && message.Type == MessageType.Text && message.Text.ToLower() == "/crypto") || (message.Chat.Id < 0 && message.Type == MessageType.Text && message.Text == $"/crypto{botName}"))
            {


                HttpResponseMessage responsee = await httpClient.GetAsync(stringAPI);
                if (responsee.IsSuccessStatusCode)
                {
                    string apiresponse = await responsee.Content.ReadAsStringAsync();
                    ApiResponseWrapper apiWrapper = JsonConvert.DeserializeObject<ApiResponseWrapper>(apiresponse);
                    List<ResultItem> resultItems = apiWrapper.Result;
                    var cryptoKeys = new StringBuilder();
                    foreach (var item in resultItems)
                    {
                        cryptoKeys.Append($"{item.key} ,");
                    }
                    await botClient.SendTextMessageAsync
                    (
                    chatId: message.Chat,
                   text: $"Choose one of these: {cryptoKeys}",
                   parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown
                    );
                    userStates[e.Message.Chat.Id] = "crypto";
                    LastId = message.MessageId;

                }
            }
            if (userStates.ContainsKey(e.Message.Chat.Id) && message.MessageId > LastId + 2)
            {
                var state = userStates[e.Message.Chat.Id];
                if (state == "word")
                {
                    // Look up the word definition
                    var api_url = $"https://api.dictionaryapi.dev/api/v2/entries/en/{message.Text}";
                    var client = new HttpClient();
                    var response = await client.GetAsync(api_url);
                    if (response.IsSuccessStatusCode)
                    {
                        // Parse the JSON response
                        var json = await response.Content.ReadAsStringAsync();
                        var definition = JArray.Parse(json)[0]["meanings"][0]["definitions"][0]["definition"].ToString();

                        // Send the definition to the user
                        await botClient.SendTextMessageAsync(e.Message.Chat.Id, $"The definition of {message.Text} is:\n\n{definition}");
                    }
                    else
                    {
                        // Handle errors
                        await botClient.SendTextMessageAsync(e.Message.Chat.Id, $"Sorry, I couldn't find a definition for {message.Text}.");
                    }

                    userStates.Remove(e.Message.Chat.Id);
                }
                else if (state == "crypto")
                {
                    HttpResponseMessage response = await httpClient.GetAsync(stringAPI);
                    if (response.IsSuccessStatusCode)
                    {
                        string apiresponse = await response.Content.ReadAsStringAsync();
                        ApiResponseWrapper apiWrapper = JsonConvert.DeserializeObject<ApiResponseWrapper>(apiresponse);
                        List<ResultItem> resultItems = apiWrapper.Result;
                        string RL = message.Text;
                        if (resultItems.Exists(X => X.key == RL))
                        {
                            ResultItem item = resultItems.Find(X => X.key == RL);
                            Console.WriteLine(item.price);
                            await botClient.SendTextMessageAsync(e.Message.Chat.Id, $"The current price of {item.name_en} is: {item.price} \n Possible price in 1 hour is: {item.prediction()}");
                        }
                    }
                }
            }
        }
    }
}