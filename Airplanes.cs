public class Airplanes
{

    public Dictionary<string, HashSet<string>> ColorToAirplaneIds { get; set; } = new Dictionary<string, HashSet<string>>();

    public Airplanes()
    {
        List<string> colors = [ AMBER, CYAN, MAGENTA, OCHER ];
        foreach (string color in colors)
        {
            ColorToAirplaneIds[color] = new HashSet<string>();
        }
    }
}