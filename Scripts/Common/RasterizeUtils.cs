using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class RasterizeUtils
{
    public static List<Vector4> ClipWithPlane(List<Vector4> vertices, ClipPlane clipPlane)
    {
        List<Vector4> clippedVertices = new List<Vector4>();

        for (int i = 0; i < vertices.Count; ++i)
        {
            int startIndex = i;
            int endIndex = (i + 1 + vertices.Count) % vertices.Count;

            // 边的起点
            Vector4 startVertex = vertices[startIndex];
            // 边的终点
            Vector4 endVertex = vertices[endIndex];

            bool startVertexIn = false;
            bool endVertexIn = false;

            switch (clipPlane)
            {
                case ClipPlane.Near:
                    if (startVertex.z > -startVertex.w)
                        startVertexIn = true;
                    if (endVertex.z > -endVertex.w)
                        endVertexIn = true;
                    break;
                case ClipPlane.Far:
                    if (startVertex.z < startVertex.w)
                        startVertexIn = true;
                    if (endVertex.z < endVertex.w)
                        endVertexIn = true;
                    break;
                case ClipPlane.Left:
                    if (startVertex.x > -startVertex.w)
                        startVertexIn = true;
                    if (endVertex.x > -endVertex.w)
                        endVertexIn = true;
                    break;
                case ClipPlane.Right:
                    if (startVertex.x < startVertex.w)
                        startVertexIn = true;
                    if (endVertex.x < endVertex.w)
                        endVertexIn = true;
                    break;
                case ClipPlane.Top:
                    if (startVertex.y < startVertex.w)
                        startVertexIn = true;
                    if (endVertex.y < endVertex.w)
                        endVertexIn = true;
                    break;
                case ClipPlane.Bottom:
                    if (startVertex.y > -startVertex.w)
                        startVertexIn = true;
                    if (endVertex.y > -endVertex.w)
                        endVertexIn = true;
                    break;
            }

        
            if (startVertexIn != endVertexIn)
            {
                float t = 0;
                switch (clipPlane)
                {
                    case ClipPlane.Near:
                        t = (startVertex.w + startVertex.z) / (-(endVertex.z - startVertex.z) - (endVertex.w - startVertex.w));
                        break;
                    case ClipPlane.Far:
                        t = (startVertex.w - startVertex.z) / ((endVertex.z - startVertex.z) - (endVertex.w - startVertex.w));
                        break;
                    case ClipPlane.Left:
                        t = (startVertex.w + startVertex.x) / (-(endVertex.x - startVertex.x) - (endVertex.w - startVertex.w));
                        break;
                    case ClipPlane.Right:
                        t = (startVertex.w - startVertex.x) / ((endVertex.x - startVertex.x) - (endVertex.w - startVertex.w));
                        break;
                    case ClipPlane.Top:
                        t = (startVertex.w - startVertex.y) / ((endVertex.y - startVertex.y) - (endVertex.w - startVertex.w));
                        break;
                    case ClipPlane.Bottom:
                        t = (startVertex.w + startVertex.y) / (-(endVertex.y - startVertex.y) - (endVertex.w - startVertex.w));
                        break;
                }

                Vector4 intersection = Vector4.Lerp(startVertex, endVertex, t);
                clippedVertices.Add(intersection);
            }

            if (endVertexIn)
            {
                clippedVertices.Add(endVertex);
            }
        }

        return clippedVertices;
    }

    public static bool IsInsideTriangle(float x, float y, Vertex vert1, Vertex vert2, Vertex vert3, bool back = false)
    {

        Vector3 v0v1 = new Vector3(vert2.vert_pixel_coor.x - vert1.vert_pixel_coor.x, vert2.vert_pixel_coor.y - vert1.vert_pixel_coor.y, 0);
        Vector3 v1v2 = new Vector3(vert3.vert_pixel_coor.x - vert2.vert_pixel_coor.x, vert3.vert_pixel_coor.y - vert2.vert_pixel_coor.y, 0);
        Vector3 v2v0 = new Vector3(vert1.vert_pixel_coor.x - vert3.vert_pixel_coor.x, vert1.vert_pixel_coor.y - vert3.vert_pixel_coor.y, 0);


        Vector3 v0p = new Vector3(x - vert1.vert_pixel_coor.x, y - vert1.vert_pixel_coor.y, 0);
        Vector3 v1p = new Vector3(x - vert2.vert_pixel_coor.x, y - vert2.vert_pixel_coor.y, 0);
        Vector3 v2p = new Vector3(x - vert3.vert_pixel_coor.x, y - vert3.vert_pixel_coor.y, 0);

        if (back)
        {
            if (Vector3.Cross(v0v1, v0p).z > 0
            && Vector3.Cross(v1v2, v1p).z > 0
            && Vector3.Cross(v2v0, v2p).z > 0)
                return true;
            else
                return false;
        }
        else
        {
            if (Vector3.Cross(v0v1, v0p).z < 0
            && Vector3.Cross(v1v2, v1p).z < 0
            && Vector3.Cross(v2v0, v2p).z < 0)
                return true;
            else
                return false;
        }

    }

    public static Vector3 BarycentricCoordinate(float x, float y, Vertex vert1, Vertex vert2, Vertex vert3)
    {
        Vector3 v1v2 = new Vector3(vert2.vert_pixel_coor.x - vert1.vert_pixel_coor.x, vert2.vert_pixel_coor.y - vert1.vert_pixel_coor.y, 0);
        Vector3 v2v3 = new Vector3(vert3.vert_pixel_coor.x - vert2.vert_pixel_coor.x, vert3.vert_pixel_coor.y - vert2.vert_pixel_coor.y, 0);
        Vector3 v3v1 = new Vector3(vert1.vert_pixel_coor.x - vert3.vert_pixel_coor.x, vert1.vert_pixel_coor.y - vert3.vert_pixel_coor.y, 0);


        Vector3 v1p = new Vector3(x - vert1.vert_pixel_coor.x, y - vert1.vert_pixel_coor.y, 0);
        Vector3 v2p = new Vector3(x - vert2.vert_pixel_coor.x, y - vert2.vert_pixel_coor.y, 0);
        Vector3 v3p = new Vector3(x - vert3.vert_pixel_coor.x, y - vert3.vert_pixel_coor.y, 0);

        // 因为v0v1v2,p的z坐标都是0，叉乘向量的x、y是0，直接取z当作模长
        float area_v2v3p = Mathf.Abs(Vector3.Cross(v2v3, v2p).z) / 2;
        float area_v3v1p = Mathf.Abs(Vector3.Cross(v3v1, v3p).z) / 2;
        float area_v1v2p = Mathf.Abs(Vector3.Cross(v1v2, v1p).z) / 2;
        float area_v1v2v3 = Mathf.Abs(Vector3.Cross(v1v2, v3v1).z) / 2;

        return new Vector3(area_v2v3p / area_v1v2v3, area_v3v1p / area_v1v2v3, area_v1v2p / area_v1v2v3);
    }

    public static double ComputeMipMapLevel(float x, float y, Vertex v1, Vertex v2, Vertex v3,Texture2D sampleTex2D, bool isOrthographic)
    {
        float[] s0 = SampleTexel(x, y, v1, v2, v3, sampleTex2D, isOrthographic);
        float[] s1 = SampleTexel(x + 1, y, v1, v2, v3, sampleTex2D, isOrthographic);
        float[] s2 = SampleTexel(x, y + 1, v1, v2, v3, sampleTex2D, isOrthographic);

        double dx = s1[0] - s0[0]; // 0 width 1 height
        double dy = s1[1] - s0[1];
        float L1 = (float)Math.Sqrt((dx * dx + dy * dy));

        dx = s2[0] - s0[0];
        dy = s2[1] - s0[1];
        float L2 = (float)Math.Sqrt((dx * dx + dy * dy));


        float Lmax = Mathf.Max(L1, L2);
        double level;
        if (Lmax < 1)
            level = 0;
        else
            level = Math.Log(Lmax, 2);

        return level;
    }


    public static float[] SampleTexel(float x, float y, Vertex v1, Vertex v2, Vertex v3, Texture2D sampleTex2D, bool isOrthographic)
    {
        float[] f = new float[2];

        Vector3 barycentricCoordinate = BarycentricCoordinate(x,y, v1, v2, v3);

        Vector2 uv;
        if (isOrthographic)
        {
            uv = v1.uv * barycentricCoordinate.x +
                 v2.uv * barycentricCoordinate.y +
                 v3.uv * barycentricCoordinate.z;
        }
        else
        {
            float z_view = 1.0f / (barycentricCoordinate.x / v1.vert_proj_coor.w + barycentricCoordinate.y / v2.vert_proj_coor.w + barycentricCoordinate.z / v3.vert_proj_coor.w);

            uv = z_view * (v1.uv / v1.vert_view_coor.z * barycentricCoordinate.x +
                           v2.uv / v2.vert_view_coor.z * barycentricCoordinate.y +
                           v3.uv / v3.vert_view_coor.z * barycentricCoordinate.z);
        }
        
        float width = uv.x * sampleTex2D.width;
        float height = uv.y * sampleTex2D.height;
        f[0] = width;
        f[1] = height;
        return f;
    }

    public static Color GetColorByBilinear(float[] texel, Texture2D sampleTex2D ,int level = 0)
    {
        int sampleTexelX = (int)texel[0];
        int sampleTexelY = (int)texel[1];

        float fw = GetFloat(texel[0]);
        float fh = GetFloat(texel[1]);

        int scale = (int)Math.Pow(2, level);

        sampleTexelX = (int)(sampleTexelX / scale);
        sampleTexelY = (int)(sampleTexelY / scale);
		
		int sampleSizeWidth = (int)(sampleTex2D.width / scale);
        int sampleSizeHeight = (int)(sampleTex2D.height / scale);
		
        int sampleSizeTexelX = sampleSizeWidth - 1;
        int sampleSizeTexelY = sampleSizeHeight - 1;

        int dx = sampleTexelX >= sampleSizeTexelX ? sampleSizeTexelX - 1 : sampleTexelX;
        int dy = sampleTexelY >= sampleSizeTexelY ? sampleSizeTexelY - 1 : sampleTexelY;
		//Debug.LogError(sampleSizeWidth + "___" + sampleSizeHeight + "___" + sampleTexelX + "___" + sampleTexelY + "___" + dx + "___" + dy + "___" + level);
		int simpleCount;
		if(sampleSizeWidth == sampleSizeHeight && sampleSizeWidth == 1)
		{
			simpleCount = 1;
			dx = 0;
			dy = 0;
		}
		else
			simpleCount = 2;
		
		Color result;
        Color[] colors = sampleTex2D.GetPixels(dx, dy, simpleCount, simpleCount,level);
		if(colors.Length == 1)
		{
			result = colors[0];
		}
		else
		{
			Color c1 = colors[0];
			Color c2 = colors[1];
			Color c3 = colors[2];
			Color c4 = colors[3];

			Color c1c2 = Color.Lerp(c1, c2, fw);
			Color c3c4 = Color.Lerp(c3, c4, fw);
			result = Color.Lerp(c1c2, c3c4, fh);
		}
       
        return result;
    }


    public static Vertex[] GetVertexArray(MeshFilter meshFilter)
    {
        Vector3[] vertices = meshFilter.mesh.vertices;
        int[] indices = meshFilter.mesh.triangles;//三角形索引数组的顺序来绘制
        Vector2[] uvs = meshFilter.mesh.uv;
        Vector3[] normals = meshFilter.mesh.normals;
        Vector4[] tangents = meshFilter.mesh.tangents;
        Color[] colors = meshFilter.mesh.colors;

        Vertex[] vertexArray = new Vertex[vertices.Length];
        if (colors.Length > 0)
        {
            for (int i = 0; i < vertices.Length; i++)
                vertexArray[i] = new Vertex(vertices[i], uvs[i], normals[i], tangents[i], colors[i]);
        }
        else
        {
            for (int i = 0; i < vertexArray.Length; i++)
                vertexArray[i] = new Vertex(vertices[i], uvs[i], normals[i], tangents[i], Color.white);
        }

        return vertexArray;
    }

    public static float GetFloat(float a)
    {
        int b = (int)a;
        float c = a - b;
        return c;
    }

    public static float GetFloat(double a)
    {
        int b = (int)a;
        float c = (float)(a - b);
        return c;
    }
	
    public static Color OnDiffuse(Light light, Vector3 shaderPointCoor, Vector3 shaderPointNormal)
    {
        Color diffuse;
        LightType lightType = light.type;
        Color lightColor = light.color;
        float attenuationFactor = 1;
        Vector3 lightCoor = light.transform.position;
        Vector3 lightDir = (lightCoor - shaderPointCoor).normalized;
        float intensity = light.intensity;

        if (lightType == LightType.Directional)
        {
            attenuationFactor = 1;
        }
        else if (lightType == LightType.Point)
        {
            attenuationFactor = GetLightAttenuationFactor("Point", light, shaderPointCoor);

        }
        else if (lightType == LightType.Spot)
        {
            float angle = light.spotAngle / 2;
            Vector3 spotLightDir = light.transform.forward;
            float cutoff = (float)Math.Cos(angle * Math.PI / 180);
            float spotFactor = Vector3.Dot(-1 * lightDir, spotLightDir);

            if (spotFactor > cutoff)
            {
                intensity *= (1.0f - (1.0f - spotFactor) / (1.0f - cutoff));
                attenuationFactor = GetLightAttenuationFactor("Attenuation2", light, shaderPointCoor);
            }
            else
                intensity = 0;
        }
        else
        {
            intensity = 0;
        }

        diffuse = lightColor * Mathf.Max(0, Vector3.Dot(lightDir, shaderPointNormal)) * attenuationFactor * intensity;
        diffuse = intensity == 0 ? Color.black : diffuse;
        return diffuse;

    }

    public static Color OnSpecular(Light light, Vector3 shaderPointCoor, Vector3 shaderPointNormal, Vector3 viewWorldCoor)
    {
        Color specular;
        LightType lightType = light.type;
        float intensity = light.intensity;
        float attenuationFactor = 1;

        Vector3 lightCoor = light.transform.position;
        Vector3 lightDir = lightCoor - shaderPointCoor;
        Vector3 viewDir = viewWorldCoor - shaderPointCoor;
        Vector3 h = (lightDir + viewDir).normalized;

        if (lightType == LightType.Directional)
        {
            intensity = 0;
        }
        else if (lightType == LightType.Point)
        {
            attenuationFactor = GetLightAttenuationFactor("Point", light, shaderPointCoor);
        }
        else if (lightType == LightType.Spot)
        {
            //attenuationFactor = GetLightAttenuationFactor("Point", light, shaderPointCoor);
            intensity = 0;
        }
        else
        {
            intensity = 0;
        }
        specular = light.color * Mathf.Pow(Mathf.Max(Vector3.Dot(h, shaderPointNormal), 0), 2) * intensity * attenuationFactor;
        specular = intensity == 0 ? Color.black : specular;
        return specular;
    }


    public static Light[] GetManyLightsToObject(GameObject obj,Light[] lights,int limitCount = 4)
    {
        List<Light> tempList1 = new List<Light>();
        List<Light> tempList2 = new List<Light>();
        AABB aabb = new AABB(obj.transform);
        for (int i= 0; i < lights.Length; i++)
        {
            Light light = lights[i];
            Vector3 nPoint = aabb.NearestPointToPoint(light.transform.position);
            float dis = Vector3.Distance(nPoint, light.transform.position);
            if (dis < light.range)
                tempList1.Add(light);
        }

        tempList1.Sort(delegate (Light light1, Light light2)
            {
                float dis1 = Vector3.Distance(light1.transform.position, obj.transform.position);
                float dis2 = Vector3.Distance(light2.transform.position, obj.transform.position);
                return dis2.CompareTo(dis1);
            }
        );

        for (int i = 0; i < tempList1.Count; i++)
        {
            if (i >= limitCount) break;
            Light light = tempList1[i];
            tempList2.Add(light);
        }

        return tempList2.ToArray();
    }

    public static float GetVisibleFactorForHardShadow(FrameBuffer frameBuffer, Vector3 shaderPointCoor,Matrix4x4 view, Matrix4x4 projection, bool isOrthographic)
    {
        float visibleFactor;
        float shaderPointDepth;
        float bias;
        Vector4 view_coor = view.MultiplyPoint(shaderPointCoor);
        view_coor.w = 1;
        Vector4 proj_coor = projection * view_coor;
        Vector3 ndc_coor = new Vector3(proj_coor.x / proj_coor.w,proj_coor.y / proj_coor.w,proj_coor.z / proj_coor.w);
        Vector2 viewport_coor = new Vector2((ndc_coor.x + 1) / 2, (ndc_coor.y + 1) / 2);
        Vector2 pixel_coor = new Vector2(viewport_coor.x * Screen.width, viewport_coor.y * Screen.height);
        int pixel_x = (int)pixel_coor.x;
        int pixel_y = (int)pixel_coor.y;

        float shadowMapDepthBuffer = frameBuffer.GetShadowMapDepthBuffer(pixel_x, pixel_y);

        if (isOrthographic)
            shaderPointDepth = view_coor.z;
        else
            shaderPointDepth = proj_coor.z;

        bias = 0.1f;
        if (shaderPointDepth > shadowMapDepthBuffer + bias)
        {
            visibleFactor = 0;
        }
        else
        {
            visibleFactor = 1;
        }
        return visibleFactor;
    }


    public static float GetVisibleFactorForPCF(FrameBuffer frameBuffer, Vector3 shaderPointCoor, Matrix4x4 view, Matrix4x4 projection,int penumbraSize, bool isOrthographic)
    {
        float visibleFactor;
        int visibleCount = 0;
        float shaderPointDepth;
        float bias;
        float penumbraArea = (penumbraSize * 2 + 1) * (penumbraSize * 2 + 1);

        Vector4 view_coor = view.MultiplyPoint(shaderPointCoor);
        view_coor.w = 1;
        Vector4 proj_coor = projection * view_coor;
        Vector3 ndc_coor = new Vector3(proj_coor.x / proj_coor.w, proj_coor.y / proj_coor.w, proj_coor.z / proj_coor.w);
        Vector2 viewport_coor = new Vector2((ndc_coor.x + 1) / 2, (ndc_coor.y + 1) / 2);
        Vector2 pixel_coor = new Vector2(viewport_coor.x * Screen.width, viewport_coor.y * Screen.height);
        int pixel_x = (int)pixel_coor.x;
        int pixel_y = (int)pixel_coor.y;

        if (isOrthographic)
            shaderPointDepth = view_coor.z;
        else
            shaderPointDepth = proj_coor.z;

        bias = 0.1f;

        for (int i = -1 * penumbraSize; i <= penumbraSize; i++)
        {
            for (int j = -1 * penumbraSize; j <= penumbraSize; j++)
            {
                float shadowMapDepthBuffer = frameBuffer.GetShadowMapDepthBuffer(pixel_x + i, pixel_y + j);
                if (shaderPointDepth > shadowMapDepthBuffer + bias)
                {
                    visibleCount += 0;
                }
                else
                {
                    visibleCount += 1;
                }

            }
        }

        visibleFactor = visibleCount / penumbraArea;
        return visibleFactor;
    }

    public static float GetVisibleFactorForPCSS(FrameBuffer frameBuffer, Vector3 shaderPointCoor, Matrix4x4 view, Matrix4x4 projection, int lightSize, bool isOrthographic,bool debug)
    {
        float shaderPointDepth;
        int penumbraSize;

        Vector4 view_coor = view.MultiplyPoint(shaderPointCoor);
        view_coor.w = 1;
        Vector4 proj_coor = projection * view_coor;
        Vector3 ndc_coor = new Vector3(proj_coor.x / proj_coor.w, proj_coor.y / proj_coor.w, proj_coor.z / proj_coor.w);
        Vector2 viewport_coor = new Vector2((ndc_coor.x + 1) / 2, (ndc_coor.y + 1) / 2);
        Vector2 pixel_coor = new Vector2(viewport_coor.x * Screen.width, viewport_coor.y * Screen.height);
        int pixel_x = (int)pixel_coor.x;
        int pixel_y = (int)pixel_coor.y;

        if (isOrthographic)
            shaderPointDepth = view_coor.z;
        else
            shaderPointDepth = proj_coor.z;

        float bias;
        float blockerAverageDistance = 0;
        int blockerCount = 0;
        float blockerDistance = 0;

        bias = 0.1f;

        float shadowMapDepthBuffer1 = frameBuffer.GetShadowMapDepthBuffer(pixel_x , pixel_y);
       
        for (int i = -1 * lightSize; i <= lightSize; i++)
        {
            for (int j = -1 * lightSize; j <= lightSize; j++)
            {
                float shadowMapDepthBuffer = frameBuffer.GetShadowMapDepthBuffer(pixel_x + i, pixel_y + j);

                if (shaderPointDepth > shadowMapDepthBuffer + bias)
                {
                    blockerCount += 1;
                    blockerDistance += shadowMapDepthBuffer;
                }
            }
        }

        if (blockerCount != 0)
        {
            blockerAverageDistance = blockerDistance / blockerCount;
            penumbraSize = (int)((shaderPointDepth - blockerAverageDistance) * lightSize / blockerAverageDistance );
            if (penumbraSize <= 0) penumbraSize = 1;
        }
        else
        {
            penumbraSize = 1;
        }
        float visibleFactor = GetVisibleFactorForPCF(frameBuffer, shaderPointCoor, view, projection, penumbraSize, isOrthographic);
        return visibleFactor;
    }

    public static float GetVisibleFactorForVSM(FrameBuffer frameBuffer, Vector3 shaderPointCoor, Matrix4x4 view, Matrix4x4 projection, int searchSize,bool isOrthographic,bool debug)
    {
        Vector4 view_coor = view.MultiplyPoint(shaderPointCoor);
        view_coor.w = 1;
        Vector4 proj_coor = projection * view_coor;
        Vector3 ndc_coor = new Vector3(proj_coor.x / proj_coor.w, proj_coor.y / proj_coor.w, proj_coor.z / proj_coor.w);
        Vector2 viewport_coor = new Vector2((ndc_coor.x + 1) / 2, (ndc_coor.y + 1) / 2);
        Vector2 pixel_coor = new Vector2(viewport_coor.x * Screen.width, viewport_coor.y * Screen.height);
        int pixel_x = (int)pixel_coor.x;
        int pixel_y = (int)pixel_coor.y;

        int penumbraSize;
        Texture2D sampleTex2D = frameBuffer.ShadowMapDepthBufferTex;
        float shaderPointDepth = RasterizeUtils.GetShaderPointDepth(shaderPointCoor, view, projection, isOrthographic);

        float shadowMapDepthBuffer1 = frameBuffer.GetShadowMapDepthBuffer(pixel_x, pixel_y);

        Color depthTexel = RasterizeUtils.GetShadowMapDepthTexelByMipMap(sampleTex2D, shaderPointCoor, view, projection, searchSize);
        
        float averageDepth = depthTexel.r;
        float squareDepth = depthTexel.g;
        float p;
        if (shaderPointDepth >= averageDepth)//12.54323 10.484
            p = 0;//完全遮挡
        else
            p = 1;//完全暴露

        float variance = Math.Max(squareDepth - averageDepth * averageDepth, 0.00002f);
        float d = shaderPointDepth - averageDepth;
        float p_max = variance / (variance + d * d);
        p_max = Math.Min(Math.Max(p, p_max),1);
        float blockerAverageDistance;
        if (p_max == 1)
        {
            blockerAverageDistance = shaderPointDepth;
        }
        else
        {
            blockerAverageDistance = (averageDepth - p_max * shaderPointDepth) / (float)(1.0 - p_max);
        }
        penumbraSize = (int)((shaderPointDepth - blockerAverageDistance) * searchSize / blockerAverageDistance);
        penumbraSize = Math.Max(penumbraSize, 1);
        float visibleFactor = GetVisibleFactorForPCF(frameBuffer, shaderPointCoor, view, projection, penumbraSize, isOrthographic);
        // Debug.LogError(penumbraSize);
        return visibleFactor;
    }
    public static Color GetShadowMapDepthTexelByMipMap(Texture2D sampleTex2D, Vector3 shaderPointCoor, Matrix4x4 view, Matrix4x4 projection, int sampleSize)
    {
        Vector4 view_coor = view.MultiplyPoint(shaderPointCoor);
        view_coor.w = 1;
        Vector4 proj_coor = projection * view_coor;
        Vector3 ndc_coor = new Vector3(proj_coor.x / proj_coor.w, proj_coor.y / proj_coor.w, proj_coor.z / proj_coor.w);
        Vector2 viewport_coor = new Vector2((ndc_coor.x + 1) / 2, (ndc_coor.y + 1) / 2);
     //   Debug.LogError(" ndc_coor " + ndc_coor + " viewport_coor " + viewport_coor);
        Vector2 pixel_coor = new Vector2(viewport_coor.x * Screen.width, viewport_coor.y * Screen.height);
        Vector2 sampleTexelCoor = new Vector2(pixel_coor.x, pixel_coor.y);
        Color color = GetAverageColorByMipMap(sampleTexelCoor, sampleTex2D, sampleSize);
        return color;
    }

    public static Color GetAverageColorByMipMap(Vector2 sampleTexelCoor,Texture2D sampleTex2D, int sampleSize)
    {
        int level = (int)Math.Log(sampleSize, 2);
       

        int sampleTexelX = (int)sampleTexelCoor.x;
        int sampleTexelY = (int)sampleTexelCoor.y;

        sampleTexelX = (int)(sampleTexelX / sampleSize);
        sampleTexelY = (int)(sampleTexelY / sampleSize);

        int sampleSizeWidth = (int)(sampleTex2D.width / sampleSize);
        int sampleSizeHeight = (int)(sampleTex2D.height / sampleSize);

        int sampleSizeTexelX = sampleSizeWidth - 1;
        int sampleSizeTexelY = sampleSizeHeight - 1;

        int dx = sampleTexelX >= sampleSizeTexelX ? sampleSizeTexelX - 1 : sampleTexelX;
        int dy = sampleTexelY >= sampleSizeTexelY ? sampleSizeTexelY - 1 : sampleTexelY;

        //Debug.LogError(" level " + level  + " dx " + dx + " dy " + dy + "  levelCount " + sampleTex2D.mipmapCount + " sampleSize "+ sampleSize + " sampleSizeWidth " + sampleSizeWidth + " sampleSizeTexelY "+ sampleSizeTexelY + " sampleTexelX " + sampleTexelX + " sampleTexelY " + sampleTexelY + " sampleTexelCoor "+ sampleTexelCoor);
        Color average = sampleTex2D.GetPixels(dx, dy, 1, 1, 0)[0];
      
        return average;
    }

    public static float GetShaderPointDepth(Vector3 shaderPointCoor, Matrix4x4 view, Matrix4x4 projection, bool isOrthographic)
    {
        float shaderPointDepth;
        Vector4 view_coor = view.MultiplyPoint(shaderPointCoor);
        view_coor.w = 1;
        Vector4 proj_coor = projection * view_coor;
        Vector3 ndc_coor = new Vector3(proj_coor.x / proj_coor.w, proj_coor.y / proj_coor.w, proj_coor.z / proj_coor.w);
        Vector2 viewport_coor = new Vector2((ndc_coor.x + 1) / 2, (ndc_coor.y + 1) / 2);
        Vector2 pixel_coor = new Vector2(viewport_coor.x * Screen.width, viewport_coor.y * Screen.height);
        Vector2 sampleTexelCoor = new Vector2(pixel_coor.x, pixel_coor.y);

        if (isOrthographic)
            shaderPointDepth = view_coor.z;
        else
            shaderPointDepth = proj_coor.z;

        return shaderPointDepth;
    }


    public static float GetLightAttenuationFactor(string attenuationType, Light light, Vector3 shaderPointCoor)
    {
        float factor = 0;

        float lightRange = light.range;
        Vector3 lightCoor = light.transform.position;
        Vector3 lightDir = (lightCoor - shaderPointCoor).normalized;
        float distance = Vector3.Distance(lightCoor, shaderPointCoor);

        if (attenuationType == "Attenuation")
        {
            factor = (float)Math.Pow((distance / lightRange * 5), 2);
            factor = 1 / (factor + 1);
        }
        else if (attenuationType == "Attenuation2")
        {
            factor = (distance / light.range + 1);
            factor = (1 / (factor * factor));
        }
        else if (attenuationType == "Linear")
        {
            factor = 1 / (distance * distance);
            factor = distance > lightRange ? 0 : factor;
        }
        else if (attenuationType == "Point")
        {
            factor = (lightRange - distance) / lightRange;
            factor = distance > lightRange ? 0 : factor;
        }
        else if (attenuationType == "Spot")
        {
            factor = (lightRange - distance) / lightRange;
            factor = distance > lightRange ? 0 : factor;
        }
        return factor;
    }
}



