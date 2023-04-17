using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AABB 
{
    public Transform transform;
    public float[] min;
    public float[] max;
    public float[] center;
    public AABB(Transform tf)
    {
        transform = tf;
        MeshFilter meshFilter = transform.GetComponent<MeshFilter>();

        Vector3[] vertices = meshFilter.mesh.vertices;
        Vector3 v = transform.localToWorldMatrix.MultiplyPoint(vertices[0]);
        min = new float[] { v.x, v.y, v.z };
        max = new float[] { v.x, v.y, v.z };
        center = new float[] { 0.0f, 0.0f, 0.0f };

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vertex = vertices[i];
            Vector3 v_world = transform.localToWorldMatrix.MultiplyPoint(vertex);

            if (v_world.x < min[0]) min[0] = v_world.x;
            if (v_world.x > max[0]) max[0] = v_world.x;
            if (v_world.y < min[1]) min[1] = v_world.y;
            if (v_world.y > max[1]) max[1] = v_world.y;
            if (v_world.z < min[2]) min[2] = v_world.z;
            if (v_world.z > max[2]) max[2] = v_world.z;

        }

        center[0] = (min[0] + max[0]) * 0.5f;
        center[1] = (min[1] + max[1]) * 0.5f;
        center[2] = (min[2] + max[2]) * 0.5f;
    }

    public Vector3 NearestPointToPoint(Vector3 point)
    {
        float x = point.x;
        x = x > max[0] ? max[0] : x;
        x = x < min[0] ? min[0] : x;

        float y = point.y;
        y = y > max[1] ? max[1] : y;
        y = y < min[1] ? min[1] : y;

        float z = point.z;
        z = z > max[2] ? max[2] : z;
        z = z < min[2] ? min[2] : z;

        Vector3 v = new Vector3(x, y, z);
        //Debug.LogError(v);
        return v;
    }

    public bool CheckPoint(float x, float y, float z)
    {
        float X = Mathf.Floor(x * 10000) / 10000;
        float Y = Mathf.Floor(y * 10000) / 10000;
        float Z = Mathf.Floor(z * 10000) / 10000;
        bool b = (X >= min[0]) && (X <= max[0]) &&
                 (Y >= min[1]) && (Y <= max[1]) &&
                (Z >= min[2]) && (Z <= max[2]);
        return b;
    }
}
