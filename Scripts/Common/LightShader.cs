using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightShader
{
    public Light ambientLight;
    public Light directionLight;

    public Vector3 normal;
    public Vector3 world_coor;

    public Vector3 main_view_world_coor;

    public Color diffuse;
    public Color specular;
    public Color ambient;
    public LightShader(Light ambientL,Light directL)
    {
        ambientLight = ambientL;
        directionLight = directL;

        OnAmbient();
    }


    public void OnLightProcess()
    {
        OnDiffuse(directionLight);
        OnSpecular(directionLight);
    }

    private void OnDiffuse(Light light)
    {
        diffuse = Color.black;
        Color lightColor = light.color;
        Vector3 lightPos = light.transform.position;
        Vector3 lightDir = (lightPos - world_coor).normalized;
        diffuse += lightColor * Mathf.Max(0, Vector3.Dot(lightDir, normal));

    }

    private void OnSpecular(Light light)
    {
        specular = Color.black;

        Vector3 lightPos = light.transform.position;
        Vector3 lightDir = lightPos - world_coor;

        float distance = Vector3.Distance(lightPos, world_coor);
        float attenuation = 1 / distance;

        Vector3 viewDir = main_view_world_coor - world_coor;
        Vector3 h = (lightDir + viewDir).normalized;

        specular += Color.white * Mathf.Max(0, Mathf.Pow(Vector3.Dot(h, normal), 64)) * attenuation;
    }

    private void OnAmbient()
    {
        ambient = ambientLight.color;

    }


}
