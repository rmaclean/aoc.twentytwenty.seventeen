using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;

var data = await File.ReadAllLinesAsync("data.txt");

var universe = new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, bool>>>>();
SetupInitialUniverse();
for (int i = 0; i < 6; i++)
{
    universe = IterateUniverse();
}

var result = 0;
foreach (var row in universe)
{
    foreach (var col in row.Value)
    {
        foreach (var depth in col.Value)
        {
            foreach (var time in depth.Value)
            {
                result += time.Value ? 1 : 0;
            }
        }
    }
}

Console.WriteLine($"Result = {result}");

Tuple<Tuple<int, int>, Tuple<int, int>, Tuple<int, int>, Tuple<int, int>> DetermineUniverseBounds()
{
    var (minRow, maxRow) = universe.MinAndMax();

    var minCol = int.MaxValue;
    var maxCol = int.MinValue;

    var minDepth = int.MaxValue;
    var maxDepth = int.MinValue;

    var minTime = int.MaxValue;
    var maxTime = int.MinValue;
    foreach (var row in universe)
    {
        var (thisColMin, thisColMax) = row.Value.MinAndMax();
        if (thisColMin < minCol)
        {
            minCol = thisColMin;
        }

        if (thisColMax > maxCol)
        {
            maxCol = thisColMax;
        }

        foreach (var col in row.Value)
        {
            var (thisDepthMin, thisDepthMax) = col.Value.MinAndMax();
            if (thisDepthMin < minDepth)
            {
                minDepth = thisDepthMin;
            }

            if (thisDepthMax > maxDepth)
            {
                maxDepth = thisDepthMax;
            }

            foreach (var depth in col.Value)
            {
                var (thisTimeMin, thisTimeMax) = col.Value.MinAndMax();
                if (thisTimeMin < minTime)
                {
                    minTime = thisTimeMin;
                }

                if (thisTimeMax > maxTime)
                {
                    maxTime = thisTimeMax;
                }
            }
        }
    }

    return Tuple.Create(
            Tuple.Create(minRow, maxRow),
            Tuple.Create(minCol, maxCol),
            Tuple.Create(minDepth, maxDepth),
            Tuple.Create(minTime, maxTime)
            );
}

Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, bool>>>> IterateUniverse()
{
    var result = new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, bool>>>>();
    var ((minRow, maxRow), (minCol, maxCol), (minDepth, maxDepth), (minTime, maxTime)) = DetermineUniverseBounds();

    for (var rowIndex = minRow - 1; rowIndex < maxRow + 2; rowIndex++)
    {
        if (!universe.ContainsKey(rowIndex))
        {
            universe[rowIndex] = new Dictionary<int, Dictionary<int, Dictionary<int, bool>>>();
        }

        var row = universe[rowIndex];

        for (var colIndex = minCol - 1; colIndex < maxCol + 2; colIndex++)
        {
            if (!row.ContainsKey(colIndex))
            {
                row[colIndex] = new Dictionary<int, Dictionary<int, bool>>();
            }

            var col = row[colIndex];
            for (var depthIndex = minDepth - 1; depthIndex < maxDepth + 2; depthIndex++)
            {
                if (!col.ContainsKey(depthIndex))
                {
                    col[depthIndex] = new Dictionary<int, bool>();
                }

                var depth = col[depthIndex];
                for (var timeIndex = minTime - 1; timeIndex < maxTime + 2; timeIndex++)
                {
                    var currentState = State(rowIndex, colIndex, depthIndex, timeIndex);
                    var neighbours = Count(rowIndex, colIndex, depthIndex, timeIndex);
                    var alive = false;
                    if (currentState)
                    {
                        alive = (neighbours == 2 || neighbours == 3);
                    }
                    else
                    {
                        alive = neighbours == 3;
                    }

                    SetState(result, rowIndex, colIndex, depthIndex, timeIndex, alive);
                }
            }
        }
    }

    return result;
}

bool State(int row, int col, int depth, int time)
{
    if (!universe.ContainsKey(row))
    {
        return false;
    }

    var targetRow = universe[row];
    if (!targetRow.ContainsKey(col))
    {
        return false;
    }

    var targetCol = targetRow[col];
    if (!targetCol.ContainsKey(depth))
    {
        return false;
    }

    var targetDepth = targetCol[depth];
    if (!targetDepth.ContainsKey(time))
    {
        return false;
    }

    return targetDepth[time];
}

int Count(int row, int col, int depth, int time)
{
    var result = 0;
    for (var rowOffset = -1; rowOffset < 2; rowOffset++)
    {
        var targetRowIndex = row + rowOffset;
        if (!universe.ContainsKey(targetRowIndex))
        {
            continue;
        }

        var targetRow = universe[targetRowIndex];
        for (var colOffset = -1; colOffset < 2; colOffset++)
        {
            var targetColIndex = col + colOffset;
            if (!targetRow.ContainsKey(targetColIndex))
            {
                continue;
            }

            var targetCol = targetRow[targetColIndex];
            for (var depthOffset = -1; depthOffset < 2; depthOffset++)
            {
                var depthOffsetIndex = depth + depthOffset;
                if (!targetCol.ContainsKey(depthOffsetIndex))
                {
                    continue;
                }

                var targetDepth = targetCol[depthOffsetIndex];
                for (var timeOffset = -1; timeOffset < 2; timeOffset++)
                {
                    var timeOffsetIndex = time + timeOffset;
                    if (!targetDepth.ContainsKey(timeOffsetIndex))
                    {
                        continue;
                    }

                    if (rowOffset == 0 && colOffset == 0 && depthOffset == 0 && timeOffset == 0)
                    {
                        continue;
                    }

                    result += targetDepth[timeOffsetIndex] ? 1 : 0;
                }
            }
        }
    }

    return result;
}

void SetupInitialUniverse()
{
    for (var row = 0; row < data.Length; row++)
    {
        var line = data[row];
        for (var col = 0; col < line.Length; col++)
        {
            var character = line[col];
            var state = character == '#';
            SetState(universe, row, col, 0, 0, state);
        }
    }
}

void SetState(Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, bool>>>> uni, int row, int col, int depth, int time, bool state)
{
    if (!uni.ContainsKey(row))
    {
        uni[row] = new Dictionary<int, Dictionary<int, Dictionary<int, bool>>>();
    }

    var universeRow = uni[row];

    if (!universeRow.ContainsKey(col))
    {
        universeRow[col] = new Dictionary<int, Dictionary<int, bool>>();
    }

    var universeCol = universeRow[col];
    if (!universeCol.ContainsKey(depth))
    {
        universeCol[depth] = new Dictionary<int, bool>();
    }

    var universeDepth = universeCol[depth];
    universeDepth[time] = state;
}


public static class Extensions
{
    public static Tuple<int, int> MinAndMax<T>(this Dictionary<int, T> series) => Tuple.Create(series.Min(i => i.Key), series.Max(i => i.Key));
}