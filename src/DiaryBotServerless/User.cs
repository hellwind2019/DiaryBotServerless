using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace DiaryBotServerless;

[JsonObject]
public class User
{
    public long Id { get; set; }
    public long ChannelId { get; set; }

    public bool IsPostedToday { get; set; }

    public bool IsPostingNow { get; set; }
    
    public int PostCount { get; set;}
    
    public string CurrentPostText { get; set; }
    
    public bool IsRegistered { get; set; }
    public User()
    {
        Id = -1;
        ChannelId = -1;
        IsPostedToday = IsPostingNow = IsRegistered = false;
        PostCount = 0;
        CurrentPostText = "";
    }

    public User(long id) : this()
    {
        Id = id;
    }

    // public UserFields Fields { get; set; }
    //
    // public User()
    // {
    //     Id = new Random().Next(100, 1000).ToString();
    //     Fields = new UserFields();
    // }
   
}

