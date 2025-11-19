using System;
using System.Collections.Generic;
namespace GlobalConquest;

public class DijkstraAlgorithm
{

    public static Dictionary<string, string> FindShortestPaths(Dictionary<string, Node> graph, string startNodeId)
    {
        var distances = new Dictionary<string, int>();
        var previousNodes = new Dictionary<string, string>();
        var visited = new HashSet<string>();
        var priorityQueue = new PriorityQueue<string, int>(); // NodeId, Distance

        // Initialize distances
        foreach (var nodeId in graph.Keys)
        {
            distances[nodeId] = int.MaxValue;
            previousNodes[nodeId] = "-1,-1"; // No predecessor initially
        }
        distances[startNodeId] = 0;
        priorityQueue.Enqueue(startNodeId, 0);

        while (priorityQueue.Count > 0)
        {
            var currentNodeId = priorityQueue.Dequeue();

            if (visited.Contains(currentNodeId))
            {
                continue;
            }
            visited.Add(currentNodeId);

            if (graph.ContainsKey(currentNodeId))
            {
                foreach (var edge in graph[currentNodeId].Edges)
                {
                    var neighborId = edge.Target.Name;
                    var newDistance = int.MaxValue;
                    if (distances.ContainsKey(currentNodeId))
                        newDistance = distances[currentNodeId] + edge.Weight;

                    if (distances.ContainsKey(neighborId) && newDistance < distances[neighborId])
                    {
                        distances[neighborId] = newDistance;
                        previousNodes[neighborId] = currentNodeId;
                        priorityQueue.Enqueue(neighborId, newDistance);
                    }
                }
            }
        }

        return previousNodes; // return previousNodes to reconstruct paths with ReconstructPath
    }

    public static List<string> ReconstructPath(Dictionary<string, string> previousNodes, string startNodeId, string endNodeId)
    {
        var path = new List<string>();
        string current = endNodeId;
        while (!"-1,-1".Equals(current) && !current.Equals(startNodeId))
        {
            path.Add(current);
            current = previousNodes[current];
        }
        if (current.Equals(startNodeId))
        {
            path.Add(startNodeId);
        }
        path.Reverse();
        return path;
    }

}

public class Node
{
    public string Name { get; set; }
    public MapHex MapHex { get; set; }
    public List<Edge> Edges { get; set; } = new List<Edge>();

    public Node(MapHex mapHex)
    {
        MapHex = mapHex;
        Name = mapHex.X + "," + mapHex.Y;
    }
}

public class Edge
{
    public Node Target { get; set; }
    public int Weight { get; set; }

    public Edge(Node target)
    {
        Target = target;
        Weight = 1;
    }

    public Edge(Node target, int weight)
    {
        Target = target;
        Weight = weight;
    }
}
