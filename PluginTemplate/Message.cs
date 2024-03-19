using Newtonsoft.Json;
using static 跨服聊天;

public class Message
{
    public string Text { get; set; }

    public byte R { get; set; }

    public byte G { get; set; }

    public byte B { get; set; }

    public Message(string text, byte r, byte g, byte b)
    {
        Text = text;
        R = r;
        G = g;
        B = b;
    }

    public override string ToString()
    {
        return JsonConvert.SerializeObject((object)this, (Formatting)1);
    }

    public static Message FromJson(string js)
    {
        return JsonConvert.DeserializeObject<Message>(js);
    }
}
