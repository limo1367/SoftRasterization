using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vertex 
{
    public Vector3 vert_coor;
    public Vector2 uv;
    public Vector3 normal;
    public Vector4 tangent;
    public Color color;

    public Vector3 vert_world_coor;
    public Vector4 vert_view_coor;
    public Vector4 vert_proj_coor;

    public Vector3 vert_ndc_coor;
    public Vector3 vert_viewport_coor;
    public Vector2 vert_pixel_coor;
    public Vertex(Vector3 _vert_pos, Vector2 _uv, Vector3 _normal, Vector4 _tangent, Color _color)
    {
        vert_coor = _vert_pos;
        uv = _uv;
        normal = _normal;
        tangent = _tangent;
        color = _color;
    }


    public void UNITY_MATRIX_MVP(Matrix4x4 model,Matrix4x4 view, Matrix4x4 projection)
    {
        vert_world_coor = model.MultiplyPoint(vert_coor);
        vert_view_coor = view.MultiplyPoint(vert_world_coor);
        vert_view_coor.w = 1;
        vert_proj_coor = projection * vert_view_coor;
    }


    public void UNITY_NDC()
    {
        vert_ndc_coor = new Vector3(vert_proj_coor.x / vert_proj_coor.w, vert_proj_coor.y / vert_proj_coor.w, vert_proj_coor.z / vert_proj_coor.w);
    }


    public void UNITY_TRANSFER_VIEWPORT()
    {
        vert_viewport_coor = new Vector3((vert_ndc_coor.x + 1) / 2, (vert_ndc_coor.y + 1) / 2, 0);
    }


    public void UNITY_TRANSFER_PIXEL()
    {
        vert_pixel_coor = new Vector2(vert_viewport_coor.x * Screen.width, vert_viewport_coor.y * Screen.height);
    }


    public void UnityObjectToViewPort(Matrix4x4 model, Matrix4x4 view, Matrix4x4 projection)
    {
        UNITY_MATRIX_MVP(model, view, projection);
        UNITY_NDC();
        UNITY_TRANSFER_VIEWPORT();
    }
}
