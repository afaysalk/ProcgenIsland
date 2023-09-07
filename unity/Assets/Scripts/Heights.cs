using System.Collections.Generic;
using UnityEngine;
using CoastlinesGen;
using System.Linq;

public class Heights
{
    private Graph graph;
    Dictionary<Vector3, float> heights;
    float decay;
    float sharpness;
    ISet<Vector3> visited;
    float waterLevel;

    public Heights(Graph graph, float decay, float sharpness, float waterLevel)
    {
        this.waterLevel = waterLevel;
        this.graph = graph;
        this.decay = decay;
        this.sharpness = sharpness;
        heights = new Dictionary<Vector3, float>();
        visited = new HashSet<Vector3>();
    }

    internal void Create(Vector3 firstPoint)
    {
        heights.Clear();

        var height = Random.Range(0.75f, 1f);
        UpdateHeights(firstPoint, height, decay);
    }

    internal void AddTo(Vector3 pos)
    {
        var height = Random.Range(0.1f, 0.25f);
        height = Mathf.Clamp(height + heights[pos], 0, 1);
        UpdateHeights(pos, height, decay * 0.25f);
    }

    void UpdateHeights(Vector3 firstPoint, float firstHeight, float decay)
    {
        heights[firstPoint] = firstHeight;

        var queue = new Queue<Vector3>();
        queue.Enqueue(firstPoint);
        visited.Add(firstPoint);

        while (queue.Count > 0)
        {
            var p = queue.Dequeue();
            visited.Add(p);

            EnqueueNeighbours(queue, p, (neighbour) =>
            {
                //var modifier = 1;
                var modifier = 1 - Random.value * sharpness;
                if (Mathf.Approximately(modifier, 0))
                    modifier = 1;
                var h = heights[p] * decay;
                if (heights.ContainsKey(neighbour))
                    h += heights[neighbour];
                h *= modifier;
                heights[neighbour] = Mathf.Clamp(h, 0, 1);
            });
        }
        visited.Clear();

        AdjustPolygonNearWaterLevel();
        UpdateCenterHeights();
    }

    private void AdjustPolygonNearWaterLevel()
    {
        var underWater = new HashSet<Vector3>();
        var overWater = new HashSet<Vector3>();


        void ClassifyPoint(Vector3 pos)
        {
            var list = (heights[pos] > waterLevel ? overWater : underWater);
            list.Add(pos);
        };

        foreach (var node in graph.nodes)
        {
            underWater.Clear();
            overWater.Clear();

            node.edges.ForEach(x =>
            {
                ClassifyPoint(x.startPoint);
                ClassifyPoint(x.endPoint);
            });

            if (underWater.Count == 0 || overWater.Count == 0)
                continue;


            var moveUp = (underWater.Count <= overWater.Count);
            var positions = (moveUp ? underWater : overWater);
            var delta = 0.001f;
            foreach (var p in positions)
            {
                heights[p] = waterLevel + delta * (moveUp ? 1 : -1);
            }
        }
    }

    private void UpdateCenterHeights()
    {
        foreach (var node in graph.nodes)
        {
            var h = node.edges.Average(x => heights[x.startPoint]);
            heights[node.point] = h;
        }
    }

    private void EnqueueNeighbours(Queue<Vector3> queue, Vector3 point, System.Action<Vector3> action)
    {
        foreach (var neighbourPoint in graph.edgeNeighbours[point])
        {
            if (!visited.Contains(neighbourPoint) && !queue.Contains(neighbourPoint))
            {
                action(neighbourPoint);
                queue.Enqueue(neighbourPoint);
            }
        }
    }

    internal float Height(Vector3 pos)
    {
        if (!heights.ContainsKey(pos))
        {
            Debug.Log("no height for " + pos + "!");
            return 0;
        }
        return heights[pos];
    }
}
