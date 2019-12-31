using System.IO;
using UnityEngine;
using AsImpL;
using LVonasek;
using UnityEngine.UI;

public class ObjReader : MonoBehaviour
{
    public static string FilePath { get; set; }
    [SerializeField]
    private string objectName = "MyObject";
    [SerializeField]
    private ImportOptions importOptions = new ImportOptions();

    [SerializeField]
    private PathSettings pathSettings;

    private ObjectImporter objImporter;

    public Text loadingText;

    private string screenshotname = null;

    private void Awake()
    {
        screenshotname = FilePath;
        FilePath = pathSettings.RootPath + FilePath;
        if (File.Exists(FilePath))
        {
            objImporter = gameObject.GetComponent<ObjectImporter>();
            if (objImporter == null)
            {
                objImporter = gameObject.AddComponent<ObjectImporter>();
            }

            objImporter.ImportingStart += () => { Handheld.StartActivityIndicator(); };
            objImporter.ImportedModel += (baseModel, name) =>
            {
                var model = baseModel.transform.GetChild(0);
                var center = model.GetComponent<MeshFilter>().mesh.bounds.center;
                model.transform.position = -center;
                loadingText.enabled = false;
                Handheld.StopActivityIndicator();
                if (!File.Exists(Application.persistentDataPath + '/' + screenshotname.Replace("obj", "png")))
                {
                    ScreenCapture.CaptureScreenshot(screenshotname.Replace("obj", "png"));
                }
            };
            objImporter.ImportError += (err) => { Utils.ShowAndroidToastMessage(err); };
        }
        else
        {
            Utils.ShowAndroidToastMessage("path: '" + FilePath + "' not exist");
        }
    }

    private void Start()
    {
        objImporter.ImportModelAsync(objectName, FilePath, transform, importOptions);
    }

    private void OnValidate()
    {
        if (pathSettings == null)
        {
            pathSettings = PathSettings.FindPathComponent(gameObject);
        }
    }
}
