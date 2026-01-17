using static UnitConstants;
using System.Reflection;
using System.Runtime.CompilerServices;
using GlobalConquest.Units;
using Microsoft.Xna.Framework;
using System.Security.Cryptography.X509Certificates;

namespace GlobalConquest;

public class GameEvent
{
    public static readonly string GAME_EVENT_UNIT_ATTACKED = "unitAttacked";
    public static readonly string GAME_EVENT_ENEMY_UNIT_ATTACKED = "enemyUnitAttacked";
    public static readonly string GAME_EVENT_UNIT_DESTROYED = "unitDestroyed";
    public static readonly string GAME_EVENT_ENEMY_UNIT_DESTROYED = "enemyUnitDestroyed";
    public static readonly string GAME_EVENT_AIRPLANE_MISSION_SUCEEDED = "airplaneMissionSuceeded";
    public static readonly string GAME_EVENT_AIRPLANE_MISSION_FAILED = "airplaneMissionFailed";
    public static readonly string GAME_EVENT_AIRPLANE_STRIKE_SUCEEDED = "airplaneStrikeSuceeded";
    public static readonly string GAME_EVENT_AIRPLANE_BOMBING_SUCEEDED = "airplaneBombingSuceeded";
    public static readonly string GAME_EVENT_PLAYER_LOST_GAME = "playerLostGame";
    public static readonly string GAME_EVENT_ENEMY_PLAYER_LOST_GAME = "enemyPlayerLostGame";
    public static readonly string GAME_EVENT_PLAYER_WON_GAME = "playerWonGame";
    public static readonly string GAME_EVENT_ENEMY_PLAYER_WON_GAME = "enemyPlayerWonGame";
    public static readonly string GAME_EVENT_GAME_OVER = "gameOver";
    public static readonly string GAME_EVENT_BURB_CAPTURED = "burbCaptured";
    public static readonly string GAME_EVENT_BURB_LOST = "burbLost";
    public static readonly string GAME_EVENT_UNIT_MOVEMENT_BLOCKED = "unitMovementBlocked";
    public static readonly string GAME_EVENT_UNIT_SUFFERED_ATTRITION = "unitSufferedAttrition";
    public static readonly string GAME_EVENT_ENEMY_UNIT_DISCOVERED = "enemyUnitDiscovered";
    public static readonly string GAME_EVENT_BURB_DISCOVERED = "burbDiscovered";
    public static readonly string GAME_EVENT_GRACE_PERIOD_STARTED = "gracePeriodStarted";
    public static readonly string GAME_EVENT_SERVER_MESSAGE = "serverMessage";
    public static readonly string GAME_EVENT_PLANE_DEFENDING = "planeDefending";
    public static readonly string GAME_EVENT_PLANE_IN_DOG_FIGHT = "planeInDogfight";
    public static readonly string GAME_EVENT_PLANNING_PHASE_ENDED = "planningPhaseEnded";
    public static readonly string GAME_EVENT_PLANNING_PHASE_STARTING = "planningPhaseStarting";
    public static readonly string GAME_EVENT_JOINED_GAME= "joinedGame";
    public static readonly string GAME_EVENT_BURB_SABOTAGED = "burbSabotaged";

    // Used to send separate message to clients for Events.
    // TODO: Also keep track of these events in a server log.
    public string EventType { get; set; }
    public long Ticks { get; set; }
    public int Turn { get; set; }
    public int Round { get; set; }
    //public string? Message {get; set;}

    public List<MapHex>? MapHexBuffer { get; set; } = new List<MapHex>();
    public bool IsLastMapHexBufferUpdate { get; set; } = false;
    public GameState? GameState { get; set; }

    public GlobalConquestGame? Game { get; set; }

    public MapHex? MapHex { get; set; }
    public Unit? Unit { get; set; }
    public string? EnemyColor { get; set; }
    public string? EventString { get; set; }
    public string? TargetScreenId { get; set; }
    private int secondsForPopupToAppear = 10;



    public HashSet<string> GamePlayEvents { get; set; } = new HashSet<string>();

    public GameEvent()
    {
        initializeGamePlayEvents();
    }

    public GameEvent(string eventType)
    {
        EventType = eventType;
        initializeGamePlayEvents();
    }

    private void initializeGamePlayEvents()
    {
        var gamePlayEvents = new string[]
        {
            GAME_EVENT_UNIT_ATTACKED,         // UnitType at MapHex attacked
            GAME_EVENT_ENEMY_UNIT_ATTACKED,    // EnemyColor UnitType attacked at MapHex 
            GAME_EVENT_UNIT_DESTROYED,        // UnitType at MapHex destroyed           
            GAME_EVENT_ENEMY_UNIT_DESTROYED,   // EnemyColor UnitType destroyed at MapHex
            GAME_EVENT_AIRPLANE_MISSION_SUCEEDED,
            GAME_EVENT_AIRPLANE_STRIKE_SUCEEDED,
            GAME_EVENT_AIRPLANE_BOMBING_SUCEEDED,
            GAME_EVENT_AIRPLANE_MISSION_FAILED,
            GAME_EVENT_PLAYER_LOST_GAME,       // Game Lost           
            GAME_EVENT_ENEMY_PLAYER_LOST_GAME,  // EnemyColor Lost Game
            GAME_EVENT_PLAYER_WON_GAME,        // Game Won           
            GAME_EVENT_ENEMY_PLAYER_WON_GAME,   // EnemyColor Won Game
            GAME_EVENT_GAME_OVER,             // Game Over                           
            GAME_EVENT_BURB_CAPTURED,         // BurbType BurbName captured at MapHex
            GAME_EVENT_BURB_LOST,             // BurbType BurbName lost at MapHex
            GAME_EVENT_UNIT_MOVEMENT_BLOCKED,  // UnitType at MapHex movement blocked
            GAME_EVENT_UNIT_SUFFERED_ATTRITION,// UnitType at Maphex suffered attrition
            GAME_EVENT_ENEMY_UNIT_DISCOVERED,  // EnemyColor UnitType discovered at MapHex
            GAME_EVENT_BURB_DISCOVERED,       // EnemyColor BurbType BurbName discovered at MapHex
            GAME_EVENT_GRACE_PERIOD_STARTED,
            GAME_EVENT_SERVER_MESSAGE,
            GAME_EVENT_PLANE_DEFENDING,
            GAME_EVENT_PLANE_IN_DOG_FIGHT,
            GAME_EVENT_PLANNING_PHASE_ENDED,
            GAME_EVENT_PLANNING_PHASE_STARTING,
            GAME_EVENT_JOINED_GAME,
            GAME_EVENT_BURB_SABOTAGED
        };
        GamePlayEvents.UnionWith(gamePlayEvents);
    }

    public bool IsGamePlayEvent()
    {
        bool isGamePlayEvent = false;
        if (GamePlayEvents.Contains(EventType))
            isGamePlayEvent = true;
        return isGamePlayEvent;
    }

    public void handleGamePlayEvent(GlobalConquestGame game)
    {
        Game = game;
        MethodInfo eventMethodHandler = this.GetType().GetMethod(EventType + "Handler");
        object[] parameters = new object[] { };
        try
        {
            Globals.Log("handleGamePlayEvent(): " + EventString);
            Thread eventMethodHandlerThread = new Thread(() => eventMethodHandler?.Invoke(this, parameters));
            eventMethodHandlerThread.IsBackground = true;
            eventMethodHandlerThread.Start();
            //eventMethodHandler?.Invoke(this, parameters);
        }
        catch (Exception ex)
        {
            Globals.Log("handleGamePlayEvent(): " + EventString + ", Exception: " + ex);
        }

    }

    public string GetUnitType()
    {
        string unitType = "[unitType]";
        if (Unit != null)
        {
            unitType = Unit.UnitType;
        }
        else if (MapHex != null && MapHex.getUnit() != null)
            unitType = MapHex.getUnit().UnitType;
        return unitType;
    }

    string GetBurbEnemyColor()
    {
        string enemyColor = "[enemyColor]";
        if (MapHex != null && MapHex.Burb != null)
            enemyColor = MapHex.Burb.OwnerColor;
        return enemyColor;
    }

    string GetUnitEnemyColor()
    {
        string enemyColor = "[enemyColor]";
        if (MapHex != null && MapHex.getUnit() != null)
            enemyColor = MapHex.getUnit().Color;
        return enemyColor;
    }

    public string GetEnemyColor()
    {
        string enemyColor = "[enemyColor]";
        if (EnemyColor != null)
            enemyColor = EnemyColor;
        else if (EventType.StartsWith("burb"))
            enemyColor = GetBurbEnemyColor();
        else if (EventType.StartsWith("unit"))
            enemyColor = GetUnitEnemyColor();
        return enemyColor;
    }

    public string GetLocation()
    {
        return GetLocation(false);
    }
    public string GetLocation(bool returnEmpty)
    {
        string location = "[location]";
        if (returnEmpty)
            location = "";
        if (MapHex != null)
            location = MapHex.X + "," + MapHex.Y;
        return location;
    }

    public string GetUnitLocation(bool returnEmpty)
    {
        string location = "[location]";
        if (returnEmpty)
            location = "";
        if (MapHex != null)
            location = Unit.X + "," + Unit.Y;
        return location;
    }
    public string GetUnitLocation()
    {
        return GetUnitLocation(false);
    }


    public string GetBurbType()
    {
        string burbType = "[burbType]";
        if (MapHex != null && MapHex.Burb != null)
            burbType = MapHex.Burb.Type;
        return burbType;
    }

    public string GetBurbName()
    {
        string burbName = "[burbName]";
        if (MapHex != null && MapHex.Burb != null)
        {
            burbName = MapHex.Burb.Name;
            if (burbName == null || burbName.Length < 1)
                burbName = MapHex.Burb.ParentBurbName;
            if (burbName == null)
                burbName = "[burbName]";
        }
        return burbName;
    }

    public string GetEnemyFactionName()
    {
        string factionName = "[factionName]";
        string color = GetEnemyColor();
        if (GameState != null && color != null)
        {
            if (GameState.Factions.ColorToFaction.ContainsKey(color))
            {
                factionName = GameState.Factions.ColorToFaction[color].Name;
            }
        }
        return factionName;
    }

    public string GetEnemyPlayerName()
    {
        string playerName = "[playerName]";
        string color = GetEnemyColor();
        if (GameState != null && color != null)
        {
            if (GameState.Players.colorToPlayer.ContainsKey(color))
            {
                playerName = GameState.Players.colorToPlayer[color].Name;
            }
        }
        return playerName;
    }


    public void unitAttackedHandler()
    {
        // UnitType at MapHex attacked
        EventString = GetUnitType() + " at " + GetLocation() + " attacked.";
        Game.playSoundEffect(EventType);
        if (COMMAND_CENTER.Equals(GetUnitType()))
        {
            Game.playSoundEffect("comcenAttacked");
        }
        Game.addGamePlayEvent(this);
        //Game.scrollToPosition(MapHex.Y, MapHex.X);
        try
        {
            Game.MainGameScreen.showTimedLocationPopup(EventString, secondsForPopupToAppear, MapHex);    
        }
        catch(Exception ex)
        {
            //Globals.Log("unitAttackedHandler(): Exception " + ex);
        }
    }

    public void unitDestroyedHandler()
    {
        // UnitType at MapHex destroyed
        EventString = GetUnitType() + " at " + GetLocation() + " destroyed.";
        Game.playSoundEffect(EventType + "1");
        Game.playSoundEffect(EventType + "2");
        Game.addGamePlayEvent(this);
        try
        {
            Game.MainGameScreen.showTimedLocationPopup(EventString, secondsForPopupToAppear, MapHex);            
        }
        catch(Exception exIgnore) {}
    }

    public void unitMovementBlockedHandler()
    {
        // Movement blocked for UnitType at MapHex
        EventString = "Movement blocked for " + GetUnitType() + " at " + GetLocation() + ".";
        //Game.addGamePlayEvent(this);
    }

    public void unitSufferedAttritionHandler()
    {
        // UnitType at MapHex suffered attrition
        EventString = GetUnitType() + " at " + GetLocation() + " suffered attrition.";
        Game.addGamePlayEvent(this);
    }

    public void enemyUnitDiscoveredHandler()
    {
        // EnemyColor UnitType discovered at MapHex
        EventString = GetEnemyColor() + " " + GetUnitType() + " discovered at " + GetLocation();
        //Game.addGamePlayEvent(this);
    }

    public void enemyUnitAttackedHandler()
    {
        // EnemyColor UnitType attacked at MapHex
        EventString = GetEnemyColor() + " " + GetUnitType() + " attacked at " + GetLocation();
        //Game.playSoundEffect(EventType);
        //Game.addGamePlayEvent(this);
    }

    public void enemyUnitDestroyedHandler()
    {
        // EnemyColor UnitType destroyed at MapHex
        EventString = GetEnemyColor() + " " + GetUnitType() + " destroyed at " + GetLocation();
        Game.playSoundEffect(EventType);
        Game.addGamePlayEvent(this);
    }

    public void burbDiscoveredHandler()
    {
        // EnemyColor BurbType BurbName discovered at MapHex
        EventString = GetEnemyColor() + " " + GetBurbType() + " " + GetBurbName() + " discovered at " + GetLocation();
        Game.addGamePlayEvent(this);
    }

    public void burbCapturedHandler()
    {
        // BurbType BurbName captured at MapHex
        EventString = GetBurbType() + " " + GetBurbName() + " captured from " + GetEnemyColor() + " at " + GetLocation();
        Game.playSoundEffect(EventType);
        Game.addGamePlayEvent(this);
    }

    public void burbLostHandler()
    {
        // BurbType BurbName lost at MapHex
        EventString = GetBurbType() + " " + GetBurbName() + " lost to " + GetEnemyColor() + " at " + GetLocation();
        Game.playSoundEffect(EventType);
        Game.addGamePlayEvent(this);
        try
        {
            Game.MainGameScreen.showTimedLocationPopup(EventString, secondsForPopupToAppear, MapHex);    
        }
        catch(Exception exIgnore) {}
    }

    public void playerLostGameHandler()
    {
        // Game Lost
        EventString = "You Lost the Game.";
        Game.playSoundEffect(EventType);
        Game.addGamePlayEvent(this);
        Game.MainGameScreen.showMessage(EventString);
    }

    public void playerWonGameHandler()
    {
        // Game Won
        EventString = "You Won the Game.";
        //Game.playSoundEffect(EventType + "1");
        Game.playSoundEffect(EventType + "2");
        Game.addGamePlayEvent(this);
        Game.MainGameScreen.showMessage(EventString);
    }

    public void enemyPlayerLostGameHandler()
    {
        // EnemyColor Lost Game
        EventString = GetEnemyColor() + " Lost the Game.";
        Game.playSoundEffect(EventType);
        Game.addGamePlayEvent(this);
    }

    public void enemyPlayerWonGameHandler()
    {
        // EnemyColor Won Game
        Game.playSoundEffect(GAME_EVENT_PLAYER_LOST_GAME);
        EventString = GetEnemyColor() + " Won the Game.";
        Game.addGamePlayEvent(this);
        Game.MainGameScreen.showMessage(EventString);
    }

    public void gameOverHandler()
    {
        // Game Over
        EventString = "The Game is Over.";
        Game.addGamePlayEvent(this);
    }

    public void airplaneMissionSuceededHandler()
    {
        EventString = "Air mission suceeded for " + GetUnitType() + " at " + GetLocation() + ".";
        Game.playSoundEffect("airplaneNotification");
        //Game.addGamePlayEvent(this);
    }
    public void airplaneStrikeSuceededHandler()
    {
        EventString = "Air strike suceeded for " + GetUnitType() + " at " + GetLocation() + ".";
        Game.playSoundEffect(GAME_EVENT_ENEMY_UNIT_ATTACKED);
        //Game.addGamePlayEvent(this);
    }

    public void airplaneMissionFailedHandler()
    {
        EventString = "Air mission failed for " + GetUnitType() + " at " + GetLocation() + ".";
        Game.playSoundEffect(EventType);
        //Game.addGamePlayEvent(this);
    }

    public void airplaneBombingSuceededHandler()
    {
        EventString = "Bombing suceeded for " + GetUnitType() + " at " + GetLocation() + ".";
        Game.playSoundEffect(GAME_EVENT_ENEMY_UNIT_DESTROYED);
        //Game.addGamePlayEvent(this);
    }

    public void gracePeriodStartedHandler()
    {
        EventString = "Grace period started before execution.";
        Game.playSoundEffect(EventType);
    }

    public void serverMessageHandler()
    {
        Globals.Log("serverMessageHandler(): enter");
        if (TargetScreenId == null)
            Game.MainGameScreen.showMessage(EventString);
        else if (TargetScreenId.Equals("JoinGameScreen"))
        {
            Game.MainGameScreen.hide();
            Game.JoinGameScreen.show();
            Game.JoinGameScreen.showMessage(EventString);
        }

    }

    public void planeDefendingHandler()
    {
        Globals.Log("planeDefendingHandler(): enter");
        Game.playSoundEffect("jetFlyby");
        EventString = "Plane at " + GetUnitLocation() + " grounded from defense action at " + GetLocation() + ".";
        Game.addGamePlayEvent(this);
    }

    public void planeInDogfightHandler()
    {
        Globals.Log("planeInDogfightHandler(): enter");
        Game.playSoundEffect("jetFlyby");
        EventString = "Plane at " + GetUnitLocation() + " grounded from dogfight.";
        Game.addGamePlayEvent(this);
    }

    public void planningPhaseEndedHandler()
    {
        Globals.Log("planningPhaseEndedHandler(): enter");
        Game.playSoundEffect("stopPlanningStartExecution");
        //Thread.Sleep(2000);
    }

    public void planningPhaseStartingHandler()
    {
        Globals.Log("planningPhaseStartingHandler(): enter");
        Game.playSoundEffect("startTurnPlanning");
    }

    public void joinedGameHandler()
    {
        Globals.Log("joinedGameHandler(): enter");
        // TODO: fix scrollToMetro
        //Game.scrollToMetro();
    }

    public void burbSabotagedHandler()
    {
        Globals.Log("burbSabotagedHandler(): enter");
        EventString = "Burb " + GetBurbName() + " at " + GetLocation() + " sabotaged by spy.";
        Game.addGamePlayEvent(this);
        Game.playSoundEffect("unitDestroyed2");
    }

}