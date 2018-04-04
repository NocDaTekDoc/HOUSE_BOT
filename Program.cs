using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

class Program
{
    private readonly DiscordSocketClient _client;

    // Keep the CommandService and IServiceCollection around for use with commands.
    private readonly IServiceCollection _map = new ServiceCollection();
    private readonly CommandService _commands = new CommandService();
    private IServiceProvider services;
    // Program entry point
    static void Main(string[] args)
    {
        // Call the Program constructor, followed by the 
        // MainAsync method and wait until it finishes (which should be never).
        new Program().MainAsync().GetAwaiter().GetResult();
    }

    private Program()
    {
        _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            // How much logging do you want to see?
            LogLevel = LogSeverity.Info,
        });
    }

    // Example of a logging handler. This can be re-used by addons
    // that ask for a Func<LogMessage, Task>.
    private static Task Logger(LogMessage message)
    {
        var cc = Console.ForegroundColor;
        switch (message.Severity)
        {
            case LogSeverity.Critical:
            case LogSeverity.Error:
                Console.ForegroundColor = ConsoleColor.Red;
                break;
            case LogSeverity.Warning:
                Console.ForegroundColor = ConsoleColor.Yellow;
                break;
            case LogSeverity.Info:
                Console.ForegroundColor = ConsoleColor.White;
                break;
            case LogSeverity.Verbose:
            case LogSeverity.Debug:
                Console.ForegroundColor = ConsoleColor.DarkGray;
                break;
        }
        Console.WriteLine($"{DateTime.Now,-19} [{message.Severity,8}] {message.Source}: {message.Message}");
        Console.ForegroundColor = cc;

        // If you get an error saying 'CompletedTask' doesn't exist,
        // your project is targeting .NET 4.5.2 or lower. You'll need
        // to adjust your project's target framework to 4.6 or higher
        // (instructions for this are easily Googled).
        // If you *need* to run on .NET 4.5 for compat/other reasons,
        // the alternative is to 'return Task.Delay(0);' instead.
        return Task.CompletedTask;
    }

    private async Task MainAsync()                          // <-- ALL YOU CODE GOES IN HERE //
    {
        // Subscribe the logging handler.
        _client.Log += Logger;

        // Centralize the logic for commands into a seperate method.
        //await InitCommands();

        // Login and connect.
        await _client.LoginAsync(TokenType.Bot, "NDI2NTUwNDIzNjU1MDIyNTky.DZc8zw.na2hFfIJ5cWjbUe1BRlGUCRI2Vc");
        await _client.StartAsync();
        _services = new ServiceCollection().BuildServiceProvider();
        await InstallCommands();
        // Wait infinitely so your bot actually stays connected.
        await Task.Delay(-1);
    }

    private IServiceProvider _services;
    
    public async Task InstallCommands()
    {
        // Hook the MessageReceived Event into our Command Handler
        _client.MessageReceived += HandleCommand;
        // Discover all of the commands in this assembly and load them.
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
    }

    public async Task HandleCommand(SocketMessage messageParam)
    {
        // Don't process the command if it was a System Message
        var message = messageParam as SocketUserMessage;
        if (message == null) return;
        // Create a number to track where the prefix ends and the command begins
        int argPos = 0;
        // Determine if the message is a command, based on if it starts with '!' or a mention prefix
        if (!(message.HasCharPrefix('▲', ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos))) return;
        // Create a Command Context
        var context = new CommandContext(_client, message);
        // Execute the command. (result does not indicate a return value, 
        // rather an object stating if the command executed successfully)
        var result = await _commands.ExecuteAsync(context, argPos, services);
        if (!result.IsSuccess)
            await context.Channel.SendMessageAsync(result.ErrorReason);
    }
    private async Task HandleCommandAsync(SocketMessage arg)
    {
        // Bail out if it's a System Message.
        var msg = arg as SocketUserMessage;
        if (msg == null) return;

        // Create a number to track where the prefix ends and the command begins
        int pos = 0;
        // Replace the '!' with whatever character
        // you want to prefix your commands with.
        // Uncomment the second half if you also want
        // commands to be invoked by mentioning the bot instead.
        if (msg.HasCharPrefix('!', ref pos) /* || msg.HasMentionPrefix(_client.CurrentUser, ref pos) */)
        {
            // Create a Command Context.
            var context = new SocketCommandContext(_client, msg);

            // Execute the command. (result does not indicate a return value, 
            // rather an object stating if the command executed succesfully).
            var result = await _commands.ExecuteAsync(context, pos, _services);

            // Uncomment the following lines if you want the bot
            // to send a message if it failed (not advised for most situations).
            //if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
            //    await msg.Channel.SendMessageAsync(result.ErrorReason);
        }
    }
}
