

// ClassLibrary1, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// TestPlugin.ConfigFile
using System;
using System.IO;
using Newtonsoft.Json;
using static 跨服聊天;

public class ConfigFile
{
	public string[] Rest地址 = new string[3] { "127.0.0.1:7878", "127.0.0.1:6767", "IP:Port" };

	public string Token令牌 = "abcdef";

	public string 聊天格式 = "[1号服务器] {0}{1}{2}: {3}";

	public string 进入格式 = "{0} 进入1号服务器。";

	public string 离开格式 = "{0} 离开1号服务器。";

	public bool 发送聊天 = true;

	public bool 发送进入离开 = true;

	public bool 接收消息 = true;

	public static Action<ConfigFile> ConfigR;

	public static ConfigFile Read(string Path)
	{
		if (!File.Exists(Path))
		{
			return new ConfigFile();
		}
		using FileStream stream = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.Read);
		return Read(stream);
	}

	public static ConfigFile Read(Stream stream)
	{
		using StreamReader streamReader = new StreamReader(stream);
		ConfigFile configFile = JsonConvert.DeserializeObject<ConfigFile>(streamReader.ReadToEnd());
		if (ConfigR != null)
		{
			ConfigR(configFile);
		}
		return configFile;
	}

	public void Write(string Path)
	{
		using FileStream stream = new FileStream(Path, FileMode.Create, FileAccess.Write, FileShare.Write);
		Write(stream);
	}

	public void Write(Stream stream)
	{
		string value = JsonConvert.SerializeObject((object)this, (Formatting)1);
		using StreamWriter streamWriter = new StreamWriter(stream);
		streamWriter.Write(value);
	}
}
