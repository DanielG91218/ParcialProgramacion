using DronParcial.Domain;

namespace DronParcial.Services;

public sealed class DronePathSolver
{
    private static readonly Coordinate[] Moves =
    [
        new Coordinate(2, 1),
        new Coordinate(2, -1),
        new Coordinate(-2, 1),
        new Coordinate(-2, -1),
        new Coordinate(1, 2),
        new Coordinate(1, -2),
        new Coordinate(-1, 2),
        new Coordinate(-1, -2)
    ];

    public DroneRouteResult Solve(int size, Coordinate start)
    {
        if (size < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(size), "El terreno debe tener N >= 1.");
        }

        if (!IsInside(start, size))
        {
            throw new ArgumentOutOfRangeException(nameof(start), "La coordenada inicial esta fuera del terreno.");
        }

        bool[,] reachableMap = BuildReachableMap(size, start, out int reachableCount);
        int[,] board = CreateEmptyBoard(size);
        var route = new List<Coordinate>(reachableCount);

        board[start.X, start.Y] = 0;
        route.Add(start);

        bool success = Search(start, step: 1, size, reachableCount, reachableMap, board, route);
        IReadOnlyList<Coordinate> finalRoute = success ? route.ToArray() : Array.Empty<Coordinate>();

        return new DroneRouteResult(size, start, success, reachableCount, reachableMap, board, finalRoute);
    }

    private static bool Search(
        Coordinate current,
        int step,
        int size,
        int targetCount,
        bool[,] reachableMap,
        int[,] board,
        List<Coordinate> route)
    {
        if (step == targetCount)
        {
            return true;
        }

        List<CandidateMove> candidates = GetOrderedCandidates(current, size, reachableMap, board);

        int index = 0;
        while (index < candidates.Count)
        {
            Coordinate next = candidates[index].Position;
            board[next.X, next.Y] = step;
            route.Add(next);

            if (Search(next, step + 1, size, targetCount, reachableMap, board, route))
            {
                return true;
            }

            route.RemoveAt(route.Count - 1);
            board[next.X, next.Y] = -1;
            index++;
        }

        return false;
    }

    private static List<CandidateMove> GetOrderedCandidates(
        Coordinate current,
        int size,
        bool[,] reachableMap,
        int[,] board)
    {
        var candidates = new List<CandidateMove>();

        int index = 0;
        while (index < Moves.Length)
        {
            Coordinate next = Add(current, Moves[index]);

            if (IsInside(next, size) && reachableMap[next.X, next.Y] && board[next.X, next.Y] == -1)
            {
                int degree = CountAvailableMoves(next, size, reachableMap, board);
                candidates.Add(new CandidateMove(next, degree));
            }

            index++;
        }

        candidates.Sort(static (left, right) =>
        {
            int degreeComparison = left.Degree.CompareTo(right.Degree);
            if (degreeComparison != 0)
            {
                return degreeComparison;
            }

            int xComparison = left.Position.X.CompareTo(right.Position.X);
            if (xComparison != 0)
            {
                return xComparison;
            }

            return left.Position.Y.CompareTo(right.Position.Y);
        });

        return candidates;
    }

    private static int CountAvailableMoves(Coordinate from, int size, bool[,] reachableMap, int[,] board)
    {
        int count = 0;
        int index = 0;

        while (index < Moves.Length)
        {
            Coordinate next = Add(from, Moves[index]);

            if (IsInside(next, size) && reachableMap[next.X, next.Y] && board[next.X, next.Y] == -1)
            {
                count++;
            }

            index++;
        }

        return count;
    }

    private static bool[,] BuildReachableMap(int size, Coordinate start, out int reachableCount)
    {
        var reachable = new bool[size, size];
        var pending = new Queue<Coordinate>();

        reachable[start.X, start.Y] = true;
        pending.Enqueue(start);
        reachableCount = 1;

        while (pending.Count > 0)
        {
            Coordinate current = pending.Dequeue();
            int index = 0;

            while (index < Moves.Length)
            {
                Coordinate next = Add(current, Moves[index]);

                if (IsInside(next, size) && !reachable[next.X, next.Y])
                {
                    reachable[next.X, next.Y] = true;
                    reachableCount++;
                    pending.Enqueue(next);
                }

                index++;
            }
        }

        return reachable;
    }

    private static int[,] CreateEmptyBoard(int size)
    {
        var board = new int[size, size];

        int x = 0;
        while (x < size)
        {
            int y = 0;
            while (y < size)
            {
                board[x, y] = -1;
                y++;
            }

            x++;
        }

        return board;
    }

    private static bool IsInside(Coordinate coordinate, int size)
    {
        return coordinate.X >= 0
            && coordinate.X < size
            && coordinate.Y >= 0
            && coordinate.Y < size;
    }

    private static Coordinate Add(Coordinate left, Coordinate right)
    {
        return new Coordinate(left.X + right.X, left.Y + right.Y);
    }

    private readonly record struct CandidateMove(Coordinate Position, int Degree);
}
