using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using nadena.dev.modular_avatar.core;
using VeryAnimation;
[CustomEditor(typeof(ReplaceAvatarBone)), CanEditMultipleObjects]
public class ReplaceAvatarBoneEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        ReplaceAvatarBone replaceAvatarBone = (ReplaceAvatarBone)target;
        ModularAvatarBoneProxy modularAvatarBoneProxy = replaceAvatarBone.gameObject.GetComponent<ModularAvatarBoneProxy>();

        EditorGUI.LabelField("選択したHumanoidBone(UpperLeg_L等)をアタッチしたオブジェクトに疑似的に置き換えます。");
        EditorGUI.LabelField(" ");

        EditorGUI.LabelField("置き換える対象のHumanoidBoneを下のリストから選択してください。");
        replaceAvatarBone.humanBodyBones = (HumanBodyBones)EditorGUILayout.EnumPopup("HumanoidBone", selected: (HumanBodyBones)replaceAvatarBone.humanBodyBones);

        EditorGUI.LabelField(" ");
        EditorGUI.LabelField("注意 : 一緒に追加されるMABoneProxyは削除または値の変更はしないでください。");


        if (modularAvatarBoneProxy != null)
        {
            modularAvatarBoneProxy.boneReference = replaceAvatarBone.humanBodyBones;
            modularAvatarBoneProxy.attachmentMode = BoneProxyAttachmentMode.AsChildKeepWorldPose;
            modularAvatarBoneProxy.ResolveReferences();
        }
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(replaceAvatarBone);
        }
    }
}
