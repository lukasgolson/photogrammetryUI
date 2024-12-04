using System.Collections.ObjectModel;

public class FiducialConstraints
{
    public bool Enabled { get; set; }
    public ObservableCollection<Pair> Pairs { get; set; } = new();
    public ObservableCollection<Position> Position { get; set; } = new();
}

public class Pair
{
    public string Target1 { get; set; }
    public string Target2 { get; set; }
    public double Distance { get; set; }
}

public class Position
{
    public string Target { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
    public double Accuracy { get; set; }
}