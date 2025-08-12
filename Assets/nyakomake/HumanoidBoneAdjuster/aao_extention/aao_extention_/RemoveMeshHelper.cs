using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Anatawa12.AvatarOptimizer;
using UnityEngine.EventSystems;
using VRC.SDKBase;
[ExecuteAlways]
public class RemoveMeshHelper : MonoBehaviour, VRC.SDKBase.IEditorOnly
{
    public GameObject attachObject;
    public RemoveMeshInBox removeMeshInBox;
    public int selectNum;

    [System.Serializable]
    public struct RemoveMeshBox
    {
        public Vector3 Center;
        public Vector3 Size;
        public Vector3 Rotation;
    }

    public RemoveMeshBox removeMeshBox;

    void Reset()
    {
        removeMeshBox.Center = new Vector3(0, 1f, 0);
        removeMeshBox.Size = Vector3.one;
        removeMeshBox.Rotation = Vector3.zero;
    }

    [SerializeField] string targetObjectName = "TargetGameObject";

    void OnEnable()
    {
        // Hierarchy変更イベント購読
        EditorApplication.hierarchyChanged += OnHierarchyChanged;
    }

    void OnDisable()
    {
        // イベント購読解除
        EditorApplication.hierarchyChanged -= OnHierarchyChanged;

    }

    private void OnHierarchyChanged()
    {
        if (!Application.isPlaying)
        {
            var rootObj = GetRootTransform();
            if (rootObj == null)
            {
                if (removeMeshInBox != null) Undo.DestroyObjectImmediate(removeMeshInBox);
            }
        }
    }
    Transform GetRootTransform()
    {
        var rootObject = transform;
        while (rootObject != null)
        {
            if (rootObject.GetComponent<VRC_AvatarDescriptor>() != null) break;
            rootObject = rootObject.transform.parent;
        }
        return rootObject;
    }

    void OnDestroy()
    {
        if (!Application.isPlaying)
        {

            if (removeMeshInBox != null) Undo.DestroyObjectImmediate(removeMeshInBox);

        }
    }



}
