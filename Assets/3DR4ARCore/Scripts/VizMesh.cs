using System.Collections.Generic;
using UnityEngine;

namespace LVonasek
{
    public class VizMesh : Vizualisation
    {
        public GameObject mesh;
        private Dictionary<string, GameObject> reconstruction = new Dictionary<string, GameObject>();

        public void Start()
        {
            Init(mesh);
        }

        public override void OnMeshClear()
        {
            ClearGrid(reconstruction);
        }

        public override GameObject OnMeshUpdate(string id, Vector3[] vertices, Vector3[] normals, Color[] colors, int[] indices)
        {
            Mesh data = UpdateGrid(reconstruction, id, mesh);
            data.Clear(true);
            data.vertices = vertices;
            data.normals = normals;
            if (colors != null)
            {
                data.colors = colors;
            }
            data.SetIndices(indices, MeshTopology.Triangles, 0, false);

            return reconstruction[id];
        }
    }
}