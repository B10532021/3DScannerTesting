using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using UnityEngine;
using GoogleARCore;


public class GetFrameImage : MonoBehaviour
{
    public struct CameraImage
    {
        public byte[] y;
        public byte[] uv;
        public int width;
        public int height;
    }
    bool save;
    string filePath;
    // Start is called before the first frame update
    void Start()
    {
        save = true;
        filePath = Application.persistentDataPath;
        if (File.Exists(filePath + "/Log.txt"))
        {
            try
            {
                File.Delete(filePath + "/Log.txt");
                Debug.Log("file delete");
            }
            catch (System.Exception e)
            {
                Debug.LogError("cannot delete file");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(save)
        {
            CameraImage cameraImage = new CameraImage();
            if(GetCameraImage(ref cameraImage))
            {
                try
                {
                    Texture2D texUVchannels = new Texture2D(cameraImage.width, cameraImage.height, TextureFormat.RG16, false, false);
                    texUVchannels.LoadRawTextureData(cameraImage.uv);
                    texUVchannels.Apply();
                    var bytes = texUVchannels.EncodeToJPG();
                    Destroy(texUVchannels);
                    System.IO.File.WriteAllBytes(filePath + "/image.jpg", bytes);
                    save = false;
                }
                catch (SystemException e)
                {
                    StreamWriter s = new StreamWriter(filePath + "/Log.txt", true);
                    s.Write(e);
                    s.Close();
                }
                
            }
                
        }
        
    }

    public bool GetCameraImage(ref CameraImage cameraImage)
    {
        //Set LuminanceOnly to true if you need a Y component only (black and and white)
        const bool LuminanceOnly = false;

        using (var imageBytes = Frame.CameraImage.AcquireCameraImageBytes())
        {
            if (!imageBytes.IsAvailable)
            {
                //Y shoud be 1 byte per pixel. Not doing anything otherwise.
                return false;
            }

            cameraImage.width = imageBytes.Width;
            cameraImage.height = imageBytes.Height;

            if (imageBytes.YRowStride != imageBytes.Width)
            {
                //Y shoud be 1 byte per pixel. Not doing anything otherwise.
                return false;
            }

            //We expect 1 byte per pixel for Y
            int bufferSize = imageBytes.Width * imageBytes.Height;
            if (cameraImage.y == null || cameraImage.y.Length != bufferSize)
                cameraImage.y = new byte[bufferSize];

            //Y plane is copied as is.
            Marshal.Copy(imageBytes.Y, cameraImage.y, 0, bufferSize);


            if (LuminanceOnly || imageBytes.UVRowStride != imageBytes.Width || imageBytes.UVPixelStride != 2)
            {
                //Weird values. Y is probably enough.
                cameraImage.uv = null;
                return true;
            }

            //We expect 2 bytes per pixel, interleaved U/V, with 2x2 subsampling
            bufferSize = imageBytes.Width * imageBytes.Height / 2;
            if (cameraImage.uv == null || cameraImage.uv.Length != bufferSize)
                cameraImage.uv = new byte[bufferSize];

            //Because U an V planes are returned separately, while remote expects interleaved U/V
            //same as ARKit, we merge the buffers ourselves
            unsafe
            {
                fixed (byte* uvPtr = cameraImage.uv)
                {
                    byte* UV = uvPtr;

                    byte* U = (byte*)imageBytes.U.ToPointer();
                    byte* V = (byte*)imageBytes.V.ToPointer();

                    for (int i = 0; i < bufferSize; i += 2)
                    {
                        *UV++ = *U;
                        *UV++ = *V;

                        U += imageBytes.UVPixelStride;
                        V += imageBytes.UVPixelStride;
                    }
                }
            }
            return true;
        }
        

    }
}
