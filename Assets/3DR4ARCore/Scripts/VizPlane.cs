using System.Collections.Generic;
using UnityEngine;

namespace LVonasek
{
    public class VizPlane : Vizualisation
    {
        public float limitCeiling = -0.75f;
        public float limitFloor = 0.75f;

        public GameObject meshCelling;
        public GameObject meshFloor;
        public GameObject meshWall;

        private Dictionary<string, GameObject> reconstructionCeiling = new Dictionary<string, GameObject>();
        private Dictionary<string, GameObject> reconstructionFloor = new Dictionary<string, GameObject>();
        private Dictionary<string, GameObject> reconstructionWall = new Dictionary<string, GameObject>();

        public void Start()
        {
            Init(meshCelling);
            Init(meshFloor);
            Init(meshWall);
        }

        public override void OnMeshClear()
        {
            ClearGrid(reconstructionCeiling);
            ClearGrid(reconstructionFloor);
            ClearGrid(reconstructionWall);
        }

        public override GameObject OnMeshUpdate(string id, Vector3[] vertices, Vector3[] normals, Color[] colors, int[] indices)
        {
            //update grid
            Mesh dataCelling = UpdateGrid(reconstructionCeiling, id, meshCelling);
            Mesh dataFloor = UpdateGrid(reconstructionFloor, id, meshFloor);
            Mesh dataWall = UpdateGrid(reconstructionWall, id, meshWall);

            //make ceilings and floors more flat
            MakeFlatFlat(vertices, normals, indices);

            //create geometry
            List<Vector3> verticesCeiling = new List<Vector3>();
            List<Vector3> verticesFloor = new List<Vector3>();
            List<Vector3> verticesWall = new List<Vector3>();
            for (int i = 0; i < indices.Length; i += 3)
            {
                Vector3 a = vertices[indices[i + 0]];
                Vector3 b = vertices[indices[i + 1]];
                Vector3 c = vertices[indices[i + 2]];

                if (IsNormalUnderLimit(normals, indices, i, limitFloor))
                {
                    if (IsNormalUnderLimit(normals, indices, i, limitCeiling))
                    {
                        AddTriangle(verticesCeiling, a, b, c);
                        AddExtrude(verticesWall, a, b, c, Vector3.up * 0.1f);
                    } else
                    {
                        AddTriangle(verticesWall, a, b, c);
                        if (Camera.main.transform.position.y > (a.y + b.y + c.y) / 3.0f)
                        {
                            AddExtrude(verticesWall, a, b, c, Vector3.down * 2.0f);
                        }
                    }
                } else
                {
                    float down = meshWall == null ? 2.0f : 0.1f;
                    AddTriangle(verticesFloor, a, b, c);
                    AddExtrude(verticesWall, a, b, c, Vector3.down * down);
                }
            }

            //put geometry to the mesh
            ApplyGeometry(dataCelling, verticesCeiling);
            ApplyGeometry(dataFloor, verticesFloor);
            ApplyGeometry(dataWall, verticesWall);
            return null;
        }

        private void MakeFlatFlat(Vector3[] vertices, Vector3[] normals, int[] indices)
        {
            for (int i = 0; i < indices.Length; i += 3)
            {
                bool doit = false;
                if (!IsNormalOverLimit(normals, indices, i, limitCeiling))
                {
                    doit = true;
                }
                else if (!IsNormalUnderLimit(normals, indices, i, limitFloor))
                {
                    doit = true;
                }
                if (doit)
                {
                    Vector3 a = vertices[indices[i + 0]];
                    Vector3 b = vertices[indices[i + 1]];
                    Vector3 c = vertices[indices[i + 2]];
                    float y = (a.y + b.y + c.y) / 3.0f;
                    vertices[indices[i + 0]].y = y;
                    vertices[indices[i + 1]].y = y;
                    vertices[indices[i + 2]].y = y;
                }
            }
        }
    }
}