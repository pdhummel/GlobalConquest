using GlobalConquest.Units;
namespace GlobalConquest.Actions;

public class UnitAction
{
    public string? Action { get; set; }

    public int TargetX { get; set; }
    public int TargetY { get; set; }
    public long Ticks { get; set; } = DateTime.Now.Ticks;

    public UnitAction()
    {

    }
}