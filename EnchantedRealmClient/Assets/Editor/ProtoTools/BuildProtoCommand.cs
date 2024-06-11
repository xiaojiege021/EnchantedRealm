using System;
using System.Collections;
using System.Collections.Generic;
using HybridCLR.Editor.Commands;
using UnityEditor;
using UnityEngine;

public class BuildProtoCommand:EditorWindow 
{
   // [MenuItem("Tools/1. 构建ProtoBuf消息",false,1)]
    public static void BuildAndCopyABAOTHotUpdateDlls()
    {
        // BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
        // // BuildAssetBundleByTarget(target);
        // CompileDllCommand.CompileDll(target);
        // CopyABAOTHotUpdateDlls(target);
        AssetDatabase.Refresh();
    }
}
