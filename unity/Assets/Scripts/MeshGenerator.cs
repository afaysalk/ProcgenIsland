using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using CoastlinesGen;

public class MeshGenerator
{
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh mesh;
    private int maxMaterials;
    private Heights heightMap;
    private Gradient terrainColors;
    private Gradient waterColors;
    Vector3 position;
    private List<Vector3> vertices;
    private Dictionary<Vector3, int> verticesHash;
    private List<int> triangles;
    private List<Vector3> allVertices;
    private Dictionary<int, List<int>> allTriangs;
    private Graph graph;
    private float waterLevel;
    float maxHeight;

    public MeshGenerator(float maxHeight, int maxMaterials, Heights heightMap, Gradient terrainColors, Gradient waterColors, Vector3 position, Graph graph, MeshFilter meshFilter, MeshRenderer meshRenderer, float waterLevel)
    {
        this.waterLevel = waterLevel;
        this.maxHeight = maxHeight;
        this.maxMaterials = maxMaterials;
        this.heightMap = heightMap;
        this.terrainColors = terrainColors;
        this.waterColors = waterColors;
        this.position = position;
        this.graph = graph;
        this.meshFilter = meshFilter;
        this.meshRenderer = meshRenderer;
        mesh = meshFilter.mesh;

    }

    internal void Create()
    {
        allVertices = new List<Vector3>();
        allTriangs = new Dictionary<int, List<int>>();
        verticesHash = new Dictionary<Vector3, int>();

        vertices = new List<Vector3>();
        triangles = new List<int>();

        foreach (var node in graph.nodes)
        {
            if (CreateCellMesh(node))
                CopySubmesh(node);

            vertices.Clear();
            triangles.Clear();
        }

        SendMesh();
        CreateMaterials();
    }

    private void CopySubmesh(Node node)
    {

        foreach (var vertex in vertices)
        {
            if (!verticesHash.ContainsKey(vertex))
            {
                verticesHash[vertex] = allVertices.Count;
                allVertices.Add(vertex);
            }
        }
        var submesh = (int)Mathf.Min(Mathf.RoundToInt(heightMap.Height(node.point) * maxMaterials), maxMaterials - 1);
        if (!allTriangs.ContainsKey(submesh))
            allTriangs[submesh] = new List<int>();
        foreach (var index in triangles)
        {
            var i = verticesHash[vertices[index]];
            allTriangs[submesh].Add(i);
        }
    }

    private bool CreateCellMesh(Node node)
    {
        if (node.edges.Count == 0) return false;

        vertices.Add(EdgePosition(node.point));
        triangles.Add(0);
        var lastV = node.edges.Count;
        for (int v = 1; v <= lastV; v++)
        {
            vertices.Add(EdgePosition(node.edges[v - 1].startPoint));

            triangles.Add(v);
            if (v != lastV)
            {
                triangles.Add(v + 1);
                triangles.Add(0);
            }
        }
        triangles.Add(1);

        return true;
    }

    Dictionary<Vector3, HashSet<Vector3>> boundaryNearSites;
    Dictionary<Vector3, float> edgeHeights;
    private Vector3 EdgePosition(Vector3 site)
    {
        var pos = site - position;
        var h = heightMap.Height(site);
        pos.y = h * maxHeight;

        return pos;
    }

    private void SendMesh()
    {
        var triangCount = allTriangs.Values.Sum(values => values.Count);
        Debug.Log("mesh vertices: " + allVertices.Count + ", triangles:" + triangCount);

        mesh.Clear();
        mesh.vertices = allVertices.ToArray();

        mesh.subMeshCount = maxMaterials;
        var keys = allTriangs.Keys.ToArray<int>();
        System.Array.Sort<int>(keys);
        foreach (var index in keys)
        {
            mesh.SetIndices(allTriangs[index].ToArray(), MeshTopology.Triangles, index);
        }

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

    private void CreateMaterials()
    {
        var materials = new List<Material>();
        var heightMaterial = Resources.Load<Material>("Height");
        while (materials.Count < maxMaterials)
            materials.Add(heightMaterial);
        meshRenderer.materials = materials.ToArray();

        var colorIndex = 0f;
        foreach (var m in meshRenderer.materials)
        {
            var gradient = (colorIndex < waterLevel ? waterColors : terrainColors);
            m.color = gradient.Evaluate(colorIndex);
            colorIndex += 1f / maxMaterials;
        }
    }
}
