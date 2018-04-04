using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

// Create a module with no prefix
public class Info : ModuleBase
{
    // ~say hello -> hello
    [Command("say"), Summary("Echos a message.")]
    public async Task Say([Remainder, Summary("The text to echo")] string echo)
    {
        // ReplyAsync is a method on ModuleBase
        await ReplyAsync(echo);
    }
    // ~sample square 20 -> 400
    [Command("square"), Summary("Squares a number.")]
    public async Task Square([Summary("The number to square.")] int num)
    {
        // We can also access the channel from the Command Context.
        await Context.Channel.SendMessageAsync($"{num}^2 = {Math.Pow(num, 2)}");
    }

    // ~sample userinfo --> foxbot#0282
    // ~sample userinfo @Khionu --> Khionu#8708
    // ~sample userinfo Khionu#8708 --> Khionu#8708
    // ~sample userinfo Khionu --> Khionu#8708
    // ~sample userinfo 96642168176807936 --> Khionu#8708
    // ~sample whois 96642168176807936 --> Khionu#8708
    [Command("userinfo"), Summary("Returns info about the current user, or the user parameter, if one passed.")]
    [Alias("user", "whois")]
    public async Task UserInfo([Summary("The (optional) user to get info for")] IUser user = null)
    {
        var userInfo = user ?? Context.Client.CurrentUser;
        await ReplyAsync($"{userInfo.Username}#{userInfo.Discriminator}");
    }
    // this will post a message with the embedded link in it. // 
    [Command("salt"), Summary("Displays the Salty Hub")]
    [Alias("show", "hub")]
    public async Task Display([Summary("Starts the salty bet hub")]String sent)
    {
        var eb = new EmbedBuilder() { Title = "SaltyBet", Description = "This brings up the Salty Bet display", Url = "https://saltybet.com"};
        await Context.Channel.SendMessageAsync("", false, eb);
    }
}
