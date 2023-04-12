using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class laserBeam
{

    Vector3 position, direction;

    public GameObject laserObject;
    LineRenderer laser;
    //holds points along beam
    List<Vector3> laserIndices = new List<Vector3>();

    //different materials
    Dictionary<string, float> refractiveMaterials = new Dictionary<string, float>()
    {
        { "Air", 1.0f },
        {"Glass", 1.5f },
        {"Diamond", 2.4f },
        {"Water", 1.3f},
        {"Wall", 0f }
    };


    //sets up laser and calls CastRay()
    public laserBeam(Vector3 position, Vector3 direction, Material material)
    {
        //set up laser beam object
        this.laser = new LineRenderer();
        this.laserObject = new GameObject();
        this.laserObject.name = "laser beam";
        this.position = position;
        this.direction = direction;

        //add line renderer to game object to create laser beam
        this.laser = this.laserObject.AddComponent(typeof(LineRenderer)) as LineRenderer;
        //configure laser
        this.laser.startWidth = 0.1f;
        this.laser.endWidth = 0.1f;
        this.laser.material = material;
        this.laser.startColor = Color.blue;
        this.laser.endColor = Color.blue;

        CastRay(position, direction, laser);
    }


    //creates actual beam and detects hit
    void CastRay(Vector3 position, Vector3 direction, LineRenderer laser)
    {

        laserIndices.Add(position);
        Ray ray = new Ray(position, direction);
        RaycastHit hit;

        //if it hits something within 100 units
        if (Physics.Raycast(ray, out hit, 100, 1))
        {
            //check whats been hit
            CheckHit(hit, direction, laser);
        }
        else
        {
            //add position
            laserIndices.Add(ray.GetPoint(30));
            UpdateLaser();
        }
    }

    void UpdateLaser()
    {
        int count = 0;
        laser.positionCount = laserIndices.Count;


        foreach(Vector3 idx in laserIndices)
        {
            laser.SetPosition(count, idx);
            count++;
        }

    }

    void CheckHit(RaycastHit hitInfo, Vector3 direction, LineRenderer laser)
    {
        //If laser hits a mirror:
        if (hitInfo.collider.gameObject.tag == "Mirror")
        {
            //takes current position and reflected direction
            Vector3 pos = hitInfo.point;
            Vector3 dir = Vector3.Reflect(direction, hitInfo.normal);

            //cast ray in new direction
            CastRay(pos, dir, laser);
        }
        else if (hitInfo.collider.gameObject.tag != "Mirror")
        {

            //add position at hit position
            Vector3 pos = hitInfo.point;
            laserIndices.Add(pos);

            //Offset initial contact necessary to make new position slightly inside the object to avoid errors
            Vector3 newPos1 = new Vector3(Mathf.Abs(direction.x) / (direction.x + 0.0001f) * 0.0001f + pos.x,
                Mathf.Abs(direction.y) / (direction.y + 0.0001f) * 0.0001f + pos.y,
                Mathf.Abs(direction.z) / (direction.z + 0.0001f) * 0.0001f + pos.z);

            //get refractive indices
            float n1 = refractiveMaterials["Air"];
            float n2 = refractiveMaterials[hitInfo.collider.name];

            //get normal and incident
            Vector3 norm = hitInfo.normal;
            Vector3 incident = direction;

            Vector3 refractedVector = Refract(n1, n2, norm, incident);

            //cast new ray DOESNT WORK BECAUSE CAST INSIDE BOX COLLIDER SO DOESNT RECOFNISE OUTER EGDE OF OBJECT
            //CastRay(newPos1, refractedVector, laser);

            //working: cast new ray

            //starts new ray x units along rafracted vector pointing towards hit position
            Ray ray1 = new Ray(newPos1, refractedVector);
            //start new inverted ray x units away
            Vector3 newRayStartPos = ray1.GetPoint(11f);


            Ray ray2 = new Ray(newRayStartPos, -refractedVector);

            RaycastHit hit2;

            //check if inverted ray hits a collider within x units
            if(Physics.Raycast(ray2, out hit2, 11.5f, 1))
            {
                //if it does add to indices array
                laserIndices.Add(hit2.point);
            }

            UpdateLaser();


            Vector3 refractedVector2 = Refract(n2, n1, -hit2.normal, refractedVector);

            CastRay(hit2.point, refractedVector2, laser);
            

        }
        else
        {
            laserIndices.Add(hitInfo.point);
            UpdateLaser();
        }
    }


    //takes info to return refracted vector
    Vector3 Refract(float n1, float n2, Vector3 normal, Vector3 incident)
    {
        incident.Normalize();

        //maths to get refracted vector
        Vector3 refractedVector = (n1 / n2 * Vector3.Cross(normal, Vector3.Cross(-normal, incident)) - normal * Mathf.Sqrt(1 - Vector3.Dot(Vector3.Cross(normal, incident)
            * (n1 / n2 * n1 / n2), Vector3.Cross(normal, incident)))).normalized;





        return refractedVector;
    }
}







