using System.Diagnostics.Tracing;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using VRC.SDK3.Avatars.Components;
using nadena.dev.modular_avatar.core;

namespace nyakomake
{
    public struct ChangePosRotHumanBone
    {
        public HumanBodyBones humanBodyBones;
        public Transform refPosRotTransform;
    }
    public class HumanoidAvatarBuilder
    {
        #region ### Veriables ###


        private Avatar _srcAvatar;



        private GameObject _avatar;



        private Avatar _baseAvatar;


        private float _heightOffset = 0;




        private float _hipsHeightOffset = 0;


        #region ### ボーンへの参照 ###

        private Transform _root;

        private Transform _hips;
        private Transform _leftUpperLeg;
        private Transform _rightUpperLeg;
        private Transform _leftLowerLeg;
        private Transform _rightLowerLeg;
        private Transform _leftFoot;
        private Transform _rightFoot;
        private Transform _spine;
        private Transform _chest;
        private Transform _upperChest;
        private Transform _neck;
        private Transform _head;
        private Transform _leftShoulder;
        private Transform _rightShoulder;
        private Transform _leftUpperArm;
        private Transform _rightUpperArm;
        private Transform _leftLowerArm;
        private Transform _rightLowerArm;
        private Transform _leftHand;
        private Transform _rightHand;
        private Transform _leftToes;
        private Transform _rightToes;
        private Transform _leftEye;
        private Transform _rightEye;
        private Transform _jaw;
        private Transform _leftThumbProximal;
        private Transform _leftThumbIntermediate;
        private Transform _leftThumbDistal;
        private Transform _leftIndexProximal;
        private Transform _leftIndexIntermediate;
        private Transform _leftIndexDistal;
        private Transform _leftMiddleProximal;
        private Transform _leftMiddleIntermediate;
        private Transform _leftMiddleDistal;
        private Transform _leftRingProximal;
        private Transform _leftRingIntermediate;
        private Transform _leftRingDistal;
        private Transform _leftLittleProximal;
        private Transform _leftLittleIntermediate;
        private Transform _leftLittleDistal;
        private Transform _rightThumbProximal;
        private Transform _rightThumbIntermediate;
        private Transform _rightThumbDistal;
        private Transform _rightIndexProximal;
        private Transform _rightIndexIntermediate;
        private Transform _rightIndexDistal;
        private Transform _rightMiddleProximal;
        private Transform _rightMiddleIntermediate;
        private Transform _rightMiddleDistal;
        private Transform _rightRingProximal;
        private Transform _rightRingIntermediate;
        private Transform _rightRingDistal;
        private Transform _rightLittleProximal;
        private Transform _rightLittleIntermediate;
        private Transform _rightLittleDistal;
        #endregion ### ボーンへの参照 ###

        private Dictionary<string, string> _transformDefinision = new Dictionary<string, string>();
        private List<Transform> _skeletonBones = new List<Transform>();
        private Dictionary<string, Transform> _skeletonBonesDic = new Dictionary<string, Transform>();

        private HumanPoseHandler _srchandler;

        public VRCAvatarDescriptor _avatarDescripter;
        #endregion ### Veriables ###

        public void SetAvatarObj(GameObject avatarObj)
        {
            _avatar = avatarObj;
        }





        public Avatar CreateBonePosRotChangeAvatar(List<ChangePosRotHumanBone> changePosRotHumanBone)
        {
            //CacheBoneNameMap(BoneNameConvention.FBX, _assetName);
            if (_baseAvatar == null) SetBaseAvatarAsset();
            if (_root == null) SetRoot();

            if (_avatarDescripter == null) SetAvatarDesc();

            SetupSkeleton();
            SetupSkeletonDic();
            SetupBones_PosRotChange(changePosRotHumanBone);
            ReadAvatar();
            BuildRemapAvatar();
            return _srcAvatar;
            //SetupLineRenderers();
        }

        [ContextMenu("Find hips")]
        public void SetRoot()
        {
            HumanBone[] basehumanBones = _baseAvatar.humanDescription.human;

            string hipsBoneName = "";
            foreach (var hb in basehumanBones)
            {
                if (hb.humanName == "Hips")
                {
                    hipsBoneName = hb.boneName;
                    Debug.Log(hipsBoneName);
                }
            }

            Transform hips = RecursiveTransformFind(_avatar.transform, hipsBoneName);
            _root = hips.parent;


        }

        private void SetBaseAvatarAsset()
        {
            _baseAvatar = _avatar.GetComponent<Animator>().avatar;
        }
        private void SetAvatarDesc()
        {
            _avatarDescripter = _avatar.GetComponent<VRCAvatarDescriptor>();
        }

        Transform RecursiveTransformFind(Transform current, string name)
        {
            Debug.Log(current.name);
            if (current.name == name) return current;
            Transform tr = null;
            for (int i = 0; i < current.childCount; i++)
            {
                Transform child = current.GetChild(i);
                tr = RecursiveTransformFind(child, name);
                if (tr == null) continue;
                if (tr.name == name) break;
            }
            return tr;
        }







        public void ReadAvatar()
        {
            //SetupBones();
            SetupSkeletonDic();
            ReadHumanoidBoneFromAvatar();
        }

        private void ReadHumanoidBoneFromAvatar()
        {
            HumanBone[] basehumanBones = _baseAvatar.humanDescription.human;

            //humanBonesの名前をキーにして_skeletonBonesからTransformをもってくる
            foreach (var hb in basehumanBones)
            {
                //Debug.Log(hb.humanName);
                //Debug.Log(hb.boneName);
                if (_transformDefinision.ContainsKey(hb.humanName))
                {
                    if (_transformDefinision[hb.humanName] != "") continue;
                    _transformDefinision[hb.humanName] = hb.boneName;

                }

            }

        }


        public List<string> changePosRotBoneNames = new List<string>();
        private void SetupBones_PosRotChange(List<ChangePosRotHumanBone> changePosRotHumanBones)
        {
            _transformDefinision.Clear();
            changePosRotBoneNames.Clear();

            for (int i = 0; i < HumanTrait.BoneCount; i++)
            {
                _transformDefinision.Add(HumanTrait.BoneName[i], "");
            }

            Animator animator = _avatar.GetComponent<Animator>();

            foreach (ChangePosRotHumanBone bone in changePosRotHumanBones)
            {
                if (_transformDefinision.ContainsKey(HumanTrait.BoneName[(int)bone.humanBodyBones]))
                {
                    _transformDefinision[HumanTrait.BoneName[(int)bone.humanBodyBones]] = animator.GetBoneTransform(bone.humanBodyBones).name;
                    changePosRotBoneNames.Add(animator.GetBoneTransform(bone.humanBodyBones).name);
                    Debug.Log("changePosRotBoneName : " + animator.GetBoneTransform(bone.humanBodyBones).name);
                }
            }

        }

        /// <summary>
        /// 再帰的にボーン構造走査して構成を把握する
        /// </summary>
        private void SetupSkeleton()
        {
            _skeletonBones.Clear();
            RecursiveSkeleton(_root, ref _skeletonBones);
        }

        /// <summary>
        /// 再帰的にTransformを走査して、ボーン構造を生成する
        /// </summary>
        /// <param name="current">現在のTransform</param>
        private void RecursiveSkeleton(Transform current, ref List<Transform> skeletons)
        {
            skeletons.Add(current);

            for (int i = 0; i < current.childCount; i++)
            {
                Transform child = current.GetChild(i);
                if (child.gameObject.name.Split('_').Last() != "noSkeleton") RecursiveSkeleton(child, ref skeletons);
            }
        }

        /// <summary>
        /// 再帰的にボーン構造走査して構成を把握する
        /// </summary>
        private void SetupSkeletonDic()
        {
            _skeletonBonesDic.Clear();
            RecursiveSkeletonDic(_root, ref _skeletonBonesDic);
        }

        /// <summary>
        /// 再帰的にTransformを走査して、ボーン構造を生成する
        /// </summary>
        /// <param name="current">現在のTransform</param>
        private void RecursiveSkeletonDic(Transform current, ref Dictionary<string, Transform> skeletons)
        {
            if (!skeletons.ContainsKey(current.name))
            {
                skeletons.Add(current.name, current);
            }


            for (int i = 0; i < current.childCount; i++)
            {
                Transform child = current.GetChild(i);
                if (child.gameObject.name.Split('_').Last() != "noSkeleton")
                {
                    RecursiveSkeletonDic(child, ref skeletons);
                };
                //RecursiveSkeletonDic(child, ref skeletons);
            }
        }


        /// <summary>
        /// アバターのセットアップ
        /// </summary>
        private void BuildRemapAvatar()
        {
            _srcAvatar = null;
            string[] humanTraitBoneNames = HumanTrait.BoneName;

            HumanBone[] basehumanBones = _baseAvatar.humanDescription.human;

            Dictionary<string, HumanBone> basehumanBonesDic = new Dictionary<string, HumanBone>();


            foreach (var hb in basehumanBones)
            {
                basehumanBonesDic.Add(hb.boneName, hb);
            }

            SkeletonBone[] baseSkeltonBones = _baseAvatar.humanDescription.skeleton;

            Dictionary<string, SkeletonBone> baseSkeltonBonesDic = new Dictionary<string, SkeletonBone>();


            foreach (var hb in baseSkeltonBones)
            {
                baseSkeltonBonesDic.Add(hb.name, hb);
            }

            string hipBoneName = "";
            string leftEyeBoneName = "";
            string headBoneName = "";
            Transform hipTransform = _hips;
            Transform leftEyeTransform = _leftEye;
            Transform headTransform = _head;

            List<HumanBone> humanBones = new List<HumanBone>(humanTraitBoneNames.Length);
            for (int i = 0; i < humanTraitBoneNames.Length; i++)
            {
                string humanBoneName = humanTraitBoneNames[i];

                string bone;
                if (_transformDefinision.TryGetValue(humanBoneName, out bone))
                {
                    HumanBone humanBone = new HumanBone();
                    humanBone.humanName = humanBoneName;
                    Debug.Log(humanBoneName);
                    if ((bone == null))
                    {
                        Debug.Log(humanBoneName);
                        continue;
                    }

                    if ((bone == "") && !basehumanBonesDic.ContainsKey(bone)) continue;
                    humanBone.boneName = bone;

                    if (basehumanBonesDic.ContainsKey(bone))
                    {
                        humanBone.limit.useDefaultValues = basehumanBonesDic[bone].limit.useDefaultValues;
                        humanBone.limit = basehumanBonesDic[bone].limit;
                    }
                    else
                    {
                        humanBone.limit.useDefaultValues = true;
                    }

                    humanBones.Add(humanBone);

                    if (humanBoneName == "Hips") hipBoneName = bone;
                    if (leftEyeBoneName == "LeftEye") leftEyeBoneName = bone;
                    if (headBoneName == "Head") headBoneName = bone;
                }
            }

            List<SkeletonBone> skeletonBones = new List<SkeletonBone>(_skeletonBones.Count + 1);

            //bool isAvatarHeightSetting = false;
            Debug.Log("changePosRotBoneNames");
            foreach (string str in changePosRotBoneNames)
            {
                Debug.Log("changePosRotBoneNames : " + str);
            }
            Debug.Log("skeletonSetup ");

            for (int i = 0; i < _skeletonBones.Count; i++)
            {
                Transform bone = _skeletonBones[i];

                if (bone.name == hipBoneName) hipTransform = bone;
                if (bone.name == leftEyeBoneName) leftEyeTransform = bone;
                if (bone.name == headBoneName) headTransform = bone;

                SkeletonBone skelBone = new SkeletonBone();
                skelBone.name = bone.name;
                Debug.Log(bone.name);
                if (changePosRotBoneNames.Contains(bone.name))
                {
                    skelBone.position = bone.localPosition;
                    skelBone.rotation = bone.localRotation;
                    skelBone.scale = bone.localScale;
                    Debug.Log("changePosRotBone is " + bone.name);
                }
                else if (baseSkeltonBonesDic.ContainsKey(bone.name))
                {
                    /*
                    if (!isAvatarHeightSetting)
                    {
                        for (int j = 0; j < bone.childCount; j++)
                        {
                            if (bone.GetChild(j).name == hipBoneName)
                            {
                                //skelBone.position = baseSkeltonBonesDic[bone.name].position + new Vector3(0, _heightOffset, 0);
                                skelBone.position = baseSkeltonBonesDic[bone.name].position;
                                Debug.Log("set height " + skelBone.name);
                                isAvatarHeightSetting = true;
                                break;
                            }
                        }
                        skelBone.position = baseSkeltonBonesDic[bone.name].position;
                    }
                    else
                    {
                        skelBone.position = baseSkeltonBonesDic[bone.name].position;
                    }
                    */

                    if (bone.name == hipBoneName)
                    {
                        float hipsYOffset = 0;
                        if (_leftToes != null && _rightToes != null)
                        {
                            hipsYOffset = _leftToes.position.y - _root.position.y;
                        }

                        skelBone.position = baseSkeltonBonesDic[bone.name].position
                                            + Quaternion.Inverse(baseSkeltonBonesDic[bone.name].rotation) * new Vector3(0, -hipsYOffset + _hipsHeightOffset, 0);
                        Debug.Log("Height offset is " + (hipsYOffset + _hipsHeightOffset));
                    }
                    else
                    {
                        skelBone.position = baseSkeltonBonesDic[bone.name].position;
                    }



                    skelBone.rotation = baseSkeltonBonesDic[bone.name].rotation;
                    skelBone.scale = baseSkeltonBonesDic[bone.name].scale;
                }
                else
                {
                    skelBone.position = bone.localPosition;
                    skelBone.rotation = bone.localRotation;
                    skelBone.scale = bone.localScale;
                }

                skeletonBones.Add(skelBone);



            }

            HumanDescription humanDesc = _baseAvatar.humanDescription;
            humanDesc.human = humanBones.ToArray();
            humanDesc.skeleton = skeletonBones.ToArray();

            humanDesc.hasTranslationDoF = false;

            _srcAvatar = AvatarBuilder.BuildHumanAvatar(_avatar, humanDesc);
            _srcAvatar.name = "AvatarSystem";
            if (_srcAvatar == null) Debug.Log("srcavatar is null!");

            if (!_srcAvatar.isValid || !_srcAvatar.isHuman)
            {
                Debug.LogError("setup error");
                return;
            }

            _srchandler = new HumanPoseHandler(_srcAvatar, _avatar.transform);

        }






    }
}