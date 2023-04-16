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
    public LightShader(GameObject lightRoot)
    {
		Light[] lights = lightRoot.transform.GetComponentsInChildren<Light>();
		for(int i = 0; i < lights.Length;i++)
		{
			Light light = lights[i];
			if(light.gameObject.name.ToLower().Contains("ambient"))
			{
				ambientLight = light;
			}
			else if(light.gameObject.name.ToLower().Contains("direction"))
			{
				directionLight = light;
			}
		}	
		
       
    }


    public void OnLightForwardShader()
    {
		diffuse = Color.black;
		specular = Color.black;
		
        diffuse += RasterizeUtils.OnDiffuse(directionLight,world_coor,normal);
        specular += RasterizeUtils.OnSpecular(directionLight,world_coor,normal,main_view_world_coor);
		
		ambient = ambientLight.color;
    }
	
	 public void OnLightDeferredShader()
    {
		diffuse = Color.black;
		specular = Color.black;
		
        diffuse += RasterizeUtils.OnDiffuse(directionLight,world_coor,normal);
        specular += RasterizeUtils.OnSpecular(directionLight,world_coor,normal,main_view_world_coor);
		
		ambient = ambientLight.color;
    }


}
