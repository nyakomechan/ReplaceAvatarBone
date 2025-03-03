using System.Collections;
using System.Collections.Generic;
using nyakomake;
using UnityEngine;
using nadena.dev.modular_avatar.core;

[RequireComponent(typeof(ModularAvatarBoneProxy))]
public class ReplaceAvatarBone : HumanoidBoneAdjuster
{
    void Reset()
    {
        refPosRotTransform = transform;
        adjustType = AdjustType.PositionAndRotation;
    }
}
