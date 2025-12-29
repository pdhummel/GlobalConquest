using System.Text.Json;
using GlobalConquest.Units;
using LiteNetLib;
using static UnitTypeConstants;
namespace GlobalConquest.Actions;

public class AirstrikeAction : PlayerAction
{
    public Unit Plane {get; set;}
    public int StrikeX { get; set; }
    public int StrikeY { get; set; }

    public new void deserializeAndExecute(NetPeer peer, Object serverObj)
    {
        if (MessageAsJson != null)
        {
            AirstrikeAction? action =
                    JsonSerializer.Deserialize<AirstrikeAction>(this.MessageAsJson);
            action?.execute(peer, serverObj);
        }
    }

    public void execute(Server server)
    {
        Globals.Log("execute()");
        if (Plane == null)
        {
            return;
        }
        GameState gameState = server.gameState;
        Map map = gameState.Map;
        if (StrikeX >= 0 && StrikeX < map.X && StrikeY >= 0 && StrikeY < map.Y)
        {
            MapHex strikeMapHex = map.Hexes[StrikeY, StrikeX];
            PlaneUnitType planeType = new PlaneUnitType();
            Unit existingPlane = planeType.getExistingPlane(map, Plane);
            if (existingPlane == null  || existingPlane.StrengthPoints <= 0 || existingPlane.TurnsUnavailable > 0)
            {
                Globals.Log("execute(): plane is unavailable");
                return;
            }

            AirplaneMissionOutcome outcome = planeType.determineMissionOutcome(gameState, existingPlane, strikeMapHex);
            if (!outcome.IsShortRangeMission && !outcome.IsMediumRangeMission)
            {
                Globals.Log("execute(): target hex is not in range.");
                return;
            }
            Globals.Log("execute(): turnsUnavailable=" + existingPlane.TurnsUnavailable);
            //Globals.Log("execute(): outcome.turnsUnavailable=" + outcome.Plane.turnsUnavailable);
            if (outcome.IsMissionSuccessful)
            {
                MapHex targetMapHex = map.Hexes[StrikeY, StrikeX];
                Unit targetUnit = targetMapHex.getUnit();
                int factor = 1;
                if (outcome.IsShortRangeMission)
                {
                    Globals.Log("execute(): short range mission");
                    factor = 1;
                }
                if (outcome.IsMediumRangeMission)
                {
                    Globals.Log("execute(): medium range mission");
                    factor = 2;
                }
                if (targetUnit != null)
                {
                    // Short Range Air Strikes against enemy armor units and 
                    // non-dug-in infantry units, removes half of the unit's remaining strength. 
                    // For Comcens on land and infantry that is dug-in, 
                    // the air strikes remove one-third of their remaining strength. 
                    // Against battleships, carriers, and Comcens at sea, 
                    // planes reduce the defender by a fixed 25% of original strength. 
                    // Against subs, the strength-reduction rate is 34% of original strength, 
                    // and against transports, the damage is a whopping 50%.
                    // Medium Range Air Strikes cause damage at HALF of the short-range rate.
                    outcome.EnemyPlane = targetUnit;
                    string type = targetUnit.UnitType;
                    int damage = 0;
                    if (ARMOR.Equals(type) || ARMOR.Equals(type) || INFANTRY.Equals(type))
                    {
                        damage = ((targetUnit.StrengthPoints / 2) / factor);
                    }
                    else if (DUG_IN_INFANTRY.Equals(type) || (COMMAND_CENTER.Equals(type) && !"sea".Equals(targetMapHex.Terrain)) )
                    {
                        damage = ((targetUnit.StrengthPoints / 3) / factor);
                    }
                    else if (BATTLESHIP.Equals(type) || "carrier".Equals(type) || (COMMAND_CENTER.Equals(type) && "sea".Equals(targetMapHex.Terrain)))
                    {
                        damage = (25 / factor);
                    }
                    else if ("sub".Equals(type) || "submarine".Equals(type))
                    {
                        damage = (34 / factor);
                    }
                    else if (TRANSPORT_INFANTRY.Equals(type) || TRANSPORT_ARMOR.Equals(type))
                    {
                        damage = (50 / factor);
                    }
                    targetUnit.StrengthPoints -= damage;
                    targetMapHex.setUnit(targetUnit);
                    if (targetUnit.StrengthPoints < 0)
                    {
                        targetUnit.StrengthPoints = 0;
                        targetMapHex.setUnit(null);
                        GameEvent gameEvent = new GameEvent("enemyUnitDestroyed");
                        gameEvent.MapHex = targetMapHex;
                        gameEvent.Unit = targetUnit;
                        gameEvent.EnemyColor = targetUnit.Color;
                        server.sendGamePlayEvent(Plane.Color, gameEvent);
                        gameEvent.EventType = "unitDestroyed";
                        server.sendGamePlayEvent(targetUnit.Color, gameEvent);
                        Globals.Log("execute(): airstrike destroyed enemy");

                        if (COMMAND_CENTER.Equals(targetUnit.UnitType))
                        {
                            Faction faction = server.gameState.Factions.ColorToFaction[targetUnit.Color];
                            faction.HasComCen = false;
                            if (!server.gameState.GameSettings.CanLoseComCen)
                            {
                                gameEvent = new GameEvent("enemyPlayerLostGame");
                                gameEvent.EnemyColor = targetUnit.Color;
                                server.sendGamePlayEvent(Plane.Color, gameEvent);
                                gameEvent.EventType = "playerLostGame";
                                server.sendGamePlayEvent(targetUnit.Color, gameEvent);
                            }
                        }
                    }
                    else
                    {
                        GameEvent gameEvent = new GameEvent("airplaneStrikeSuceeded");
                        gameEvent.MapHex = targetMapHex;
                        gameEvent.Unit = existingPlane;
                        gameEvent.EnemyColor = targetUnit.Color;
                        server.sendGamePlayEvent(Plane.Color, gameEvent);
                        gameEvent = new GameEvent("unitAttacked");
                        gameEvent.MapHex = targetMapHex;
                        gameEvent.Unit = targetUnit;
                        gameEvent.EnemyColor = existingPlane.Color;
                        server.sendGamePlayEvent(targetUnit.Color, gameEvent);
                    }
                    server.sendGameStateAndMapHex(existingPlane.X, existingPlane.Y);
                    server.sendGameStateAndMapHex(targetMapHex.X, targetMapHex.Y);
                    Globals.Log("execute(): airstrike attack complete, damage=" + damage);
                    Globals.Log("execute(): airstrike attack complete, existing=" + existingPlane.X + "," + existingPlane.Y);
                    Globals.Log("execute(): airstrike attack complete, target=" + targetMapHex.X + "," + targetMapHex.Y);
                }
                Globals.Log("execute(): airstrike complete");
            }
            else if (outcome.IsEnemyPlaneShotDown)
            {
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
            else if (outcome.IsPlaneShotDown)
            {
                GameEvent gameEvent = new GameEvent("unitDestroyed");
                gameEvent.MapHex = map.Hexes[existingPlane.Y, existingPlane.X];
                gameEvent.Unit = Plane;
                gameEvent.EnemyColor = outcome.EnemyPlane.Color;
                planeType.handlePlaneShotDown(gameState, existingPlane);
                server.sendGameStateAndMapHex(existingPlane.X, existingPlane.Y);
                //server.sendGameStateAndMapHex(StrikeX, StrikeY);
                server.sendGameStateAndMapHex(outcome.EnemyPlane.X, outcome.EnemyPlane.Y);
                server.sendGamePlayEvent(Plane.Color, gameEvent);
            }
            else
            {
                GameEvent gameEvent = new GameEvent("airplaneMissionFailed");
                gameEvent.MapHex = map.Hexes[existingPlane.Y, existingPlane.X];
                gameEvent.Unit = Plane;
                server.sendGameStateAndMapHex(existingPlane.X, existingPlane.Y);
                server.sendGamePlayEvent(Plane.Color, gameEvent);     
            }

            // Update enemy plane status change caused by defending
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
                gameEvent.MapHex = map.Hexes[StrikeY, StrikeX];
                gameEvent.EventType = "planeDefending";
                server.sendGamePlayEvent(outcome.EnemyPlane.Color, gameEvent);
            }

            Globals.Log("execute(): airstrike action complete");
        }
    }

    public new void execute(NetPeer peer, Object serverObj)
    {
        Server server = (Server)serverObj;
        execute(server);
    }
}
