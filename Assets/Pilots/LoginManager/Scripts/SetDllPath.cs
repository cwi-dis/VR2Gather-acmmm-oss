﻿using UnityEngine;

public class SetDllPath {
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnBeforeSceneLoadRuntimeMethod() {
        string pathvar = System.Environment.GetEnvironmentVariable("PATH");
        string dllsPath = $"{Application.dataPath}/../dlls/";
        if (System.IO.Directory.Exists(dllsPath)) { 
            System.Environment.SetEnvironmentVariable("PATH", $"{dllsPath};{pathvar}", System.EnvironmentVariableTarget.Process);
        }
    }
}