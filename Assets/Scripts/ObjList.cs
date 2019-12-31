using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
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
                    if(File.Exists(Application.persistentDataPath + "/" + name.Replace("obj", "png")))
                    {
                        go.transform.GetChild(1).GetComponent<Image>().sprite = LoadNewSprite(Application.persistentDataPath + "/" + name.Replace("obj", "png"));
                    }  
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

    public static Sprite LoadNewSprite(string FilePath, float PixelsPerUnit = 100.0f, SpriteMeshType spriteType = SpriteMeshType.Tight)
    {

        // Load a PNG or JPG image from disk to a Texture2D, assign this texture to a new sprite and return its reference

        Texture2D SpriteTexture = LoadTexture(FilePath);
        Sprite NewSprite = Sprite.Create(SpriteTexture, new Rect(0, 760, SpriteTexture.width, SpriteTexture.width), new Vector2(0, 0), PixelsPerUnit, 0, spriteType);

        return NewSprite;
    }

    public static Texture2D LoadTexture(string FilePath)
    {

        // Load a PNG or JPG file from disk to a Texture2D
        // Returns null if load fails

        Texture2D Tex2D;
        byte[] FileData;

        if (File.Exists(FilePath))
        {
            FileData = File.ReadAllBytes(FilePath);
            Tex2D = new Texture2D(2, 2);           // Create new "empty" texture
            if (Tex2D.LoadImage(FileData))           // Load the imagedata into the texture (size is set automatically)
                return Tex2D;                 // If data = readable -> return texture
        }
        return null;                     // Return null if load failed
    }
}
