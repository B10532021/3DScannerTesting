using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawBoundary : MonoBehaviour
{
    private List<LineDrawer> lines;
    // Start is called before the first frame update
    void Start()
    {
        lines = new List<LineDrawer>();
    }


    public void DrawBoundaries(List<GameObject> gameObjects)
    {
        //List<Edge> edges = new List<Edge>();
        foreach (var gameObject in gameObjects)
        {
            if (gameObject.GetComponent<MeshFilter>().mesh.triangles == null || gameObject.GetComponent<MeshFilter>().mesh.triangles.Length <= 3 || gameObject == null)
                continue;

            Vector3[] vertices = gameObject.GetComponent<MeshFilter>().mesh.vertices;
            int[] indices = gameObject.GetComponent<MeshFilter>().mesh.triangles;
            for (int i = 0; i < indices.Length; i += 3)
            {
                Vector3 v1 = vertices[i];
                Vector3 v2 = vertices[i + 1];
                Vector3 v3 = vertices[i + 2];
                /*edges.Add(new Edge(v1, v2, gameobject.Key));
                edges.Add(new Edge(v2, v3, gameobject.Key));
                edges.Add(new Edge(v3, v1, gameobject.Key));*/
                LineDrawer lineDrawer1 = new LineDrawer();
                LineDrawer lineDrawer2 = new LineDrawer();
                LineDrawer lineDrawer3 = new LineDrawer();
                lineDrawer1.DrawLineInGameView(v1, v2, Color.red);
                lineDrawer2.DrawLineInGameView(v1, v3, Color.red);
                lineDrawer3.DrawLineInGameView(v2, v3, Color.red);
                lines.Add(lineDrawer1);
                lines.Add(lineDrawer2);
                lines.Add(lineDrawer3);
            }
        }
    }


    public struct Edge
    {
        public Vector3 v1;
        public Vector3 v2;
        public string triangleID;
        public Edge(Vector3 aV1, Vector3 aV2, string ID)
        {
            v1 = aV1;
            v2 = aV2;
            triangleID = ID;
        }
    }

    public struct LineDrawer
    {
        private LineRenderer lineRenderer;
        private float lineSize;

        public LineDrawer(float lineSize = 0.008f)
        {
            GameObject lineObj = new GameObject("LineObj");
            lineRenderer = lineObj.AddComponent<LineRenderer>();
            //Particles/Additive
            lineRenderer.material = new Material(Shader.Find("Hidden/Internal-Colored"));

            this.lineSize = lineSize;
        }

        private void init(float lineSize = 0.008f)
        {
            if (lineRenderer == null)
            {
                GameObject lineObj = new GameObject("LineObj");
                lineRenderer = lineObj.AddComponent<LineRenderer>();
                //Particles/Additive
                lineRenderer.material = new Material(Shader.Find("Hidden/Internal-Colored"));

                this.lineSize = lineSize;
            }
        }

        //Draws lines through the provided vertices
        public void DrawLineInGameView(Vector3 start, Vector3 end, Color color)
        {
            if (lineRenderer == null)
            {
                init(0.008f);
            }

            //Set color
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;

            //Set width
            lineRenderer.startWidth = lineSize;
            lineRenderer.endWidth = lineSize;

            //Set line count which is 2
            lineRenderer.positionCount = 2;

            //Set the postion of both two lines
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);
        }

        public void Destroy()
        {
            if (lineRenderer != null)
            {
                UnityEngine.Object.Destroy(lineRenderer.gameObject);
            }
        }
    }
}
