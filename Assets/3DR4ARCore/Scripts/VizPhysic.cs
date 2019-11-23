using UnityEngine;

namespace LVonasek
{
    public class VizPhysic : VizMesh
    {
        public override GameObject OnMeshUpdate(string id, Vector3[] vertices, Vector3[] normals, Color[] colors, int[] indices)
        {
            GameObject go = base.OnMeshUpdate(id, vertices, normals, colors, indices);
            go.GetComponent<MeshCollider>().sharedMesh = null;
            go.GetComponent<MeshCollider>().sharedMesh = go.GetComponent<MeshFilter>().mesh;
            return go;
        }
    }
}