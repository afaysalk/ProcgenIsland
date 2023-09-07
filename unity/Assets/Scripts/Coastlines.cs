using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Voronoi;
using Cell = Voronoi.Cell;

public class Coastlines : MonoBehaviour
{
    public int numSites = 36;
    public Bounds bounds;
    public int relaxationSteps;
    public GameObject chunkObj;
    public Gradient heightColors;
    public Gradient heightWaterColors;
    public float heightDecay;
    public float sharpness;
    public int maxMaterials;
    public float maxHeight;
    public float waterLevel;

    private List<Point> sites;
    private FortuneVoronoi voronoi;
    public VoronoiGraph graph;
    public CoastlinesGen.Graph newGraph;
    private List<FractureChunk> chunks;
    private Queue<FractureChunk> chunksPool;
    Heights heightMap;
    [HideInInspector]
    public bool drawCells;

    void Start()
    {
        sites = new List<Point>();
        voronoi = new FortuneVoronoi();
        chunks = new List<FractureChunk>();
        chunksPool = new Queue<FractureChunk>();

        CreateMap();
    }

    private void CreateMap()
    {
        var start = System.DateTime.Now;
        CreateSites(true, true, relaxationSteps);
        var finish = System.DateTime.Now;
        var elapsedSites = (finish - start).TotalSeconds;

        start = System.DateTime.Now;
        CreateHeights();
        finish = System.DateTime.Now;
        var elapsedHeights = (finish - start).TotalSeconds;

        start = System.DateTime.Now;
        CreateChunks();
        finish = System.DateTime.Now;
        var elapsedChunks = (finish - start).TotalSeconds;

        var colliderForUI = GetComponent<BoxCollider>();
        colliderForUI.center = bounds.center;
        colliderForUI.size = bounds.size;

        Debug.Log("sites:" + elapsedSites + "s"
        + ", heights:" + elapsedHeights + "s"
        + ", chunks:" + elapsedChunks + "s"
        + ", nSites:" + numSites);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
            drawCells = !drawCells;

        if (Input.GetKeyDown(KeyCode.G))
            CreateMap();

        if (Input.GetMouseButtonDown(0))
            ChangeHeights();
    }

    void ChangeHeights()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            var pos = hit.point;
            var minD = float.MaxValue;
            CoastlinesGen.Node nearestSite = null;
            foreach (var node in newGraph.nodes)
            {
                var d = Vector3.Distance(pos, node.point);
                if (minD > d)
                {
                    minD = d;
                    nearestSite = node;
                }
            }

            if (nearestSite == null)
                return;

            var p = nearestSite.edges[0].startPoint;
            Debug.Log("adding height to " + p);

            heightMap.AddTo(p);
            CreateChunks();
        }
    }

    void CreateChunks()
    {
        var generator = new MeshGenerator(maxHeight, maxMaterials, heightMap, heightColors, heightWaterColors, transform.position,
            newGraph, GetComponent<MeshFilter>(), GetComponent<MeshRenderer>(), waterLevel);
        generator.Create();
    }

    void CreateHeights()
    {
        heightMap = new Heights(newGraph, heightDecay, sharpness, waterLevel);
        //var firstPoint = graph.edgeNeighboursKeys[Random.Range(0, graph.edgeNeighboursKeys.Count)];
        var initialPoint = Vector3.zero;
        float minD = float.MaxValue;
        foreach (var pos in newGraph.edgeNeighboursKeys)
        {
            var d = Vector3.Magnitude(pos);
            if (minD > d)
            {
                minD = d;
                initialPoint = pos;
            }
        }
        heightMap.Create(initialPoint);
    }

    void Compute(List<Point> sites)
    {
        this.sites = sites;
        this.graph = this.voronoi.Compute(sites, this.bounds);
    }

    void CreateSites(bool clear = true, bool relax = false, int relaxCount = 2)
    {
        List<Point> sites = new List<Point>();
        if (!clear)
        {
            sites = this.sites.Take(this.sites.Count).ToList();
        }

        // create vertices
        for (int i = 0; i < numSites; i++)
        {
            Point site = new Point(Random.Range(bounds.min.x, bounds.max.x), Random.Range(bounds.min.z, bounds.max.z), 0);
            sites.Add(site);
        }

        Compute(sites);

        if (relax)
        {
            RelaxSites(relaxCount);
        }

        newGraph = new CoastlinesGen.Graph(this.graph);
    }

    void RelaxSites(int iterations)
    {
        for (int i = 0; i < iterations; i++)
        {
            if (!this.graph)
            {
                return;
            }

            Point site;
            List<Point> sites = new List<Point>();
            float dist = 0;

            float p = 1 / graph.cells.Count * 0.1f;

            for (int iCell = graph.cells.Count - 1; iCell >= 0; iCell--)
            {
                Voronoi.Cell cell = graph.cells[iCell];
                float rn = Random.value;

                // probability of apoptosis
                if (rn < p)
                {
                    continue;
                }

                site = CellCentroid(cell);
                dist = Distance(site, cell.site);

                // don't relax too fast
                if (dist > 2)
                {
                    site.x = (site.x + cell.site.x) / 2;
                    site.y = (site.y + cell.site.y) / 2;
                }
                // probability of mytosis
                if (rn > (1 - p))
                {
                    dist /= 2;
                    sites.Add(new Point(site.x + (site.x - cell.site.x) / dist, site.y + (site.y - cell.site.y) / dist));
                }
                sites.Add(site);
            }

            Compute(sites);
        }
    }

    float Distance(Point a, Point b)
    {
        float dx = a.x - b.x;
        float dy = a.y - b.y;
        return Mathf.Sqrt(dx * dx + dy * dy);
    }

    Point CellCentroid(Voronoi.Cell cell)
    {
        float x = 0f;
        float y = 0f;
        Point p1, p2;
        float v;

        for (int iHalfEdge = cell.halfEdges.Count - 1; iHalfEdge >= 0; iHalfEdge--)
        {
            HalfEdge halfEdge = cell.halfEdges[iHalfEdge];
            p1 = halfEdge.GetStartPoint();
            p2 = halfEdge.GetEndPoint();
            v = p1.x * p2.y - p2.x * p1.y;
            x += (p1.x + p2.x) * v;
            y += (p1.y + p2.y) * v;
        }
        v = CellArea(cell) * 6;
        return new Point(x / v, y / v);
    }

    float CellArea(Voronoi.Cell cell)
    {
        float area = 0.0f;
        Point p1, p2;

        for (int iHalfEdge = cell.halfEdges.Count - 1; iHalfEdge >= 0; iHalfEdge--)
        {
            HalfEdge halfEdge = cell.halfEdges[iHalfEdge];
            p1 = halfEdge.GetStartPoint();
            p2 = halfEdge.GetEndPoint();
            area += p1.x * p2.y;
            area -= p1.y * p2.x;
        }
        area /= 2;
        return area;
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(bounds.size.x, 1, bounds.size.x));

        if (newGraph != null && drawCells)
        {
            foreach (var node in newGraph.nodes)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawCube(new Vector3(node.point.x, 0, node.point.y), Vector3.one);

                if (node.edges.Count > 0)
                {
                    for (int i = 0; i < node.edges.Count; i++)
                    {
                        var edge = node.edges[i];

                        Gizmos.color = Color.red;
                        Gizmos.DrawLine(edge.startPoint,
                                        edge.endPoint);
                    }
                }
            }

            //if (graph && drawCells)
            //{
            //foreach (Voronoi.Cell cell in graph.cells)
            //{
            //    Gizmos.color = Color.black;
            //    Gizmos.DrawCube(new Vector3(cell.site.x, 0, cell.site.y), Vector3.one);

            //    if (cell.halfEdges.Count > 0)
            //    {
            //        for (int i = 0; i < cell.halfEdges.Count; i++)
            //        {
            //            HalfEdge halfEdge = cell.halfEdges[i];

            //            Gizmos.color = Color.red;
            //            Gizmos.DrawLine(halfEdge.GetStartPoint().ToVector3(),
            //                            halfEdge.GetEndPoint().ToVector3());
            //        }
            //    }
            //}

            //foreach (var edge in graph.edges)
            //{
            //    if (edge.rSite != null)
            //    {
            //        Gizmos.color = Color.yellow;
            //        Gizmos.DrawLine(edge.lSite.ToVector3(), edge.rSite.ToVector3());
            //        Gizmos.DrawSphere(edge.lSite.ToVector3(), 0.2f);

            //        Gizmos.color = Color.green;
            //        Gizmos.DrawSphere(edge.rSite.ToVector3(), 0.2f);
            //    }
            //}
        }
    }
}
