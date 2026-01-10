using System.Text.Json;
using GlobalConquest.Units;
using static GlobalConquest.GameEvent;
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
        Globals.Log("execute()");
        if (Plane == null)
        {
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
            Unit parentUnit = null;
            Unit existingPlane = null;
            if (Plane.ParentUnitId != null)
            {
                if (map.UnitIdToUnit.ContainsKey(Plane.ParentUnitId))
                {
                    parentUnit = map.UnitIdToUnit[Plane.ParentUnitId];
                    existingPlane = parentUnit.Airplane;
                    if (existingPlane != null)
                    {
                        existingPlane.X = parentUnit.X;
                        existingPlane.Y = parentUnit.Y;
                        existingPlane = parentUnit.Airplane;
                        planeHex = map.Hexes[existingPlane.Y, existingPlane.X];
                    }
                }
            }
            if (Plane.ParentUnitId == null)
            {
                existingPlane = planeHex.Airplane; 
            }
            if (existingPlane == null  || existingPlane.StrengthPoints <= 0 || existingPlane.TurnsUnavailable > 0)
            {
                Globals.Log("execute(): plane is unavailable");
                return;
            }

            AirplaneMissionOutcome outcome = planeType.determineMissionOutcome(gameState, existingPlane, mapHex);
            if (!outcome.IsShortRangeMission && !outcome.IsMediumRangeMission)
            {
                Globals.Log("execute(): target hex is not in range.");
                return;
            }
            if (parentUnit != null && parentUnit.Airplane != null)
            {
                parentUnit.Airplane = outcome.Plane;
            }
            else if (planeHex != null && planeHex.Airplane != null)
            {
                planeHex.Airplane = outcome.Plane;
            }
            Globals.Log("execute(): turnsUnavailable=" + existingPlane.TurnsUnavailable);
            //Globals.Log("execute(): outcome.turnsUnavailable=" + outcome.Plane.turnsUnavailable);
            if (outcome.IsMissionSuccessful)
            {
                // Recon missions uncover any terrain within a radius of 8 spaces from the chosen spot and 
                // any units within 12 spaces.
                // This logic essentially creates a dummy plane and dummy unitType for 
                // the scan methods.
                GameLogic gameLogic = new GameLogic();
                Unit fakePlane = Plane.clone();
                fakePlane.X = ReconX;
                fakePlane.Y = ReconY;
                planeType.ScanningRange = 12;
                planeType.DiscoveryRange = 8;
                gameLogic.scanUnits(server, fakePlane, planeType);
                gameLogic.scanTerrain(server, fakePlane, planeType);
                server.sendGameStateAndMapHex(existingPlane.X, existingPlane.Y);
                GameEvent gameEvent = new GameEvent(GAME_EVENT_AIRPLANE_MISSION_SUCEEDED);
                gameEvent.MapHex = mapHex;
                gameEvent.Unit = existingPlane;
                server.sendGamePlayEvent(Plane.Color, gameEvent);             
                Globals.Log("execute(): recon scans complete");
            }
            else if (outcome.IsEnemyPlaneShotDown)
            {
                GameEvent gameEvent = new GameEvent(GAME_EVENT_ENEMY_UNIT_DESTROYED);
                gameEvent.MapHex = map.Hexes[outcome.EnemyPlane.Y, outcome.EnemyPlane.X];
                gameEvent.Unit = outcome.EnemyPlane;
                gameEvent.EnemyColor = outcome.EnemyPlane.Color;
                server.sendGamePlayEvent(Plane.Color, gameEvent);
                gameEvent.EventType = GAME_EVENT_UNIT_DESTROYED;
                server.sendGameStateAndMapHex(existingPlane.X, existingPlane.Y);
                server.sendGameStateAndMapHex(outcome.EnemyPlane.X, outcome.EnemyPlane.Y);
                server.sendGamePlayEvent(outcome.EnemyPlane.Color, gameEvent);
            }
            else if (outcome.IsPlaneShotDown)
            {
                GameEvent gameEvent = new GameEvent(GAME_EVENT_UNIT_DESTROYED);
                gameEvent.MapHex = map.Hexes[existingPlane.Y, existingPlane.X];
                gameEvent.Unit = Plane;
                gameEvent.EnemyColor = outcome.EnemyPlane.Color;
                server.sendGameStateAndMapHex(existingPlane.X, existingPlane.Y);
                server.sendGameStateAndMapHex(outcome.EnemyPlane.X, outcome.EnemyPlane.Y);
                server.sendGamePlayEvent(Plane.Color, gameEvent);
            }
            else
            {
                GameEvent gameEvent = new GameEvent(GAME_EVENT_AIRPLANE_MISSION_FAILED);
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
                gameEvent.MapHex = map.Hexes[ReconY, ReconX];
                gameEvent.EventType = GAME_EVENT_PLANE_DEFENDING;
                server.sendGamePlayEvent(outcome.EnemyPlane.Color, gameEvent);
            }

            Globals.Log("execute(): recon action complete");
        }


    }
}
