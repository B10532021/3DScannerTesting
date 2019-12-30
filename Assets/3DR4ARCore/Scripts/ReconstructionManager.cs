using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

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
        private bool threadSaving = false;

        //drawboundaries
        private bool startdrawing = false;

        public Dictionary<string, GameObject> gamesObjects = new Dictionary<string, GameObject>();
        public GameObject mesh;
        private GameObject insideBoundary;
        private string filePath;

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
            }
            else
            {
                threadOpDisable = true;
                //DrawBoundaries();
            }
        }

        public void SaveScan()
        {
            threadOpPause = true;
            threadOpSave = Application.persistentDataPath + "/" + DateTime.Now.ToString("yyMMdd_HHmm") + ".obj";
        }

        private void Update()
        {
            //exporting
            if (threadSaving)
            {
                if (plugin.CallStatic<bool>("IsSaveFinished"))
                {
                    Handheld.StopActivityIndicator();

                    bool success = plugin.CallStatic<bool>("IsSaveSuccessful");
                    if (success)
                    {
                        ObjReader.FilePath = threadOpSave.Replace(Application.persistentDataPath + "/", "");
                        SceneManager.LoadScene("ViewObj");
                    }
                    else
                    {
                        Utils.ShowAndroidToastMessage("儲存失敗");
                        arProvider.ResumeSession();
                        threadOpSave = null;
                        threadSaving = false;
                        SceneManager.LoadScene("Meshbuilder");
                    }
                }
                return;
            }

            if (startdrawing)
            {
                DrawBoundaries();
                GameObject.Find("SavingCanvas").GetComponentInChildren<Text>().text = null;
                startdrawing = false;
            }

            //meshing
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
                    }
                    else
                    {
                        plugin.CallStatic("OnEnd");
                    }
                }
            }

            //visualize mesh
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

            //request save mesh
            if (!string.IsNullOrEmpty(threadOpSave) && !threadOpPause)
            {
                foreach (Vizualisation vizualisation in vizualisations)
                {
                    vizualisation.OnMeshClear();
                }
                GameObject.Find("UI").SetActive(false);
                GameObject.Find("SavingCanvas").GetComponentInChildren<Text>().text = "儲存中...\n不要關閉應用程式";
                Handheld.StartActivityIndicator();

                plugin.CallStatic("Save", threadOpSave);
                threadSaving = true;
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
                gamesObjects.Clear();
                DestroyBoundaries();
                threadOpClean = false;
            }
            if (threadOpDisable)
            {
                plugin.CallStatic("SetActive", false);
                GameObject.Find("SavingCanvas").GetComponentInChildren<Text>().text = "計算空洞中...";
                startdrawing = true;
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
                        job.colors[i].a = 1.0f;
                    }
                }
                else
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
            Dictionary<Edge, int> edges = new Dictionary<Edge, int>();
            Dictionary<Vector3, int> allVertices = new Dictionary<Vector3, int>();

            foreach (var gameObject in gamesObjects)
            {
                if (gameObject.Value == null)
                    continue;

                Vector3[] verts = gameObject.Value.GetComponent<MeshFilter>().mesh.vertices;
                int[] inds = gameObject.Value.GetComponent<MeshFilter>().mesh.triangles;
                for (int i = 0; i < inds.Length; i += 3)
                {
                    Vector3[] triangleVert = new Vector3[3]; //這個triangle的三個點
                    for (int j = 0; j < 3; j++) //看此點是否已經在已出現點的陣列裡面
                    {
                        triangleVert[j] = verts[inds[i + j]];

                        if (!allVertices.ContainsKey(triangleVert[j]))
                        {
                            allVertices.Add(triangleVert[j], allVertices.Count);
                        }
                    }

                    if (edges.ContainsKey(new Edge(triangleVert[0], triangleVert[1], triangleVert[2]))) //查看這個邊是否已經出現過，出現過就count + 1, 未出現就初始化這條邊
                    {
                        edges[new Edge(triangleVert[0], triangleVert[1], triangleVert[2])] += 1;
                    }
                    else
                    {
                        edges.Add(new Edge(triangleVert[0], triangleVert[1], triangleVert[2]), 1);

                    }

                    if (edges.ContainsKey(new Edge(triangleVert[0], triangleVert[2], triangleVert[1])))
                    {
                        edges[new Edge(triangleVert[0], triangleVert[2], triangleVert[1])] += 1;
                    }
                    else
                    {
                        edges.Add(new Edge(triangleVert[0], triangleVert[2], triangleVert[1]), 1);
                    }

                    if (edges.ContainsKey(new Edge(triangleVert[1], triangleVert[2], triangleVert[0])))
                    {
                        edges[new Edge(triangleVert[1], triangleVert[2], triangleVert[0])] += 1;
                    }
                    else
                    {
                        edges.Add(new Edge(triangleVert[1], triangleVert[2], triangleVert[0]), 1);
                    }
                }

            }

            List<Edge> singleEdges = new List<Edge>();
            foreach (var edge in edges) //等所有的邊都檢查過一次後，把count只有出現一次的邊取出來，並取出這條邊的兩個點在點陣列中的位置，然後丟進indices(用來之後要在mesh裡面把它畫出來)
            {
                if (edge.Value == 1)
                {
                    singleEdges.Add(edge.Key);
                    //indices.Add(allVertices[edge.Key.v1]);
                    //indices.Add(allVertices[edge.Key.v2]);
                }
            }
            edges.Clear();

            List<List<Vector3>> Vertices = new List<List<Vector3>>();
            List<Vector3> vertices = new List<Vector3>();
            List<List<Vector3>> ThirdVertices = new List<List<Vector3>>();
            List<Vector3> thirdvertices = new List<Vector3>();

            vertices.Add(singleEdges[0].v1);
            vertices.Add(singleEdges[0].v2);
            thirdvertices.Add(singleEdges[0].v3);
            singleEdges.RemoveAt(0);
            int k = 0;
            while (singleEdges.Count != 0)
            {
                if (Vector3.Equals(singleEdges[k].v1, vertices.Last()))
                {
                    vertices.Add(singleEdges[k].v2);
                    thirdvertices.Add(singleEdges[k].v3);
                    singleEdges.RemoveAt(k);
                    k = 0;
                }
                else if (Vector3.Equals(singleEdges[k].v2, vertices.Last()))
                {
                    vertices.Add(singleEdges[k].v1);
                    thirdvertices.Add(singleEdges[k].v3);
                    singleEdges.RemoveAt(k);
                    k = 0;
                }
                else if (k == singleEdges.Count - 1)
                {
                    Vertices.Add(vertices.ToList());
                    vertices.Clear();
                    ThirdVertices.Add(thirdvertices.ToList());
                    thirdvertices.Clear();
                    vertices.Add(singleEdges[0].v1);
                    vertices.Add(singleEdges[0].v2);
                    thirdvertices.Add(singleEdges[0].v3);
                    singleEdges.RemoveAt(0);
                    k = 0;
                }
                else
                {
                    k += 1;
                }
            }

            Vertices.Add(vertices.ToList());
            vertices.Clear();
            ThirdVertices.Add(thirdvertices.ToList());
            thirdvertices.Clear();

            List<int> indices = new List<int>();
            for (int i = 0; i < Vertices.Count; i++)
            {
                if (!PointInsidePolygon(ThirdVertices[i], Vertices[i]))
                {
                    Mesh temp = new Mesh();
                    temp.vertices = Vertices[i].ToArray();
                    Vector3 center = temp.bounds.center;
                    allVertices.Add(center, allVertices.Count);
                    /*for (int j = 0; j < Vertices[i].Count - 1; j++)
                    {
                        indices.Add(allVertices[Vertices[i][j]]);
                        indices.Add(allVertices[center]);
                        indices.Add(allVertices[Vertices[i][j + 1]]);
                        indices.Add(allVertices[Vertices[i][j + 1]]);
                        indices.Add(allVertices[center]);
                        indices.Add(allVertices[Vertices[i][j]]);
                    }*/
                    float area = 0f;
                    List<int> tempIndices = new List<int>();
                    for (int j = 0; j < Vertices[i].Count - 1; j++)
                    {
                        tempIndices.Add(allVertices[Vertices[i][j]]);
                        tempIndices.Add(allVertices[center]);
                        tempIndices.Add(allVertices[Vertices[i][j + 1]]);
                        tempIndices.Add(allVertices[Vertices[i][j + 1]]);
                        tempIndices.Add(allVertices[center]);
                        tempIndices.Add(allVertices[Vertices[i][j]]);
                        area += Vector3.Cross(Vertices[i][j]-center, Vertices[i][j+1]-center).magnitude * 0.5f;
                    }
                    if(area >= 0.01)
                    {
                        indices.AddRange(tempIndices);
                    }
                    else
                    {
                        allVertices.Remove(center);
                    }
                }
            }

            Vector3[] vertss = new Vector3[allVertices.Keys.Count];
            allVertices.Keys.CopyTo(vertss, 0);

            List<Color> colors = new List<Color>();
            for (int i = 0; i < allVertices.Count; i++)
            {
                colors.Add(Color.red);
            }

            // 畫mesh
            insideBoundary = Instantiate(mesh);
            insideBoundary.SetActive(true);
            insideBoundary.GetComponent<MeshFilter>().mesh.vertices = vertss;
            insideBoundary.GetComponent<MeshFilter>().mesh.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0, false);
            insideBoundary.GetComponent<MeshFilter>().mesh.colors = colors.ToArray();
        }

        public void DestroyBoundaries()
        {
            Destroy(insideBoundary);
        }

        public bool PointInsidePolygon(List<Vector3> points, List<Vector3> polygon)
        {
            if (polygon.Count < 3 || polygon[0] != polygon.Last())
            {
                return false;
            }

            int outside = 0;
            int inside = 0;
            Vector3 normal = new Vector3(); ;
            for (int i = 0; i < polygon.Count - 2; i++)
            {
                normal += Vector3.Cross(polygon[i] - polygon[i + 1], polygon[i + 2] - polygon[i + 1]);
            }
            normal = normal.normalized;
            List<Vector3> newPolygon = new List<Vector3>();
            foreach (var vertice in polygon)
            {
                newPolygon.Add(Vector3.ProjectOnPlane(vertice, normal));
            }

            for (int i = 0; i < points.Count; i++)
            {
                Vector3 projectedPoint = Vector3.ProjectOnPlane(points[i], normal);


                Vector3 originalOrientation = Orientation(polygon[i], polygon[i + 1], points[i]);
                Vector3 projectedOrientation = Orientation(newPolygon[i], newPolygon[i + 1], projectedPoint);

                Vector3 direction = (newPolygon[i] + newPolygon[i + 1]) / 2 - projectedPoint;
                Vector3 extreme = projectedPoint + direction * 100;
                int count = 0;

                for (int j = 0; j < newPolygon.Count - 1; j++)
                {
                    if (DoIntersect(newPolygon[j], newPolygon[j + 1], projectedPoint, extreme))
                    {
                        count += 1;
                    }
                }
                if ((count % 2 == 1 && Vector3.Dot(originalOrientation, projectedOrientation) > 0) || (count % 2 == 0 && Vector3.Dot(originalOrientation, projectedOrientation) < 0))
                {
                    inside += 1;
                }
                else
                {
                    outside += 1;
                }
            }


            if (inside > outside)
            {
                return true;
            }
            return false;
        }

        public bool DoIntersect(Vector3 edgeV1, Vector3 edgeV2, Vector3 p1, Vector3 p2)
        {
            // Find the four orientations needed for  
            // general and special cases 
            Vector3 o1 = Orientation(edgeV1, edgeV2, p1);
            Vector3 o2 = Orientation(edgeV1, edgeV2, p2);
            Vector3 o3 = Orientation(p1, p2, edgeV1);
            Vector3 o4 = Orientation(p1, p2, edgeV2);

            // General case 
            if (Vector3.Dot(o1, o2) < 0 && Vector3.Dot(o3, o4) < 0)
            {
                return true;
            }

            return false;
        }

        public Vector3 Orientation(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            return Vector3.Cross((v2 - v1), (v3 - v1));
        }

        public bool OnSegment(Vector3 p, Vector3 q, Vector3 r)
        {
            if (q.x <= Math.Max(p.x, r.x) &&
                q.x >= Math.Min(p.x, r.x) &&
                q.y <= Math.Max(p.y, r.y) &&
                q.y >= Math.Min(p.y, r.y))
            {
                return true;
            }
            return false;
        }

        public Vector3 GetMeshCenter()
        {
            return insideBoundary.GetComponent<MeshFilter>().mesh.bounds.center;
        }

        public struct Edge : IEquatable<Edge>
        {
            public Vector3 v1;
            public Vector3 v2;
            public Vector3 v3; //此三角形第三個點的位置
            private float ID;
            public Edge(Vector3 aV1, Vector3 aV2, Vector3 aV3)
            {
                v1 = aV1;
                v2 = aV2;
                v3 = aV3;
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
