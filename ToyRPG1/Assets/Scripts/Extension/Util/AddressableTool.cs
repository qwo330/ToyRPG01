using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class AddressableTool : SingletonMono<AddressableTool>
{
    public static T LoadAsset<T>(string assetAddress) where T : UnityEngine.Object
    {
        if (string.IsNullOrEmpty(assetAddress))
        {
            MyDebug.LogError($"Wrong Asset Address : {assetAddress}");
            return default;
        }

        var asset = LoadAddressable<T>(assetAddress);
        if (asset != null)
            return asset;

        asset = LoadAddressableComponent<T>(assetAddress);
        if (asset != null)
            return asset;

#if UNITY_EDITOR
        asset = LoadEditorAsset<T>(assetAddress);
        if (asset != null)
            return asset;
#endif

        MyDebug.LogError($"Cannot load asset: {assetAddress}");
        return default;
    }

    static T LoadAddressable<T>(string assetAddress) where T : UnityEngine.Object
    {
        AsyncOperationHandle<T> op = default;
        try
        {
            op = Addressables.LoadAssetAsync<T>(assetAddress);
            var asset = op.WaitForCompletion();
            if (op.Status == AsyncOperationStatus.Succeeded)
                return asset;

            if (op.IsValid())
                Addressables.Release(op);
        }
        catch (Exception e)
        {
            if (op.IsValid())
                Addressables.Release(op);

            MyDebug.LogWarning($"Addressable load failed: {assetAddress}, {e.Message}");
        }

        return default;
    }

    static T LoadAddressableComponent<T>(string assetAddress) where T : UnityEngine.Object
    {
        if (!typeof(Component).IsAssignableFrom(typeof(T)))
            return default;

        var prefab = LoadAddressable<GameObject>(assetAddress);
        if (prefab == null)
            return default;

        return prefab.GetComponent(typeof(T)) as T;
    }

#if UNITY_EDITOR
    static T LoadEditorAsset<T>(string assetAddress) where T : UnityEngine.Object
    {
        var path = assetAddress.StartsWith("Assets/")
            ? assetAddress
            : $"Assets/Bundles/Prefabs/{assetAddress}";

        if (typeof(Component).IsAssignableFrom(typeof(T)))
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            return prefab != null ? prefab.GetComponent(typeof(T)) as T : default;
        }

        return AssetDatabase.LoadAssetAtPath<T>(path);
    }
#endif
}
