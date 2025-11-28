using System.Text.Json;
using GlobalConquest.Units;
using LiteNetLib;
namespace GlobalConquest.Actions;

public class DogfightAction : PlayerAction
{
    public Unit Plane {get; set;}
    public int StrikeX { get; set; }
    public int StrikeY { get; set; }
    private Random rand = new System.Random();

    public new void deserializeAndExecute(NetPeer peer, Object serverObj)
    {
        if (MessageAsJson != null)
        {
            DogfightAction? action =
                    JsonSerializer.Deserialize<DogfightAction>(this.MessageAsJson);
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
        if (StrikeX >= 0 && StrikeX < map.X && StrikeY >= 0 && StrikeY < map.Y)
        {
            MapHex planeHex = map.Hexes[Plane.Y, Plane.X];
            MapHex mapHex = map.Hexes[StrikeY, StrikeX];
            PlaneUnitType planeType = new PlaneUnitType();
            Unit existingPlane = planeType.getExistingPlane(map, Plane);
            if (existingPlane == null  || existingPlane.StrengthPoints <= 0 || existingPlane.TurnsUnavailable > 0)
            {
                Globals.Log("execute(): plane is unavailable");
                return;
            }
            MapHex targetMapHex = map.Hexes[StrikeY, StrikeX];
            Unit targetUnit = targetMapHex.getUnit();
            Unit enemyPlane = null;
            if (targetUnit != null)
            {   
                if ("plane".Equals(targetUnit.UnitType))
                    enemyPlane = targetUnit;
                else if (targetUnit.Airplane != null)
                    enemyPlane = targetUnit.Airplane;
            }
            else if (targetMapHex.Airplane != null)
                enemyPlane = targetMapHex.Airplane;
            if (enemyPlane == null)
            {
                enemyPlane = planeType.getEnemyPlaneForDogfight(gameState, targetMapHex, Plane.Color);
            }
            if (enemyPlane == null)
            {
                Globals.Log("execute(): no enemy planes were found for a dogfight.");
            }
            AirplaneMissionOutcome outcome = new AirplaneMissionOutcome();
            if (planeType.isShortRangeMission(gameState, planeHex, targetMapHex))
                outcome.IsShortRangeMission = true;
            if (planeType.isMediumRangeMission(gameState, planeHex, targetMapHex))
                outcome.IsMediumRangeMission = true;
            if (!outcome.IsShortRangeMission && !outcome.IsMediumRangeMission)
            {
                Globals.Log("execute(): target hex is not in range.");
                return;
            }

            // dogfight logic
            int chance = rand.Next(0, 100);
            outcome.EnemyPlane = enemyPlane;
            enemyPlane.TurnsUnavailable += 0.5f;
            // Short Range Dogfight Missions:
            // If your opponent's plane is available, 
            // there is a 30% chance that your plane will be downed, 
            // but a 37% chance that the opposition's plane will be destroyed. 
            // If your foe's plane is unavailable, your chances of destroying the targeted plane jump to 40%, 
            // and the possibility that your plane will be eliminated drops to 17%.
            if (outcome.IsShortRangeMission)
            {
                if (enemyPlane.TurnsUnavailable <= 0)
                {
                    if (chance < 30)
                    {
                        outcome.IsPlaneShotDown = true;
                    }
                    else if (chance < 67)
                    {
                        outcome.IsEnemyPlaneShotDown = true;
                    }
                }
                else
                {
                    if (chance < 17)
                    {
                        outcome.IsPlaneShotDown = true;
                    }
                    else if (chance < 57)
                    {
                        outcome.IsEnemyPlaneShotDown = true;
                    }
                }
            }
            // Medium Range Dogfight Missions:
            // When your opponent's plane is available, the chances for either one of the planes to be destroyed is 25%. 
            // When the opposition's plane is unavailable, your chances for losing your plane drop to 10%, 
            // while the probability that your adversary will lose his plane remains at 25%.
            else if (outcome.IsMediumRangeMission)
            {
                if (enemyPlane.TurnsUnavailable <= 0)
                {
                    if (chance < 25)
                    {
                        outcome.IsPlaneShotDown = true;
                    }
                    else if (chance < 50)
                    {
                        outcome.IsEnemyPlaneShotDown = true;
                    }
                }
                else
                {
                    if (chance < 10)
                    {
                        outcome.IsPlaneShotDown = true;
                    }
                    else if (chance < 35)
                    {
                        outcome.IsEnemyPlaneShotDown = true;
                    }
                }
                
            }
            if (outcome.IsPlaneShotDown)
            {
                planeType.handlePlaneShotDown(gameState, Plane);
                GameEvent gameEvent = new GameEvent("unitDestroyed");
                gameEvent.MapHex = map.Hexes[existingPlane.Y, existingPlane.X];
                gameEvent.Unit = Plane;
                gameEvent.EnemyColor = outcome.EnemyPlane.Color;
                server.sendGameStateAndMapHex(existingPlane.X, existingPlane.Y);
                server.sendGameStateAndMapHex(outcome.EnemyPlane.X, outcome.EnemyPlane.Y);
                server.sendGamePlayEvent(Plane.Color, gameEvent);
            }
            if (outcome.IsEnemyPlaneShotDown)
            {
                planeType.handlePlaneShotDown(gameState, enemyPlane);
                GameEvent gameEvent = new GameEvent("enemyUnitDestroyed");
                gameEvent.MapHex = map.Hexes[outcome.EnemyPlane.Y, outcome.EnemyPlane.X];
                gameEvent.Unit = outcome.EnemyPlane;
                gameEvent.EnemyColor = outcome.EnemyPlane.Color;
                server.sendGamePlayEvent(Plane.Color, gameEvent);
                gameEvent.EventType = "unitDestroyed";
                server.sendGameStateAndMapHex(existingPlane.X, existingPlane.Y);
                server.sendGameStateAndMapHex(outcome.EnemyPlane.X, outcome.EnemyPlane.Y);
                server.sendGamePlayEvent(outcome.EnemyPlane.Color, gameEvent);

            }
            server.sendGameStateAndMapHex(outcome.EnemyPlane.X, outcome.EnemyPlane.Y);
            Globals.Log("execute(): dogfight action complete");
        }


    }
}
