using System.Collections.Generic;
using UnityEngine;

namespace LVonasek
{
    public abstract class Vizualisation : MonoBehaviour
    {
        public abstract void OnMeshClear();

        public abstract GameObject OnMeshUpdate(string id, Vector3[] vertices, Vector3[] normals, Color[] colors, int[] indices);

        public void AddCube(List<Vector3> list, Vector3 min, Vector3 max, bool horizontal = true, bool vertical = true)
        {
            //points
            Vector3 v000 = new Vector3(min.x, min.y, min.z);
            Vector3 v001 = new Vector3(min.x, min.y, max.z);
            Vector3 v010 = new Vector3(min.x, max.y, min.z);
            Vector3 v011 = new Vector3(min.x, max.y, max.z);
            Vector3 v100 = new Vector3(max.x, min.y, min.z);
            Vector3 v101 = new Vector3(max.x, min.y, max.z);
            Vector3 v110 = new Vector3(max.x, max.y, min.z);
            Vector3 v111 = new Vector3(max.x, max.y, max.z);

            //planes XY
            if (vertical)
            {
                AddTriangle(list, v010, v100, v000);
                AddTriangle(list, v100, v010, v110);
                AddTriangle(list, v001, v101, v011);
                AddTriangle(list, v111, v011, v101);
            }

            //planes XZ
            if (horizontal)
            {
                AddTriangle(list, v000, v100, v001);
                AddTriangle(list, v101, v001, v100);
                AddTriangle(list, v011, v110, v010);
                AddTriangle(list, v110, v011, v111);
            }

            //planes YZ
            if (vertical)
            {
                AddTriangle(list, v001, v010, v000);
                AddTriangle(list, v010, v001, v011);
                AddTriangle(list, v100, v110, v101);
                AddTriangle(list, v111, v101, v110);
            }
        }

        public void AddExtrude(List<Vector3> list, Vector3 a, Vector3 b, Vector3 c, Vector3 extrude)
        {
            Vector3 ad = a + extrude;
            Vector3 bd = b + extrude;
            Vector3 cd = c + extrude;
            AddTriangle(list, b, a, ad);
            AddTriangle(list, ad, bd, b);
            AddTriangle(list, c, b, bd);
            AddTriangle(list, bd, cd, c);
            AddTriangle(list, a, c, cd);
            AddTriangle(list, cd, ad, a);
        }

        public void AddTriangle(List<Vector3> list, Vector3 a, Vector3 b, Vector3 c)
        {
            list.Add(a);
            list.Add(b);
            list.Add(c);
        }

        public void ApplyGeometry(Mesh data, List<Vector3> vertices)
        {
            if (data != null)
            {
                int[] indices = new int[vertices.Count];
                for (int i = 0; i < vertices.Count; i++)
                {
                    indices[i] = i;
                }
                data.Clear();
                data.vertices = vertices.ToArray();
                data.SetIndices(indices, MeshTopology.Triangles, 0);
                data.RecalculateNormals();
            }
        }

        public void ClearGrid(Dictionary<string, GameObject> reconstruction)
        {
            foreach (GameObject go in reconstruction.Values)
            {
                Destroy(go);
            }
            reconstruction.Clear();
        }

        public void Init(GameObject go)
        {
            if (go != null)
            {
                go.SetActive(false);
            }
        }

        public bool IsNormalOverLimit(Vector3[] normals, int[] indices, int i, float limit)
        {
            bool under = true;
            if (normals[indices[i + 0]].y < limit) under = false;
            if (normals[indices[i + 1]].y < limit) under = false;
            if (normals[indices[i + 2]].y < limit) under = false;
            return under;
        }

        public bool IsNormalUnderLimit(Vector3[] normals, int[] indices, int i, float limit)
        {
            bool under = true;
            if (normals[indices[i + 0]].y > limit) under = false;
            if (normals[indices[i + 1]].y > limit) under = false;
            if (normals[indices[i + 2]].y > limit) under = false;
            return under;
        }

        public Mesh UpdateGrid(Dictionary<string, GameObject> reconstruction, string id, GameObject mesh)
        {
            if (mesh != null)
            {
                if (!reconstruction.ContainsKey(id))
                {
                    GameObject go = Instantiate(mesh);
                    go.SetActive(true);
                    reconstruction.Add(id, go);
                    Mesh m = go.GetComponent<MeshFilter>().mesh;
                    m.bounds = new Bounds(Vector3.zero, Vector3.one * 99999);
                }
                return reconstruction[id].GetComponent<MeshFilter>().mesh;
            }
            return null;
        }
    }
}