using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightShader
{
    public Light ambientLight;
    public Light directionLight;
	public Light[] pointLightArray;
	public Light[] spotLightArray;

	public Light[] lightForViewArray;

    public Vector3 normal;
    public Vector3 world_coor;

    public Vector3 main_view_world_coor;

    public Color diffuse;
    public Color specular;
    public Color ambient;
    public LightShader(GameObject lightRoot)
    {
		Light[] lights = lightRoot.transform.GetComponentsInChildren<Light>();
		List<Light> tempPointLightList = new List<Light>();
		List<Light> tempSpotLightList = new List<Light>();

		for (int i = 0; i < lights.Length;i++)
		{
			Light light = lights[i];
			if(light.gameObject.name.ToLower().Contains("ambient"))
			{
				ambientLight = light;
			}
			else if(light.type == LightType.Directional)
			{
				directionLight = light;
			}
			else if(light.type == LightType.Point)
			{
				tempPointLightList.Add(light);
			}
			else if (light.type == LightType.Spot)
			{
				tempSpotLightList.Add(light);
			}
		}	
		pointLightArray = tempPointLightList.ToArray();
		spotLightArray = tempSpotLightList.ToArray();

	}


    public void OnLightForwardShader()
    {
		diffuse = Color.black;
		specular = Color.black;
		
        diffuse += RasterizeUtils.OnDirectionalDiffuse(directionLight,world_coor,normal);
		
		for(int i = 0; i < lightForViewArray.Length; i++)
		{
			Light light = lightForViewArray[i];
			if (light.type == LightType.Point)
			{
				diffuse += RasterizeUtils.OnPointDiffuse(light, world_coor, normal);
				specular += RasterizeUtils.OnSpecular(light, world_coor, normal, main_view_world_coor);
			}
			else if (light.type == LightType.Spot)
			{
				diffuse += RasterizeUtils.OnSpotDiffuse(light, world_coor, normal);
			}


		}
		
		ambient = ambientLight.color;
    }
	
	public void OnLightDeferredShader(int x,int y,FrameBuffer frameBuffer)
	{
		diffuse = Color.black;
		specular = Color.black;
		
		normal = frameBuffer.GetNormalBuffer(x, y);
        world_coor = frameBuffer.GetWorldCoorBuffer(x, y);
                
        diffuse += RasterizeUtils.OnDirectionalDiffuse(directionLight,world_coor,normal);

		diffuse += frameBuffer.GetLightsColorDiffuseBuffer(x,y);
		specular += frameBuffer.GetLightsColorSpecularBuffer(x, y);

		ambient = ambientLight.color;
    }


}
