namespace DronParcial.Domain;

public sealed class DroneRouteResult
{
    public DroneRouteResult(
        int size,
        Coordinate start,
        bool success,
        int reachableCount,
        bool[,] reachableMap,
        int[,] board,
        IReadOnlyList<Coordinate> route)
    {
        Size = size;
        Start = start;
        Success = success;
        ReachableCount = reachableCount;
        ReachableMap = reachableMap;
        Board = board;
        Route = route;
    }

    public int Size { get; }

    public Coordinate Start { get; }

    public bool Success { get; }

    public int ReachableCount { get; }

    public bool[,] ReachableMap { get; }

    public int[,] Board { get; }

    public IReadOnlyList<Coordinate> Route { get; }
}
