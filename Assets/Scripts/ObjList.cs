using System.IO;
using UnityEngine;
using LVonasek;

public class ObjList : MonoBehaviour
{
    public GameObject buttonTemplate;

    public GameObject blankTemplate;
    void Start()
    {
        string rootPath = Application.persistentDataPath + "/";
        DirectoryInfo dataDir = new DirectoryInfo(rootPath);
        try
        {
            FileInfo[] fileInfos = dataDir.GetFiles();
            
            foreach (var fileInfo in fileInfos)
            {
                string name = fileInfo.Name;
                string ext = Path.GetExtension(name);
                if (ext == ".obj")
                {
                    GameObject go = Instantiate(buttonTemplate) as GameObject;
                    go.SetActive(true);
                    var button = go.GetComponent<ObjButton>();
                    button.SetName(name);
                    go.transform.SetParent(buttonTemplate.transform.parent);
                }
            }
            var blank = Instantiate(blankTemplate) as GameObject;
            blank.SetActive(true);
            blank.transform.SetParent(blankTemplate.transform.parent);
        }
        catch (System.Exception e)
        {
            Utils.ShowAndroidToastMessage(e.ToString());
        }
    }

    public void ButtonClicked(string str)
    {
        ObjReader.FilePath = str;
    }
}
