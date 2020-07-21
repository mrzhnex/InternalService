using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InternalService.Administrative
{
    public static class Manage
    {
        #region Foundation
        public static void SetPost(string roleId, string name, string gameRole, int position, ref string result)
        {
            using (FoundationContext db = new FoundationContext(Info.ConnectionString))
            {
                Post post = new Post()
                {
                    DiscordId = roleId,
                    GameRole = gameRole,
                    Name = name,
                    Position = position
                };
                if (db.Posts.Where(x => x.GameRole == post.GameRole).FirstOrDefault() != default)
                {
                    result = "Игровая роль \"" + post.GameRole + "\" уже определена!";
                    return;
                }
                if (db.Posts.Where(x => x.DiscordId == post.DiscordId).FirstOrDefault() != default)
                {
                    Post editPost = db.Posts.Where(x => x.DiscordId == post.DiscordId).FirstOrDefault();
                    editPost.GameRole = post.GameRole;
                    result = "Должности " + editPost.Name + " переопределена игровая роль - \"" + editPost.GameRole + "\".";
                }
                else
                {
                    db.Posts.Add(post);
                    result = "Должности " + post.Name + " определена игровая роль - \"" + post.GameRole + "\".";
                }
                db.SaveChanges();
            }
        }
        public static void RemovePost(string roleId, string name, ref string result)
        {
            using (FoundationContext db = new FoundationContext(Info.ConnectionString))
            {
                if (db.Posts.Where(x => x.DiscordId == roleId).FirstOrDefault() == default)
                {
                    result = "Игровая роль для должности " + name + " не определена";
                    return;
                }
                Post removePost = db.Posts.Where(x => x.DiscordId == roleId).FirstOrDefault();
                result = "Удалено определение игровой роли - \"" + removePost.GameRole + "\".";
                db.Posts.Remove(removePost);

                //remove employees from removePost

                db.SaveChanges();
            }
        }
        public static void SetEmployee(string discordId, string name, string steamId, string postId, ref string result)
        {
            using (FoundationContext db = new FoundationContext(Info.ConnectionString))
            {
                Employee employee = new Employee()
                {
                    DiscordId = discordId,
                    SteamId = steamId,
                    Name = name,
                    Post = db.Posts.Where(x => x.DiscordId == postId).FirstOrDefault()
                };
                if (db.Employees.Where(x => x.SteamId == employee.SteamId).FirstOrDefault() != default)
                {
                    result = "Идентификатор стима \"" + employee.SteamId + "\" уже определен!";
                    return;
                }
                if (db.Employees.Where(x => x.DiscordId == employee.DiscordId).FirstOrDefault() != default)
                {
                    Employee editEmployee = db.Employees.Where(x => x.DiscordId == employee.DiscordId).FirstOrDefault();
                    editEmployee.SteamId = employee.SteamId;
                    result = "Сотруднику " + editEmployee.Name + " переопределен идентификатор стима - \"" + editEmployee.SteamId + "\".";
                }
                else
                {
                    db.Employees.Add(employee);
                    result = "Сотруднику " + employee.Name + " определен идентификатор стима - \"" + employee.SteamId + "\".";
                }
                db.SaveChanges();
            }
        }
        public static void RemoveEmployee(string discordId, string name, ref string result)
        {
            using (FoundationContext db = new FoundationContext(Info.ConnectionString))
            {
                if (db.Employees.Where(x => x.DiscordId == discordId).FirstOrDefault() == default)
                {
                    result = "Сотрудник \"" + name + "\" не найден!";
                    return;
                }
                Employee removeEmployee = db.Employees.Where(x => x.DiscordId == discordId).FirstOrDefault();
                result = "Удалено определение идентификатора стима - \"" + removeEmployee.SteamId + "\" для сотрудника " + removeEmployee.Name + ".";
                db.Employees.Remove(removeEmployee);
                db.SaveChanges();
            }
        }
        #endregion

        #region Helper
        public static Post GetMaxPostFromDiscordUser(this SocketGuildUser socketGuildUser)
        {
            using (FoundationContext db = new FoundationContext(Info.ConnectionString))
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
            using (FoundationContext db = new FoundationContext(Info.ConnectionString))
            {
                return db.Employees.Where(x => x.DiscordId == discordId).FirstOrDefault() != default;
            }
        }
        public static bool IsPost(string discordId)
        {
            using (FoundationContext db = new FoundationContext(Info.ConnectionString))
            {
                return db.Posts.Where(x => x.DiscordId == discordId).FirstOrDefault() != default;
            }
        }
        #endregion

        #region Discord Events
        public static async Task OnRoleUpdated(SocketRole arg1, SocketRole arg2)
        {
            if (arg1.Id == arg2.Id && arg1.Position != arg2.Position && IsPost(arg2.Id.ToString()))
            {
                string result = Info.UnknownErrorMessage;
                using (FoundationContext db = new FoundationContext(Info.ConnectionString))
                {
                    string discordId = arg2.Id.ToString();
                    Post post = db.Posts.Where(x => x.DiscordId == discordId).FirstOrDefault();
                    post.Position = arg2.Position;
                    result = "Должности " + post.Name + " переопределен приоритет - \"" + post.Position + "\".";
                    await db.SaveChangesAsync();
                }
                await Main.Manage.Log(Main.LogType.Info, result);
            }
        }
        public static async Task OnRoleDeleted(SocketRole arg)
        {
            if (IsPost(arg.Id.ToString()))
            {
                string result = Info.UnknownErrorMessage;
                RemovePost(arg.Id.ToString(), arg.Mention, ref result);
                await Main.Manage.Log(Main.LogType.Info, result);
            }
        }
        public static async Task OnUserLeft(SocketGuildUser arg)
        {
            if (UserOnTheServer(arg.Id) && IsEmployee(arg.Id.ToString()))
            {
                string result = Info.UnknownErrorMessage;
                RemoveEmployee(arg.Id.ToString(), arg.Mention, ref result);
                await Main.Manage.Log(Main.LogType.Warn, result);
            }
        }
        public static async Task OnUserBanned(SocketUser arg1, SocketGuild arg2)
        {
            if (UserOnTheServer(arg1.Id) && IsEmployee(arg1.Id.ToString()))
            {
                string result = Info.UnknownErrorMessage;
                RemoveEmployee(arg1.Id.ToString(), arg1.Mention, ref result);
                await Main.Manage.Log(Main.LogType.Warn, result);
            }
        }
        public static async Task OnGuildMemberUpdated(SocketGuildUser arg1, SocketGuildUser arg2)
        {
            if (arg1.Roles.Count == arg2.Roles.Count)
            {
                return;
            }
            if (IsEmployee(arg2.Id.ToString()))
            {
                string result = Info.UnknownErrorMessage;
                Post post = arg2.GetMaxPostFromDiscordUser();
                if (post == null)
                {
                    RemoveEmployee(arg2.Id.ToString(), arg2.Mention, ref result);
                }
                else
                {
                    using (FoundationContext db = new FoundationContext(Info.ConnectionString))
                    {
                        string discordId = arg2.Id.ToString();
                        Employee employee = db.Employees.Where(x => x.DiscordId == discordId).FirstOrDefault();
                        employee.Post = db.Posts.Where(x => x.DiscordId == post.DiscordId).FirstOrDefault();
                        result = "Сотруднику " + employee.Name + " переопределена должность - \"" + employee.Post.Name + "\".";
                        db.SaveChanges();
                    }
                }
                if (result != Info.UnknownErrorMessage)
                    await Main.Manage.Log(Main.LogType.Info, result);
            }
        }
        public static async Task OnMessageReceived(SocketMessage arg)
        {
            if (arg.Content is null || arg.Author.IsBot || arg.Channel.Id != 733606297681002586 || arg.Author.Id != 278885474893037568)
            {
                return;
            }

            #region Parse Args
            string[] args = arg.Content.ToString().Split(' ');
            if (args[0] == null || args[0] == string.Empty || args[0].Length < 2 || args[0].ToCharArray()[0].ToString() != Info.Prefix || args.Length < 2)
            {
                return;
            }
            args[0] = args[0].Substring(1);
            args[0] = args[0].Substring(0, 1).ToUpper() + args[0].Substring(1);
            args[1] = args[1].Substring(0, 1).ToUpper() + args[1].Substring(1);
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
                            SetPost(socketRole.Id.ToString(), args[2], args[3], socketRole.Position, ref result);
                            await arg.Channel.SendMessageAsync(result);
                            break;
                        case Command.Remove:
                            if (Main.Manage.DiscordSocketClient.GetGuild(Main.Info.MainServerId).Roles.Where(x => x.Mention == args[2]).FirstOrDefault() == default)
                            {
                                await arg.Channel.SendMessageAsync("Должность \"" + args[2] + "\" не найдена!");
                                return;
                            }
                            RemovePost(Main.Manage.DiscordSocketClient.GetGuild(Main.Info.MainServerId).Roles.Where(x => x.Mention == args[2]).FirstOrDefault().Id.ToString(), Main.Manage.DiscordSocketClient.GetGuild(Main.Info.MainServerId).Roles.Where(x => x.Mention == args[2]).FirstOrDefault().Mention, ref result);
                            await arg.Channel.SendMessageAsync(result);
                            break;
                        case Command.List:
                            result = "Список должностей:";
                            List<Post> posts = new List<Post>();
                            using (FoundationContext db = new FoundationContext(Info.ConnectionString))
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
                            SetEmployee(socketGuildUser.Id.ToString(), socketGuildUser.Mention, steamId.ToString(), post.DiscordId, ref result);
                            await arg.Channel.SendMessageAsync(result);
                            break;
                        case Command.Remove:
                            if (Main.Manage.DiscordSocketClient.GetGuild(Main.Info.MainServerId).Users.Where(x => x.Mention == args[2]).FirstOrDefault() == default)
                            {
                                await arg.Channel.SendMessageAsync("Сотрудник \"" + args[2] + "\" не найден!");
                                return;
                            }
                            RemoveEmployee(Main.Manage.DiscordSocketClient.GetGuild(Main.Info.MainServerId).Users.Where(x => x.Mention == args[2]).FirstOrDefault().Id.ToString(), Main.Manage.DiscordSocketClient.GetGuild(Main.Info.MainServerId).Users.Where(x => x.Mention == args[2]).FirstOrDefault().Mention, ref result);
                            await arg.Channel.SendMessageAsync(result);
                            break;
                        case Command.List:
                            result = "Список сотрудников:";
                            using (FoundationContext db = new FoundationContext(Info.ConnectionString))
                            {
                                if (db.Employees.ToList().Count() == 0)
                                    result = "Должности не определены.";
                                foreach (Employee employee in db.Employees.ToList())
                                {
                                    result = result + "\n[" + employee.Post.GameRole + "] - " + employee.Name + " - " + employee.Post.Name;
                                }
                            }
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