using Microsoft.Xna.Framework;
namespace GlobalConquest;

public class PlayerAction
{
    public string ClientIdentifier { get; set; }

    public string Type { get; set; }

    public PlayerAction(string clientIdentifier, string type)
    {
        ClientIdentifier = clientIdentifier;
        Type = type;
    }



}