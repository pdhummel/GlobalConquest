using System.Text.Json;
using GlobalConquest.Units;
using LiteNetLib;
namespace GlobalConquest.Actions;

public class BombAction : PlayerAction
{
    public Unit Plane {get; set;}
    public int BombX { get; set; }
    public int BombY { get; set; }

    public new void deserializeAndExecute(NetPeer peer, Object serverObj)
    {
        if (MessageAsJson != null)
        {
            BombAction? action =
                    JsonSerializer.Deserialize<BombAction>(this.MessageAsJson);
            action?.execute(peer, serverObj);
        }
    }


    public new void execute(NetPeer peer, Object serverObj)
    {
        Globals.Log("execute()");
        if (Plane == null)
        {
            return;
        }
        Server server = (Server)serverObj;
        GameState gameState = server.gameState;
        Map map = gameState.Map;
        if (BombX >= 0 && BombX < map.X && BombY >= 0 && BombY < map.Y)
        {
            PlaneUnitType planeType = new PlaneUnitType();
            MapHex planeHex = planeType.getPlaneMapHex(map, Plane);
            MapHex bombHex = map.Hexes[BombY, BombX];
            Unit existingPlane = planeType.getExistingPlane(map, Plane);
            if (existingPlane == null  || existingPlane.StrengthPoints <= 0 || existingPlane.TurnsUnavailable > 0)
            {
                Globals.Log("execute(): plane is unavailable");
                if (existingPlane != null)
                {
                    Globals.Log("execute(): existingPlane: strength=" + existingPlane.StrengthPoints + ", turnsUnavailable=" + 
                                       existingPlane.TurnsUnavailable);
                }
                return;
            }
            MapHex targetMapHex = map.Hexes[BombY, BombX];
            if (targetMapHex.Burb == null)
            {
                Globals.Log("execute(): Hex is not a valid burb to bomb.");
                return;
            }

            AirplaneMissionOutcome outcome = planeType.determineMissionOutcome(gameState, existingPlane, bombHex);
            if (!outcome.IsShortRangeMission && !outcome.IsMediumRangeMission)
            {
                Globals.Log("execute(): target hex is not in range.");
                return;
            }
            if (outcome.IsMissionSuccessful)
            {

                GameEvent gameEvent = new GameEvent("airplaneBombingSuceeded");
                gameEvent.MapHex = targetMapHex;
                gameEvent.Unit = existingPlane;
                Faction faction = gameState.Factions.ColorToFaction[targetMapHex.Burb.Color];
                faction.Money -= 5;
                server.sendGamePlayEvent(Plane.Color, gameEvent);             
                server.sendGameStateAndMapHex(planeHex.X, planeHex.Y);
                server.sendGameStateAndMapHex(targetMapHex.X, targetMapHex.Y);
                Globals.Log("execute(): bombing successful");
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
                gameEvent.MapHex = map.Hexes[BombY, BombX];
                gameEvent.EventType = "planeDefending";
                server.sendGamePlayEvent(outcome.EnemyPlane.Color, gameEvent);
            }


            Globals.Log("execute(): bombing action complete");
        }

    }

}
