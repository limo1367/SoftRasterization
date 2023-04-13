using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameBuffer 
{
    private int texWidth;
    private int texHeight;
    private Texture2D depthBufferTex;
    private Texture2D colorBufferTex;
    private Texture2D normalBufferTex;
    private Texture2D worldCoorBufferTex;

    public FrameBuffer(int width, int height)
    {
        texWidth = width;
        texHeight = height;
        depthBufferTex = new Texture2D(width, height, TextureFormat.RFloat, false);
        colorBufferTex = new Texture2D(width, height, TextureFormat.RGBAFloat, false);
        normalBufferTex = new Texture2D(width, height, TextureFormat.RGBAFloat, false);
        worldCoorBufferTex = new Texture2D(width, height, TextureFormat.RGBAFloat, false);
        InitBuffer();


    }

    public void SetDepthBuffer(int x, int y, float depth)
    {
        depthBufferTex.SetPixel(x, y, new Color(depth, 0, 0));
    }

    public float GetDepthBuffer(int x, int y)
    {
        return depthBufferTex.GetPixel(x, y).r;
    }

    public void SetColorBuffer(int x, int y, Color c)
    {
        colorBufferTex.SetPixel(x, y, c);
    }

    public Color GetColorBuffer(int x, int y)
    {
        return colorBufferTex.GetPixel(x, y);
    }


    public void SetNormalBuffer(int x, int y, Vector3 v)
    {
        normalBufferTex.SetPixel(x, y, new Color(v.x, v.y, v.z));
    }

    public Vector3 GetNormalBuffer(int x, int y)
    {
        Color c = normalBufferTex.GetPixel(x, y);
        return new Vector3(c.r, c.g, c.b);
    }


    public void SetWorldCoorBuffer(int x, int y, Vector3 v)
    {
        worldCoorBufferTex.SetPixel(x, y, new Color(v.x, v.y, v.z));
    }

    public Vector3 GetWorldCoorBuffer(int x, int y)
    {
        Color c = worldCoorBufferTex.GetPixel(x, y);
        return new Vector3(c.r, c.g, c.b);
    }

    public void Apply()
    {
        depthBufferTex.Apply();
        colorBufferTex.Apply();
        normalBufferTex.Apply();
        worldCoorBufferTex.Apply();
    }


    public void InitBuffer()
    {
        for (int i = 0; i < texWidth; i++)
        {
            for (int j = 0; j < texHeight; j++)
            {
                SetDepthBuffer(i, j, -1);
                SetColorBuffer(i, j, Color.black);
                SetNormalBuffer(i, j, Vector3.zero);
                SetWorldCoorBuffer(i, j, Vector3.zero);
            }
        }
    }
}
