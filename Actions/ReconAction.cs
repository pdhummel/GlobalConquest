using System.Text.Json;
using GlobalConquest.Units;
using LiteNetLib;
namespace GlobalConquest.Actions;

public class ReconAction : PlayerAction
{
    public Unit Plane {get; set;}
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
        if (Plane == null || Plane.StrengthPoints <= 0 || Plane.turnsUnavailable > 0)
        {
            Console.WriteLine("ReconAction.execute(): plane is unavailable");
            return;
        }
        Server server = (Server)serverObj;
        GameState gameState = server.gameState;
        Map map = gameState.Map;
        if (ReconX >= 0 && ReconX < map.X && ReconY >= 0 && ReconY < map.Y)
        {
            MapHex planeHex = map.Hexes[Plane.Y, Plane.X];
            MapHex mapHex = map.Hexes[ReconY, ReconX];
            PlaneUnitType planeType = new PlaneUnitType();
            Unit existingPlane = null;
            if (Plane.ParentUnitId != null)
            {
                if (map.UnitIdToUnit.ContainsKey(Plane.ParentUnitId))
                {
                    Unit parentUnit = map.UnitIdToUnit[Plane.ParentUnitId];
                    existingPlane.X = parentUnit.X;
                    existingPlane.Y = parentUnit.Y;
                    existingPlane = parentUnit.Airplane;
                    planeHex = map.Hexes[existingPlane.Y, existingPlane.X];
                }
            }
            if (Plane.ParentUnitId == null)
            {
                existingPlane = planeHex.Airplane; 
            }
            if (existingPlane != null)
            {
                AirplaneMissionOutcome outcome = planeType.determineMissionOutcome(gameState, existingPlane, mapHex);
                if (outcome.IsMissionSuccessful)
                {
                    // Recon missions uncover any terrain within a radius of 8 spaces from the chosen spot and 
                    // any units within 12 spaces.
                    // This logic essentially creates a dummy plane and dummy unitType for 
                    // the scan methods.
                    GameLogic gameLogic = new GameLogic();
                    planeType.ScanningRange = 12;
                    planeType.DiscoveryRange = 8;
                    Plane.X = ReconX;
                    Plane.Y = ReconY;
                    gameLogic.scanUnits(server, Plane, planeType);
                    gameLogic.scanTerrain(server, Plane, planeType);
                    Console.WriteLine("execute(): recon scans complete");
                }
                else if (outcome.IsEnemyPlaneShotDown)
                {
                    GameEvent gameEvent = new GameEvent("enemyUnitDestroyed");
                    gameEvent.MapHex = map.Hexes[outcome.EnemyPlane.Y, outcome.EnemyPlane.X];
                    gameEvent.Unit = outcome.EnemyPlane;
                    gameEvent.EnemyColor = outcome.EnemyPlane.Color;
                    server.sendGamePlayEvent(Plane.Color, gameEvent);
                    gameEvent.EventType = "unitDestroyed";
                    server.sendGamePlayEvent(outcome.EnemyPlane.Color, gameEvent);
                }
                else if (outcome.IsPlaneShotDown)
                {
                    GameEvent gameEvent = new GameEvent("unitDestroyed");
                    gameEvent.MapHex = map.Hexes[existingPlane.Y, existingPlane.X];
                    gameEvent.Unit = Plane;
                    gameEvent.EnemyColor = outcome.EnemyPlane.Color;
                    server.sendGamePlayEvent(Plane.Color, gameEvent);
                }
                else
                {
                    GameEvent gameEvent = new GameEvent("airplaneMissionFailed");
                    gameEvent.MapHex = map.Hexes[existingPlane.Y, existingPlane.X];
                    gameEvent.Unit = Plane;
                    server.sendGamePlayEvent(Plane.Color, gameEvent);                    
                }
                Console.WriteLine("execute(): recon action complete");
            }

        }
    }
}
