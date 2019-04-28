using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using QRCoder;

using UnityEngine;

namespace Capstones.UnityFramework
{
    public static class Screenshot
    {

        const String screenShotFileName = "share";
        const float qrSizeFactor = 0.2f;
        const int minTargetQRWidth = 177;

        public static string Capture()
        {
            //yield return new WaitForEndOfFrame();
            String path = null;
            const String screenShotFileName = "share";
            try
            {
                int width = Screen.width;
                int height = Screen.height;

                Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);

                tex.ReadPixels(new Rect(0, 0, width, height), 0, 0, true);
                tex.Apply();


                String dir = Application.persistentDataPath + "/" + "screenshot/";
                path = dir + screenShotFileName + ".png";

                var imagebytes = tex.EncodeToPNG();
                PlatExt.PlatDependant.CreateFolder(dir);
                PlatExt.PlatDependant.DeleteFile(path);
                using (var stream = PlatExt.PlatDependant.OpenWrite(path))
                {
                    stream.Write(imagebytes, 0, imagebytes.Length);
                }

                GameObject.DestroyImmediate(tex);
            }
            catch (System.Exception e)
            {
                if (GLog.IsLogInfoEnabled) GLog.LogInfo("ScreenCaptrueError:" + e);
            }


            return path;
        }

        public static string CaptureWithQR(String qrcodePath)
        {
            if (qrcodePath == null || qrcodePath.Equals(""))
            {
                return null;
            }

            String path = null;
            try
            {
                int width = Screen.width;
                int height = Screen.height;

                Texture2D texMain = new Texture2D(width, height, TextureFormat.RGB24, false);

                texMain.ReadPixels(new Rect(0, 0, width, height), 0, 0, true);

                texMain.Apply();

                Texture2D texQR = ResManager.LoadRes(qrcodePath, typeof(Texture2D)) as Texture2D;

                int smallerEdge = width > height ? height : width;
                int targetQRWidth = (Mathf.FloorToInt(smallerEdge * qrSizeFactor)/49)*49;
                if (targetQRWidth < texQR.width)
                {
                    if (targetQRWidth < minTargetQRWidth)
                    {
                        targetQRWidth = minTargetQRWidth;
                    }

                    float qrScaleFactor = (float)targetQRWidth/texQR.width;
                    texQR = ScaleTextureBilinear(texQR, qrScaleFactor);
                }
                else
                {
                    targetQRWidth = texQR.width;
                }

                int qrX = 0;
                int qrY = 0;
                Color[] qrColorArr = texQR.GetPixels(0,0,targetQRWidth,targetQRWidth);
                texMain.SetPixels(qrX, qrY,targetQRWidth,targetQRWidth, qrColorArr);

                String dir = Application.persistentDataPath + "/" + "screenshot/";
                path = dir + screenShotFileName + ".png";

                var imagebytes = texMain.EncodeToPNG();
                PlatExt.PlatDependant.CreateFolder(dir);
                PlatExt.PlatDependant.DeleteFile(path);
                using (var stream = PlatExt.PlatDependant.OpenWrite(path))
                {
                    stream.Write(imagebytes, 0, imagebytes.Length);
                }


                GameObject.DestroyImmediate(texMain);
                GameObject.DestroyImmediate(texQR);
            }
            catch (System.Exception e)
            {
                if (GLog.IsLogInfoEnabled) GLog.LogInfo("ScreenCaptrueError:" + e);
            }

            return path;
        }

        public static string CaptureWithQRText(string text)
        {
            if (text == null ||text.Equals(""))
            {
                return null;
            }

            String path = null;
            try
            {
                int width = Screen.width;
                int height = Screen.height;

                Texture2D texMain = new Texture2D(width, height, TextureFormat.RGB24, false);

                texMain.ReadPixels(new Rect(0, 0, width, height), 0, 0, true);

                texMain.Apply();

                int smallerEdge = width > height ? height : width;
                int expectQRWidth = Mathf.FloorToInt(smallerEdge * qrSizeFactor);
                if (expectQRWidth < minTargetQRWidth)
                {
                    expectQRWidth = minTargetQRWidth;
                }
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
                Texture2D texQR = new UnityQRCode(qrCodeData).GetGraphic(Mathf.CeilToInt(expectQRWidth /qrCodeData.ModuleMatrix.Count));

                int texQRWidth = texQR.width;

                int qrX = 0;
                int qrY = 0;
                Color[] qrColorArr = texQR.GetPixels(0, 0, texQRWidth, texQRWidth);
                texMain.SetPixels(qrX, qrY, texQRWidth, texQRWidth, qrColorArr);

                String dir = Application.persistentDataPath + "/" + "screenshot/";
                path = dir + screenShotFileName + ".png";

                var imagebytes = texMain.EncodeToPNG();
                PlatExt.PlatDependant.CreateFolder(dir);
                PlatExt.PlatDependant.DeleteFile(path);
                using (var stream = PlatExt.PlatDependant.OpenWrite(path))
                {
                    stream.Write(imagebytes, 0, imagebytes.Length);
                }


                GameObject.DestroyImmediate(texMain);
                GameObject.DestroyImmediate(texQR);
            }
            catch (System.Exception e)
            {
                if (GLog.IsLogInfoEnabled) GLog.LogInfo("ScreenCaptrueError:" + e);
            }

            return path;
        }

        private static Texture2D ScaleTextureBilinear(Texture2D originalTexture, float scaleFactor)
        {
            int width = Mathf.CeilToInt(originalTexture.width * scaleFactor);
            int height =  Mathf.CeilToInt(originalTexture.height * scaleFactor);
            Texture2D newTexture = new Texture2D(width,height);
            float scale = 1.0f / scaleFactor;
            int maxX = originalTexture.width - 1;
            int maxY = originalTexture.height - 1;

            Color[] originalColorArr = originalTexture.GetPixels();
            Color[] newColorArr = new Color[newTexture.width * newTexture.height];
            for (int y = 0; y < newTexture.height; y++)
            {
                for (int x = 0; x < newTexture.width; x++)
                {
                    // Bilinear Interpolation
                    float targetX = (x+0.5f) * scale - 0.5f;
                    float targetY = (y+0.5f) * scale - 0.5f;
                    int x1 = Mathf.Min(maxX, Mathf.FloorToInt(targetX));
                    int y1 = Mathf.Min(maxY, Mathf.FloorToInt(targetY));
                    int x2 = Mathf.Min(maxX, x1 + 1);
                    int y2 = Mathf.Min(maxY, y1 + 1);

                    float u = targetX - x1;
                    float v = targetY - y1;
                    float w1 = (1 - u) * (1 - v);
                    float w2 = u * (1 - v);
                    float w3 = (1 - u) * v;
                    float w4 = u * v;
                    Color color1 = originalColorArr[x1 + y1 * originalTexture.width];
                    Color color2 = originalColorArr[x2 + y1 * originalTexture.width];
                    Color color3 = originalColorArr[x1 + y2 * originalTexture.width];
                    Color color4 = originalColorArr[x2 + y2 * originalTexture.width];
                    Color color = new Color(Mathf.Clamp01(color1.r * w1 + color2.r * w2 + color3.r * w3 + color4.r * w4),
                        Mathf.Clamp01(color1.g * w1 + color2.g * w2 + color3.g * w3 + color4.g * w4),
                        Mathf.Clamp01(color1.b * w1 + color2.b * w2 + color3.b * w3 + color4.b * w4),
                        Mathf.Clamp01(color1.a * w1 + color2.a * w2 + color3.a * w3 + color4.a * w4)
                        );

                    newColorArr[x + y * newTexture.width] = color;
                }
            }

            newTexture.SetPixels(newColorArr);
            return newTexture;
        }

    }

}
