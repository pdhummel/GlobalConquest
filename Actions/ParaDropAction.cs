using System.Text.Json;
using GlobalConquest.Units;
using LiteNetLib;
namespace GlobalConquest.Actions;

public class ParaDropAction : PlayerAction
{
    public Unit Plane {get; set;}
    public Unit ParaTrooper {get; set;}
    public int DestinationX { get; set; }
    public int DestinationY { get; set; }

    public new void deserializeAndExecute(NetPeer peer, Object serverObj)
    {
        if (MessageAsJson != null)
        {
            ParaDropAction? action =
                    JsonSerializer.Deserialize<ParaDropAction>(this.MessageAsJson);
            action?.execute(peer, serverObj);
        }
    }


    // TODO:  When doing this type of mission, 
    // you may also click on an adjacent infantry to transport along with your plane, 
    // and both units will be moved to the chosen transfer burb.
    public new void execute(NetPeer peer, Object serverObj)
    {
        Globals.Log("execute()");
        if (Plane == null || ParaTrooper == null)
        {
            return;
        }
        if (!("infantry".Equals(ParaTrooper.UnitType) || "dug-in-infantry".Equals(ParaTrooper.UnitType)))
        {
            Globals.Log("execute(): can only transport infantry: " + ParaTrooper.UnitType);
            return;
        }
        Server server = (Server)serverObj;
        GameState gameState = server.gameState;
        Map map = gameState.Map;
        if (DestinationX >= 0 && DestinationX < map.X && DestinationY >= 0 && DestinationY < map.Y)
        {
            PlaneUnitType planeType = new PlaneUnitType();
            MapHex planeHex = planeType.getPlaneMapHex(map, Plane);
            MapHex paraTrooperHex = map.Hexes[ParaTrooper.Y, ParaTrooper.X];
            MapHex destinationHex = map.Hexes[DestinationY, DestinationX];
            Unit existingPlane = planeType.getExistingPlane(map, Plane);
            if (existingPlane == null  || existingPlane.StrengthPoints <= 0 || existingPlane.TurnsUnavailable > 0)
            {
                Globals.Log("execute(): plane is unavailable");
                if (existingPlane != null)
                {
                    Globals.Log("execute(): existingPlane: " + existingPlane.StrengthPoints + ", " + 
                                       existingPlane.TurnsUnavailable);
                }
                return;
            }
            Unit existingInfantry = paraTrooperHex.getUnit();
            if (existingInfantry == null  || existingInfantry.StrengthPoints <= 0)
            {
                Globals.Log("execute(): paratrooper infantry is unavailable");
                return;
            }

            // Destination should be an unoccupied land hex.
            MapHex targetMapHex = map.Hexes[DestinationY, DestinationX];
            if (targetMapHex.getUnit() != null || "sea".Equals(targetMapHex.Terrain))
            {
                Globals.Log("execute(): destination must be an unoccupied land hex.");
                return;
            }

            AirplaneMissionOutcome outcome = planeType.determineMissionOutcome(gameState, existingPlane, destinationHex);
            if (!outcome.IsShortRangeMission && !outcome.IsMediumRangeMission)
            {
                Globals.Log("execute(): target hex is not in range.");
                return;
            }
            if (outcome.IsMissionSuccessful)
            {
                // Move paratrooper unit to new location
                map.moveUnit(existingInfantry, DestinationX, DestinationY);
                // Decrease strength by 20%
                existingInfantry.StrengthPoints -= (existingInfantry.StrengthPoints / 5);
                if (existingInfantry.StrengthPoints < 0)
                    existingInfantry.StrengthPoints = 1;
                // no longer dug-in
                existingInfantry.UnitType = "infantry";

                GameEvent gameEvent = new GameEvent("airplaneMissionSuceeded");
                gameEvent.MapHex = targetMapHex;
                gameEvent.Unit = existingPlane;
                server.sendGameStateAndMapHex(existingPlane.X, existingPlane.Y);
                server.sendGameStateAndMapHex(existingInfantry.X, existingInfantry.Y);
                server.sendGameStateAndMapHex(paraTrooperHex.X, paraTrooperHex.Y);
                server.sendGameStateAndMapHex(DestinationX, DestinationY);
                server.sendGamePlayEvent(Plane.Color, gameEvent);             
                Globals.Log("execute(): paraDrop complete");
            }
            else
            {
                GameEvent gameEvent = new GameEvent("airplaneMissionFailed");
                gameEvent.MapHex = map.Hexes[existingPlane.Y, existingPlane.X];
                gameEvent.Unit = Plane;
                server.sendGameStateAndMapHex(existingPlane.X, existingPlane.Y);
                server.sendGameStateAndMapHex(existingInfantry.X, existingInfantry.Y);
                server.sendGameStateAndMapHex(paraTrooperHex.X, paraTrooperHex.Y);
                server.sendGameStateAndMapHex(DestinationX, DestinationY);
                server.sendGamePlayEvent(Plane.Color, gameEvent);     
            }
            Globals.Log("execute(): paraDrop action complete");
        }

    }

}
