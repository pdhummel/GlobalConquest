using System.Text.Json;
using GlobalConquest.Units;
using LiteNetLib;
namespace GlobalConquest.Actions;

public class ReconAction : PlayerAction
{
    public int ReconX { get; set; }
    public int ReconY { get; set; }

    public new void deserializeAndExecute(NetPeer peer, Object serverObj)
    {
        if (MessageAsJson != null)
        {
            ReconAction? action =
                    JsonSerializer.Deserialize<ReconAction>(this.MessageAsJson);
            action?.execute(peer, serverObj);
        }
    }

    public new void execute(NetPeer peer, Object serverObj)
    {
        Console.WriteLine("ReconAction.execute()");
        Server server = (Server)serverObj;
        GameState gameState = server.gameState;
        Map map = gameState.Map;
        bool isShortRange = false;
        bool isMediumRange = false;
        if (ReconX >= 0 && ReconX < map.X && ReconY >= 0 && ReconY < map.Y)
        {
            MapHex mapHex = map.Hexes[ReconY, ReconX];
            PlaneUnitType planeType = new PlaneUnitType();

            if (!isShortRange)
            {
                HashSet<MapHex> shortRangeHexes = map.getMapHexesInRange(mapHex, planeType.shortRangeHexes);
                if (shortRangeHexes.Contains(mapHex))
                    isShortRange = true;
            }
            if (!isShortRange)
            {
                HashSet<MapHex> mediumRangeHexes = map.getMapHexesInRange(mapHex, planeType.mediumRangeHexes);
                if (mediumRangeHexes.Contains(mapHex))
                    isMediumRange = true;
            }


            if (isShortRange)
            {
                
            }
            if (isMediumRange)
            {
                
            }

        }
    }
}
