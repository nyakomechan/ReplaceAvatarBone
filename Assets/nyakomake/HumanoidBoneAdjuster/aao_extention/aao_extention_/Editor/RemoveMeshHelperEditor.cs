using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using VRC.SDKBase;
using Anatawa12.AvatarOptimizer;

#if UNITY_EDITOR

[CustomEditor(typeof(RemoveMeshHelper))] // MonoBehaviourを継承したスクリプトにアタッチできるようにする
public class RemoveMeshHelperEditor : Editor
{
    private string[] skinnedMeshRendererNames;
    private SkinnedMeshRenderer[] skinnedMeshRenderers;
    private bool isEditBoxMode = true;
    private bool isShowSettingsForCreator = false;
    RemoveMeshHelper removeMeshHelper;
    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();//編集結果の保存に必要な関数

        removeMeshHelper = (RemoveMeshHelper)target; // Inspectorに表示されているスクリプトのインスタンスを取得
        Transform rootTransform = GetRootTransform();

        if (rootTransform != null)
        {

            EditorGUILayout.LabelField("アバター内のメッシュについて表示される箱の中のポリゴンを削除します。");
            EditorGUILayout.LabelField("下のドロップダウンリストで選択したSkinnedMeshRendererがポリゴン削除の対象になります。");
            EditorGUILayout.LabelField(" ");


            // SkinnedMeshRendererを持つ子オブジェクトを検索
            skinnedMeshRenderers = rootTransform.GetComponentsInChildren<SkinnedMeshRenderer>();

            // ドロップダウンリストに表示する名前を作成
            skinnedMeshRendererNames = skinnedMeshRenderers.Select(renderer => renderer.gameObject.name).ToArray();

            // SkinnedMeshRendererが存在する場合のみドロップダウンリストを表示
            if (skinnedMeshRendererNames.Length > 0)
            {
                // 現在選択されているSkinnedMeshRendererのIndexを検索
                if (skinnedMeshRenderers.Length > 0 && removeMeshHelper.selectNum >= skinnedMeshRenderers.Length)
                {
                    removeMeshHelper.selectNum = 0; // 範囲外の場合、最初の要素を選択
                }

                // ドロップダウンリストを表示
                var newSelectNum = EditorGUILayout.Popup("ポリゴンを削除するSkinnedMeshRenderer", removeMeshHelper.selectNum, skinnedMeshRendererNames);
                if (newSelectNum != removeMeshHelper.selectNum)
                {
                    removeMeshHelper.selectNum = newSelectNum;
                    if (removeMeshHelper.removeMeshInBox != null) Undo.DestroyObjectImmediate(removeMeshHelper.removeMeshInBox);

                    SkinnedMeshRenderer selectedRenderer = skinnedMeshRenderers[removeMeshHelper.selectNum];
                    removeMeshHelper.attachObject = selectedRenderer.gameObject;
                    Debug.Log(message: "Selected SkinnedMeshRenderer: " + selectedRenderer.gameObject.name);
                    SetRemoveMeshBox(removeMeshHelper, selectedRenderer.gameObject);
                    EditorUtility.SetDirty(removeMeshHelper);
                }
                else if (removeMeshHelper.removeMeshInBox == null)
                {
                    SkinnedMeshRenderer selectedRenderer = skinnedMeshRenderers[removeMeshHelper.selectNum];
                    removeMeshHelper.attachObject = selectedRenderer.gameObject;
                    Debug.Log(message: "Selected SkinnedMeshRenderer: " + selectedRenderer.gameObject.name);
                    SetRemoveMeshBox(removeMeshHelper, selectedRenderer.gameObject);
                }


            }
            else
            {
                if (removeMeshHelper.removeMeshInBox != null) Undo.DestroyObjectImmediate(removeMeshHelper.removeMeshInBox);
                EditorGUILayout.HelpBox("SkinnedMeshRendererを持つ子オブジェクトが見つかりませんでした。", MessageType.Info);
            }
            EditorGUILayout.LabelField(" ");
            EditorGUILayout.LabelField("注意：選択できるSkinnedMeshRendererはアバター内のオブジェクトのもののみになります。");
        }
        else
        {
            EditorGUILayout.LabelField("このコンポーネントを正しく動作させるには、アバター内に配置する必要があります。");
        }


            isEditBoxMode = GUILayout.Toggle(isEditBoxMode, "ポリゴンの削除範囲を編集する");
            if (rootTransform != null)
            {
                if (GUILayout.Button("削除した結果をプレビューする"))
                {
                    if (removeMeshHelper.attachObject != null) SetRemoveMeshBox(removeMeshHelper, removeMeshHelper.attachObject);
                    EditorUtility.SetDirty(removeMeshHelper);
                }
            }

        if (EditorGUI.EndChangeCheck())//編集結果の保存に必要な関数
        {
            EditorUtility.SetDirty(removeMeshHelper);
        }



    }

    //skinnedMeshコンポーネントを持つオブジェクトにRemoveMeshInBoxコンポーネントをアタッチしポリゴンを削除する箱状の範囲をセットする
    void SetRemoveMeshBox(RemoveMeshHelper removeMeshHelper, GameObject skinnedMeshObject)
    {

        RemoveMeshInBox removeMeshInBox = removeMeshHelper.removeMeshInBox;
        if (removeMeshHelper.removeMeshInBox == null)
        {
            removeMeshInBox = Undo.AddComponent<RemoveMeshInBox>(skinnedMeshObject);
            removeMeshInBox.Initialize(1);
        }
        RemoveMeshInBox.BoundingBox boundingBox = new RemoveMeshInBox.BoundingBox();

        boundingBox.Center = removeMeshHelper.attachObject.transform.InverseTransformPoint(removeMeshHelper.transform.position + removeMeshHelper.removeMeshBox.Center);
        Vector3 scale = new Vector3(removeMeshHelper.removeMeshBox.Size.x * removeMeshHelper.transform.lossyScale.x, removeMeshHelper.removeMeshBox.Size.y * removeMeshHelper.transform.lossyScale.y, removeMeshHelper.removeMeshBox.Size.z * removeMeshHelper.transform.lossyScale.z);
        boundingBox.Size = scale;
        boundingBox.Rotation = removeMeshHelper.transform.rotation * Quaternion.Euler(removeMeshHelper.removeMeshBox.Rotation);
        removeMeshInBox.Boxes = new RemoveMeshInBox.BoundingBox[] { boundingBox };
        removeMeshHelper.removeMeshInBox = removeMeshInBox;
    }

    void Reset()
    {
        removeMeshHelper = (RemoveMeshHelper)target; // Inspectorに表示されているスクリプトのインスタンスを取得
        Transform rootTransform = GetRootTransform();

        if (rootTransform != null)
        {

            // SkinnedMeshRendererを持つ子オブジェクトを検索
            skinnedMeshRenderers = rootTransform.GetComponentsInChildren<SkinnedMeshRenderer>();

            // ドロップダウンリストに表示する名前を作成
            skinnedMeshRendererNames = skinnedMeshRenderers.Select(renderer => renderer.gameObject.name).ToArray();

            // SkinnedMeshRendererが存在する場合のみドロップダウンリストを表示
            if (skinnedMeshRendererNames.Length > 0)
            {

                SkinnedMeshRenderer selectedRenderer = skinnedMeshRenderers[0];
                removeMeshHelper.attachObject = selectedRenderer.gameObject;
                Debug.Log(message: "Selected SkinnedMeshRenderer: " + selectedRenderer.gameObject.name);

            }
        }
        else
        {
            if (removeMeshHelper.removeMeshInBox != null) Undo.DestroyObjectImmediate(removeMeshHelper.removeMeshInBox);
        }
    }

    void OnDestroy()
    {
        if (!target)
        {
            RemoveMeshHelper removeMeshHelper = (RemoveMeshHelper)target;
            // 関連コンポーネントの削除
            if (removeMeshHelper.removeMeshInBox != null) Undo.DestroyObjectImmediate(removeMeshHelper.removeMeshInBox);
        }
    }


    Transform GetRootTransform()
    {
        var rootObject = ((MonoBehaviour)target).transform;
        while (rootObject != null)
        {
            if (rootObject.GetComponent<VRC_AvatarDescriptor>() != null) break;
            rootObject = rootObject.transform.parent;
        }
        return rootObject;
    }

    //ポリゴンを削除する箱状の範囲をエディタ上で表示する
    [DrawGizmo(GizmoType.Active)]
    static void DrawGizmo(RemoveMeshHelper scr, GizmoType gizmoType)
    {
        if (scr.attachObject == null) return;

        Vector3 position = scr.transform.position + scr.removeMeshBox.Center;
        Vector3 scale = new Vector3(scr.removeMeshBox.Size.x * scr.transform.lossyScale.x, scr.removeMeshBox.Size.y * scr.transform.lossyScale.y, scr.removeMeshBox.Size.z * scr.transform.lossyScale.z);

        Matrix4x4 worldmatrix = Matrix4x4.TRS(position, scr.transform.rotation * Quaternion.Euler(scr.removeMeshBox.Rotation), scale);

        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.matrix = worldmatrix;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        Debug.Log("OnDrawGizmosSelected");
    }

    //ポリゴンを削除する箱状の範囲をエディタ上で編集するハンドルを表示する
    protected virtual void OnSceneGUI()
    {
        if (!isEditBoxMode) return;
        removeMeshHelper = (RemoveMeshHelper)target;


        EditorGUI.BeginChangeCheck();//編集結果の保存に必要な関数

        Vector3 pos = removeMeshHelper.removeMeshBox.Center + removeMeshHelper.transform.position;
        Vector3 size = removeMeshHelper.removeMeshBox.Size;
        Quaternion rot = removeMeshHelper.transform.rotation * Quaternion.Euler(removeMeshHelper.removeMeshBox.Rotation);
        Handles.TransformHandle(ref pos, ref rot, ref size);
        if (EditorGUI.EndChangeCheck())//編集結果の保存に必要な関数
        {
            Undo.RecordObject(removeMeshHelper, "Change Look At Target Position");
            removeMeshHelper.removeMeshBox.Center = pos - removeMeshHelper.transform.position;
            removeMeshHelper.removeMeshBox.Size = size;
            removeMeshHelper.removeMeshBox.Rotation = (Quaternion.Inverse(removeMeshHelper.transform.rotation) * rot).eulerAngles;
        }
    }






}

#endif
