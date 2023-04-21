using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScene : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        float degrees = 180;
        double angle = Math.PI * degrees / 180.0;
     //   Debug.LogError(Math.Cos(angle));
     //   Debug.LogError(this.transform.forward);
      //  Debug.LogError(Quaternion.Euler(45, 0, 0) * this.transform.forward);
     //   this.transform.rotation = Quaternion.Euler(45, 0, 0) *  this.transform.rotation ;

        Debug.LogError(Math.Pow(2, 3));
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
