using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 多光照渲染
/// </summary>
public class RasterizeForManyLights : MonoBehaviour
{
    Matrix4x4 main_model;
    Matrix4x4 main_view;
    Matrix4x4 main_projection;

    Camera main_camera;
    LightShader lightShader;

    RawImage rasterizeImage;
    Texture2D rasterizeTex2D;

    ModelProperty modelProperty;
    Texture2D sampleTex2D;
    
    FrameBuffer frameBuffer;


    void Start()
    {
        GameObject CanvasGo = GameObject.Find("Canvas");
        GameObject RasterizeImageGameObject = CanvasGo.transform.Find("RasterizeImage").gameObject;
        RasterizeImageGameObject.gameObject.SetActive(true);

        rasterizeImage = RasterizeImageGameObject.GetComponent<RawImage>();
        rasterizeTex2D = new Texture2D(Screen.width, Screen.height, TextureFormat.RGBA32, true);

        modelProperty = GetComponent<ModelProperty>();
        sampleTex2D = modelProperty.albedo;

        main_camera = GameObject.Find("MainCamera").GetComponent<Camera>();
		
		GameObject lightRoot = GameObject.Find("LightRoot");

        lightShader = new LightShader(lightRoot);
        frameBuffer = new FrameBuffer(Screen.width, Screen.height);

        MeshFilter[] gameObjectMeshs = GetComponentsInChildren<MeshFilter>();

        ClearTexture();

        List<Light> viewLightList = new List<Light>();
        for (int i = 0; i < gameObjectMeshs.Length; i++)
        {
            MeshFilter mesh = gameObjectMeshs[i];
            OnApplication(mesh.gameObject, ref viewLightList);
        }

        lightShader.lightForViewArray = viewLightList.ToArray();
        lightShader.main_view_world_coor = main_camera.transform.position;

        for (int i = 0; i < gameObjectMeshs.Length; i++)
        {
            MeshFilter mesh = gameObjectMeshs[i];
            OnObjectsRender(mesh);
        }

        Light[] lightArray = lightShader.lightForViewArray;
        for (int i = 0; i < lightArray.Length; i++)
        {
            Light light = lightArray[i];
            OnLightsRender(light);
        }

       
        for (int i = 0; i < Screen.width; i++)
        {
            for (int j = 0; j < Screen.height; j++)
            {
                float depth = frameBuffer.GetDepthBuffer(i, j);
                if (depth < 0) continue;

                Color color = frameBuffer.GetColorBuffer(i, j);
                lightShader.OnLightDeferredShader(i, j,frameBuffer);

                Color pixelColor = (lightShader.ambient + lightShader.diffuse  + lightShader.specular) * color;
                rasterizeTex2D.SetPixel(i, j, pixelColor);
            }
        }

        rasterizeTex2D.Apply();
        rasterizeImage.texture = rasterizeTex2D;
    }

    /// <summary>
    /// 逐像素处理
    /// </summary>
    public void OnObjectsRender(MeshFilter mesh)
    {
        MeshFilter meshFilter = mesh;
        SetModel(meshFilter.transform.localToWorldMatrix);
        SetView(main_camera.transform.worldToLocalMatrix);
        SetProjection(main_camera);

        int[] indices = meshFilter.mesh.triangles;//三角形索引数组的顺序来绘制
        Vertex[] vertexArray = RasterizeUtils.GetVertexArray(meshFilter);

        for (int i = 0; i < indices.Length; i += 3)
        {
            int index1 = indices[i];
            int index2 = indices[i + 1];
            int index3 = indices[i + 2];


            Vertex vert1 = vertexArray[index1];
            Vertex vert2 = vertexArray[index2];
            Vertex vert3 = vertexArray[index3];

            vert1.UnityObjectToViewPort(main_model, main_view, main_projection);
            vert2.UnityObjectToViewPort(main_model, main_view, main_projection);
            vert3.UnityObjectToViewPort(main_model, main_view, main_projection);

     
            Vector3 v1v2 = vert2.vert_ndc_coor - vert1.vert_ndc_coor;
            Vector3 v1v3 = vert3.vert_ndc_coor - vert1.vert_ndc_coor;
            float Cullz = Vector3.Cross(v1v2, v1v3).z;
           
            if (Cullz >= 0)
                continue;
   
            vert1.UNITY_TRANSFER_PIXEL();
            vert2.UNITY_TRANSFER_PIXEL();
            vert3.UNITY_TRANSFER_PIXEL();

  
            List<Vector4> clippedVertices0 = new List<Vector4> { vert1.vert_proj_coor, vert2.vert_proj_coor, vert3.vert_proj_coor };
            List<Vector4> clippedVertices1 = RasterizeUtils.ClipWithPlane(clippedVertices0, ClipPlane.Near);
            List<Vector4> clippedVertices2 = RasterizeUtils.ClipWithPlane(clippedVertices1, ClipPlane.Far);
            List<Vector4> clippedVertices3 = RasterizeUtils.ClipWithPlane(clippedVertices2, ClipPlane.Left);
            List<Vector4> clippedVertices4 = RasterizeUtils.ClipWithPlane(clippedVertices3, ClipPlane.Right);
            List<Vector4> clippedVertices5 = RasterizeUtils.ClipWithPlane(clippedVertices4, ClipPlane.Top);
            List<Vector4> clippedVertices6 = RasterizeUtils.ClipWithPlane(clippedVertices5, ClipPlane.Bottom);
      
            if (clippedVertices6.Count < 3)
                continue;
   
            for (int iClipVert = 0; iClipVert < clippedVertices6.Count - 2; iClipVert++)
            {
                
                int clipIndex1 = 0;
                int clipIndex2 = iClipVert + 1;
                int clipIndex3 = iClipVert + 2;

                Vector4 vert1_proj_clip = clippedVertices6[clipIndex1];
                Vector4 vert2_proj_clip = clippedVertices6[clipIndex2];
                Vector4 vert3_proj_clip = clippedVertices6[clipIndex3];

    
                Vector3 vert1_ndc_clip = new Vector3(vert1_proj_clip.x / vert1_proj_clip.w, vert1_proj_clip.y / vert1_proj_clip.w, vert1_proj_clip.z / vert1_proj_clip.w);
                Vector3 vert2_ndc_clip = new Vector3(vert2_proj_clip.x / vert2_proj_clip.w, vert2_proj_clip.y / vert2_proj_clip.w, vert2_proj_clip.z / vert2_proj_clip.w);
                Vector3 vert3_ndc_clip = new Vector3(vert3_proj_clip.x / vert3_proj_clip.w, vert3_proj_clip.y / vert3_proj_clip.w, vert3_proj_clip.z / vert3_proj_clip.w);

                Vector3 vert1_viewport_clip = new Vector3((vert1_ndc_clip.x + 1) / 2, (vert1_ndc_clip.y + 1) / 2, 0);
                Vector3 vert2_viewport_clip = new Vector3((vert2_ndc_clip.x + 1) / 2, (vert2_ndc_clip.y + 1) / 2, 0);
                Vector3 vert3_viewport_clip = new Vector3((vert3_ndc_clip.x + 1) / 2, (vert3_ndc_clip.y + 1) / 2, 0);

                Vector2 vert1_pixel_clip = new Vector2(vert1_viewport_clip.x * Screen.width, vert1_viewport_clip.y * Screen.height);
                Vector2 vert2_pixel_clip = new Vector2(vert2_viewport_clip.x * Screen.width, vert2_viewport_clip.y * Screen.height);
                Vector2 vert3_pixel_clip = new Vector2(vert3_viewport_clip.x * Screen.width, vert3_viewport_clip.y * Screen.height);

                Vector2Int bboxMin = new Vector2Int((int)Mathf.Min(Mathf.Min(vert1_pixel_clip.x, vert2_pixel_clip.x), vert3_pixel_clip.x),
                                                (int)Mathf.Min(Mathf.Min(vert1_pixel_clip.y, vert2_pixel_clip.y), vert3_pixel_clip.y));
                Vector2Int bboxMax = new Vector2Int((int)(Mathf.Max(Mathf.Max(vert1_pixel_clip.x, vert2_pixel_clip.x), vert3_pixel_clip.x) + 0.5f),
                                                (int)(Mathf.Max(Mathf.Max(vert1_pixel_clip.y, vert2_pixel_clip.y), vert3_pixel_clip.y) + 0.5f));

                for (int ii = bboxMin.x; ii < bboxMax.x; ii++)
                {
                    
                    for (int jj = bboxMin.y; jj < bboxMax.y; jj++)
                    {
                        if (RasterizeUtils.IsInsideTriangle(ii + 0.5f, jj + 0.5f, vert1, vert2, vert3))
                        {
                            
                            Vector3 barycentricCoordinate = RasterizeUtils.BarycentricCoordinate(ii + 0.5f, jj + 0.5f, vert1, vert2, vert3);

                            bool isOrthographic = main_camera.orthographic;
                            float depth;
                            Vector3 worldCoor;
                            Vector3 normal;

                            if (isOrthographic)
                            {
                                depth = vert1.vert_view_coor.z * barycentricCoordinate.x +
                                        vert2.vert_view_coor.z * barycentricCoordinate.y +
                                        vert3.vert_view_coor.z * barycentricCoordinate.z;

                                worldCoor = vert1.vert_world_coor * barycentricCoordinate.x +
                                            vert2.vert_world_coor * barycentricCoordinate.y +
                                            vert3.vert_world_coor * barycentricCoordinate.z;

                                normal = vert1.normal * barycentricCoordinate.x +
                                         vert2.normal * barycentricCoordinate.y +
                                         vert3.normal * barycentricCoordinate.z;
                            }
                            else
                            {
                                float z_view = 1.0f / (barycentricCoordinate.x / vert1.vert_proj_coor.w + barycentricCoordinate.y / vert2.vert_proj_coor.w + barycentricCoordinate.z / vert3.vert_proj_coor.w);

                                depth = z_view * (vert1.vert_proj_coor.z / vert1.vert_proj_coor.w * barycentricCoordinate.x +
                                                  vert2.vert_proj_coor.z / vert2.vert_proj_coor.w * barycentricCoordinate.y +
                                                  vert3.vert_proj_coor.z / vert3.vert_proj_coor.w * barycentricCoordinate.z);

                                worldCoor = z_view * (vert1.vert_world_coor / vert1.vert_view_coor.z * barycentricCoordinate.x +
                                                      vert2.vert_world_coor / vert2.vert_view_coor.z * barycentricCoordinate.y +
                                                      vert3.vert_world_coor / vert3.vert_view_coor.z * barycentricCoordinate.z);

                                normal = z_view * (vert1.normal / vert1.vert_view_coor.z * barycentricCoordinate.x +
                                                   vert2.normal / vert2.vert_view_coor.z * barycentricCoordinate.y +
                                                   vert3.normal / vert3.vert_view_coor.z * barycentricCoordinate.z);
                            }

                            float depthBuffer = frameBuffer.GetDepthBuffer(ii, jj);


                            if (depthBuffer < 0)
                            {

                            }
                            else
                            {
                                if (depth > depthBuffer) continue;
                            }

                            Vector3 transposeNormal = main_model.inverse.transpose.MultiplyVector(normal);

                            double level = RasterizeUtils.ComputeMipMapLevel(ii, jj, vert1, vert2, vert3, sampleTex2D, isOrthographic);
                            float levelRate = RasterizeUtils.GetFloat(level);
                            int level1 = (int)level;
                            float[] texel = RasterizeUtils.SampleTexel(ii, jj, vert1, vert2, vert3, sampleTex2D, isOrthographic);
                            Color c1 = RasterizeUtils.GetColorByBilinear(texel,sampleTex2D,level1);
                            Color c2 = RasterizeUtils.GetColorByBilinear(texel, sampleTex2D,level1 + 1);
                            Color mainTexColor = Color.Lerp(c1, c2, levelRate);
                          
                            frameBuffer.SetDepthBuffer(ii, jj, depth);
                            frameBuffer.SetColorBuffer(ii, jj, mainTexColor);
                            frameBuffer.SetNormalBuffer(ii, jj, transposeNormal);
                            frameBuffer.SetWorldCoorBuffer(ii, jj, worldCoor);
                        }
                    }
                }
            }
        }
    }

    private void SetModel(Matrix4x4 model)
    {
        main_model = model;
    }

    private void SetView(Matrix4x4 view)
    {
        main_view = view;
    }

    private void SetProjection(Matrix4x4 projection)
    {
        main_projection = projection;
    }

    private void SetProjection(Camera camera)
    {
        if (camera.orthographic)
        {
            Matrix4x4 orthographicProjection = Orthographic(camera.nearClipPlane, camera.farClipPlane, camera.orthographicSize * 2, camera.aspect);
            SetProjection(orthographicProjection);
        }
        else
        {
            Matrix4x4 perspectiveProjection = Perspective(camera.nearClipPlane, camera.farClipPlane, camera.fieldOfView, camera.aspect);
            SetProjection(perspectiveProjection);
        }
    }

    private Matrix4x4 Orthographic(float near, float far, float height, float aspect)
    {
        float width = height * aspect;
        Matrix4x4 orthographicMatrix = new Matrix4x4(new Vector4(2f / width, 0, 0, 0),
                                                     new Vector4(0, 2f / height, 0, 0),
                                                     new Vector4(0, 0, 2f / (far - near), 0),
                                                     new Vector4(0, 0, -(far + near) / (far - near), 1));
        return orthographicMatrix;
    }

    private Matrix4x4 Perspective(float near, float far, float fov, float aspect)
    {
        float height = 2 * near * Mathf.Tan(Mathf.Deg2Rad * (fov / 2));
        float width = aspect * height;

        Matrix4x4 perspectiveMatrix = new Matrix4x4(new Vector4(2 * near / width, 0, 0, 0),
                                                    new Vector4(0, 2 * near / height, 0, 0),
                                                    new Vector4(0, 0, far / (far - near), 1),
                                                    new Vector4(0, 0, -(near * far) / (far - near), 0));

        return perspectiveMatrix;
    }


    private void ClearTexture()
    {
        for (int i = 0; i < Screen.width; i++)
        {
            for (int j = 0; j < Screen.height; j++)
            {
                rasterizeTex2D.SetPixel(i, j, Color.white);
            }
        }
    }

    private void OnApplication(GameObject obj,ref List<Light> list)
    {
        Light[] lights = lightShader.pointLightArray;
        Light[] tempLightArray = RasterizeUtils.GetManyLightsToObject(obj, lights, 10);
        for (int i = 0; i < tempLightArray.Length; i++)
        {
            Light light = tempLightArray[i];
            if (!list.Contains(light))
                list.Add(light);
        }
    }

    public void OnLightsRender(Light light)
    {
        SetModel(light.gameObject.transform.localToWorldMatrix);
        SetView(main_camera.transform.worldToLocalMatrix);
        SetProjection(main_camera);

        Texture2D depthBufferTex = new Texture2D(Screen.width, Screen.height, TextureFormat.RFloat, false);
        for (int i = 0; i < Screen.width; i++)
        {
            for (int j = 0; j < Screen.height; j++)
            {
                depthBufferTex.SetPixel(i, j, new Color(-1, 0, 0));
            }
        }

        OnWriteLightBackDepthBuffer(light, ref depthBufferTex);
        
        OnWriteLightBuffer(light, ref depthBufferTex);
    }

    public void OnWriteLightBackDepthBuffer(Light light, ref Texture2D depthTex)
    {
        MeshFilter meshFilter = light.gameObject.GetComponent<MeshFilter>();
        int[] indices = meshFilter.mesh.triangles;//三角形索引数组的顺序来绘制
        Vertex[] vertexArray = RasterizeUtils.GetVertexArray(meshFilter);

        for (int i = 0; i < indices.Length; i += 3)
        {
            int index1 = indices[i];
            int index2 = indices[i + 1];
            int index3 = indices[i + 2];

            Vertex vert1 = vertexArray[index1];
            Vertex vert2 = vertexArray[index2];
            Vertex vert3 = vertexArray[index3];

            vert1.UnityObjectToViewPort(main_model, main_view, main_projection);
            vert2.UnityObjectToViewPort(main_model, main_view, main_projection);
            vert3.UnityObjectToViewPort(main_model, main_view, main_projection);

            vert1.UNITY_TRANSFER_PIXEL();
            vert2.UNITY_TRANSFER_PIXEL();
            vert3.UNITY_TRANSFER_PIXEL();

            Vector3 v1v2 = vert2.vert_ndc_coor - vert1.vert_ndc_coor;
            Vector3 v1v3 = vert3.vert_ndc_coor - vert1.vert_ndc_coor;
            float Cullz = Vector3.Cross(v1v2, v1v3).z;

            if (Cullz < 0)
                continue;

            List<Vector4> clippedVertices0 = new List<Vector4> { vert1.vert_proj_coor, vert2.vert_proj_coor, vert3.vert_proj_coor };
            List<Vector4> clippedVertices1 = RasterizeUtils.ClipWithPlane(clippedVertices0, ClipPlane.Near);
            List<Vector4> clippedVertices2 = RasterizeUtils.ClipWithPlane(clippedVertices1, ClipPlane.Far);
            List<Vector4> clippedVertices3 = RasterizeUtils.ClipWithPlane(clippedVertices2, ClipPlane.Left);
            List<Vector4> clippedVertices4 = RasterizeUtils.ClipWithPlane(clippedVertices3, ClipPlane.Right);
            List<Vector4> clippedVertices5 = RasterizeUtils.ClipWithPlane(clippedVertices4, ClipPlane.Top);
            List<Vector4> clippedVertices6 = RasterizeUtils.ClipWithPlane(clippedVertices5, ClipPlane.Bottom);

            if (clippedVertices6.Count < 3) continue;
    
            for (int iClipVert = 0; iClipVert < clippedVertices6.Count - 2; iClipVert++)
            {

                int clipIndex1 = 0;
                int clipIndex2 = iClipVert + 1;
                int clipIndex3 = iClipVert + 2;

                Vector4 vert1_proj_clip = clippedVertices6[clipIndex1];
                Vector4 vert2_proj_clip = clippedVertices6[clipIndex2];
                Vector4 vert3_proj_clip = clippedVertices6[clipIndex3];


                Vector3 vert1_ndc_clip = new Vector3(vert1_proj_clip.x / vert1_proj_clip.w, vert1_proj_clip.y / vert1_proj_clip.w, vert1_proj_clip.z / vert1_proj_clip.w);
                Vector3 vert2_ndc_clip = new Vector3(vert2_proj_clip.x / vert2_proj_clip.w, vert2_proj_clip.y / vert2_proj_clip.w, vert2_proj_clip.z / vert2_proj_clip.w);
                Vector3 vert3_ndc_clip = new Vector3(vert3_proj_clip.x / vert3_proj_clip.w, vert3_proj_clip.y / vert3_proj_clip.w, vert3_proj_clip.z / vert3_proj_clip.w);

                Vector3 vert1_viewport_clip = new Vector3((vert1_ndc_clip.x + 1) / 2, (vert1_ndc_clip.y + 1) / 2, 0);
                Vector3 vert2_viewport_clip = new Vector3((vert2_ndc_clip.x + 1) / 2, (vert2_ndc_clip.y + 1) / 2, 0);
                Vector3 vert3_viewport_clip = new Vector3((vert3_ndc_clip.x + 1) / 2, (vert3_ndc_clip.y + 1) / 2, 0);

                Vector2 vert1_pixel_clip = new Vector2(vert1_viewport_clip.x * Screen.width, vert1_viewport_clip.y * Screen.height);
                Vector2 vert2_pixel_clip = new Vector2(vert2_viewport_clip.x * Screen.width, vert2_viewport_clip.y * Screen.height);
                Vector2 vert3_pixel_clip = new Vector2(vert3_viewport_clip.x * Screen.width, vert3_viewport_clip.y * Screen.height);

                Vector2Int bboxMin = new Vector2Int((int)Mathf.Min(Mathf.Min(vert1_pixel_clip.x, vert2_pixel_clip.x), vert3_pixel_clip.x),
                                                (int)Mathf.Min(Mathf.Min(vert1_pixel_clip.y, vert2_pixel_clip.y), vert3_pixel_clip.y));
                Vector2Int bboxMax = new Vector2Int((int)(Mathf.Max(Mathf.Max(vert1_pixel_clip.x, vert2_pixel_clip.x), vert3_pixel_clip.x) + 0.5f),
                                                (int)(Mathf.Max(Mathf.Max(vert1_pixel_clip.y, vert2_pixel_clip.y), vert3_pixel_clip.y) + 0.5f));

                for (int ii = bboxMin.x; ii < bboxMax.x; ii++)
                {

                    for (int jj = bboxMin.y; jj < bboxMax.y; jj++)
                    {
                        if (RasterizeUtils.IsInsideTriangle(ii + 0.5f, jj + 0.5f, vert1, vert2, vert3,true))
                        {

                            Vector3 barycentricCoordinate = RasterizeUtils.BarycentricCoordinate(ii + 0.5f, jj + 0.5f, vert1, vert2, vert3);

                            bool isOrthographic = main_camera.orthographic;
                            float depth;
                
                            if (isOrthographic)
                            {
                                depth = vert1.vert_view_coor.z * barycentricCoordinate.x +
                                        vert2.vert_view_coor.z * barycentricCoordinate.y +
                                        vert3.vert_view_coor.z * barycentricCoordinate.z;
                            }
                            else
                            {
                                float z_view = 1.0f / (barycentricCoordinate.x / vert1.vert_proj_coor.w + barycentricCoordinate.y / vert2.vert_proj_coor.w + barycentricCoordinate.z / vert3.vert_proj_coor.w);

                                depth = z_view * (vert1.vert_proj_coor.z / vert1.vert_proj_coor.w * barycentricCoordinate.x +
                                                  vert2.vert_proj_coor.z / vert2.vert_proj_coor.w * barycentricCoordinate.y +
                                                  vert3.vert_proj_coor.z / vert3.vert_proj_coor.w * barycentricCoordinate.z);
                            }

                            float depthBuffer = frameBuffer.GetDepthBuffer(ii, jj);

                            if (depthBuffer < 0) continue;
                            if (depth <= depthBuffer) continue;
                            depthTex.SetPixel(ii, jj, new Color(depth, 0, 0));
                        }
                    }
                }
            }
        }
    }

    public void OnWriteLightBuffer(Light light, ref Texture2D depthTex)
    {
        MeshFilter meshFilter = light.gameObject.GetComponent<MeshFilter>();
        int[] indices = meshFilter.mesh.triangles;//三角形索引数组的顺序来绘制
        Vertex[] vertexArray = RasterizeUtils.GetVertexArray(meshFilter);

        for (int i = 0; i < indices.Length; i += 3)
        {
            int index1 = indices[i];
            int index2 = indices[i + 1];
            int index3 = indices[i + 2];


            Vertex vert1 = vertexArray[index1];
            Vertex vert2 = vertexArray[index2];
            Vertex vert3 = vertexArray[index3];

            vert1.UnityObjectToViewPort(main_model, main_view, main_projection);
            vert2.UnityObjectToViewPort(main_model, main_view, main_projection);
            vert3.UnityObjectToViewPort(main_model, main_view, main_projection);


            Vector3 v1v2 = vert2.vert_ndc_coor - vert1.vert_ndc_coor;
            Vector3 v1v3 = vert3.vert_ndc_coor - vert1.vert_ndc_coor;
            float Cullz = Vector3.Cross(v1v2, v1v3).z;

            if (Cullz >= 0)
                continue;

            vert1.UNITY_TRANSFER_PIXEL();
            vert2.UNITY_TRANSFER_PIXEL();
            vert3.UNITY_TRANSFER_PIXEL();


            List<Vector4> clippedVertices0 = new List<Vector4> { vert1.vert_proj_coor, vert2.vert_proj_coor, vert3.vert_proj_coor };
            List<Vector4> clippedVertices1 = RasterizeUtils.ClipWithPlane(clippedVertices0, ClipPlane.Near);
            List<Vector4> clippedVertices2 = RasterizeUtils.ClipWithPlane(clippedVertices1, ClipPlane.Far);
            List<Vector4> clippedVertices3 = RasterizeUtils.ClipWithPlane(clippedVertices2, ClipPlane.Left);
            List<Vector4> clippedVertices4 = RasterizeUtils.ClipWithPlane(clippedVertices3, ClipPlane.Right);
            List<Vector4> clippedVertices5 = RasterizeUtils.ClipWithPlane(clippedVertices4, ClipPlane.Top);
            List<Vector4> clippedVertices6 = RasterizeUtils.ClipWithPlane(clippedVertices5, ClipPlane.Bottom);

            if (clippedVertices6.Count < 3)
                continue;

            for (int iClipVert = 0; iClipVert < clippedVertices6.Count - 2; iClipVert++)
            {

                int clipIndex1 = 0;
                int clipIndex2 = iClipVert + 1;
                int clipIndex3 = iClipVert + 2;

                Vector4 vert1_proj_clip = clippedVertices6[clipIndex1];
                Vector4 vert2_proj_clip = clippedVertices6[clipIndex2];
                Vector4 vert3_proj_clip = clippedVertices6[clipIndex3];


                Vector3 vert1_ndc_clip = new Vector3(vert1_proj_clip.x / vert1_proj_clip.w, vert1_proj_clip.y / vert1_proj_clip.w, vert1_proj_clip.z / vert1_proj_clip.w);
                Vector3 vert2_ndc_clip = new Vector3(vert2_proj_clip.x / vert2_proj_clip.w, vert2_proj_clip.y / vert2_proj_clip.w, vert2_proj_clip.z / vert2_proj_clip.w);
                Vector3 vert3_ndc_clip = new Vector3(vert3_proj_clip.x / vert3_proj_clip.w, vert3_proj_clip.y / vert3_proj_clip.w, vert3_proj_clip.z / vert3_proj_clip.w);

                Vector3 vert1_viewport_clip = new Vector3((vert1_ndc_clip.x + 1) / 2, (vert1_ndc_clip.y + 1) / 2, 0);
                Vector3 vert2_viewport_clip = new Vector3((vert2_ndc_clip.x + 1) / 2, (vert2_ndc_clip.y + 1) / 2, 0);
                Vector3 vert3_viewport_clip = new Vector3((vert3_ndc_clip.x + 1) / 2, (vert3_ndc_clip.y + 1) / 2, 0);

                Vector2 vert1_pixel_clip = new Vector2(vert1_viewport_clip.x * Screen.width, vert1_viewport_clip.y * Screen.height);
                Vector2 vert2_pixel_clip = new Vector2(vert2_viewport_clip.x * Screen.width, vert2_viewport_clip.y * Screen.height);
                Vector2 vert3_pixel_clip = new Vector2(vert3_viewport_clip.x * Screen.width, vert3_viewport_clip.y * Screen.height);

                Vector2Int bboxMin = new Vector2Int((int)Mathf.Min(Mathf.Min(vert1_pixel_clip.x, vert2_pixel_clip.x), vert3_pixel_clip.x),
                                                (int)Mathf.Min(Mathf.Min(vert1_pixel_clip.y, vert2_pixel_clip.y), vert3_pixel_clip.y));
                Vector2Int bboxMax = new Vector2Int((int)(Mathf.Max(Mathf.Max(vert1_pixel_clip.x, vert2_pixel_clip.x), vert3_pixel_clip.x) + 0.5f),
                                                (int)(Mathf.Max(Mathf.Max(vert1_pixel_clip.y, vert2_pixel_clip.y), vert3_pixel_clip.y) + 0.5f));

                for (int ii = bboxMin.x; ii < bboxMax.x; ii++)
                {

                    for (int jj = bboxMin.y; jj < bboxMax.y; jj++)
                    {
                        if (RasterizeUtils.IsInsideTriangle(ii + 0.5f, jj + 0.5f, vert1, vert2, vert3))
                        {

                            Vector3 barycentricCoordinate = RasterizeUtils.BarycentricCoordinate(ii + 0.5f, jj + 0.5f, vert1, vert2, vert3);

                            bool isOrthographic = main_camera.orthographic;
                            float depth;
                    
                            if (isOrthographic)
                            {
                                depth = vert1.vert_view_coor.z * barycentricCoordinate.x +
                                        vert2.vert_view_coor.z * barycentricCoordinate.y +
                                        vert3.vert_view_coor.z * barycentricCoordinate.z;
                            }
                            else
                            {
                                float z_view = 1.0f / (barycentricCoordinate.x / vert1.vert_proj_coor.w + barycentricCoordinate.y / vert2.vert_proj_coor.w + barycentricCoordinate.z / vert3.vert_proj_coor.w);

                                depth = z_view * (vert1.vert_proj_coor.z / vert1.vert_proj_coor.w * barycentricCoordinate.x +
                                                  vert2.vert_proj_coor.z / vert2.vert_proj_coor.w * barycentricCoordinate.y +
                                                  vert3.vert_proj_coor.z / vert3.vert_proj_coor.w * barycentricCoordinate.z);
                            }

                            float depthBuffer = frameBuffer.GetDepthBuffer(ii, jj);
                            Vector3 worldCoor = frameBuffer.GetWorldCoorBuffer(ii, jj);
                            Vector3 normal = frameBuffer.GetNormalBuffer(ii, jj);

                            if (depthBuffer < 0) continue;

                            float backDepth = depthTex.GetPixel(ii, jj).r;
                            if (backDepth < 0) continue;

                            if (backDepth > depthBuffer && depthBuffer > depth)
                            {
                                Color diffuseColor = RasterizeUtils.OnDiffuse(light, worldCoor, normal);
                                Color colorBuffer = frameBuffer.GetLightsColorDiffuseBuffer(ii, jj);
                                colorBuffer += diffuseColor;
                                frameBuffer.SetLightsColorDiffuseBuffer(ii, jj, colorBuffer);

                                Color specularColor = RasterizeUtils.OnSpecular(light, worldCoor, normal, lightShader.main_view_world_coor);
                                Color colorBuffer2 = frameBuffer.GetLightsColorSpecularBuffer(ii, jj);
                                colorBuffer2 += specularColor;
                                frameBuffer.SetLightsColorSpecularBuffer(ii, jj, colorBuffer2);
                            }
                        }
                    }
                }
            }
        }
    }
}
