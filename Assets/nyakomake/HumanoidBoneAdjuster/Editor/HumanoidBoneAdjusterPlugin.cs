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
using nadena.dev.modular_avatar.core;
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
                    Debug.Log("nyakomake.humanoidBoneAdjuster run");
                    Transform hipsTransform = ctx.AvatarRootObject.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Hips);
                    Transform armatureTransform = hipsTransform.parent;
                    List<Transform> armatureChildren = new List<Transform>();
                    foreach(Transform child in armatureTransform)
                    {
                        child.SetParent(null);
                        armatureChildren.Add(child);

                    }
                    armatureTransform.rotation = Quaternion.identity;
                    foreach(Transform child in armatureChildren)
                    {
                        child.SetParent(armatureTransform);

                    }
                    
                    var humanoidBoneAdjusters = ctx.AvatarRootObject.GetComponentsInChildren<HumanoidBoneAdjuster>();
                    if (humanoidBoneAdjusters != null && humanoidBoneAdjusters.Length > 0)
                    {
                        foreach (HumanoidBoneAdjuster bone in humanoidBoneAdjusters)
                        {
                            bone.ApplyChangePosRotHumanBone();
                        }
                        var replaceAvatarBones = ctx.AvatarRootObject.GetComponentsInChildren<ReplaceAvatarBone>();
                        float eyeYOffset = float.MaxValue;
                        foreach (ReplaceAvatarBone bone in replaceAvatarBones)
                        {
                            bone.ApplyChangePosRotHumanBone();
                            if(bone.gameObject.GetComponent<ModularAvatarBoneProxy>() != null)
                            {
                                if(bone.gameObject.GetComponent<ModularAvatarBoneProxy>().boneReference == HumanBodyBones.LeftToes)
                                {
                                    eyeYOffset = -bone.gameObject.transform.position.y;
                                    Debug.Log("!eyeYOffset! : "+eyeYOffset);
                                }
                            }
                        }
                        Avatar avatar = CreateHumanoidBoneAdjustAvatar(ctx.AvatarRootObject, humanoidBoneAdjusters, ref eyeYOffset);
                        if (avatar == null) Debug.Log("avatar is null!");

                        //ctx.AssetSaver.SaveAsset(avatar);
                        //armatureTransform.position = new Vector3(armatureTransform.position.x,armatureTransformTransform.position.y+eyeYOffset,armatureTransform.position.z);
                        DestroyImmediate(ctx.AvatarRootObject.GetComponent<Animator>());
                        ctx.AvatarRootObject.AddComponent<Animator>();
                        ctx.AvatarRootObject.GetComponent<Animator>().applyRootMotion = true;
                        ctx.AvatarRootObject.GetComponent<Animator>().avatar = avatar;

                        Vector3 viewPos = ctx.AvatarRootObject.GetComponent<VRC_AvatarDescriptor>().ViewPosition;

                        ctx.AvatarRootObject.GetComponent<VRC_AvatarDescriptor>().ViewPosition = new Vector3(viewPos.x, viewPos.y + eyeYOffset, viewPos.z);
                    }

                    foreach (HumanoidBoneAdjuster humanoidBoneAdjuster in humanoidBoneAdjusters)
                    {
                        DestroyImmediate(humanoidBoneAdjuster);
                    }
                });
        }

        Avatar CreateHumanoidBoneAdjustAvatar(GameObject sourceObject, HumanoidBoneAdjuster[] humanoidBoneAdjusters, ref float eyeYOffset)
        {
            Debug.Log("Bone Clone");
            Transform hipsTransform = sourceObject.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Hips);
            Debug.Log(hipsTransform.parent.gameObject.name);
            var armature_clone = Instantiate(hipsTransform.parent.gameObject);
            var sourceObject_clone = new GameObject("sourceObject");
            armature_clone.transform.SetParent(sourceObject_clone.transform);
            armature_clone.name = hipsTransform.parent.name;

            //var sourceObject_clone= Instantiate(sourceObject);

            sourceObject_clone.AddComponent<Animator>();
            sourceObject_clone.GetComponent<Animator>().applyRootMotion = true;
            sourceObject_clone.GetComponent<Animator>().avatar = sourceObject.GetComponent<Animator>().avatar;

            var humanoidBoneListInObject = GetMappedBoneList(sourceObject_clone);
            ExecuteDeleteObjectWithoutList(sourceObject_clone.transform, humanoidBoneListInObject);
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
            //eyeYOffset = 0f;
            Avatar remapAvatar = humanoidAvatarBuilder.CreateBonePosRotChangeAvatar(changePosRotHumanBones_, ref eyeYOffset);
            DestroyImmediate(sourceObject_clone);
            return remapAvatar;

        }


        // void listbone(GameObject sourceObj)
        // {
        //     LogMappedBones(GetMappedBoneDic(sourceObj));
        //     keepTransforms = GetMappedBoneList(sourceObj);
        // }

        public static Dictionary<HumanBodyBones, Transform> GetMappedBoneDic(GameObject targetObject)
        {
            Dictionary<HumanBodyBones, Transform> boneMap = new Dictionary<HumanBodyBones, Transform>();
            Animator animator = targetObject.GetComponent<Animator>();

            if (animator == null || animator.avatar == null || !animator.avatar.isValid)
            {
                Debug.LogError("有効なAnimatorとAvatarが設定されていません。");
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
                    Debug.Log($"Humanoidボーン {bone} は見つかりませんでした。");
                }

            }

            return boneMap;
        }
        public static List<Transform> GetMappedBoneList(GameObject targetObject)
        {
            List<Transform> boneMap = new List<Transform>();
            Animator animator = targetObject.GetComponent<Animator>();

            if (animator == null || animator.avatar == null || !animator.avatar.isValid)
            {
                Debug.LogError("有効なAnimatorとAvatarが設定されていません。");
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
                    Debug.Log($"Humanoidボーン {bone} は見つかりませんでした。");
                }

            }

            return boneMap;
        }


        public static void LogMappedBones(Dictionary<HumanBodyBones, Transform> boneMap)
        {
            Debug.Log("--- Bone Mappings ---");
            if (boneMap.Count == 0)
            {
                Debug.Log("マッピングされたボーンがありません。");
                return;
            }

            foreach (KeyValuePair<HumanBodyBones, Transform> pair in boneMap)
            {
                Debug.Log($"{pair.Key}: {pair.Value.name} (Path: {GetTransformPath(pair.Value)})");
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
        //private List<Transform> keepTransforms;

        void ExecuteDeleteObjectWithoutList(Transform rootObj, List<Transform> keepTransforms)
        {
            List<Transform> keepTransforms_parentFix = new List<Transform>();

            foreach (Transform keepTransform in keepTransforms)
            {
                keepTransforms_parentFix.Add(keepTransform);
                var parentList = GetAllParent(keepTransform);
                keepTransforms_parentFix = keepTransforms_parentFix.Concat(parentList).ToList();
            }
            keepTransforms_parentFix = keepTransforms_parentFix.Distinct().ToList();
            foreach (Transform keepTransform in keepTransforms_parentFix)
            {
                Debug.Log(keepTransform.name);
            }
            List<Transform> childTransforms = GetAllChildren(rootObj);
            foreach (Transform childTransform in childTransforms)
            {
                if (!keepTransforms_parentFix.Contains(childTransform))
                {
                    if (childTransform != null) DestroyImmediate(childTransform.gameObject);
                }
            }
        }

        /*
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
        */
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