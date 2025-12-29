
using System.Reflection;
using System.Runtime.CompilerServices;
using GlobalConquest.Units;
using Microsoft.Xna.Framework;

namespace GlobalConquest;

public class GameEvent
{
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
            "unitAttacked",         // UnitType at MapHex attacked
            "enemyUnitAttacked",    // EnemyColor UnitType attacked at MapHex 
            "unitDestroyed",        // UnitType at MapHex destroyed           
            "enemyUnitDestroyed",   // EnemyColor UnitType destroyed at MapHex
            "airplaneMissionSuceeded",
            "airplaneStrikeSuceeded",
            "airplaneBombingSuceeded",
            "airplaneMissionFailed",
            "playerLostGame",       // Game Lost           
            "enemyPlayerLostGame",  // EnemyColor Lost Game
            "playerWonGame",        // Game Won           
            "enemyPlayerWonGame",   // EnemyColor Won Game
            "gameOver",             // Game Over                           
            "burbCaptured",         // BurbType BurbName captured at MapHex
            "burbLost",             // BurbType BurbName lost at MapHex
            "unitMovementBlocked",  // UnitType at MapHex movement blocked
            "unitSufferedAttrition",// UnitType at Maphex suffered attrition
            "enemyUnitDiscovered",  // EnemyColor UnitType discovered at MapHex
            "burbDiscovered",       // EnemyColor BurbType BurbName discovered at MapHex
            "gracePeriodStarted",
            "serverMessage",
            "planeDefending",
            "planeInDogfight",
            "planningPhaseEnded",
            "planningPhaseStarting",
            "joinedGame",
            "burbSabotaged"
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
        if ("comcen".Equals(GetUnitType()))
        {
            Game.playSoundEffect("comcenAttacked");
        }
        Game.addGamePlayEvent(this);
        //Game.scrollToPosition(MapHex.Y, MapHex.X);
        Game.MainGameScreen.showTimedLocationPopup(EventString, secondsForPopupToAppear, MapHex);
    }

    public void unitDestroyedHandler()
    {
        // UnitType at MapHex destroyed
        EventString = GetUnitType() + " at " + GetLocation() + " destroyed.";
        Game.playSoundEffect(EventType + "1");
        Game.playSoundEffect(EventType + "2");
        Game.addGamePlayEvent(this);
        Game.MainGameScreen.showTimedLocationPopup(EventString, secondsForPopupToAppear, MapHex);
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
        Game.MainGameScreen.showTimedLocationPopup(EventString, secondsForPopupToAppear, MapHex);
    }

    public void playerLostGameHandler()
    {
        // Game Lost
        EventString = "You Lost the Game.";
        Game.playSoundEffect(EventType);
        Game.addGamePlayEvent(this);
    }

    public void playerWonGameHandler()
    {
        // Game Won
        EventString = "You Won the Game.";
        //Game.playSoundEffect(EventType + "1");
        Game.playSoundEffect(EventType + "2");
        Game.addGamePlayEvent(this);
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
        EventString = GetEnemyColor() + " Won the Game.";
        Game.addGamePlayEvent(this);
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
        Game.playSoundEffect("enemyUnitAttacked");
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
        Game.playSoundEffect("enemyUnitDestroyed");
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