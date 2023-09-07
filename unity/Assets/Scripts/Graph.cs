using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace CoastlinesGen
{

    public class Edge
    {
        public Vector3 startPoint;
        public Vector3 endPoint;

        public Edge(Vector3 startPoint, Vector3 endPoint)
        {
            this.startPoint = startPoint;
            this.endPoint = endPoint;
        }
    }

    public class Node
    {
        public Vector3 point;
        public HashSet<Node> neighbours;
        public List<Edge> edges;

        public Node(Vector3 pos)
        {
            neighbours = new HashSet<Node>();
            edges = new List<Edge>();
            point = pos;
        }
    }

    public class Graph
    {
        public List<Node> nodes;
        public Dictionary<Vector3, HashSet<Vector3>> edgeNeighbours;
        public List<Vector3> edgeNeighboursKeys;

        public Graph(Voronoi.VoronoiGraph graph)
        {
            nodes = new List<Node>();
            edgeNeighbours = new Dictionary<Vector3, HashSet<Vector3>>();
            edgeNeighboursKeys = new List<Vector3>();
            void AddNeighbour(Vector3 a, Vector3 b)
            {
                if (a == null || b == null)
                    return;

                if (!edgeNeighbours.ContainsKey(a))
                    edgeNeighbours[a] = new HashSet<Vector3>();
                edgeNeighbours[a].Add(b);

                edgeNeighboursKeys.Add(a);
            };

            foreach (var cell in graph.cells)
            {
                var nodeSite = cell.site;
                var node = new Node(nodeSite.ToVector3());
                foreach (var edge in cell.halfEdges)
                {
                    var e = new Edge(edge.GetStartPoint().ToVector3(), edge.GetEndPoint().ToVector3());
                    node.edges.Add(e);

                    AddNeighbour(e.startPoint, e.endPoint);
                    AddNeighbour(e.endPoint, e.startPoint);
                }

                nodes.Add(node);
            }

            Debug.Log("graph: " + nodes.Count + " nodes, " + edgeNeighboursKeys.Count + " edgeNeighbours");
        }
    }


}
