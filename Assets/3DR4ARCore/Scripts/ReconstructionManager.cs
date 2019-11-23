using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace LVonasek
{
    struct MeshingJob
    {
        public string id;
        public Vector3[] vertices;
        public Vector3[] normals;
        public Color[] colors;
        public int[] indices;
    }

    public class ReconstructionManager : MonoBehaviour
    {
        [System.Serializable]
        public struct ScanSettings
        {
            public double resolutionMeter;
            public double nearFilterMeter;
            public double farFilterMeter;
            public bool colorizeMesh;
            public bool extendingPointCloud;
            public bool fillingHoles;
            public bool noiseFilter;

            public bool experimentalTexturing;
        }

        public ScanSettings ARCoreSettings;
        public ScanSettings AREngineSettings;
        public Vizualisation[] vizualisations;

        //native code access
        private ARProvider arProvider;
        private List<MeshingJob> jobs = new List<MeshingJob>();
        private static AndroidJavaObject plugin = null;
        private bool needUpdate = true;

        //cpu image access objects
        private RenderTexture cpuRTT;
        private Texture2D cpuTexture;
        private byte[] cpuPixels = { };
        private int cpuWidth = 90;
        private int cpuHeight = 160;
        private int cpuScale = 4;

        //thread flags
        private bool threadOpClean = false;
        private bool threadOpDisable = false;
        private bool threadOpEnable = false;
        private bool threadOpPause = false;
        private string threadOpSave = null;
        private bool threadRunning = false;

        public Dictionary<string, GameObject> gamesObjects = new Dictionary<string, GameObject>();
        public GameObject mesh;
        private GameObject go;
        private string filePath;
        private StreamWriter s;

        private void Start()
        {
            Screen.orientation = ScreenOrientation.Portrait;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            arProvider = FindObjectOfType<ARProvider>();

            filePath = Application.persistentDataPath + "/Log.txt";
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            s = new StreamWriter(filePath, true);
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause && (plugin != null))
            {
                plugin.CallStatic("Stop");
            }
        }

        public void ClearScanning()
        {
            threadOpClean = true;
        }

        public void EnableScanning(bool on)
        {
            if (on)
            {
                threadOpEnable = true;
                DestroyBoundaries();
            } else
            {
                threadOpDisable = true;
                DrawBoundaries();
            }
        }

        public void SaveScan()
        {
            threadOpPause = true;
            threadOpSave = Application.persistentDataPath + "/" + DateTime.Now.ToString("yyMMdd_HHmm") + ".obj";
        }

        private void Update()
        {
            ARState state = arProvider.GetState();

            if (!threadRunning && state.tracked)
            {
                ConfigurePlugin();
                if (plugin != null)
                {
                    int count = plugin.CallStatic<int>("OnBegin", state.sessionHandle, state.frameHandle);
                    if (count >= 0)
                    {
                        threadRunning = true;
                        ScanSettings scan = arProvider.GetSelectedAR() == ARSDK.ARENGINE ? AREngineSettings : ARCoreSettings;
                        if (scan.colorizeMesh)
                        {
                            arProvider.GetCamera().targetTexture = cpuRTT;
                            RenderTexture.active = cpuRTT;
                            Camera cam = arProvider.GetCamera();
                            int mask = cam.cullingMask;
                            cam.cullingMask = 0;
                            cam.Render();
                            cam.cullingMask = mask;
                            cpuTexture.ReadPixels(new Rect(0, 0, cpuWidth, cpuHeight), 0, 0);
                            RenderTexture.active = null;
                            arProvider.GetCamera().targetTexture = null;
                            cpuPixels = cpuTexture.GetRawTextureData();
                            plugin.CallStatic("OnPixels", cpuWidth, cpuHeight, cpuPixels, cpuScale);
                        }
                        plugin.CallStatic("OnProcess");
                        GenerateJobs(count);
                    } else
                    {
                        plugin.CallStatic("OnEnd");
                    }
                }
            }

            if (!string.IsNullOrEmpty(threadOpSave) && !threadOpPause)
            {
                bool success = plugin.CallStatic<bool>("Save", threadOpSave);
                arProvider.ResumeSession();
                Utils.ShowAndroidToastMessage(success ? threadOpSave + " was saved successfully." : "Saving failed!");
                threadOpSave = null;
            }

            DateTime t = DateTime.UtcNow;
            if (threadRunning)
            {
                while (true)
                {
                    if ((DateTime.UtcNow - t).Milliseconds > 33)
                    {
                        break;
                    }
                    else if (jobs.Count > 0)
                    {
                        MeshingJob job = jobs[jobs.Count - 1];
                        foreach (Vizualisation vizualisation in vizualisations)
                        {
                           GameObject gameObject = vizualisation.OnMeshUpdate(job.id, job.vertices, job.normals, job.colors, job.indices);
                           if (!gamesObjects.ContainsKey(job.id) && gameObject != null)
                           {
                                gamesObjects.Add(job.id, gameObject);
                                
                           }
                           s.Write("job-id:" + job.id + ", indice count:" + job.indices.Length + "\n");
                        }
                        jobs.RemoveAt(jobs.Count - 1);
                    }
                    else
                    {
                        Finish();
                        break;
                    }
                }
            }
        }

        private void ConfigurePlugin()
        {
            if (cpuRTT == null)
            {
                cpuTexture = new Texture2D(cpuWidth, cpuHeight, TextureFormat.RGB24, false);
                cpuPixels = cpuTexture.GetRawTextureData();
                cpuRTT = new RenderTexture(cpuWidth, cpuHeight, 24);
            }
            if (plugin == null)
            {
                plugin = new AndroidJavaObject("com.lvonasek.liboc.JNI");
                if (plugin != null)
                {
                    ScanSettings scan = arProvider.GetSelectedAR() == ARSDK.ARENGINE ? AREngineSettings : ARCoreSettings;
                    string dataset = scan.experimentalTexturing ? Application.persistentDataPath + "/dataset/" : "";
                    if (!string.IsNullOrEmpty(dataset))
                    {
                        Utils.DeleteDirectory(dataset);
                        Directory.CreateDirectory(dataset);
                    }
                    plugin.CallStatic("OnARServiceConnected", dataset, arProvider.GetMode());
                    plugin.CallStatic("OnSurfaceChanged", Screen.width, Screen.height);
                }
            }
            if (plugin != null)
            {
                if (needUpdate)
                {
                    ScanSettings scan = arProvider.GetSelectedAR() == ARSDK.ARENGINE ? AREngineSettings : ARCoreSettings;
                    plugin.CallStatic("SetActive", true);
                    plugin.CallStatic("SetExtendingPointCloud", scan.extendingPointCloud);
                    plugin.CallStatic("SetFillingHoles", scan.fillingHoles);
                    plugin.CallStatic("SetMeshing", scan.resolutionMeter, scan.nearFilterMeter, scan.farFilterMeter, scan.noiseFilter);
                    needUpdate = false;
                }
            }
        }

        private void Finish()
        {
            plugin.CallStatic("OnEnd");

            //user operations
            if (threadOpClean)
            {
                plugin.CallStatic("Clear");
                foreach (Vizualisation vizualisation in vizualisations)
                {
                    vizualisation.OnMeshClear();
                }
                threadOpClean = false;
            }
            if (threadOpDisable)
            {
                plugin.CallStatic("SetActive", false);
                threadOpDisable = false;
            }
            if (threadOpEnable)
            {
                plugin.CallStatic("SetActive", true);
                threadOpEnable = false;
            }

            if (threadOpPause)
            {
                arProvider.PauseSession();
                threadOpPause = false;
            }

            threadRunning = false;
        }

        private void GenerateJobs(int count)
        {
            //unpack the geometry from plugin
            jobs.Clear();
            for (int index = 0; index < count; index++)
            {
                MeshingJob job = new MeshingJob
                {
                    id = plugin.CallStatic<string>("OnDataIndex", index)
                };

                //extract vertices
                float[] vertices = plugin.CallStatic<float[]>("OnDataGeomVertex");
                float[] normals = plugin.CallStatic<float[]>("OnDataGeomNormal");
                job.vertices = new Vector3[vertices.Length / 3];
                job.normals = new Vector3[normals.Length / 3];
                for (int i = 0; i < vertices.Length / 3; i++)
                {
                    job.vertices[i].x = -vertices[i * 3 + 0];
                    job.vertices[i].y = vertices[i * 3 + 1];
                    job.vertices[i].z = vertices[i * 3 + 2];
                    job.normals[i].x = -normals[i * 3 + 0];
                    job.normals[i].y = normals[i * 3 + 1];
                    job.normals[i].z = normals[i * 3 + 2];
                }

                //extract colors
                ScanSettings scan = arProvider.GetSelectedAR() == ARSDK.ARENGINE ? AREngineSettings : ARCoreSettings;
                if (scan.colorizeMesh)
                {
                    int[] colors = plugin.CallStatic<int[]>("OnDataGeomColor");
                    job.colors = new Color[colors.Length / 3];
                    for (int i = 0; i < colors.Length / 3; i++)
                    {
                        job.colors[i].r = colors[i * 3 + 0] / 255.0f;
                        job.colors[i].g = colors[i * 3 + 1] / 255.0f;
                        job.colors[i].b = colors[i * 3 + 2] / 255.0f;
                        job.colors[i].a = 0.5f;
                        //job.colors[i].a = 1.0f;
                    }
                } else
                {
                    job.colors = null;
                }

                //extract indices
                int[] indices = plugin.CallStatic<int[]>("OnDataGeomFace");
                job.indices = new int[indices.Length];
                for (int i = 0; i < indices.Length / 3; i++)
                {
                    job.indices[i * 3 + 0] = indices[i * 3 + 1];
                    job.indices[i * 3 + 1] = indices[i * 3 + 0];
                    job.indices[i * 3 + 2] = indices[i * 3 + 2];
                }
                jobs.Add(job);
            }
        }

        public void DrawBoundaries()
        {
            List<int> indices = new List<int>();
            Dictionary<Edge, int> edges = new Dictionary<Edge, int>();
            Dictionary<Vector3, int> vertices = new Dictionary<Vector3, int>();

            foreach (var gameObject in gamesObjects)
            {
                if (gameObject.Value.GetComponent<MeshFilter>().mesh.triangles == null || gameObject.Value.GetComponent<MeshFilter>().mesh.triangles.Length <= 3 || gameObject.Value == null)
                    continue;

                Vector3[] verts = gameObject.Value.GetComponent<MeshFilter>().mesh.vertices;
                int[] inds = gameObject.Value.GetComponent<MeshFilter>().mesh.triangles;
                for (int i = 0; i < inds.Length; i += 3)
                {
                    Vector3[] triangleVert = new Vector3[3]; //這個triangle的三個點
                    for (int j = 0; j < 3; j++) //看此點是否已經在已出現點的陣列裡面
                    {
                        triangleVert[j] = verts[inds[i + j]];

                        if(!vertices.ContainsKey(triangleVert[j]))
                        {
                            vertices.Add(triangleVert[j], vertices.Count);
                        }
                    }

                    if (edges.ContainsKey(new Edge(triangleVert[0], triangleVert[1]))) //查看這個邊是否已經出現過，出現過就count + 1, 未出現就初始化這條邊
                    {
                        edges[new Edge(triangleVert[0], triangleVert[1])] += 1;
                    }
                    else
                    {
                        edges.Add(new Edge(triangleVert[0], triangleVert[1]), 1);
                    }

                    if (edges.ContainsKey(new Edge(triangleVert[0], triangleVert[2])))
                    {
                        edges[new Edge(triangleVert[0], triangleVert[2])] += 1;
                    }
                    else
                    {
                        edges.Add(new Edge(triangleVert[0], triangleVert[2]), 1);
                    }

                    if (edges.ContainsKey(new Edge(triangleVert[1], triangleVert[2])))
                    {
                        edges[new Edge(triangleVert[1], triangleVert[2])] += 1;
                    }
                    else
                    {
                        edges.Add(new Edge(triangleVert[1], triangleVert[2]), 1);
                    }
                }

            }

            foreach (var edge in edges) //等所有的邊都檢查過一次後，把count只有出現一次的邊取出來，並取出這條邊的兩個點在點陣列中的位置，然後丟進indices(用來之後要在mesh裡面把它畫出來)
            {
                if(edge.Value == 1)
                {
                    //indices.Add(vertices.FindIndex(x => Vector3.Equals(x, edge.Key.v1)));
                    //indices.Add(vertices.FindIndex(x => Vector3.Equals(x, edge.Key.v2)));
                    indices.Add(vertices[edge.Key.v1]);
                    indices.Add(vertices[edge.Key.v2]);
                }
            }

            Vector3[] vertss = new Vector3[vertices.Keys.Count];
            vertices.Keys.CopyTo(vertss, 0);

            // 畫mesh
            go = Instantiate(mesh);
            go.SetActive(true);
            //go.GetComponent<MeshFilter>().mesh.vertices = vertices.ToArray();
            go.GetComponent<MeshFilter>().mesh.vertices = vertss;
            go.GetComponent<MeshFilter>().mesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0, false);
            List<Color> colors = new List<Color>();
            for(int i = 0; i < vertices.Count; i++)
            {
                colors.Add(Color.red);
            }
            go.GetComponent<MeshFilter>().mesh.colors = colors.ToArray();
            go.transform.position +=  Vector3.up * 0.1f;
        }

        public void DestroyBoundaries()
        {
            Destroy(go);
        }

        public struct Edge:IEquatable<Edge>
        {
            public Vector3 v1;
            public Vector3 v2;
            private float ID;
            public Edge(Vector3 aV1, Vector3 aV2){
                v1 = aV1;
                v2 = aV2;
                ID = (v1 + v2).magnitude + (v1 - v2).magnitude;
            }
            public bool Equals(Edge other)
            {
                if ((Vector3.Equals(this.v1, other.v1) && Vector3.Equals(this.v2, other.v2)) || (Vector3.Equals(this.v1, other.v2) && Vector3.Equals(this.v2, other.v1)))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public override int GetHashCode()
            {
                return ID.GetHashCode();
            }
        }
        /*public struct Edge : IEquatable<Edge>
        {
            public Vector3 v1;
            public Vector3 v2;
            public Edge(Vector3 aV1, Vector3 aV2)
            {
                v1 = aV1;
                v2 = aV2;
            }
            public override bool Equals(object obj)
            {
                if (obj == null)
                    return false;

                return Equals((Edge)obj);
            }
            public bool Equals(Edge other)
            {
                if ((Equals(v1, other.v1) && Equals(v2, other.v2)) || (Equals(v1, other.v2) && Equals(v2, other.v1)))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }*/
    }
}
