using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Image2Grayscale
{
    private static Material mat;

    private static readonly Texture overlay;
    public static Texture2D ConvertToGrayscale(Texture2D image)
    {
        Color32[] pixels = image.GetPixels32();
        for (int x = 0; x < image.width; x++)
        {
            for (int y = 0; y < image.height; y++)
            {
                Color32 pixel = pixels[x + y * image.width];
                int p = ((256 * 256 + pixel.r) * 256 + pixel.b) * 256 + pixel.g;
                int b = p % 256;
                p = Mathf.FloorToInt(p / 256);
                int g = p % 256;
                p = Mathf.FloorToInt(p / 256);
                int r = p % 256;
                float l = (0.2126f * r / 255f) + 0.7152f * (g / 255f) + 0.0722f * (b / 255f);
                Color c = new Color(l, l, l, 1);
                image.SetPixel(x, y, c);
            }
        }
        image.Apply(false);

        return image;
    }

    public static RenderTexture ApplyGrayScaleShader(RenderTexture image)
    {
        if (!mat)
            mat = new Material(Shader.Find("Hidden/GrayScaleShader"));

        mat.SetTexture("_Mask", overlay);

        RenderTexture grayScale = new RenderTexture(image);

        Graphics.Blit(image, grayScale, mat);

        return grayScale;
    }
}