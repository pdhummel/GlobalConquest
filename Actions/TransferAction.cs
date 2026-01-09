using System.Text.Json;
using GlobalConquest.Units;
using LiteNetLib;
using static UnitTypeConstants;
using static GameConstants;
namespace GlobalConquest.Actions;

public class TransferAction : PlayerAction
{
    public Unit Plane {get; set;}
    public int DestinationX { get; set; }
    public int DestinationY { get; set; }

    public new void deserializeAndExecute(NetPeer peer, Object serverObj)
    {
        if (MessageAsJson != null)
        {
            TransferAction? action =
                    JsonSerializer.Deserialize<TransferAction>(this.MessageAsJson);
            action?.execute(peer, serverObj);
        }
    }


    // TODO:  When doing this type of mission, 
    // you may also click on an adjacent infantry to transport along with your plane, 
    // and both units will be moved to the chosen transfer burb.
    public new void execute(Server server)
    {
        Globals.Log("execute()");
        if (Plane == null)
        {
            return;
        }
        GameState gameState = server.gameState;
        Map map = gameState.Map;
        if (DestinationX >= 0 && DestinationX < map.X && DestinationY >= 0 && DestinationY < map.Y)
        {
            PlaneUnitType planeType = new PlaneUnitType();
            MapHex planeHex = planeType.getPlaneMapHex(map, Plane);
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
            // Destination should be an unoccupied friendly comcen or carrier OR
            // unoccupied airfield (burb center or land suburb) of a friendly burb.
            bool useTargetUnit = false;
            bool useTargetHex = false;
            MapHex targetMapHex = map.Hexes[DestinationY, DestinationX];
            Unit targetUnit = targetMapHex.getUnit();
            if (targetUnit != null && targetUnit.Color.Equals(Plane.Color) &&
                (COMMAND_CENTER.Equals(targetUnit.UnitType) || AIRCRAFT_CARRIER.Equals(targetUnit.UnitType)) &&
                targetUnit.Airplane == null
               )
            {
                useTargetUnit = true;
            }
            else if (targetMapHex.Burb != null && targetMapHex.Burb.OwnerColor.Equals(Plane.Color) &&
                     !BURB_DOCK.Equals(targetMapHex.Burb.Type) &&
                     targetMapHex.Airplane == null)
            {
                useTargetHex = true;
            }
            if (!useTargetUnit && !useTargetHex)
            {
                Globals.Log("execute(): destination is not valid for transfer.");
                if (targetUnit != null)
                    Globals.Log("execute(): targetUnit=" + targetUnit.Airplane);
                if (targetMapHex != null)
                    Globals.Log("execute(): targetHex=" + targetMapHex.Airplane);
                return;
            }

            AirplaneMissionOutcome outcome = planeType.determineMissionOutcome(gameState, existingPlane, destinationHex);
            if (!outcome.IsShortRangeMission && !outcome.IsMediumRangeMission && !outcome.IsLongRangeMission)
            {
                Globals.Log("execute(): target hex is not in range.");
                return;
            }
            if (outcome.IsMissionSuccessful)
            {
                // Leave the old location.
                Unit parentUnit = planeType.getParentUnit(map, existingPlane);
                if (parentUnit != null)
                {
                    parentUnit.Airplane = null;
                }
                else
                {
                    planeHex.Airplane = null;
                }
                existingPlane.ParentUnitId = null;

                // Move to the new location.
                if (useTargetUnit)
                {
                    targetUnit.Airplane = existingPlane;
                    existingPlane.ParentUnitId = targetUnit.Id;
                    Globals.Log("execute(): targetUnitId=" + targetUnit.Id);
                }
                else if (useTargetHex)
                {
                    existingPlane.Y = targetMapHex.Y;
                    existingPlane.X = targetMapHex.X;
                    targetMapHex.Airplane = existingPlane;
                }

                GameEvent gameEvent = new GameEvent("airplaneMissionSuceeded");
                gameEvent.MapHex = targetMapHex;
                gameEvent.Unit = existingPlane;
                server.sendGamePlayEvent(Plane.Color, gameEvent);             
                server.sendGameStateAndMapHex(planeHex.X, planeHex.Y);
                server.sendGameStateAndMapHex(targetMapHex.X, targetMapHex.Y);
                Globals.Log("execute(): transfer complete");
            }
            else
            {
                GameEvent gameEvent = new GameEvent("airplaneMissionFailed");
                gameEvent.MapHex = map.Hexes[existingPlane.Y, existingPlane.X];
                gameEvent.Unit = Plane;
                server.sendGameStateAndMapHex(existingPlane.X, existingPlane.Y);
                server.sendGamePlayEvent(Plane.Color, gameEvent);     
            }


            if (outcome.EnemyPlane != null && !outcome.IsEnemyPlaneShotDown)
            {
                if (outcome.EnemyPlane.ParentUnitId != null && map.UnitIdToUnit.ContainsKey(outcome.EnemyPlane.ParentUnitId))
                {
                    Unit enemyParentUnit = map.UnitIdToUnit[outcome.EnemyPlane.ParentUnitId];
                    server.sendGameStateAndMapHex(enemyParentUnit.X, enemyParentUnit.Y);
                }    
                server.sendGameStateAndMapHex(outcome.EnemyPlane.X, outcome.EnemyPlane.Y);
                GameEvent gameEvent = new GameEvent();
                gameEvent.Unit = outcome.EnemyPlane;
                gameEvent.MapHex = map.Hexes[DestinationY, DestinationX];
                gameEvent.EventType = "planeDefending";
                server.sendGamePlayEvent(outcome.EnemyPlane.Color, gameEvent);
            }

            Globals.Log("execute(): transfer action complete");
        }

    }

    public new void execute(NetPeer peer, Object serverObj)
    {
        Server server = (Server)serverObj;
        execute(server);
    }

}
