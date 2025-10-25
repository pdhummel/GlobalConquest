using System.Collections;
using GlobalConquest.Actions;
namespace GlobalConquest;

public class Destinations
{
    // List of UnitAction
    Dictionary<string, List<UnitAction>> colorToDestinations;

    public Destinations()
    {
        colorToDestinations = new Dictionary<string, List<UnitAction>>();
    }
}