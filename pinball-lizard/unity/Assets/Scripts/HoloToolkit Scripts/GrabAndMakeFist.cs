﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

#if UNITY_WSA && UNITY_2017_2_OR_NEWER
using UnityEngine.XR.WSA.Input;
#endif

namespace HoloToolkit.Unity.InputModule.Examples.Grabbables
{
    /// <summary>
    /// Extends its behaviour from BaseGrabber. This is non-abstract script that's actually attached to the gameObject that will
    /// be grabbing/carrying the object. 
    /// </summary>
    public class GrabAndMakeFist : BaseGrabber
    {
        [SerializeField]
        private LayerMask grabbableLayers = ~0;

#if UNITY_WSA && UNITY_2017_2_OR_NEWER
        [SerializeField]
        private InteractionSourcePressType pressType;
#endif

        public GameObject _myFist;
        public Animator _handAnimator;


        ///Subscribe GrabStart and GrabEnd to InputEvents for GripPressed
        protected override void OnEnable()
        {
            base.OnEnable();
#if UNITY_WSA && UNITY_2017_2_OR_NEWER
            InteractionManager.InteractionSourcePressed += InteractionSourcePressed;
            InteractionManager.InteractionSourceReleased += InteractionSourceReleased;
#endif
        }

        protected override void OnDisable()
        {
#if UNITY_WSA && UNITY_2017_2_OR_NEWER
            InteractionManager.InteractionSourcePressed -= InteractionSourcePressed;
            InteractionManager.InteractionSourceReleased -= InteractionSourceReleased;
#endif
            base.OnDisable();
        }

#if UNITY_WSA && UNITY_2017_2_OR_NEWER
        private void InteractionSourcePressed(InteractionSourcePressedEventArgs obj)
        {
            if (obj.pressType == pressType && obj.state.source.handedness == handedness)
            {
                GrabStart();
                _myFist.SetActive(true);
                _handAnimator.SetTrigger("HandGrab");
            }
        }

        private void InteractionSourceReleased(InteractionSourceReleasedEventArgs obj)
        {
            if (obj.pressType == pressType && obj.state.source.handedness == handedness)
            {
                TrySetThrowableObject(GrabbedObjects.Count > 0 ? GrabbedObjects[0] : null, obj.state.sourcePose);
                GrabEnd();
                _myFist.SetActive(false);
                _handAnimator.SetTrigger("HandRelease");

            }
        }
#endif

        /// <summary>
        /// Controller grabbers find available grabbable objects via triggers
        /// </summary>
        /// <param name="other"></param>
        protected virtual void OnTriggerEnter(Collider other)
        {
            //Debug.Log("Entered trigger with " + other.name);
            if (((1 << other.gameObject.layer) & grabbableLayers.value) == 0)
            {
                return;
            }

            BaseGrabbable bg = other.GetComponent<BaseGrabbable>();
            if (bg == null && other.attachedRigidbody != null)
            {
                bg = other.attachedRigidbody.GetComponent<BaseGrabbable>();
            }

            if (bg == null)
            {
                return;
            }

            //Debug.Log("Adding contact");

            AddContact(bg);
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            //Debug.Log("Exited trigger with " + other.name);
            if (((1 << other.gameObject.layer) & grabbableLayers.value) == 0)
            {
                return;
            }

            BaseGrabbable bg = other.GetComponent<BaseGrabbable>();
            if (bg == null && other.attachedRigidbody != null)
            {
                bg = other.attachedRigidbody.GetComponent<BaseGrabbable>();
            }

            if (bg == null)
            {
                return;
            }

            //Debug.Log("Removing contact");

            RemoveContact(bg);
        }

#if UNITY_WSA && UNITY_2017_2_OR_NEWER
        public bool TrySetThrowableObject(BaseGrabbable grabbable, InteractionSourcePose poseInfo)
        {
            if (grabbable == null)
            {
                return false;
            }

            if (!grabbable.GetComponent<BaseThrowable>())
            {
                return false;
            }

            if (!grabbable.GetComponent<Rigidbody>())
            {
                return false;
            }

            Rigidbody rb = grabbable.GetComponent<Rigidbody>();
            //Debug.Log("name of our rb.center of mass ========= " + rb.name);
            ControllerReleaseData controlReleaseData = grabbable.GetComponent<Rigidbody>().GetThrowReleasedVelocityAndAngularVelocity(rb.centerOfMass, poseInfo);

            //grabbable.GetComponent<BaseThrowable>().LatestControllerThrowVelocity = vel;
            //grabbable.GetComponent<BaseThrowable>().LatestControllerThrowAngularVelocity = vel;

            grabbable.GetComponent<BaseThrowable>().LatestControllerThrowVelocity = controlReleaseData.Velocity;
            grabbable.GetComponent<BaseThrowable>().LatestControllerThrowAngularVelocity = controlReleaseData.AngleVelocity;
            grabbable.GetComponent<TrailRenderer>().enabled = true;
            return true;
        }
#endif
    }
}