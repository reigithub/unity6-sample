using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Game.Core.Services
{
    public class AddressableAssetService : GameService
    {
        // Debug.Log(Addressables.BuildPath);
        // Debug.Log(Addressables.PlayerBuildDataPath);
        // Debug.Log(Addressables.RuntimePath);

        public async Task<T> LoadAssetAsync<T>(string address) where T : UnityEngine.Object
        {
            return await Addressables.LoadAssetAsync<T>(address);
        }
    }
}