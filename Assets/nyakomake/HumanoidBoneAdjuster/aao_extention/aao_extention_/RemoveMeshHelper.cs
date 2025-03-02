using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Anatawa12.AvatarOptimizer;
using UnityEngine.EventSystems;
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
        removeMeshBox.Center = new Vector3(0,1f,0);
        removeMeshBox.Size = Vector3.one;
        removeMeshBox.Rotation = Vector3.zero;
    }




}
