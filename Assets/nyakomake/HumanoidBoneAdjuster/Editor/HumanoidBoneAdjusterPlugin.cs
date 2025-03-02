using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Object;
using nadena.dev.ndmf;
using nyakomake;
using System;
using System.Linq;
using UnityEditor;
using VRC.SDKBase;
[assembly: ExportsPlugin(typeof(HumanoidBoneAdjusterPlugin))]

namespace nyakomake
{
    public class HumanoidBoneAdjusterPlugin : Plugin<HumanoidBoneAdjusterPlugin>
    {
        protected override void Configure()
        {
            InPhase(BuildPhase.Transforming)
                .BeforePlugin("nadena.dev.modular-avatar")
                .Run("nyakomake.humanoidBoneAdjuster", ctx =>
                {
                    //Transform boneTransform =  ctx.AvatarRootObject.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.LeftLowerLeg);
                    //boneTransform.position = new Vector3(boneTransform.position.x, boneTransform.position.y-1f,boneTransform.position.z);
                    var humanoidBoneAdjusters = ctx.AvatarRootObject.GetComponentsInChildren<HumanoidBoneAdjuster>();
                    if (humanoidBoneAdjusters != null && humanoidBoneAdjusters.Length > 0)
                    {
                        foreach (HumanoidBoneAdjuster bone in humanoidBoneAdjusters)
                        {
                            bone.ApplyChangePosRotHumanBone();
                        }
                        float eyeYOffset;
                        Avatar avatar = CreateHumanoidBoneAdjustAvatar(ctx.AvatarRootObject, humanoidBoneAdjusters,out eyeYOffset);
                        if (avatar == null) //Debug.Log("avatar is null!");

                        ctx.AssetSaver.SaveAsset(avatar);
                        DestroyImmediate(ctx.AvatarRootObject.GetComponent<Animator>());
                        ctx.AvatarRootObject.AddComponent<Animator>();
                        ctx.AvatarRootObject.GetComponent<Animator>().applyRootMotion = true;
                        ctx.AvatarRootObject.GetComponent<Animator>().avatar = avatar;

                        Vector3 viewPos = ctx.AvatarRootObject.GetComponent<VRC_AvatarDescriptor>().ViewPosition;

                        ctx.AvatarRootObject.GetComponent<VRC_AvatarDescriptor>().ViewPosition = new Vector3(viewPos.x, viewPos.y+eyeYOffset, viewPos.z);
                    }

                    foreach (HumanoidBoneAdjuster humanoidBoneAdjuster in humanoidBoneAdjusters)
                    {
                        DestroyImmediate(humanoidBoneAdjuster);
                    }
                });
        }

        // public struct ChangePosRotHumanBone_isRotOnly
        // {
        //     public HumanBodyBones humanBodyBones;
        //     public Transform refPosRotTransform;
        //     public bool isRotOnly;
        // }

        //public GameObject sourceObject;
        //List<Transform> transformsToKeep;

        //public HumanoidBoneAdjuster[] humanoidBoneAdjusters;
        Avatar CreateHumanoidBoneAdjustAvatar(GameObject sourceObject, HumanoidBoneAdjuster[] humanoidBoneAdjusters,out float eyeYOffset)
        {
            var sourceObject_clone = Instantiate(sourceObject);
            listbone(sourceObject_clone);
            ExecuteDeleteObjectWithoutList(sourceObject_clone.transform);
            HumanoidAvatarBuilder humanoidAvatarBuilder = new HumanoidAvatarBuilder();
            humanoidAvatarBuilder.SetAvatarObj(sourceObject_clone);

            List<ChangePosRotHumanBone> changePosRotHumanBones_ = new List<ChangePosRotHumanBone>();
            foreach (HumanoidBoneAdjuster bone in humanoidBoneAdjusters)
            {
                ChangePosRotHumanBone bone_ = new ChangePosRotHumanBone();
                bone_.refPosRotTransform = bone.refPosRotTransform;
                bone_.humanBodyBones = bone.humanBodyBones;
                changePosRotHumanBones_.Add(bone_);
            }
            eyeYOffset = 0f;
            Avatar remapAvatar = humanoidAvatarBuilder.CreateBonePosRotChangeAvatar(changePosRotHumanBones_,out eyeYOffset);
            DestroyImmediate(sourceObject_clone);
            return remapAvatar;

        }

        public void ApplyChangePosRotHumanBone(GameObject sourceObject, HumanoidBoneAdjuster[] humanoidBoneAdjusters)
        {
            Animator animator = sourceObject.GetComponent<Animator>();
            foreach (HumanoidBoneAdjuster bone in humanoidBoneAdjusters)
            {
                Transform boneTransform = animator.GetBoneTransform(bone.humanBodyBones);
                if (bone.adjustType == HumanoidBoneAdjuster.AdjustType.RotationOnly) boneTransform.SetPositionAndRotation(boneTransform.position, bone.refPosRotTransform.rotation);
                if (bone.adjustType == HumanoidBoneAdjuster.AdjustType.PositionOnly) boneTransform.SetPositionAndRotation(bone.refPosRotTransform.position, boneTransform.rotation);
                else
                {

                    boneTransform.SetPositionAndRotation(bone.refPosRotTransform.position, bone.refPosRotTransform.rotation);
                    //Debug.Log("adjust : " + bone.refPosRotTransform.name);
                    //Debug.Log("adjustBone : " + boneTransform.name);
                    //Debug.Log("adjustPos : " + boneTransform.position.x + "," + boneTransform.position.y + "," + boneTransform.position.z);
                }
            }
        }
        public void RevertChangePosRotHumanBone(GameObject sourceObject)
        {
            Animator animator = sourceObject.GetComponent<Animator>();
            var humanBodyBonesList = (HumanBodyBones[])Enum.GetValues(typeof(HumanBodyBones));
            foreach (HumanBodyBones bone in humanBodyBonesList)
            {
                if (bone == HumanBodyBones.LastBone) continue;
                Transform boneTransform = animator.GetBoneTransform(bone);
                if (boneTransform != null)
                {
                    RevertPosRotTransform(boneTransform);
                }
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

        [ContextMenu("List Avatar Objects")]
        void listbone(GameObject sourceObj)
        {
            //keepTransforms.Clear();
            //keepTransforms.AddRange(transformsToKeep);
            LogMappedBones(GetMappedBones(sourceObj));
            keepTransforms = GetMappedBones2(sourceObj);
        }

        public static Dictionary<HumanBodyBones, Transform> GetMappedBones(GameObject targetObject)
        {
            Dictionary<HumanBodyBones, Transform> boneMap = new Dictionary<HumanBodyBones, Transform>();
            Animator animator = targetObject.GetComponent<Animator>();

            if (animator == null || animator.avatar == null || !animator.avatar.isValid)
            {
                //Debug.LogError("指定されたGameObjectに有効なAnimatorとAvatarが設定されていません。");
                return boneMap;
            }

            // Humanoidボーンの種類を列挙
            foreach (HumanBodyBones bone in System.Enum.GetValues(typeof(HumanBodyBones)))
            {
                if (bone == HumanBodyBones.LastBone) continue; // LastBoneはスキップ

                Transform boneTransform = animator.GetBoneTransform(bone);
                if (boneTransform != null)
                {
                    boneMap.Add(bone, boneTransform);
                }
                else
                {
                    //Debug.LogWarning($"Humanoidボーン {bone} は見つかりませんでした。");
                }

            }

            return boneMap;
        }
        public static List<Transform> GetMappedBones2(GameObject targetObject)
        {
            List<Transform> boneMap = new List<Transform>();
            Animator animator = targetObject.GetComponent<Animator>();

            if (animator == null || animator.avatar == null || !animator.avatar.isValid)
            {
                //Debug.LogError("指定されたGameObjectに有効なAnimatorとAvatarが設定されていません。");
                return boneMap;
            }

            // Humanoidボーンの種類を列挙
            foreach (HumanBodyBones bone in System.Enum.GetValues(typeof(HumanBodyBones)))
            {
                if (bone == HumanBodyBones.LastBone) continue; // LastBoneはスキップ

                Transform boneTransform = animator.GetBoneTransform(bone);
                if (boneTransform != null)
                {
                    boneMap.Add(boneTransform);
                }
                else
                {
                    //Debug.LogWarning($"Humanoidボーン {bone} は見つかりませんでした。");
                }

            }

            return boneMap;
        }


        public static void LogMappedBones(Dictionary<HumanBodyBones, Transform> boneMap)
        {
            if (boneMap.Count == 0)
            {
                //Debug.Log("マッピングされたボーンがありません。");
                return;
            }

            //Debug.Log("--- Bone Mappings ---");
            foreach (KeyValuePair<HumanBodyBones, Transform> pair in boneMap)
            {
                //Debug.Log($"{pair.Key}: {pair.Value.name} (Path: {GetTransformPath(pair.Value)})");
            }
        }



        // Transform の絶対パスを文字列で取得するヘルパー関数
        static string GetTransformPath(Transform transform)
        {
            if (transform.parent == null)
            {
                return transform.name;
            }
            else
            {
                return GetTransformPath(transform.parent) + "/" + transform.name;
            }

        }
        private List<Transform> keepTransforms;

        [ContextMenu("Delete List Objects")]
        public void ExecuteDeleteObjectWithoutList(Transform rootObj)
        {
            List<Transform> keepTransforms_ = new List<Transform>();

            foreach (Transform keepTransform in keepTransforms)
            {
                keepTransforms_.Add(keepTransform);
                var parentList = GetAllParent(keepTransform);
                keepTransforms_ = keepTransforms_.Concat(parentList).ToList();
            }
            keepTransforms_ = keepTransforms_.Distinct().ToList();
            foreach (Transform keepTransform in keepTransforms_)
            {
                //Debug.Log(keepTransform.name);
            }
            List<Transform> childTransforms = GetAllChildren(rootObj);
            foreach (Transform childTransform in childTransforms)
            {
                if (!keepTransforms_.Contains(childTransform))
                {
                    if (childTransform != null) DestroyImmediate(childTransform.gameObject);
                }
            }
        }

        void DeleteObjectWithoutList(Transform parent, ref bool isKeep)
        {

            for (int i = parent.childCount - 1; i > 0; i--)
            {
                var child = parent.GetChild(i);
                //Debug.Log(child.name);
                if (keepTransforms.Contains(child)) isKeep = isKeep || true;
                var isChildKeep = false;
                DeleteObjectWithoutList(child, ref isChildKeep);

                if ((!isKeep && !isChildKeep) || (child.childCount == 0 && !keepTransforms.Contains(child)))
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }

        public static List<Transform> GetAllChildren(Transform root)
        {
            List<Transform> children = new List<Transform>();
            GetChildrenRecursive(root, children);
            return children;
        }

        private static void GetChildrenRecursive(Transform parent, List<Transform> children)
        {
            foreach (Transform child in parent)
            {
                children.Add(child);
                GetChildrenRecursive(child, children);
            }
        }

        public static List<Transform> GetAllParent(Transform root)
        {
            List<Transform> children = new List<Transform>();
            GetParentRecursive(root, children);
            return children;
        }
        private static void GetParentRecursive(Transform child, List<Transform> parents)
        {
            if (child.parent == null) return;
            parents.Add(child.parent);
            GetParentRecursive(child.parent, parents);

        }
    }
}