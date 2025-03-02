using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using nadena.dev.ndmf;
using static UnityEngine.Object;
[assembly: ExportsPlugin(typeof(RemoveMeshHelperPlugin))]
public class RemoveMeshHelperPlugin : Plugin<RemoveMeshHelperPlugin>
{
    
    protected override void Configure()
    {
        InPhase(BuildPhase.Transforming)
                .BeforePlugin("nadena.dev.modular-avatar")
                .Run("nyakomake.removeMeshHelperPlugin", ctx =>
                {
                    var removeMeshHelpers = ctx.AvatarRootObject.GetComponentsInChildren<RemoveMeshHelper>();
                    foreach (RemoveMeshHelper removeMeshHelper in removeMeshHelpers)
                    {
                        DestroyImmediate(removeMeshHelper);
                    }
                });
    }
}
