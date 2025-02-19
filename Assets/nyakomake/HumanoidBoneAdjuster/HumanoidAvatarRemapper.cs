using System.Diagnostics.Tracing;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using VRC.SDK3.Avatars.Components;
using nadena.dev.modular_avatar.core;

public struct ChangePosRotHumanBone
{
    public HumanBodyBones humanBodyBones;
    public Transform refPosRotTransform;
}
public class HumanoidAvatarRemapper
{

    #region ### Veriables ###


    private Avatar _srcAvatar;

    [SerializeField]
    [Tooltip("改変するアバターを指定")]
    private GameObject _avatar;


    [SerializeField]
    [Tooltip("ベースとなる.avatarファイル")]
    private Avatar _baseAvatar;

    [SerializeField]
    [Tooltip("地面と足を浮かせる距離")]
    private float _heightOffset = 0;

    /*
        [SerializeField]
        [Tooltip("地面と足を浮かせる距離")]
        private float _heightOffset = 0;
    */

    [SerializeField]
    [Tooltip("地面と足を浮かせる距離")]
    private float _hipsHeightOffset = 0;


    #region ### ボーンへの参照 ###
    [SerializeField]
    [Tooltip("ArmatureなどのHipsの親のオブジェクト : 指定必須")]
    private Transform _root;

    [SerializeField] private Transform _hips;
    [SerializeField] private Transform _leftUpperLeg;
    [SerializeField] private Transform _rightUpperLeg;
    [SerializeField] private Transform _leftLowerLeg;
    [SerializeField] private Transform _rightLowerLeg;
    [SerializeField] private Transform _leftFoot;
    [SerializeField] private Transform _rightFoot;
    [SerializeField] private Transform _spine;
    [SerializeField] private Transform _chest;
    [SerializeField] private Transform _upperChest;
    [SerializeField] private Transform _neck;
    [SerializeField] private Transform _head;
    [SerializeField] private Transform _leftShoulder;
    [SerializeField] private Transform _rightShoulder;
    [SerializeField] private Transform _leftUpperArm;
    [SerializeField] private Transform _rightUpperArm;
    [SerializeField] private Transform _leftLowerArm;
    [SerializeField] private Transform _rightLowerArm;
    [SerializeField] private Transform _leftHand;
    [SerializeField] private Transform _rightHand;
    [SerializeField] private Transform _leftToes;
    [SerializeField] private Transform _rightToes;
    [SerializeField] private Transform _leftEye;
    [SerializeField] private Transform _rightEye;
    [SerializeField] private Transform _jaw;
    [SerializeField] private Transform _leftThumbProximal;
    [SerializeField] private Transform _leftThumbIntermediate;
    [SerializeField] private Transform _leftThumbDistal;
    [SerializeField] private Transform _leftIndexProximal;
    [SerializeField] private Transform _leftIndexIntermediate;
    [SerializeField] private Transform _leftIndexDistal;
    [SerializeField] private Transform _leftMiddleProximal;
    [SerializeField] private Transform _leftMiddleIntermediate;
    [SerializeField] private Transform _leftMiddleDistal;
    [SerializeField] private Transform _leftRingProximal;
    [SerializeField] private Transform _leftRingIntermediate;
    [SerializeField] private Transform _leftRingDistal;
    [SerializeField] private Transform _leftLittleProximal;
    [SerializeField] private Transform _leftLittleIntermediate;
    [SerializeField] private Transform _leftLittleDistal;
    [SerializeField] private Transform _rightThumbProximal;
    [SerializeField] private Transform _rightThumbIntermediate;
    [SerializeField] private Transform _rightThumbDistal;
    [SerializeField] private Transform _rightIndexProximal;
    [SerializeField] private Transform _rightIndexIntermediate;
    [SerializeField] private Transform _rightIndexDistal;
    [SerializeField] private Transform _rightMiddleProximal;
    [SerializeField] private Transform _rightMiddleIntermediate;
    [SerializeField] private Transform _rightMiddleDistal;
    [SerializeField] private Transform _rightRingProximal;
    [SerializeField] private Transform _rightRingIntermediate;
    [SerializeField] private Transform _rightRingDistal;
    [SerializeField] private Transform _rightLittleProximal;
    [SerializeField] private Transform _rightLittleIntermediate;
    [SerializeField] private Transform _rightLittleDistal;
    #endregion ### ボーンへの参照 ###

    private Dictionary<string, string> _transformDefinision = new Dictionary<string, string>();
    private List<Transform> _skeletonBones = new List<Transform>();
    private Dictionary<string, Transform> _skeletonBonesDic = new Dictionary<string, Transform>();

    private HumanPoseHandler _srchandler;
    #endregion ### Veriables ###

    public void SetAvatarObj(GameObject avatarObj)
    {
        _avatar = avatarObj;
    }




    [ContextMenu("Create remap avatar file")]
    public Avatar CreateRemapAvatar()
    {
        //CacheBoneNameMap(BoneNameConvention.FBX, _assetName);
        if (_baseAvatar == null) SetBaseAvatarAsset();
        if (_root == null) SetRoot();

        if (_avatarDescripter == null) SetAvatarDesc();

        SetupSkeleton();
        SetupSkeletonDic();
        SetupBones();
        ReadAvatar();
        BuildRemapAvatar();
        return _srcAvatar;
        //SetupLineRenderers();
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




    /*
    private void OnDrawGizmos()
    {
        if (!_showVisualizer)
        {
            return;
        }

        Gizmos.color = Color.red;

        if (_head == null) return;

        Gizmos.DrawLine(_head.position, _neck.position);
        Gizmos.DrawLine(_neck.position, _spine.position);
        Gizmos.DrawLine(_neck.position, _leftShoulder.position);
        Gizmos.DrawLine(_neck.position, _rightShoulder.position);
        Gizmos.DrawLine(_chest.position, _spine.position);
        Gizmos.DrawLine(_spine.position, _hips.position);

        Gizmos.DrawLine(_hips.position, _leftUpperLeg.position);
        Gizmos.DrawLine(_hips.position, _rightUpperLeg.position);

        Gizmos.DrawLine(_leftShoulder.position, _leftUpperArm.position);
        Gizmos.DrawLine(_leftUpperArm.position, _leftLowerArm.position);
        Gizmos.DrawLine(_leftLowerArm.position, _leftHand.position);

        Gizmos.DrawLine(_rightShoulder.position, _rightUpperArm.position);
        Gizmos.DrawLine(_rightUpperArm.position, _rightLowerArm.position);
        Gizmos.DrawLine(_rightLowerArm.position, _rightHand.position);

        Gizmos.DrawLine(_leftUpperLeg.position, _leftLowerLeg.position);
        Gizmos.DrawLine(_leftLowerLeg.position, _leftFoot.position);
        Gizmos.DrawLine(_leftFoot.position, _leftToes.position);

        Gizmos.DrawLine(_rightUpperLeg.position, _rightLowerLeg.position);
        Gizmos.DrawLine(_rightLowerLeg.position, _rightFoot.position);
        Gizmos.DrawLine(_rightFoot.position, _rightToes.position);
    }
    */


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
                //_transformDefinision[hb.humanName] = _skeletonBonesDic[hb.boneName].name;
                //Debug.Log(hb.boneName +" "+_skeletonBonesDic[hb.boneName].name);
            }




        }

    }


    /// <summary>
    /// アサインされたTransformからボーンのリストをセットアップする
    /// </summary>
    private void SetupBones()
    {
        changePosRotBoneNames.Clear();
        _transformDefinision.Clear();

        if (_hips != null) _transformDefinision.Add("Hips", _hips.name); else _transformDefinision.Add("Hips", "");
        if (_leftUpperLeg != null) _transformDefinision.Add("LeftUpperLeg", _leftUpperLeg.name); else _transformDefinision.Add("LeftUpperLeg", "");
        if (_rightUpperLeg != null) _transformDefinision.Add("RightUpperLeg", _rightUpperLeg.name); else _transformDefinision.Add("RightUpperLeg", "");
        if (_leftLowerLeg != null) _transformDefinision.Add("LeftLowerLeg", _leftLowerLeg.name); else _transformDefinision.Add("LeftLowerLeg", "");
        if (_rightLowerLeg != null) _transformDefinision.Add("RightLowerLeg", _rightLowerLeg.name); else _transformDefinision.Add("RightLowerLeg", "");
        if (_leftFoot != null) _transformDefinision.Add("LeftFoot", _leftFoot.name); else _transformDefinision.Add("LeftFoot", "");
        if (_rightFoot != null) _transformDefinision.Add("RightFoot", _rightFoot.name); else _transformDefinision.Add("RightFoot", "");
        if (_spine != null) _transformDefinision.Add("Spine", _spine.name); else _transformDefinision.Add("Spine", "");
        if (_chest != null) _transformDefinision.Add("Chest", _chest.name); else _transformDefinision.Add("Chest", "");
        if (_neck != null) _transformDefinision.Add("Neck", _neck.name); else _transformDefinision.Add("Neck", "");
        if (_head != null) _transformDefinision.Add("Head", _head.name); else _transformDefinision.Add("Head", "");
        if (_leftShoulder != null) _transformDefinision.Add("LeftShoulder", _leftShoulder.name); else _transformDefinision.Add("LeftShoulder", "");
        if (_rightShoulder != null) _transformDefinision.Add("RightShoulder", _rightShoulder.name); else _transformDefinision.Add("RightShoulder", "");
        if (_leftUpperArm != null) _transformDefinision.Add("LeftUpperArm", _leftUpperArm.name); else _transformDefinision.Add("LeftUpperArm", "");
        if (_rightUpperArm != null) _transformDefinision.Add("RightUpperArm", _rightUpperArm.name); else _transformDefinision.Add("RightUpperArm", "");
        if (_leftLowerArm != null) _transformDefinision.Add("LeftLowerArm", _leftLowerArm.name); else _transformDefinision.Add("LeftLowerArm", "");
        if (_rightLowerArm != null) _transformDefinision.Add("RightLowerArm", _rightLowerArm.name); else _transformDefinision.Add("RightLowerArm", "");
        if (_leftHand != null) _transformDefinision.Add("LeftHand", _leftHand.name); else _transformDefinision.Add("LeftHand", "");
        if (_rightHand != null) _transformDefinision.Add("RightHand", _rightHand.name); else _transformDefinision.Add("RightHand", "");
        if (_leftToes != null) _transformDefinision.Add("LeftToes", _leftToes.name); else _transformDefinision.Add("LeftToes", "");
        if (_rightToes != null) _transformDefinision.Add("RightToes", _rightToes.name); else _transformDefinision.Add("RightToes", "");
        if (_leftEye != null) _transformDefinision.Add("LeftEye", _leftEye.name); else _transformDefinision.Add("LeftEye", "");
        if (_rightEye != null) _transformDefinision.Add("RightEye", _rightEye.name); else _transformDefinision.Add("RightEye", "");
        if (_jaw != null) _transformDefinision.Add("Jaw", _jaw.name); else _transformDefinision.Add("Jaw", "");
        if (_leftThumbProximal != null) _transformDefinision.Add("Left Thumb Proximal", _leftThumbProximal.name); else _transformDefinision.Add("Left Thumb Proximal", "");
        if (_leftThumbIntermediate != null) _transformDefinision.Add("Left Thumb Intermediate", _leftThumbIntermediate.name); else _transformDefinision.Add("Left Thumb Intermediate", "");
        if (_leftThumbDistal != null) _transformDefinision.Add("Left Thumb Distal", _leftThumbDistal.name); else _transformDefinision.Add("Left Thumb Distal", "");
        if (_leftIndexProximal != null) _transformDefinision.Add("Left Index Proximal", _leftIndexProximal.name); else _transformDefinision.Add("Left Index Proximal", "");
        if (_leftIndexIntermediate != null) _transformDefinision.Add("Left Index Intermediate", _leftIndexIntermediate.name); else _transformDefinision.Add("Left Index Intermediate", "");
        if (_leftIndexDistal != null) _transformDefinision.Add("Left Index Distal", _leftIndexDistal.name); else _transformDefinision.Add("Left Index Distal", "");
        if (_leftMiddleProximal != null) _transformDefinision.Add("Left Middle Proximal", _leftMiddleProximal.name); else _transformDefinision.Add("Left Middle Proximal", "");
        if (_leftMiddleIntermediate != null) _transformDefinision.Add("Left Middle Intermediate", _leftMiddleIntermediate.name); else _transformDefinision.Add("Left Middle Intermediate", "");
        if (_leftMiddleDistal != null) _transformDefinision.Add("Left Middle Distal", _leftMiddleDistal.name); else _transformDefinision.Add("Left Middle Distal", "");
        if (_leftRingProximal != null) _transformDefinision.Add("Left Ring Proximal", _leftRingProximal.name); else _transformDefinision.Add("Left Ring Proximal", "");
        if (_leftRingIntermediate != null) _transformDefinision.Add("Left Ring Intermediate", _leftRingIntermediate.name); else _transformDefinision.Add("Left Ring Intermediate", "");
        if (_leftRingDistal != null) _transformDefinision.Add("Left Ring Distal", _leftRingDistal.name); else _transformDefinision.Add("Left Ring Distal", "");
        if (_leftLittleProximal != null) _transformDefinision.Add("Left Little Proximal", _leftLittleProximal.name); else _transformDefinision.Add("Left Little Proximal", "");
        if (_leftLittleIntermediate != null) _transformDefinision.Add("Left Little Intermediate", _leftLittleIntermediate.name); else _transformDefinision.Add("Left Little Intermediate", "");
        if (_leftLittleDistal != null) _transformDefinision.Add("Left Little Distal", _leftLittleDistal.name); else _transformDefinision.Add("Left Little Distal", "");
        if (_rightThumbProximal != null) _transformDefinision.Add("Right Thumb Proximal", _rightThumbProximal.name); else _transformDefinision.Add("Right Thumb Proximal", "");
        if (_rightThumbIntermediate != null) _transformDefinision.Add("Right Thumb Intermediate", _rightThumbIntermediate.name); else _transformDefinision.Add("Right Thumb Intermediate", "");
        if (_rightThumbDistal != null) _transformDefinision.Add("Right Thumb Distal", _rightThumbDistal.name); else _transformDefinision.Add("Right Thumb Distal", "");
        if (_rightIndexProximal != null) _transformDefinision.Add("Right Index Proximal", _rightIndexProximal.name); else _transformDefinision.Add("Right Index Proximal", "");
        if (_rightIndexIntermediate != null) _transformDefinision.Add("Right Index Intermediate", _rightIndexIntermediate.name); else _transformDefinision.Add("Right Index Intermediate", "");
        if (_rightIndexDistal != null) _transformDefinision.Add("Right Index Distal", _rightIndexDistal.name); else _transformDefinision.Add("Right Index Distal", "");
        if (_rightMiddleProximal != null) _transformDefinision.Add("Right Middle Proximal", _rightMiddleProximal.name); else _transformDefinision.Add("Right Middle Proximal", "");
        if (_rightMiddleIntermediate != null) _transformDefinision.Add("Right Middle Intermediate", _rightMiddleIntermediate.name); else _transformDefinision.Add("Right Middle Intermediate", "");
        if (_rightMiddleDistal != null) _transformDefinision.Add("Right Middle Distal", _rightMiddleDistal.name); else _transformDefinision.Add("Right Middle Distal", "");
        if (_rightRingProximal != null) _transformDefinision.Add("Right Ring Proximal", _rightRingProximal.name); else _transformDefinision.Add("Right Ring Proximal", "");
        if (_rightRingIntermediate != null) _transformDefinision.Add("Right Ring Intermediate", _rightRingIntermediate.name); else _transformDefinision.Add("Right Ring Intermediate", "");
        if (_rightRingDistal != null) _transformDefinision.Add("Right Ring Distal", _rightRingDistal.name); else _transformDefinision.Add("Right Ring Distal", "");
        if (_rightLittleProximal != null) _transformDefinision.Add("Right Little Proximal", _rightLittleProximal.name); else _transformDefinision.Add("Right Little Proximal", "");
        if (_rightLittleIntermediate != null) _transformDefinision.Add("Right Little Intermediate", _rightLittleIntermediate.name); else _transformDefinision.Add("Right Little Intermediate", "");
        if (_rightLittleDistal != null) _transformDefinision.Add("Right Little Distal", _rightLittleDistal.name); else _transformDefinision.Add("Right Little Distal", "");
        if (_upperChest != null) _transformDefinision.Add("UpperChest", _upperChest.name); else _transformDefinision.Add("UpperChest", "");

    }
    public List<string> changePosRotBoneNames = new List<string>();
    private void SetupBones_PosRotChange(List<ChangePosRotHumanBone> changePosRotHumanBones)
    {
        _transformDefinision.Clear();
        changePosRotBoneNames.Clear();

        for(int i = 0; i < HumanTrait.BoneCount;i++)
        {
            _transformDefinision.Add(HumanTrait.BoneName[i],"");
        }

        Animator animator= _avatar.GetComponent<Animator>();

        foreach(ChangePosRotHumanBone bone in changePosRotHumanBones)
        {
            if(_transformDefinision.ContainsKey(HumanTrait.BoneName[(int)bone.humanBodyBones]))
            {
                _transformDefinision[HumanTrait.BoneName[(int)bone.humanBodyBones]] = animator.GetBoneTransform(bone.humanBodyBones).name;
                changePosRotBoneNames.Add(animator.GetBoneTransform(bone.humanBodyBones).name);
                Debug.Log("changePosRotBoneName : "+animator.GetBoneTransform(bone.humanBodyBones).name);
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
foreach(string str in changePosRotBoneNames)
{
    Debug.Log("changePosRotBoneNames : "+str);
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
            Debug.Log( bone.name);
            if(changePosRotBoneNames.Contains(bone.name))
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
        if(_srcAvatar == null)Debug.Log("srcavatar is null!");

        if (!_srcAvatar.isValid || !_srcAvatar.isHuman)
        {
            Debug.LogError("setup error");
            return;
        }

        _srchandler = new HumanPoseHandler(_srcAvatar, _avatar.transform);


#if UNITY_EDITOR
        AssetDatabase.CreateAsset(_srcAvatar, "Assets/nyakomake/humanoidAvatarRemapper/" + _baseAvatar.name + "_remap.asset");
        AssetDatabase.SaveAssets();
        _avatar.GetComponent<Animator>().avatar = (Avatar)AssetDatabase.LoadAssetAtPath("Assets/nyakomake/humanoidAvatarRemapper/" + _baseAvatar.name + "_remap.asset", typeof(Avatar));
        _srcAvatar = _avatar.GetComponent<Animator>().avatar;
#endif

    }

    [SerializeField] private Vector3 _defaultEyePos = new Vector3(0, 0, 0);

    public VRCAvatarDescriptor _avatarDescripter;
    [ContextMenu("Add height offset to View position in VRCAvatarDescriptor")]
    public void SetViewPos()
    {
        _avatarDescripter = _avatar.GetComponent<VRCAvatarDescriptor>();
        if (_defaultEyePos.y == 0) _defaultEyePos = _avatarDescripter.ViewPosition;
        float hipsYOffset = 0;
        if (_leftToes != null && _rightToes != null)
        {
            hipsYOffset = _leftToes.position.y - _root.position.y;
        }
        _avatarDescripter.ViewPosition = _avatarDescripter.ViewPosition + new Vector3(0, -hipsYOffset, 0);
    }
    public float GetEyeYOffset()
    {
        float hipsYOffset = 0;
        if (_leftToes != null && _rightToes != null)
        {
            hipsYOffset = _leftToes.position.y - _root.position.y;
        }
        return hipsYOffset;
    }

    public void RevertViewPos()
    {
        _avatarDescripter = _avatar.GetComponent<VRCAvatarDescriptor>();
        _avatarDescripter.ViewPosition = _defaultEyePos;
    }
}


