using KBCore.Refs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RailShooter
{
    public class Reticle : ValidatedMonoBehaviour
    {
        [SerializeField] Transform targetPoint;
        [SerializeField, Self] RectTransform rectTransform;

        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            rectTransform.position = Camera.main.WorldToScreenPoint(targetPoint.position); // Converts a point in world space into screen space;
        }
    }
}
