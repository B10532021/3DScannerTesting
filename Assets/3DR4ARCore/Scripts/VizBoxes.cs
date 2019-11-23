using System.Collections.Generic;
using UnityEngine;

namespace LVonasek
{
    public class VizBoxes : Vizualisation
    {
        public float cubeSize = 0.25f;
        public GameObject meshEarth;
        public GameObject meshGrass;

        private Dictionary<string, GameObject> reconstructionEarth = new Dictionary<string, GameObject>();
        private Dictionary<string, GameObject> reconstructionGrass = new Dictionary<string, GameObject>();

        public void Start()
        {
            Init(meshEarth);
            Init(meshGrass);
        }

        public override void OnMeshClear()
        {
            ClearGrid(reconstructionEarth);
            ClearGrid(reconstructionGrass);
        }

        public override GameObject OnMeshUpdate(string id, Vector3[] vertices, Vector3[] normals, Color[] colors, int[] indices)
        {
            Mesh earth = UpdateGrid(reconstructionEarth, id, meshEarth);
            Mesh grass = UpdateGrid(reconstructionGrass, id, meshGrass);

            //generate map of cubes
            Dictionary<string, Vector3> map = new Dictionary<string, Vector3>();
            foreach (Vector3 v in vertices)
            {
                int x = (int)(v.x / cubeSize);
                int y = (int)(v.y / cubeSize);
                int z = (int)(v.z / cubeSize);
                string key = x + ":" + y + ":" + z;
                if (!map.ContainsKey(key))
                {
                    float fx = x * cubeSize;
                    float fy = y * cubeSize;
                    float fz = z * cubeSize;
                    map[key] = new Vector3(fx, fy, fz);
                }
            }

            //generate cubes
            List<Vector3> geomEarth = new List<Vector3>();
            List<Vector3> geomGrass = new List<Vector3>();
            foreach (Vector3 v in map.Values)
            {
                AddCube(geomEarth, v, v + Vector3.one * cubeSize, false, true);
                AddCube(geomGrass, v, v + Vector3.one * cubeSize, true, false);
            }
            ApplyGeometry(earth, geomEarth);
            ApplyGeometry(grass, geomGrass);
            return null;
        }
    }
}