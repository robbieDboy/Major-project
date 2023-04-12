using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class laserEmitter : MonoBehaviour
{

    public Material material;
    laserBeam beam;


    //destroys and creates the beam
    void Update()
    {
        if(beam != null)
        {
            Destroy(beam.laserObject);
        }
        beam = new laserBeam(gameObject.transform.position, gameObject.transform.right, material);
    }
}
