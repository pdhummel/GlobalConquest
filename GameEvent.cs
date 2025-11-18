
namespace GlobalConquest;

public class GameEvent
{
    // Used to send separate message to clients for Events.
    // Also keep track of these events in a server log.
    // EventTypes:
    // gameStateUpdate
    // mapUpdate
    // gameStateAndMapUpdate
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
    public MapHex? MapHex { get; set; }
    public List<MapHex>? MapHexBuffer { get; set; } = new List<MapHex>();
    public bool IsLastMapHexBufferUpdate {get; set;} = false;
    public string? UnitType { get; set; }
    public string? EnemyColor { get; set; }
    public GameState? GameState { get; set; }

    public GameEvent()
    {

    }
}