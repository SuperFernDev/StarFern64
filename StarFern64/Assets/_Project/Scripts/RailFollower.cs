using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace RailShooter
{
    public class RailFollower : MonoBehaviour
    {
        [SerializeField] Transform player;
        [SerializeField] Transform followTarget;
        [SerializeField] float followDistance = 22f;
        [SerializeField] float smoothTime = 0.2f;

        Vector3 velocity;

        // Update is called once per frame
        void LateUpdate()
        {
            Vector3 targetPos = followTarget.position + followTarget.forward * -followDistance;
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);

            // Set the camera's rotation to match the player's rotation;
            transform.rotation = player.rotation;
        }
    }
}
