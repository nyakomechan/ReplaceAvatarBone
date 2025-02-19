using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;
using System;
using VRC.SDKBase;

namespace nyakomake
{
    public class HumanoidBoneAdjuster : MonoBehaviour, IEditorOnly
    {
        public enum AdjustType
        {
            PositionAndRotation,
            RotationOnly,
            PositionOnly
        }
        public GameObject rootObject;


        public HumanBodyBones humanBodyBones;
        public Transform refPosRotTransform;
        public AdjustType adjustType;



        void Reset()
        {
            refPosRotTransform = transform;
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
        [ContextMenu("GetRootObject")]
        GameObject GetRootObject()
        {
            var rootObject = gameObject;
            while (rootObject != null)
            {
                if (rootObject.GetComponent<VRC_AvatarDescriptor>() != null) break;
                rootObject = rootObject.transform.parent.gameObject;
            }
            return rootObject;
        }

        [ContextMenu("ApplyChangePosRotHumanBone")]
        public void ApplyChangePosRotHumanBone()
        {
            rootObject = GetRootObject();
            Animator animator = rootObject.GetComponent<Animator>();

            Transform boneTransform = animator.GetBoneTransform(humanBodyBones);
            // if (adjustType == AdjustType.RotationOnly) boneTransform.SetPositionAndRotation(boneTransform.position, transform.rotation);
            // if (adjustType == AdjustType.PositionOnly) boneTransform.SetPositionAndRotation(transform.position, boneTransform.rotation);
            // else boneTransform.SetPositionAndRotation(transform.position, transform.rotation);
            if (adjustType == AdjustType.RotationOnly)
            {
                boneTransform.rotation = new Quaternion(transform.rotation.x,transform.rotation.y,transform.rotation.z,transform.rotation.w);
            }
            if (adjustType == AdjustType.PositionOnly)
            {
                boneTransform.position = new Vector3(transform.position.x,transform.position.y,transform.position.z);
            }
            else
            {
                boneTransform.rotation = new Quaternion(transform.rotation.x,transform.rotation.y,transform.rotation.z,transform.rotation.w);
                boneTransform.position = new Vector3(transform.position.x,transform.position.y,transform.position.z);
            }

            Debug.Log("adjustBone : " + boneTransform.name);
            Debug.Log("adjustPos : " + boneTransform.position.x + "," + boneTransform.position.y + "," + boneTransform.position.z);

        }
        [ContextMenu("RevertChangePosRotHumanBone")]
        public void RevertChangePosRotHumanBone()
        {
            rootObject = GetRootObject();
            Animator animator = rootObject.GetComponent<Animator>();
            if (humanBodyBones == HumanBodyBones.LastBone) return;
            Transform boneTransform = animator.GetBoneTransform(humanBodyBones);
            if (boneTransform != null)
            {
                RevertPosRotTransform(boneTransform);
            }

        }
        void RevertPosRotTransform(Transform revertTransform)
        {
            SerializedObject serializedObject = new SerializedObject(revertTransform);
            SerializedProperty sp = serializedObject.FindProperty("m_LocalPosition");
            sp.prefabOverride = false;
            sp.serializedObject.ApplyModifiedProperties();
            sp = serializedObject.FindProperty("m_LocalRotation");
            sp.prefabOverride = false;
            sp.serializedObject.ApplyModifiedProperties();
        }
    }
}
