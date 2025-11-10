using System.Text.Json;
using LiteNetLib;
using GlobalConquest.Units;
namespace GlobalConquest.Actions;

public class ChangeUnitContextAction : PlayerAction
{
    Server? server;
    public Unit? Unit { get; set; }
    public bool IsBlitzing { get; set; }
    public bool IsSneaking { get; set; }
    public int RoundsToWait { get; set; }


    public new void deserializeAndExecute(NetPeer peer, Object serverObj)
    {
        //Console.WriteLine("ChangeUnitContextAction.deserializeAndExecute()");
        if (MessageAsJson != null)
        {
            ChangeUnitContextAction? action =
                    JsonSerializer.Deserialize<ChangeUnitContextAction>(this.MessageAsJson);
            action?.execute(peer, serverObj);
        }
    }

    public new void execute(NetPeer peer, Object serverObj)
    {
        Console.WriteLine("ChangeUnitContextAction.execute()");
        Server server = (Server)serverObj;
        GameState gameState = server.gameState;
        if (Unit != null)
        {
            MapHex mapHex = gameState.Map.Hexes[Unit.Y, Unit.X];
            Unit existingUnit = mapHex.getUnit();
            existingUnit.IsBlitzing = IsBlitzing;
            existingUnit.IsSneaking = IsSneaking;
            existingUnit.RoundsToWait = RoundsToWait;
            server.sendGameStateAndMapHex(Unit.X, Unit.Y);
        }
    }
}
