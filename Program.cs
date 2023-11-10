using Gw2Manager.Models;
using Gw2Manager.Utils;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

Console.Title = "Gw2 Manager";

var configuration = new ConfigurationBuilder()
     .AddJsonFile($"appsettings.json");

var config = configuration.Build();
var globalSettings = config.GetSection("GlobalSettings").Get<GlobalSettings>() ?? new GlobalSettings();
var commandsFromSettings = config.GetSection("Commands").Get<List<Command>>() ?? new List<Command>();

List<Command> commands = new();
for (int i = 0; i < commandsFromSettings.Count; i++)
{
    commandsFromSettings[i].Keys = GetKeysForIndex(i);
    commands.Add(commandsFromSettings[i]);
}

ShowMenu();

var isUserSelectingCommands = SelectEntry(globalSettings.AutoStartTimer);
while (isUserSelectingCommands)
{
    isUserSelectingCommands = SelectEntry();
}

await Run();


void ShowMenu(bool clear = false)
{
    if (clear)
        Console.Clear();

    Console.WriteLine("Bienvenue dans votre interface de gestion de Guild Wars 2.");
    Console.WriteLine();

    Console.WriteLine("Veuillez appuyer sur les touches correspondantes aux entrées pour activer ou désactiver des options.");

    foreach (var command in commands)
    {
        CustomWriteLine(command);
    }
}

bool SelectEntry(int maxTime = -1)
{
    try
    {
        var key = Reader.ReadKey(maxTime);

        if (key == ConsoleKey.Enter)
            return false;

        var matchingCommand = GetMatchingCommand(key);
        if (matchingCommand == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Entrée non reconnue.");
            Console.ResetColor();

            return true;
        }

        if (matchingCommand.Type == CommandType.Exec)
        {
            matchingCommand.State = matchingCommand.State switch
            {
                CommandState.Unselected => CommandState.Selected,
                CommandState.Selected => CommandState.Unselected
            };
        }
        else
        {
            matchingCommand.State = matchingCommand.State switch
            {
                CommandState.Unselected => CommandState.Selected,
                CommandState.Selected => CommandState.ToDelete,
                CommandState.ToDelete => CommandState.Unselected,
            };
        }

        ShowMenu(true);

        return true;
    }
    catch (Exception)
    {
        return false;
    }
}

async Task Run()
{
    Console.WriteLine();

    foreach (var command in commands.Where(c => c.State != CommandState.Unselected).OrderBy(c => c.ListNumber))
    {
        Console.WriteLine("---------------------");
        if (command.Type == CommandType.Exec)
        {
            Console.WriteLine($"Exécution de {command.Name}");

            Process exec = new();
            exec.StartInfo.FileName = command.Data;
            exec.StartInfo.Arguments = command.AdditionalData;

            exec.Start();
        }
        else
        {
            if (command.State == CommandState.ToDelete)
            {
                Console.WriteLine($"Suppression de {command.Name}");

                if (File.Exists(command.AdditionalData))
                {
                    File.Delete(command.AdditionalData);
                }

                Console.WriteLine($"{command.Name} supprimé");
            }
            else
            {
                Console.WriteLine($"Mise à jour de {command.Name}");

                using var client = new HttpClient();

                string url = command.Data;
                if (command.Type == CommandType.Github)
                {
                    client.DefaultRequestHeaders.UserAgent.TryParseAdd("request");

                    var json = await client.GetStringAsync(url);
                    var jObject = JObject.Parse(json);

                    url = (string)jObject["browser_download_url"];
                }


                using var s = await client.GetStreamAsync(url);
                using var fs = new FileStream(command.AdditionalData, FileMode.OpenOrCreate);
                await s.CopyToAsync(fs);

                Console.WriteLine($"{command.Name} mis à jour");
            }
        }

        Console.WriteLine("---------------------");
        Console.WriteLine();
    }

    Console.WriteLine("Toutes les commandes ont été exécutées");
    Console.ReadLine();
}

void CustomWriteLine(Command command)
{
    if (command.State == CommandState.Selected)
        Console.ForegroundColor = ConsoleColor.Green;
    else if (command.State == CommandState.ToDelete)
        Console.ForegroundColor = ConsoleColor.Red;

    string additionalText = string.Empty;
    if (command.Type != CommandType.Exec)
        additionalText = (command.State == CommandState.Selected) ? "(mise à jour)" : (command.State == CommandState.ToDelete) ? "(suppression)" : string.Empty;

    var text = $"{command.ListNumber}. {command.Name} {additionalText}";


    Console.WriteLine(text);

    Console.ResetColor();
}

Command GetMatchingCommand(ConsoleKey key)
{
    return commands.FirstOrDefault(c => c.IsLinkedToKey(key));
}

List<ConsoleKey> GetKeysForIndex(int i) =>
i switch
{
    0 => new List<ConsoleKey> { ConsoleKey.D1, ConsoleKey.NumPad1 },
    1 => new List<ConsoleKey> { ConsoleKey.D2, ConsoleKey.NumPad2 },
    2 => new List<ConsoleKey> { ConsoleKey.D3, ConsoleKey.NumPad3 },
    3 => new List<ConsoleKey> { ConsoleKey.D4, ConsoleKey.NumPad4 },
    4 => new List<ConsoleKey> { ConsoleKey.D5, ConsoleKey.NumPad5 },
    5 => new List<ConsoleKey> { ConsoleKey.D6, ConsoleKey.NumPad6 },
    6 => new List<ConsoleKey> { ConsoleKey.D7, ConsoleKey.NumPad7 },
    7 => new List<ConsoleKey> { ConsoleKey.D8, ConsoleKey.NumPad8 },
    8 => new List<ConsoleKey> { ConsoleKey.D9, ConsoleKey.NumPad9 },
    9 => new List<ConsoleKey> { ConsoleKey.D0, ConsoleKey.NumPad0 },
    _ => throw new Exception("Trop d'éléments."),
};