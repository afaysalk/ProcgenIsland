using UnityEngine;
using UnityEditor;
using System.Collections;

using CoastlinesGen;

[CustomEditor(typeof(Coastlines))]
public class CoastlinesEditor : Editor
{
    void OnSceneGUI()
    {
        Coastlines voronoi = (Coastlines)target;
        if (voronoi.newGraph == null || !voronoi.drawCells)
            return;

        //foreach (var node in voronoi.newGraph.nodes)
        //{
            //Handles.color = Color.black;
            //Handles.Label(node.point.pos + voronoi.transform.position, "Site " + node.point.pos.ToString());

            //if (node.edges.Count == 0)
                //continue;

            //int i = 0;
            //foreach (var edge in node.edges)
            //{
            //    if (edge.edge.va && halfEdge.edge.vb)
            //    {
            //        if (cell.site.id == 1)
            //        {
            //            Vector3 posA = halfEdge.edge.va.ToVector3();
            //            Vector3 posB = halfEdge.edge.vb.ToVector3();
            //            Handles.Label(posA + Vector3.up * i * 10, "Cell " + cell.site.id + " Edge " + i + " Vertex A " + posA);
            //            Handles.Label(posB + Vector3.up * i * 10, "Cell " + cell.site.id + " Edge " + i + " Vertex B " + posB);
            //            i++;
            //        }
            //    }
            //}
    //    }
    }

    //void OnSceneGUI()
    //{
    //Coastlines voronoi = (Coastlines)target;
    //if (!voronoi.graph)
    //    return;

    //foreach (Voronoi.Cell cell in voronoi.graph.cells)
    //{
    //Handles.color = Color.black;
    //Handles.Label(cell.site.ToVector3(), "Site " + cell.site.id);

    //if (cell.halfEdges.Count == 0)
    //continue;

    //int i = 0;
    //foreach (HalfEdge halfEdge in cell.halfEdges)
    //{
    //    if (halfEdge.edge.va && halfEdge.edge.vb)
    //    {
    //        if (cell.site.id == 1)
    //        {
    //            Vector3 posA = halfEdge.edge.va.ToVector3();
    //            Vector3 posB = halfEdge.edge.vb.ToVector3();
    //            Handles.Label(posA + Vector3.up * i * 10, "Cell " + cell.site.id + " Edge " + i + " Vertex A " + posA);
    //            Handles.Label(posB + Vector3.up * i * 10, "Cell " + cell.site.id + " Edge " + i + " Vertex B " + posB);
    //            i++;
    //        }
    //    }
    //}
    //}
    //}
}
