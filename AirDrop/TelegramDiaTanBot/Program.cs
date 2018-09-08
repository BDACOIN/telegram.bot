
using System;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Args;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.IO;

/*
 *         memberinfo = {
            "eth_address": "",
            "user_id": 0
        }
*/


namespace Awesome
{
    public class MemberInfo
    {
        public String eth_address { get; set; }
        public int user_id { get; set; }
        public String first_name { get; set; }
        public String last_name { get; set; }
        public String usr_name { get; set; }
    }

    class Program
    {
        static ITelegramBotClient botClient;

        static void Main()
        {
            var token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
            botClient = new TelegramBotClient(token);

            var me = botClient.GetMeAsync().Result;
            Console.WriteLine(
              $"Hello, World! I am user {me.Id} and my name is {me.FirstName}."
            );

            botClient.OnMessage += Bot_OnMessage;
            botClient.StartReceiving();
            Thread.Sleep(int.MaxValue);
        }

        static async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            try
            {
                if (e.Message.Text != null && e.Message.Text.StartsWith("0x"))
                {
                    var m = Regex.Match(e.Message.Text, "^0x[0-9a-zA-Z]{40}$");
                    if (m.Success)
                    {
                        Console.WriteLine($"Received a text message in chat {e.Message.Chat.Id}.");

                        if (e.Message.From.Username != null && e.Message.From.Username.Length > 0)
                        {
                            // e.Message.From.Id
                            await botClient.SendTextMessageAsync(
                              chatId: e.Message.Chat, text: "返信相手:\n　@" + e.Message.From.Username + "\n------------------------\nあなたのUserNameは、「@" + e.Message.From.Username + "」です。\nこのUserNameをGoogleFormへと記載してください。" + "\n");

                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(
                            chatId: e.Message.Chat, text: "返信相手:\n　" + e.Message.From.FirstName + " " + e.Message.From.LastName + "\n------------------------\nあなたはUserName (@から始まる名前) を設定していません。\n替わりにName「" + e.Message.From.FirstName + " " + e.Message.From.LastName + "」をGoogleFormへと記載してください。" + "\n");
                        }

                        MemberInfo mi = new MemberInfo();
                        mi.eth_address = e.Message.Text;
                        mi.user_id = e.Message.From.Id;
                        mi.first_name = e.Message.From.FirstName ?? "";
                        mi.last_name = e.Message.From.LastName ?? "";
                        mi.usr_name = "@" + e.Message.From.Username ?? "@";

                        var json = JsonConvert.SerializeObject(mi, Formatting.Indented);

                        using (StreamWriter sw = new StreamWriter(@".\DataMemberInfo\" + mi.user_id + ".json", false, System.Text.Encoding.UTF8))
                        {
                            // ファイルへの書き込み
                            sw.WriteLine(json);
                        }
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(
                          chatId: e.Message.Chat, text: "返信相手:\n　" + e.Message.From.FirstName + " " + e.Message.From.LastName + "\n------------------------\n" + "イーサリアムアドレスを投稿しようとしましたか？ 間違いがあるようです。");
                    }
                }
                else if (e.Message.Text != null && e.Message.Text.StartsWith("3P"))
                {
                    var m = Regex.Match(e.Message.Text, "^3P[0-9a-zA-Z]{20}"); // 本当は{33}だがまぁ適当に
                    if (m.Success)
                    {
                        await botClient.SendTextMessageAsync(
                          chatId: e.Message.Chat, text: "返信相手:\n　" + e.Message.From.FirstName + " " + e.Message.From.LastName + "\n------------------------\n" + "Wavesのウォレットアドレスを投稿しようとしていますか？\nWavesのウォレットアドレスではなくイーサウォレットアドレスを投稿してください。");
                    }
                }
            } catch(Exception ex)
            {
                await botClient.SendTextMessageAsync(
                  chatId: e.Message.Chat, text: ex.Message );

            }
        }
    }
}
