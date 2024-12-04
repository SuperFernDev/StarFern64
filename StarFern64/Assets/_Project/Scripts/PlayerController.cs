using DG.Tweening;
using KBCore.Refs;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace RailShooter
{
    public class PlayerController : ValidatedMonoBehaviour
    {
        [SerializeField, Self] InputReader input;

        [SerializeField] Transform followTarget;
        [SerializeField] Transform aimTarget;

        [SerializeField] Transform playerModel;
        [SerializeField] float followDistance;
        [SerializeField] Vector2 movementLimit = new Vector2(2f, 2f);
        [SerializeField] float movementRange = 5f;
        [SerializeField] float movementSpeed = 10f;
        [SerializeField] float smoothTime = 0.2f;

        [SerializeField] float maxRoll = 15f;
        [SerializeField] float rollSpeed = 2f;
        [SerializeField] float rollDuration = 1f;

        [SerializeField] Transform modelParent;
        [SerializeField] float rotationSpeed = 10f;

        Vector3 velocity;
        [SerializeField] float roll;

        void Awake()
        {
            input.LeftTap += OnLeftTap;
            input.RightTap += OnRightTap;
        }

        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
           HandlePosition();
           HandleRoll();
           HandleRotation();
        }

        void OnLeftTap() => BarrelRoll();
        void OnRightTap() => BarrelRoll(1);

        private void HandlePosition()
        {
            // Calculate the target pos based on follow distance and the target's position
            Vector3 targetPosition = followTarget.position + followTarget.forward * -followDistance;

            // Apply smooth damp to the player's position
            Vector3 smoothedPos = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

            // Calculate the new local position 
            Vector3 localPos = transform.InverseTransformPoint(smoothedPos);
            localPos.x += input.Move.x * movementSpeed * Time.deltaTime * movementRange;
            localPos.y += input.Move.y * movementSpeed * Time.deltaTime * movementRange;

            // Clamp the local position
            localPos.x = Mathf.Clamp(localPos.x, -movementLimit.x, movementLimit.x);
            localPos.y = Mathf.Clamp(localPos.y, -movementLimit.y, movementLimit.y);

            // Update player's position
            transform.position = transform.TransformPoint(localPos);
        }

        private void HandleRoll()
        {
            // Match the player's rptation to the follow target's rotation 
            transform.rotation = followTarget.rotation;

            // Match the roll based on player input
            //roll = Mathf.Lerp(rollSpeed, input.Move.x * maxRoll, Time.deltaTime * rollSpeed);
            roll = Mathf.Lerp(roll, input.Move.x * maxRoll, Time.deltaTime * rollSpeed);
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, roll);
        }
        private void HandleRotation()
        {
            //Determine direction to the target
            Vector3 direction = aimTarget.position - transform.position;

            //Calculate the rotation required to look at the target
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            modelParent.rotation = Quaternion.Lerp(modelParent.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        private void BarrelRoll(int direction = -1)
        {
            if (!DOTween.IsTweening(playerModel))
            {
                playerModel.DOLocalRotate(
                    new Vector3(playerModel.localEulerAngles.x, playerModel.localEulerAngles.y, 360 * direction),
                    rollDuration, RotateMode.LocalAxisAdd).SetEase(Ease.OutCubic);
            }
        }

        void OnDestroy()
        {
            input.LeftTap -= OnLeftTap;
            input.RightTap -= OnRightTap;
        }
    }
}
