using System.Threading.Tasks;
using UnityEngine;

namespace Game.Core.Extensions
{
    public static class UnityDataTypeExtensions
    {
        public static void SafeDestroy(this UnityEngine.Object @object)
        {
            if (@object == null)
                return;

#if UNITY_EDITOR
            if (!UnityEngine.Application.isPlaying)
            {
                UnityEngine.Object.DestroyImmediate(@object);
                return;
            }
#endif
            UnityEngine.Object.Destroy(@object);
        }
    }
}