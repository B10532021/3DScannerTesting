using System.IO;
using UnityEngine;
using AsImpL;
using LVonasek;

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


    private void Awake()
    {
        FilePath = pathSettings.RootPath + FilePath;
        if (File.Exists(FilePath))
        {
            objImporter = gameObject.GetComponent<ObjectImporter>();
            if (objImporter == null)
            {
                objImporter = gameObject.AddComponent<ObjectImporter>();
            }
        }
        else
        {
            Utils.ShowAndroidToastMessage("path: '" + FilePath + "' not exist");
        }
    }

    private void Start()
    {
        objImporter.ImportModelAsync(objectName, FilePath, null, importOptions);

        objImporter.ImportedModel += (gameObject, name) =>
        {
            var rotateModel = gameObject.AddComponent<RotateModel>();
            rotateModel.rotationSpeed = GetComponent<RotateModel>().rotationSpeed;
        };
    }

    private void OnValidate()
    {
        if (pathSettings == null)
        {
            pathSettings = PathSettings.FindPathComponent(gameObject);
        }
    }
}
