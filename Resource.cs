using System.Text.Json.Serialization;
using static GameConstants;
namespace GlobalConquest;



public class Resource
{
    public static readonly string RESOURCE_FUEL = "fuel";
    public static readonly string RESOURCE_MINERAL_DEPOSITS = "mineral deposits";
    public static readonly string RESOURCE_MODE_NONE = "0: None";
    public static readonly string RESOURCE_MODE_MONEY = "1: Money";
    public static readonly string RESOURCE_MODE_OIL = "2: #1 + Oil->Tanks+Planes";
    public static readonly string RESOURCE_MODE_MINERALS = "3: #2 + Minerals->Ships";

    [JsonPropertyName("T")]
    public string Type { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    [JsonPropertyName("OC")]
    public string? OwnerColor { get; set; } = NATIVE_COLOR;
    [JsonPropertyName("V")]
    public Dictionary<string, bool> Visibility { get; set; } = new Dictionary<string, bool>();
    [JsonPropertyName("PB")]
    public string ParentBurbXy {get; set; }


    public Resource()
    {

    }

    public bool IsVisibleToColor(string color)
    {
        bool isVisible = false;
        if (Visibility.ContainsKey(color))
            isVisible = Visibility[color];
        return isVisible;
    }

}