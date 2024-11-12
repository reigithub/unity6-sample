using System;
using System.Linq;
using Game.Core;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    public static partial class ProjectEditorMenu
    {
        [MenuItem("Project/EditorWindow/GameEnvConfig")]
        public static void GameEnvConfigEditorWindow()
        {
            var window = EditorWindow.GetWindow<GameEnvConfigEditorWindow>();
            window.Show();
        }
    }
}