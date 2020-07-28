using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InternalService.Administrative
{
    public static class Manage
    {
        public static async Task CheckUpdateDiscordServer()
        {
            string output = string.Empty;
            foreach (SocketGuildUser socketGuildUser in Main.Manage.DiscordSocketClient.GetGuild(Main.Info.MainServerId).Users)
            {
                string result = Info.UnknownErrorMessage;
                Post post = socketGuildUser.GetMaxPostFromDiscordUser();
                if (post == null)
                {
                    if (IsEmployee(socketGuildUser.Id.ToString()))
                        await RemoveEmployeeAsync(socketGuildUser.Id.ToString(), socketGuildUser.Mention, result);
                    else
                        continue;
                }
                else
                {
                    if (IsEmployee(socketGuildUser.Id.ToString()))
                        await UpdateEmployeeAsync(socketGuildUser.Id.ToString(), post, result);
                    else
                        await SetEmployeeAsync(socketGuildUser.Id.ToString(), socketGuildUser.Mention, Info.DefaultSteamId, post.DiscordId, result);
                }
                output = output + result + "\n";
                if (output.Length > 1900)
                {
                    await Main.Manage.Log(Main.LogType.Trace, output);
                    output = string.Empty;
                }
            }
            if (output != string.Empty)
            {
                await Main.Manage.Log(Main.LogType.Trace, output);
            }
            Info.IsUpdated = true;
        }

        #region Foundation
        public static async Task<string> SetPostAsync(string roleId, string name, string gameRole, int position, string result)
        {
            using (FoundationContext db = new FoundationContext())
            {
                Post post = new Post()
                {
                    DiscordId = roleId,
                    GameRole = gameRole,
                    Name = name,
                    Position = position
                };
                if (db.Posts.FirstOrDefault(x => x.GameRole == post.GameRole) != default)
                {
                    result = "Игровая роль \"" + post.GameRole + "\" уже определена!";
                    return result;
                }
                if (db.Posts.FirstOrDefault(x => x.DiscordId == post.DiscordId) != default)
                {
                    Post editPost = db.Posts.FirstOrDefault(x => x.DiscordId == post.DiscordId);
                    editPost.GameRole = post.GameRole;
                    result = "Должности " + editPost.Name + " переопределена игровая роль - \"" + editPost.GameRole + "\".";
                }
                else
                {
                    db.Posts.Add(post);
                    result = "Должности " + post.Name + " определена игровая роль - \"" + post.GameRole + "\".";
                }
                await db.SaveChangesAsync();
            }
            return result;
        }
        public static async Task<string> RemovePostAsync(string roleId, string name, string result)
        {
            using (FoundationContext db = new FoundationContext())
            {
                if (db.Posts.FirstOrDefault(x => x.DiscordId == roleId) == default)
                {
                    result = "Игровая роль для должности " + name + " не определена";
                    return result;
                }
                Post removePost = db.Posts.FirstOrDefault(x => x.DiscordId == roleId);
                List<Employee> remoteEmployees = db.Employees.Include(x => x.Post).Where(x => x.Post.DiscordId == removePost.DiscordId).ToList();

                result = "Удалено определение игровой роли - \"" + removePost.GameRole + "\" для " +  remoteEmployees.Count + " сотрудников.";
                db.Employees.RemoveRange(remoteEmployees);

                db.Posts.Remove(removePost);
                await db.SaveChangesAsync();
            }
            return result;
        }
        public static async Task<string> SetEmployeeAsync(string discordId, string name, string steamId, string postId, string result)
        {
            using (FoundationContext db = new FoundationContext())
            {
                Employee employee = new Employee()
                {
                    DiscordId = discordId,
                    SteamId = steamId,
                    Name = name,
                    Post = db.Posts.FirstOrDefault(x => x.DiscordId == postId)
                };
                if (db.Employees.FirstOrDefault(x => x.SteamId == employee.SteamId) != default && employee.SteamId != Info.DefaultSteamId)
                {
                    result = "Идентификатор стима \"" + employee.SteamId + "\" уже определен!";
                    return result;
                }
                if (db.Employees.FirstOrDefault(x => x.DiscordId == employee.DiscordId) != default)
                {
                    Employee editEmployee = db.Employees.FirstOrDefault(x => x.DiscordId == employee.DiscordId);
                    editEmployee.SteamId = employee.SteamId;
                    result = "Сотруднику " + editEmployee.Name + " переопределен идентификатор стима - \"" + editEmployee.SteamId + "\".";
                }
                else
                {
                    db.Employees.Add(employee);
                    result = "Сотруднику " + employee.Name + " определен идентификатор стима - \"" + employee.SteamId + "\".";
                }
                await db.SaveChangesAsync();
            }
            return result;
        }
        public static async Task<string> RemoveEmployeeAsync(string discordId, string name, string result)
        {
            using (FoundationContext db = new FoundationContext())
            {
                if (db.Employees.FirstOrDefault(x => x.DiscordId == discordId) == default)
                {
                    result = "Сотрудник \"" + name + "\" не найден!";
                    return result;
                }
                Employee removeEmployee = db.Employees.FirstOrDefault(x => x.DiscordId == discordId);
                result = "Удалено определение идентификатора стима - \"" + removeEmployee.SteamId + "\" для сотрудника " + removeEmployee.Name + ".";
                db.Employees.Remove(removeEmployee);
                await db.SaveChangesAsync();
            }
            return result;
        }
        public static async Task<string> UpdateEmployeeAsync(string discordId, Post post, string result)
        {
            using (FoundationContext db = new FoundationContext())
            {
                Employee employee = db.Employees.FirstOrDefault(x => x.DiscordId == discordId);
                employee.Post = db.Posts.FirstOrDefault(x => x.DiscordId == post.DiscordId);
                result = "Сотруднику " + employee.Name + " переопределена должность - \"" + employee.Post.Name + "\".";
                await db.SaveChangesAsync();
            }
            return result;
        }
        #endregion

        #region Helper
        public static Post GetMaxPostFromDiscordUser(this SocketGuildUser socketGuildUser)
        {
            using (FoundationContext db = new FoundationContext())
            {
                Post maxPost = new Post() { Position = 0 };
                foreach (Post post in db.Posts)
                {
                    ulong postDiscordId = ulong.Parse(post.DiscordId);
                    if (socketGuildUser.Roles.Where(x => x.Id == postDiscordId).FirstOrDefault() != default)
                    {
                        if (post.Position > maxPost.Position)
                            maxPost = post;
                    }
                }
                if (maxPost.Position == 0)
                    return null;
                return maxPost;
            }
        }
        public static bool UserOnTheServer(ulong discordId, ulong serverId = Main.Info.MainServerId)
        {
            return Main.Manage.DiscordSocketClient.GetGuild(serverId).GetUser(discordId) != null;
        }
        public static bool IsEmployee(string discordId)
        {
            using (FoundationContext db = new FoundationContext())
            {
                return db.Employees.FirstOrDefault(x => x.DiscordId == discordId) != default;
            }
        }
        public static bool IsPost(string discordId)
        {
            using (FoundationContext db = new FoundationContext())
            {
                return db.Posts.FirstOrDefault(x => x.DiscordId == discordId) != default;
            }
        }
        #endregion

        #region Discord Events
        public static async Task OnReady()
        {
            if (!Info.IsUpdated)
                await CheckUpdateDiscordServer();
        }
        public static async Task OnRoleUpdated(SocketRole arg1, SocketRole arg2)
        {
            if (arg1.Guild.Id != Main.Info.MainServerId)
                return;
            if (arg1.Id == arg2.Id && arg1.Position != arg2.Position && IsPost(arg2.Id.ToString()))
            {
                string result = Info.UnknownErrorMessage;
                using (FoundationContext db = new FoundationContext())
                {
                    string discordId = arg2.Id.ToString();
                    Post post = db.Posts.FirstOrDefault(x => x.DiscordId == discordId);
                    post.Position = arg2.Position;
                    result = "Должности " + post.Name + " переопределен приоритет - \"" + post.Position + "\".";                   
                    await db.SaveChangesAsync();
                }
                await Main.Manage.Log(Main.LogType.Info, result);
                foreach (SocketGuildUser socketGuildUser in arg2.Members)
                {
                    Post post = socketGuildUser.GetMaxPostFromDiscordUser();
                    if (IsEmployee(socketGuildUser.Id.ToString()) && post != null)
                    {
                        await UpdateEmployeeAsync(socketGuildUser.Id.ToString(), post, result);
                    }
                }
            }
        }
        public static async Task OnRoleDeleted(SocketRole arg)
        {
            if (arg.Guild.Id != Main.Info.MainServerId)
                return;
            if (IsPost(arg.Id.ToString()))
            {
                string result = Info.UnknownErrorMessage;
                await RemovePostAsync(arg.Id.ToString(), arg.Mention, result);
                await Main.Manage.Log(Main.LogType.Info, result);
            }
        }
        public static async Task OnUserLeft(SocketGuildUser arg)
        {
            if (arg.Guild.Id != Main.Info.MainServerId)
                return;
            if (UserOnTheServer(arg.Id) && IsEmployee(arg.Id.ToString()))
            {
                string result = Info.UnknownErrorMessage;
                await RemoveEmployeeAsync(arg.Id.ToString(), arg.Mention, result);
                await Main.Manage.Log(Main.LogType.Warn, result);
            }
        }
        public static async Task OnUserBanned(SocketUser arg1, SocketGuild arg2)
        {
            if (arg2.Id != Main.Info.MainServerId)
                return;
            if (UserOnTheServer(arg1.Id) && IsEmployee(arg1.Id.ToString()))
            {
                string result = Info.UnknownErrorMessage;
                await RemoveEmployeeAsync(arg1.Id.ToString(), arg1.Mention, result);
                await Main.Manage.Log(Main.LogType.Warn, result);
            }
        }
        public static async Task OnGuildMemberUpdated(SocketGuildUser arg1, SocketGuildUser arg2)
        {
            if (arg1.Guild.Id != Main.Info.MainServerId)
                return;
            if (arg1.Roles.Count == arg2.Roles.Count)
            {
                return;
            }
            string result = Info.UnknownErrorMessage;
            Post post = arg2.GetMaxPostFromDiscordUser();

            if (post == null)
            {
                if (IsEmployee(arg2.Id.ToString()))
                    await RemoveEmployeeAsync(arg2.Id.ToString(), arg2.Mention, result);
            }
            else
            {
                if (IsEmployee(arg2.Id.ToString()))
                    await UpdateEmployeeAsync(arg2.Id.ToString(), post, result);
                else
                    await SetEmployeeAsync(arg2.Id.ToString(), arg2.Mention, Info.DefaultSteamId, post.DiscordId, result);
            }
            if (result != Info.UnknownErrorMessage)
                await Main.Manage.Log(Main.LogType.Info, result);
        }
        public static async Task OnMessageReceived(SocketMessage arg)
        {
            if (arg.Content is null || arg.Author.IsBot || arg.Channel.Id != 733606297681002586)
            {
                return;
            }
            #region Parse Args
            string[] args = arg.Content.ToString().Split(' ');
            if (args[0] == null || args[0] == string.Empty || args[0].Length < 2 || args[0].ToCharArray()[0].ToString() != Info.Prefix || args.Length < 2)
            {
                return;
            }
            args[0] = args[0].Substring(1, 1).ToUpper() + args[0].Substring(2).ToLower();
            args[1] = args[1].Substring(0, 1).ToUpper() + args[1].Substring(1).ToLower();
            if (!Enum.TryParse(typeof(Entity), args[0], out object entity))
            {
                await arg.Channel.SendMessageAsync("Неизвестная сущность \"" + args[0] + "\"!");
                return;
            }
            if (!Enum.TryParse(typeof(Command), args[1], out object command))
            {
                await arg.Channel.SendMessageAsync("Неизвестная команда \"" + args[1] + "\"!");
                return;
            }
            #endregion

            #region Parse Command
            if (((Command)command == Command.Remove && args.Length < 3) || ((Command)command == Command.Set && args.Length < 4))
            {
                await arg.Channel.SendMessageAsync("Недостаточно аргументов!");
                return;
            }
            else if (((Command)command == Command.Remove && args.Length > 3) || ((Command)command == Command.Set && args.Length > 4))
            {
                await arg.Channel.SendMessageAsync("Переизбыток аргументов!");
                return;
            }
            #endregion

            string result = Info.UnknownErrorMessage;

            switch ((Entity)entity)
            {
                case Entity.Post:
                    switch ((Command)command)
                    {
                        case Command.Set:
                            SocketRole socketRole = Main.Manage.DiscordSocketClient.GetGuild(Main.Info.MainServerId).Roles.Where(x => x.Mention == args[2]).FirstOrDefault();
                            if (socketRole == default)
                            {
                                await arg.Channel.SendMessageAsync("Должность \"" + args[2] + "\" не найдена!");
                                return;
                            }
                            result = await SetPostAsync(socketRole.Id.ToString(), args[2], args[3], socketRole.Position, result);
                            await arg.Channel.SendMessageAsync(result);
                            break;
                        case Command.Remove:
                            if (Main.Manage.DiscordSocketClient.GetGuild(Main.Info.MainServerId).Roles.Where(x => x.Mention == args[2]).FirstOrDefault() == default)
                            {
                                await arg.Channel.SendMessageAsync("Должность \"" + args[2] + "\" не найдена!");
                                return;
                            }
                            result = await RemovePostAsync(Main.Manage.DiscordSocketClient.GetGuild(Main.Info.MainServerId).Roles.Where(x => x.Mention == args[2]).FirstOrDefault().Id.ToString(), Main.Manage.DiscordSocketClient.GetGuild(Main.Info.MainServerId).Roles.Where(x => x.Mention == args[2]).FirstOrDefault().Mention, result);
                            await arg.Channel.SendMessageAsync(result);
                            break;
                        case Command.List:
                            result = "Список должностей:";
                            List<Post> posts = new List<Post>();
                            using (FoundationContext db = new FoundationContext())
                            {
                                posts.AddRange(db.Posts);
                            }
                            if (posts.Count == 0)
                                result = "Должности не определены.";
                            foreach (Post post in posts)
                            {
                                result = result + "\n[" + post.Position + "] - " + post.Name + " - " + post.GameRole;
                            }
                            await arg.Channel.SendMessageAsync(result);
                            break;
                    }
                    break;
                case Entity.Employee:
                    switch ((Command)command)
                    {
                        case Command.Set:
                            SocketGuildUser socketGuildUser = Main.Manage.DiscordSocketClient.GetGuild(Main.Info.MainServerId).Users.Where(x => x.Mention == args[2]).FirstOrDefault();
                            if (socketGuildUser == default)
                            {
                                await arg.Channel.SendMessageAsync("Сотрудник \"" + args[2] + "\" не найден!");
                                return;
                            }
                            if (!ulong.TryParse(args[3], out ulong steamId))
                            {
                                await arg.Channel.SendMessageAsync("Неверный аргумент \"Идентификатор стима\"!");
                                return;
                            }
                            Post post = socketGuildUser.GetMaxPostFromDiscordUser();
                            if (post == null)
                            {
                                result = "Сотрудник не имеет должностей";
                                await arg.Channel.SendMessageAsync(result);
                                return;
                            }
                            result = await SetEmployeeAsync(socketGuildUser.Id.ToString(), socketGuildUser.Mention, steamId.ToString(), post.DiscordId, result);
                            await arg.Channel.SendMessageAsync(result);
                            break;
                        case Command.Remove:
                            if (Main.Manage.DiscordSocketClient.GetGuild(Main.Info.MainServerId).Users.Where(x => x.Mention == args[2]).FirstOrDefault() == default)
                            {
                                await arg.Channel.SendMessageAsync("Сотрудник \"" + args[2] + "\" не найден!");
                                return;
                            }
                            result = await RemoveEmployeeAsync(Main.Manage.DiscordSocketClient.GetGuild(Main.Info.MainServerId).Users.Where(x => x.Mention == args[2]).FirstOrDefault().Id.ToString(), Main.Manage.DiscordSocketClient.GetGuild(Main.Info.MainServerId).Users.Where(x => x.Mention == args[2]).FirstOrDefault().Mention, result);
                            await arg.Channel.SendMessageAsync(result);
                            break;
                        case Command.List:
                            result = "Список сотрудников:";
                            using (FoundationContext db = new FoundationContext())
                            {
                                if (db.Employees.ToList().Count() == 0)
                                    result = "Сотрудники не определены.";
                                foreach (Employee employee in db.Employees.Include(x => x.Post).ToList())
                                {
                                    result = result + "\n[" + employee.SteamId + "] - " + employee.Name + " - " + employee.Post.Name;
                                    if (result.Length > 1900)
                                    {
                                        await arg.Channel.SendMessageAsync(result);
                                        result = string.Empty;
                                    }
                                }
                            }
                            if (result != string.Empty)
                                await arg.Channel.SendMessageAsync(result);
                            break;
                    }
                    break;
            }
        }
        #endregion
    }
    public enum Command
    {
        Remove, Set, List
    }
    public enum Entity
    {
        Post, Employee
    }
}