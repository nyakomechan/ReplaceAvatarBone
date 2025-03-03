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
        private Avatar _srcAvatar;
        private GameObject _avatar;
        private Avatar _baseAvatar;

        private Transform _root;


        private Dictionary<string, string> _transformDefinision = new Dictionary<string, string>();
        private List<Transform> _skeletonBones = new List<Transform>();
        //private Dictionary<string, Transform> _skeletonBonesDic = new Dictionary<string, Transform>();

        private HumanPoseHandler _srchandler;

        public VRCAvatarDescriptor _avatarDescripter;

        public void SetAvatarObj(GameObject avatarObj)
        {
            _avatar = avatarObj;
        }

        public Avatar CreateBonePosRotChangeAvatar(List<ChangePosRotHumanBone> changePosRotHumanBone, out float eyeYOffset)
        {
            if (_baseAvatar == null) SetBaseAvatarAsset();
            if (_root == null) SetRoot();

            if (_avatarDescripter == null) SetAvatarDesc();

            SetupSkeleton();
            //SetupSkeletonDic();
            SetupBones_PosRotChange(changePosRotHumanBone);
            ReadAvatar();
            BuildRemapAvatar(out eyeYOffset);
            return _srcAvatar;
        }

        //_baseAvatarを元に_avatarオブジェクトからHipsボーンの親（Armatureオブジェクト）を引っ張ってくる

        //[ContextMenu("Find hips")]
        public void SetRoot()
        {
            HumanBone[] basehumanBones = _baseAvatar.humanDescription.human;

            string hipsBoneName = "";
            foreach (var hb in basehumanBones)
            {
                if (hb.humanName == "Hips")
                {
                    hipsBoneName = hb.boneName;
                }
            }

            Transform hips = RecursiveTransformFind(_avatar.transform, hipsBoneName);
            _root = hips.parent;


        }


        Transform RecursiveTransformFind(Transform current, string name)
        {
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

        private void SetBaseAvatarAsset()
        {
            _baseAvatar = _avatar.GetComponent<Animator>().avatar;
        }
        private void SetAvatarDesc()
        {
            _avatarDescripter = _avatar.GetComponent<VRCAvatarDescriptor>();
        }

        public void ReadAvatar()
        {
            //SetupSkeletonDic();
            ReadHumanoidBoneFromAvatar();
        }


        private void ReadHumanoidBoneFromAvatar()
        {
            HumanBone[] basehumanBones = _baseAvatar.humanDescription.human;

            foreach (var hb in basehumanBones)
            {

                if (_transformDefinision.ContainsKey(hb.humanName))
                {
                    if (_transformDefinision[hb.humanName] != "") continue;
                    _transformDefinision[hb.humanName] = hb.boneName;

                }

            }

        }

        //HumanBone構成用辞書の_transformDefinisionのセットアップ
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
                    //Debug.Log("changePosRotBoneName : " + animator.GetBoneTransform(bone.humanBodyBones).name);
                }
            }

        }


        private void SetupSkeleton()
        {
            _skeletonBones.Clear();
            RecursiveSkeleton(_root, ref _skeletonBones);
        }

        /// 再帰的にTransformを走査して、ボーン構造を生成する
        private void RecursiveSkeleton(Transform current, ref List<Transform> skeletons)
        {
            skeletons.Add(current);

            for (int i = 0; i < current.childCount; i++)
            {
                Transform child = current.GetChild(i);
                if (child.gameObject.name.Split('_').Last() != "noSkeleton") RecursiveSkeleton(child, ref skeletons);
            }
        }


/*
        private void SetupSkeletonDic()
        {
            _skeletonBonesDic.Clear();
            RecursiveSkeletonDic(_root, ref _skeletonBonesDic);
        }


        /// 再帰的にTransformを走査して、ボーン構造を生成する
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
                }
                ;
            }
        }
*/

        /// アバターのセットアップ
        private void BuildRemapAvatar(out float eyeYOffset)
        {
            _srcAvatar = null;
            string[] humanTraitBoneNames = HumanTrait.BoneName;

            //--対象AvatarからベースになるHumanBone,SkeletonBoneの情報を取得--
            
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

            //--構成ボーンの置き換え又はTransformの変更を行うHumanBoneリストを作成--

            string hipBoneName = "";
            string leftToesBoneName = "";


            List<HumanBone> humanBones = new List<HumanBone>(humanTraitBoneNames.Length);
            for (int i = 0; i < humanTraitBoneNames.Length; i++)
            {
                string humanBoneName = humanTraitBoneNames[i];

                string bone;
                if (_transformDefinision.TryGetValue(humanBoneName, out bone))
                {
                    HumanBone humanBone = new HumanBone();
                    humanBone.humanName = humanBoneName;
                    Debug.Log("humanTraitBoneName  " + humanBoneName);
                    if ((bone == null))
                    {
                        Debug.Log("humanTraitBoneName  is null " + humanBoneName);
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

                    if (humanBoneName == "Hips")
                    {
                        hipBoneName = bone;
                        Debug.Log("hipBoneName " + bone);
                    }
                    if (humanBoneName == "LeftToes")
                    {
                        leftToesBoneName = bone;
                        Debug.Log("leftToesBoneName " + bone);
                    }
                }
            }
            Debug.Log("humanBones " + humanBones.Count);
            

            //--Hipsの高さオフセット値を求める処理--

            float hipYPos = 0;
            float leftToeLerfYPos = 0;
            float rootYPos = 0;
            for (int i = 0; i < _skeletonBones.Count; i++)
            {
                Transform bone = _skeletonBones[i];
                if (bone.name == hipBoneName)
                {
                    Debug.Log("hipYPos " + bone.position.y);
                    hipYPos = bone.position.y;
                    rootYPos = bone.parent.position.y;
                }
                else if (bone.name == leftToesBoneName)
                {
                    if (bone.childCount == 0) leftToeLerfYPos = bone.position.y;
                    else leftToeLerfYPos = bone.GetChild(0).transform.position.y;
                    Debug.Log("leftToeLerfYPos " + bone.position.y);
                }
            }
            float hipsY = hipYPos - leftToeLerfYPos;
            eyeYOffset = rootYPos - leftToeLerfYPos;
            Debug.Log("eyeYOffset : " + eyeYOffset);

            Debug.Log("--changePosRotBoneNames--");
            foreach (string str in changePosRotBoneNames)
            {
                Debug.Log(str);
            }

            //--構成ボーンの置き換え又はTransformの変更を行ったSkeletonBoneリストを作成--

            List<SkeletonBone> skeletonBones = new List<SkeletonBone>(_skeletonBones.Count + 1);
            for (int i = 0; i < _skeletonBones.Count; i++)
            {
                Transform bone = _skeletonBones[i];
                SkeletonBone skelBone = new SkeletonBone();
                skelBone.name = bone.name;
                if (changePosRotBoneNames.Contains(bone.name))
                {
                    skelBone.position = bone.localPosition;
                    skelBone.rotation = bone.localRotation;
                    skelBone.scale = bone.localScale;
                }
                else if (baseSkeltonBonesDic.ContainsKey(bone.name))
                {


                    if (bone.name == hipBoneName)
                    {
                        skelBone.position = Quaternion.Inverse(baseSkeltonBonesDic[bone.name].rotation) * new Vector3(0, hipsY, 0);
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

            //--Avatarのビルド--

            HumanDescription humanDesc = _baseAvatar.humanDescription;
            humanDesc.human = humanBones.ToArray();
            humanDesc.skeleton = skeletonBones.ToArray();

            humanDesc.hasTranslationDoF = false;

            _srcAvatar = AvatarBuilder.BuildHumanAvatar(_avatar, humanDesc);
            _srcAvatar.name = "AvatarSystem";
            if (_srcAvatar == null)

                if (!_srcAvatar.isValid || !_srcAvatar.isHuman)
                {
                    return;
                }

            _srchandler = new HumanPoseHandler(_srcAvatar, _avatar.transform);

        }






    }
}