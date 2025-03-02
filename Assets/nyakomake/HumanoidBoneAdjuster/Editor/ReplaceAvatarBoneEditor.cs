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

        ReplaceAvatarBone replaceAvatarBone = (ReplaceAvatarBone)target; // Inspectorに表示されているスクリプトのインスタンスを取得
        ModularAvatarBoneProxy modularAvatarBoneProxy = replaceAvatarBone.gameObject.GetComponent<ModularAvatarBoneProxy>(); // Inspectorに表示されているスクリプトのインスタンスを取得
        //base.OnInspectorGUI(); // デフォルトのInspectorを表示
        replaceAvatarBone.humanBodyBones = (HumanBodyBones)EditorGUILayout.EnumPopup( "置き換える対象のHumanoidBone", selected: (HumanBodyBones)replaceAvatarBone.humanBodyBones );

        if(modularAvatarBoneProxy != null)
        {
            modularAvatarBoneProxy.boneReference = replaceAvatarBone.humanBodyBones;
            modularAvatarBoneProxy.attachmentMode = BoneProxyAttachmentMode.AsChildKeepWorldPose;
            modularAvatarBoneProxy.ResolveReferences();
        }
        if(EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(replaceAvatarBone);
        }
    }
}
