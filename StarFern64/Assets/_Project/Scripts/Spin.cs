using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RailShooter
{
    public class Spin : MonoBehaviour
    {
        [SerializeField] float rotationSpeed;

        // Update is called once per frame
        void Update()
        {
            Rotate();
        }

        void Rotate()
        {
            transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.Self);
            
        }
    }
}
