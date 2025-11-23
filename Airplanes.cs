public class Airplanes
{

    public Dictionary<string, HashSet<string>> ColorToAirplaneIds { get; set; } = new Dictionary<string, HashSet<string>>();

    public Airplanes()
    {
        List<string> colors = [ "amber", "cyan", "magenta", "ocher" ];
        foreach (string color in colors)
        {
            ColorToAirplaneIds[color] = new HashSet<string>();
        }
    }
}