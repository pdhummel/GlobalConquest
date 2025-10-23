namespace GlobalConquest.Actions;

public class UnitAction
{
    public string Action { get; set; }
    //public Unit Unit { get; set; }

    public Unit? TargetUnit { get; set; }
    public int TargetX { get; set; }
    public int TargetY { get; set; }

    public UnitAction()
    {
        
    }
}