using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RailShooter
{
    [System.Serializable]
    public class Annulus
    {
        public float distance; // Distance of the annulus center from the spawner
        public float innerRadius;
        public float outerRadius;

        public Vector3 GetRandomPoint()
        {
            // Random angle between 0 and 180 degrees
            float angle = Random.Range(0f, Mathf.PI); //2PI for full circle, PI for half circle

            // Random radius between inner and outer radius
            float radius = Random.Range(innerRadius, outerRadius);

            // Calculate the x and y coordinates of the point
            float x = radius * Mathf.Cos(angle);
            float y = radius * Mathf.Sin(angle);

            return new Vector3(x, y, distance);


        }

        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
