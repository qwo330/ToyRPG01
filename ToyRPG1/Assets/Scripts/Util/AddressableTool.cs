using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;

public class AddressableTool : SingletonMono<AddressableTool>
{
//     public static void LoadAssetAsync<T>(string assetPath, Action<UnityEngine.Object> onResult) where T : UnityEngine.Object
//     {
//         if (string.IsNullOrEmpty(assetPath))
//         {
//             MyDebug.LogError($"Wrong Asset Path : {assetPath}");
//             onResult?.Invoke(null);
//             return;
//         }
//         
//         string address = $"Assets/Bundles/{assetPath}";
//         
// #if ADDRESSABLES
//
// #elif UNITY_EDITOR
//         T asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(address);
//         onResult?.Invoke(asset);
// #endif
//     }

    public static T LoadAsset<T>(string assetPath) where T : UnityEngine.Object
    {
        if (string.IsNullOrEmpty(assetPath))
        {
            MyDebug.LogError($"Wrong Asset Path : {assetPath}");
            return default;
        }
        
        string address = $"Assets/Bundles/{assetPath}";
     
        return Resources.Load<T>(address); // 어드레서블 세팅 전 임시처리
        
        // 하단 코드는 어드레서블 세팅 전까지 동작하지 않음
#if ADDRESSABLES
        var op = Addressables.LoadAssetAsync<T>(address);
        op.Completed += (result) =>
        {
            
        };

        return op.WaitForCompletion();
#elif UNITY_EDITOR
        return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(address);
#else
        return Resources.Load<T>(address);
#endif
    }
}