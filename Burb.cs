namespace GlobalConquest;

public class Burb
{
    public string Type { get; set; } // village, town, city, capital, metro
    public string Name { get; set; }
    public string? Color { get; set; } = "grey";

    public string? OwnerColor { get; set; } = "grey";

    public Burb()
    {

    }
}