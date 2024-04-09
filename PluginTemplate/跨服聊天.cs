using Microsoft.Xna.Framework;
using Rests;
using System.Net;
using System.Text;
using System.Web;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Configuration;
using TShockAPI.Hooks;

[ApiVersion(2, 1)]
public class 跨服聊天 : TerrariaPlugin
{
    public override string Author => "GK 改良：羽学";

    public override string Description => "实现跨服聊天的插件";

    public override string Name => "跨服聊天";

    public override Version Version => new Version(1, 0, 0, 8);

    public static ConfigFile LConfig { get; set; }

    internal static string LConfigPath => Path.Combine(TShock.SavePath, "跨服聊天.json");

    public 跨服聊天(Main game)
        : base(game)
    {
        LConfig = new ConfigFile();
    }

    private static void RC()
    {
        try
        {
            if (File.Exists(LConfigPath))
            {
                LConfig = ConfigFile.Read(LConfigPath);
                LConfig.Write(LConfigPath); // 如果读取成功，则写入  
            }
            else
            {
                TShock.Log.ConsoleError("未找到跨服聊天配置文件，已为您创建！修改配置后重启服务器可以应用新的配置。");
                // 这里您可能需要实际创建一个新的配置文件  
                LConfig = ConfigFile.Read(LConfigPath); ; // 创建一个默认配置  
                LConfig.Write(LConfigPath); // 写入默认配置  
            }
        }
        catch (Exception ex)
        {
            TShock.Log.ConsoleError("跨服聊天配置读取错误:" + ex.ToString());
            // 考虑在这里重新抛出异常或采取其他措施  
        }
    }

    // 重新加载配置的方法    
    private static void ReloadConfig(ReloadEventArgs args)
    {
        // 重新加载配置    
        RC();
        // 向触发重新加载的玩家发送成功消息    
        args.Player?.SendSuccessMessage("[{0}] 重新加载配置完毕。", typeof(跨服聊天).Name);
    }

    public override void Initialize()
    {
        // 注册配置重新加载事件
        RC();
        GeneralHooks.ReloadEvent += ReloadConfig;
        ServerApi.Hooks.ServerChat.Register(this, OnChat);
        ServerApi.Hooks.NetGreetPlayer.Register(this, OnGreetPlayer);
        ServerApi.Hooks.ServerLeave.Register(this, OnLeave);
        TShock.RestApi.Register(new SecureRestCommand("/msc", RestChat, "msc.canchat"));
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            ServerApi.Hooks.ServerChat.Deregister(this, OnChat);
            ServerApi.Hooks.NetGreetPlayer.Deregister(this, OnGreetPlayer);
            ServerApi.Hooks.ServerLeave.Deregister(this, OnLeave);
        }
        base.Dispose(disposing);
    }

    private object RestChat(RestRequestArgs args)
    {
        if (!LConfig.接收消息)
        {
            return new RestObject();
        }
        try
        {
            string s = args.Parameters["message"];
            byte[] bytes = Convert.FromBase64String(s);
            string @string = Encoding.UTF8.GetString(bytes);
            Message message = Message.FromJson(@string);
            TShock.Utils.Broadcast(message.Text, message.R, message.G, message.B);
        }
        catch (Exception ex)
        {
            TShock.Log.ConsoleError("跨服聊天收消息错误:" + ex.ToString());
        }
        return new RestObject();
    }

    private void OnGreetPlayer(GreetPlayerEventArgs e)
    {
        //把声明玩家为空放最前面避免空引用问题
        var ply = TShock.Players[e.Who];

        if (ply == null)
        {
            return;
        }

        if (!LConfig.发送进入离开  || !TShock.Players[e.Who].ConnectionAlive || TShock.Players[e.Who].Name == null || TShock.Players[e.Who].Name == "")
        {
            return;
        }
        string text = string.Format(LConfig.进入格式, TShock.Players[e.Who].Name);
        Color yellow = Color.Yellow;
        byte r = yellow.R;
        byte g = yellow.G;
        byte b = yellow.B;
        Message message = new Message(text, r, g, b);
        ThreadPool.QueueUserWorkItem(delegate
        {
            byte[] bytes = Encoding.UTF8.GetBytes(message.ToString());
            string str = Convert.ToBase64String(bytes);
            string arg = HttpUtility.UrlEncode(str);
            string[] rest地址 = LConfig.Rest地址;
            foreach (string text2 in rest地址)
            {
                if (!(text2 == ""))
                {
                    string requestUriString = $"http://{text2}/msc?message={arg}&token={LConfig.Token令牌}";
                    try
                    {
                        HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(requestUriString);
                        using (httpWebRequest.GetResponse())
                        {
                        }
                    }
                    catch (Exception ex)
                    {
                        TShock.Log.ConsoleError("跨服聊天发消息错误:" + ex.ToString());
                    }
                }
            }
        });
    }

    private void OnLeave(LeaveEventArgs e)
    {
        //把声明玩家为空放最前面避免空引用问题
        var ply = TShock.Players[e.Who];

        if (ply == null)
        {
            return;
        }

        if (TShock.ShuttingDown || !LConfig.发送进入离开 || TShock.Players[e.Who]?.Name == null || TShock.Players[e.Who].Name == "")

        {
            return;
        }

        if (!LConfig.监听玩家断连时的离开消息 && (!ply.ConnectionAlive))
        {
            return;
        }

        string text = string.Format(LConfig.离开格式, ply.Name);
        Color yellow = Color.Yellow;
        byte r = yellow.R;
        byte g = yellow.G;
        Message message = new Message(text, r, g, yellow.B);

        ThreadPool.QueueUserWorkItem(delegate
        {
            if (ply != null)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(message.ToString());
                string str = Convert.ToBase64String(bytes);
                string arg = HttpUtility.UrlEncode(str);
                string[] rest地址 = LConfig.Rest地址;

                foreach (string text2 in rest地址)
                {
                    if (!string.IsNullOrEmpty(text2))
                    {
                        string requestUriString = $"http://{text2}/msc?message={arg}&token={LConfig.Token令牌}";
                        try
                        {
                            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(requestUriString);
                            using (httpWebRequest.GetResponse())
                            {
                            }
                        }
                        catch (Exception ex)
                        {
                            TShock.Log.ConsoleError($"跨服聊天发消息错误: {ex.ToString()}");
                        }
                    }
                }
            }
        });
    }

    private void OnChat(ServerChatEventArgs args)
    {
        int specifierLength = ((ConfigFile<TShockSettings>)(object)TShock.Config).Settings.CommandSpecifier.Length;

        // 检查命令标识符是否存在，并且不在忽略条件中
        if (specifierLength > 0 && args.Text.Length >= specifierLength &&
            args.Text.Substring(0, specifierLength) == ((ConfigFile<TShockSettings>)(object)TShock.Config).Settings.CommandSpecifier)
        {
            // 如果满足特定命令前缀，则不执行后续跨服聊天发送逻辑
            return;
        }

        // 检查是否允许发送聊天、是否已处理、文本是否为空、是否包含特殊字符等情况
        if (!LConfig.发送聊天 || args.Handled || args.Text == "" || args.Text.Contains("/") || args.Text.Contains("."))
        {
            return;
        }

        // 创建并格式化消息对象
        Message message = new Message(string.Format(LConfig.聊天格式, TShock.Players[args.Who].Group.Prefix, TShock.Players[args.Who].Name, TShock.Players[args.Who].Group.Suffix, args.Text), TShock.Players[args.Who].Group.R, TShock.Players[args.Who].Group.G, TShock.Players[args.Who].Group.B);

        // 使用线程池异步发送跨服聊天消息
        ThreadPool.QueueUserWorkItem(delegate
        {
            byte[] bytes = Encoding.UTF8.GetBytes(message.ToString());
            string encodedStr = Convert.ToBase64String(bytes);
            string urlEncodedMessage = HttpUtility.UrlEncode(encodedStr);

            foreach (string serverAddress in LConfig.Rest地址)
            {
                if (!string.IsNullOrEmpty(serverAddress))
                {
                    string requestUriString = $"http://{serverAddress}/msc?message={urlEncodedMessage}&token={LConfig.Token令牌}";

                    try
                    {
                        HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(requestUriString);
                        using (httpWebRequest.GetResponse())
                        {
                        }
                    }
                    catch (Exception ex)
                    {
                        TShock.Log.ConsoleError("跨服聊天发消息错误:" + ex.ToString());
                    }
                }
            }
        });
    }
}
