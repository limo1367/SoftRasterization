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
    private Texture2D lightsColorDiffuseBufferTex;
    private Texture2D lightsColorSpecularBufferTex;
    private Texture2D shadowMapDepthBufferTex;
    private Dictionary<string, Texture2D> shadowMapDepthBufferTexDict;

    public FrameBuffer(int width, int height)
    {
        texWidth = width;
        texHeight = height;
        depthBufferTex = new Texture2D(width, height, TextureFormat.RFloat, false);
        colorBufferTex = new Texture2D(width, height, TextureFormat.RGBAFloat, false);
        normalBufferTex = new Texture2D(width, height, TextureFormat.RGBAFloat, false);
        worldCoorBufferTex = new Texture2D(width, height, TextureFormat.RGBAFloat, false);
        lightsColorDiffuseBufferTex = new Texture2D(width, height, TextureFormat.RGBAFloat, false);
        lightsColorSpecularBufferTex = new Texture2D(width, height, TextureFormat.RGBAFloat, false);
        shadowMapDepthBufferTex = new Texture2D(width, height, TextureFormat.RGFloat, true);
        shadowMapDepthBufferTexDict = new Dictionary<string, Texture2D>();
        InitBuffer();
    }

    

    public void SetDepthBuffer(int x, int y, float depth)
    {
        depthBufferTex.SetPixel(x, y, new Color(depth, depth, 0));
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


    public void SetLightsColorDiffuseBuffer(int x, int y, Color c)
    {
        lightsColorDiffuseBufferTex.SetPixel(x, y, c);
    }

    public Color GetLightsColorDiffuseBuffer(int x, int y)
    {
        Color c = lightsColorDiffuseBufferTex.GetPixel(x, y);
        return c;
    }

    public void SetLightsColorSpecularBuffer(int x, int y, Color c)
    {
        lightsColorSpecularBufferTex.SetPixel(x, y, c);
    }

    public Color GetLightsColorSpecularBuffer(int x, int y)
    {
        Color c = lightsColorSpecularBufferTex.GetPixel(x, y);
        return c;
    }

    public void SetShadowMapDepthBuffer(int x, int y, float depth)
    {
        shadowMapDepthBufferTex.SetPixel(x, y, new Color(depth, depth * depth, 0));
    }

    public float GetShadowMapDepthBuffer(int x, int y)
    {
        return shadowMapDepthBufferTex.GetPixel(x, y).r;
    }

    public float GetShadowMapSquareDepthBuffer(int x, int y)
    {
        return shadowMapDepthBufferTex.GetPixel(x, y).g;
    }


    public void SetShadowMapDepthBufferByTexName(string name,int x, int y, float depth)
    {
        Texture2D tex = shadowMapDepthBufferTexDict[name];
        tex.SetPixel(x, y, new Color(depth, depth * depth, 0));
    }

    public float GetShadowMapDepthBufferByTexName(string name,int x, int y)
    {
        Texture2D tex = shadowMapDepthBufferTexDict[name];
        return tex.GetPixel(x, y).r;
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
                SetDepthBuffer(i, j, 0);
                SetColorBuffer(i, j, Color.black);
                SetNormalBuffer(i, j, Vector3.zero);
                SetWorldCoorBuffer(i, j, Vector3.zero);
                SetLightsColorDiffuseBuffer(i, j, Color.black);
                SetLightsColorSpecularBuffer(i, j, Color.black);
                SetShadowMapDepthBuffer(i, j, 0);
            }
        }
    }

    public void InitManyShadowMaps(Light[] lights)
    {
        for (int i = 0; i < lights.Length; i++)
        {
            Light ligt = lights[i];
            string name = ligt.gameObject.name;
            if (!shadowMapDepthBufferTexDict.ContainsKey(name))
                shadowMapDepthBufferTexDict[name] = new Texture2D(texWidth, texHeight, TextureFormat.RGFloat, true);

            Texture2D tex = shadowMapDepthBufferTexDict[name];
            for (int x = 0; x < texWidth; x++)
            {
                for (int y = 0; y < texHeight; y++)
                {
                    SetShadowMapDepthBufferByTexName(name,x,y,0);
                }
            }

        }
    }

    public Texture2D ShadowMapDepthBufferTex
    {
        get { return shadowMapDepthBufferTex; }
    }
}
