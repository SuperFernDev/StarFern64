using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RailShooter
{
    public class SpinAndBob : MonoBehaviour
    {
        [SerializeField] float rotationSpeed;
        [SerializeField] int bobSpeed;
        [SerializeField] float bobHeight;
        Vector3 startPos;

        void Start()
        {
            startPos = transform.position;
        }

        // Update is called once per frame
        void Update()
        {
            Rotate();
            Bob();
        }

        void Rotate()
        {
            transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.Self);
            
        }

        void Bob()
        {
            transform.position = new Vector3(startPos.x, startPos.y + (bobHeight * Mathf.Sin(Time.time * bobSpeed)), startPos.z);
        }
    }
}
