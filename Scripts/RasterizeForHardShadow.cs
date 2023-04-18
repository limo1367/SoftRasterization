using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 前向渲染
/// </summary>
public class RasterizeForHardShadow : MonoBehaviour
{
    Matrix4x4 main_model;
    Matrix4x4 main_view;
    Matrix4x4 main_projection;

    Matrix4x4 light_view;
    Matrix4x4 light_projection;

    Camera main_camera;
    private Light direction_light;
    private Camera direction_light_camera;

    RawImage rasterizeImage;
    Texture2D rasterizeTex2D;
    RawImage shadowMapImage;
    Texture2D shadowMapTex2D;
    Texture2D sampleTex2D;

    FrameBuffer frameBuffer;
    LightShader lightShader;
    ModelProperty modelProperty;
   

    void Start()
    {
        GameObject CanvasGo = GameObject.Find("Canvas");
        GameObject RasterizeImageGameObject = CanvasGo.transform.Find("RasterizeImage").gameObject;
        GameObject ShadowMapImageGameObject = CanvasGo.transform.Find("ShadowMapImage").gameObject;
        RasterizeImageGameObject.SetActive(true);
        ShadowMapImageGameObject.SetActive(true);

        rasterizeImage = RasterizeImageGameObject.GetComponent<RawImage>();
        rasterizeTex2D = new Texture2D(Screen.width, Screen.height, TextureFormat.RGBA32, true);

        shadowMapImage = ShadowMapImageGameObject.GetComponent<RawImage>();
        shadowMapTex2D = new Texture2D(Screen.width, Screen.height, TextureFormat.RGBA32, true);

        modelProperty = GetComponent<ModelProperty>();
        sampleTex2D = modelProperty.albedo;

        main_camera = GameObject.Find("MainCamera").GetComponent<Camera>();
       
		GameObject lightRoot = GameObject.Find("LightRoot");
        lightShader = new LightShader(lightRoot);
        frameBuffer = new FrameBuffer(Screen.width, Screen.height);

        direction_light = lightRoot.transform.Find("DirectionalLight").GetComponent<Light>();
        direction_light_camera = lightRoot.transform.Find("DirectionalLight").GetComponent<Camera>();

        MeshFilter[] gameObjectMeshs = GetComponentsInChildren<MeshFilter>();

        ClearTexture();

        for (int i = 0; i < gameObjectMeshs.Length; i++)
        {
            MeshFilter mesh = gameObjectMeshs[i];
            OnShadowMapRender(mesh);
        }

        OnShadowMapTexture();


        for (int i = 0; i < gameObjectMeshs.Length; i++)
        {
            MeshFilter mesh = gameObjectMeshs[i];

            OnApplication(mesh.gameObject);
            OnForwardRenderForPerPixel(mesh);
        }

        rasterizeTex2D.Apply();
        rasterizeImage.texture = rasterizeTex2D;
    }

    /// <summary>
    /// 逐像素处理
    /// </summary>
    public void OnForwardRenderForPerPixel(MeshFilter mesh)
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
                            frameBuffer.SetDepthBuffer(ii, jj,depth);


                            double level = RasterizeUtils.ComputeMipMapLevel(ii, jj, vert1, vert2, vert3, sampleTex2D, isOrthographic);
                            float levelRate = RasterizeUtils.GetFloat(level);
                            int level1 = (int)level;
                            float[] texel = RasterizeUtils.SampleTexel(ii, jj, vert1, vert2, vert3, sampleTex2D, isOrthographic);
                            Color c1 = RasterizeUtils.GetColorByBilinear(texel,sampleTex2D,level1);
                            Color c2 = RasterizeUtils.GetColorByBilinear(texel, sampleTex2D,level1 + 1);
                            Color mainTexColor = Color.Lerp(c1, c2, levelRate);

                            lightShader.normal = main_model.inverse.transpose.MultiplyVector(normal);
                            lightShader.world_coor = worldCoor;
                            lightShader.main_view_world_coor = main_camera.transform.position;
                            lightShader.OnLightForwardShader();

                            Color pixelColor = lightShader.ambient + lightShader.diffuse * mainTexColor + lightShader.specular;
                            rasterizeTex2D.SetPixel(ii, jj, pixelColor);
                        }
                    }
                }
            }
        }

     
    }

  
    public void OnShadowMapRender(MeshFilter mesh)
    {
        MeshFilter meshFilter = mesh;
        SetModel(meshFilter.transform.localToWorldMatrix);
        SetLightView(direction_light_camera.transform.worldToLocalMatrix);
        SetLightProjection(direction_light_camera);

        int[] indices = meshFilter.mesh.triangles;
        Vertex[] vertexArray = RasterizeUtils.GetVertexArray(meshFilter);

        for (int i = 0; i < indices.Length; i += 3)
        {
            int index1 = indices[i];
            int index2 = indices[i + 1];
            int index3 = indices[i + 2];


            Vertex vert1 = vertexArray[index1];
            Vertex vert2 = vertexArray[index2];
            Vertex vert3 = vertexArray[index3];

            vert1.UnityObjectToViewPort(main_model, light_view, light_projection);
            vert2.UnityObjectToViewPort(main_model, light_view, light_projection);
            vert3.UnityObjectToViewPort(main_model, light_view, light_projection);


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

                            bool isOrthographic = direction_light_camera.orthographic;
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

                            float depthBuffer = frameBuffer.GetShadowMapDepthBuffer(ii, jj);


                            if (depthBuffer < 0)
                            {

                            }
                            else
                            {
                                if (depth > depthBuffer) continue;
                            }
                            frameBuffer.SetShadowMapDepthBuffer(ii, jj, depth);

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

    private void SetLightView(Matrix4x4 view)
    {
        light_view = view;
    }

    private void SetLightProjection(Matrix4x4 projection)
    {
        light_projection = projection;
    }

    private void SetLightProjection(Camera camera)
    {
        if (camera.orthographic)
        {
            Matrix4x4 orthographicProjection = Orthographic(camera.nearClipPlane, camera.farClipPlane, camera.orthographicSize * 2, camera.aspect);
            SetLightProjection(orthographicProjection);
        }
        else
        {
            Matrix4x4 perspectiveProjection = Perspective(camera.nearClipPlane, camera.farClipPlane, camera.fieldOfView, camera.aspect);
            SetLightProjection(perspectiveProjection);
        }
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

        for (int i = 0; i < Screen.width; i++)
        {
            for (int j = 0; j < Screen.height; j++)
            {
                shadowMapTex2D.SetPixel(i, j, Color.white);
            }
        }
    }

    private void OnApplication(GameObject obj)
    {
        Light[] lights = lightShader.pointLightArray;
        lightShader.lightForViewArray = RasterizeUtils.GetManyLightsToObject(obj, lights);
    }

    private void OnShadowMapTexture()
    {
        float maxDepth = -1;
        for (int i = 0; i < Screen.width; i++)
        {
            for (int j = 0; j < Screen.height; j++)
            {
                float depth = frameBuffer.GetShadowMapDepthBuffer(i, j);
                if (depth > maxDepth) maxDepth = depth;
               
            }
        }
        float sumDepth = maxDepth;
        for (int i = 0; i < Screen.width; i++)
        {
            for (int j = 0; j < Screen.height; j++)
            {
                float depth = frameBuffer.GetShadowMapDepthBuffer(i, j);
                if (depth < 0) continue;
                Color depthColor = new Color(depth / sumDepth, depth / sumDepth, depth / sumDepth);
                shadowMapTex2D.SetPixel(i, j, depthColor);
            }
        }
        shadowMapTex2D.Apply();
        shadowMapImage.texture = shadowMapTex2D;

    }
}
