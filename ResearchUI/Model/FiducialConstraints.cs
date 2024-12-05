using System.Collections.ObjectModel;

namespace ResearchUI.Model;

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

    public (string, string) OrderedTargets
        => string.Compare(Target1, Target2, StringComparison.Ordinal) <= 0
            ? (Target1, Target2)
            : (Target2, Target1);

    public override bool Equals(object obj)
    {
        if (obj is Pair otherPair)
        {
            return OrderedTargets == otherPair.OrderedTargets;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return OrderedTargets.GetHashCode();
    }
}

public class Position
{
    public string Target { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
    public double Accuracy { get; set; }
}