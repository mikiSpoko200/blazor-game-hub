using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameHubShared.Models;

public class UserModel
{
    public string UserName { get; set; }
    public string Authority { get; set; }

    public UserModel(string userName, string authority)
    {
        UserName = userName;
        Authority = authority;
    }
}
