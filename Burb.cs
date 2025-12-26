namespace GlobalConquest;

public class Burb
{
    public string Type { get; set; } // village, town, city, capital, metro, suburb, dock
    public string Name { get; set; }
    public string? Color { get; set; } = "grey";
    public string? OwnerColor { get; set; } = "grey";
    public string? ParentBurbName { get; set; }
    public string? DirectionFromParent {get;set;}
    public int X { get; set; }
    public int Y { get; set; }
    public int Money {get; set;}

    public Burb()
    {

    }
}