
using System.Reflection;
using System.Runtime.CompilerServices;
using GlobalConquest.Units;
using Microsoft.Xna.Framework;

namespace GlobalConquest;

public class GameEvent
{
    // Used to send separate message to clients for Events.
    // Also keep track of these events in a server log.
    // EventTypes:
    // gameStateUpdate
    // mapUpdate
    // gameStateAndMapUpdate
    //
    // unitAttacked (comcenAttacked)
    // unitDestroyed
    // unitMovementBlocked
    // enemyUnitDiscovered
    // enemyUnitAttacked
    // enemyUnitDestroyed
    // burbDiscovered
    // burbCaptured
    // burbLost
    // playerLostGame
    // playerWonGame
    // gameOver
    public string EventType { get; set; }
    public long Ticks { get; set; }
    public List<MapHex>? MapHexBuffer { get; set; } = new List<MapHex>();
    public bool IsLastMapHexBufferUpdate {get; set;} = false;
    public GameState? GameState { get; set; }

    public GlobalConquestGame? Game { get; set; }

    public MapHex? MapHex { get; set; }
    public Unit? Unit { get; set; }
    public string? EnemyColor { get; set; }
    public string? EventString { get; set; }


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
            "playerLostGame",       // Game Lost           
            "enemyPlayerLostGame",  // EnemyColor Lost Game
            "playerWonGame",        // Game Won           
            "enemyPlayerWonGame",   // EnemyColor Won Game
            "gameOver",             // Game Over                           
            "burbCaptured",         // BurbType BurbName captured at MapHex
            "burbLost",             // BurbType BurbName lost at MapHex
            // TODO: handle these Game Play Events:
            "unitMovementBlocked",  // UnitType at MapHex movement blocked
            "unitSufferedAttrition",// UnitType at Maphex suffered attrition
            "enemyUnitDiscovered",  // EnemyColor UnitType discovered at MapHex
            "burbDiscovered"        // EnemyColor BurbType BurbName discovered at MapHex
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
        eventMethodHandler?.Invoke(this, parameters);
        Console.WriteLine("handleGamePlayEvent(): " + EventString);

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
        string location = "[location]";
        if (MapHex != null)
            location = MapHex.X + "," + MapHex.Y;
        return location;
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

    }

    public void unitDestroyedHandler()
    {        
        // UnitType at MapHex destroyed
        EventString = GetUnitType() + " at " + GetLocation() + " destroyed.";
        Game.playSoundEffect(EventType + "1");
        Game.playSoundEffect(EventType + "2");
    }

    public void unitMovementBlockedHandler() 
    {
        // Movement blocked for UnitType at MapHex
        EventString = "Movement blocked for " + GetUnitType() + " at " + GetLocation() + ".";
    }

    public void unitSufferedAttritionHandler() 
    {
        // UnitType at MapHex suffered attrition
        EventString = GetUnitType() + " at " + GetLocation() + " suffered attrition.";
    }

    public void enemyUnitDiscoveredHandler()
    {
        // EnemyColor UnitType discovered at MapHex
        EventString = GetEnemyColor() + " " + GetUnitType() + " discovered at " + GetLocation();
    } 

    public void enemyUnitAttackedHandler() 
    {    
        // EnemyColor UnitType attacked at MapHex
        EventString = GetEnemyColor() + " " + GetUnitType() + " attacked at " + GetLocation();
        //Game.playSoundEffect(EventType);
    }

    public void enemyUnitDestroyedHandler()
    {
        // EnemyColor UnitType destroyed at MapHex
        EventString = GetEnemyColor() + " " + GetUnitType() + " destroyed at " + GetLocation();
        Game.playSoundEffect(EventType);
    }

    public void burbDiscoveredHandler()
    {
        // EnemyColor BurbType BurbName discovered at MapHex
        EventString = GetEnemyColor() + " " + GetBurbType() + " " + GetBurbName() + " discovered at " + GetLocation();
    }

    public void burbCapturedHandler()
    {
        // BurbType BurbName captured at MapHex
        EventString =  GetBurbType() + " " + GetBurbName() + " captured from " + GetEnemyColor() + " at " + GetLocation();
        Game.playSoundEffect(EventType);
    }

    public void burbLostHandler()
    {
        // BurbType BurbName lost at MapHex
        EventString =  GetBurbType() + " " + GetBurbName() + " lost to " + GetEnemyColor() + " at " + GetLocation();
        Game.playSoundEffect(EventType);
    }

    public void playerLostGameHandler() {      
        // Game Lost
        EventString = "You Lost the Game.";
        Game.playSoundEffect(EventType);
    }

    public void playerWonGameHandler() {    
        // Game Won
        EventString = "You Won the Game.";
        //Game.playSoundEffect(EventType + "1");
        Game.playSoundEffect(EventType + "2");
    }

    public void enemyPlayerLostGameHandler() 
    { 
        // EnemyColor Lost Game
        EventString = GetEnemyColor() + " Lost the Game.";
        Game.playSoundEffect(EventType);
    }

    public void enemyPlayerWonGameHandler() 
    {  
        // EnemyColor Won Game
        EventString = GetEnemyColor() + " Won the Game.";
    }

    public void gameOverHandler() 
    {            
        // Game Over
        EventString = "The Game is Over.";
    }


}