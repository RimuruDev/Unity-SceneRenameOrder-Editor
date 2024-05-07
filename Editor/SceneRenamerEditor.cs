// **************************************************************** //
//
//   Copyright (c) RimuruDev. All rights reserved.
//   Contact me: 
//          - Gmail:    rimuru.dev@gmail.com
//          - LinkedIn: https://www.linkedin.com/in/rimuru/
//          - GitHub:   https://github.com/RimuruDev
//
// **************************************************************** //

using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System.Collections.Generic;

namespace RimuruDev
{
    public sealed class SceneOrderEditor : EditorWindow
    {
        private ReorderableList sceneList;

        // ReSharper disable once FieldCanBeMadeReadOnly.Local // Note: Don't limit List with readonly!!!!
        private List<SceneAsset> scenes = new();

        private Vector2 scrollPos;
        private string sceneFolderPath = "Assets/Scenes";
        private string namePattern = "Level_";
        private bool foldout = true;

        [MenuItem("RimuruDev Tools/Scene Order Editor")]
        public static void ShowWindow() =>
            GetWindow(typeof(SceneOrderEditor), false, "Scene Order Editor");

        private void OnEnable()
        {
            sceneList = new ReorderableList(scenes, typeof(SceneAsset), true, true, true, true)
            {
                drawHeaderCallback = rect => { EditorGUI.LabelField(rect, "Drag Scenes Here:"); },
                drawElementCallback = (rect, index, _, _) =>
                {
                    scenes[index] = (SceneAsset)EditorGUI.ObjectField(
                        position: new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                        scenes[index],
                        typeof(SceneAsset),
                        allowSceneObjects: false
                    );
                }
            };
        }

        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            foldout = EditorGUILayout.Foldout(foldout, "Scene Folder Path and Naming Pattern", true);

            if (foldout)
            {
                sceneFolderPath = EditorGUILayout.TextField("Folder Path:", sceneFolderPath);
                namePattern = EditorGUILayout.TextField("Name Pattern:", namePattern);

                if (GUILayout.Button("Load Scenes From Folder"))
                    LoadScenesFromFolder();
            }

            if (EditorGUI.EndChangeCheck())
                LoadScenesFromFolder();

            scrollPos = GUILayout.BeginScrollView(
                scrollPos,
                GUILayout.Width(position.width),
                GUILayout.Height(position.height - 200));

            sceneList.DoLayoutList();

            GUILayout.EndScrollView();

            if (GUILayout.Button("Rename and Reorder Scenes"))
                RenameScenes();
        }

        private void LoadScenesFromFolder()
        {
            scenes.Clear();

            var guids = AssetDatabase.FindAssets("t:Scene", new[] { sceneFolderPath });

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);

                if (sceneAsset != null)
                    scenes.Add(sceneAsset);
            }
        }

        private void RenameScenes()
        {
            for (var i = 0; i < scenes.Count; i++)
            {
                var path = AssetDatabase.GetAssetPath(scenes[i]);
                var tempSceneName = $"Temp_{namePattern}{i}";

                AssetDatabase.RenameAsset(path, tempSceneName);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            for (var i = 0; i < scenes.Count; i++)
            {
                var oldPath = AssetDatabase.GetAssetPath(scenes[i]);
                var newSceneName = $"{namePattern}{i}";

                AssetDatabase.RenameAsset(oldPath, newSceneName);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("<color=yellow>Scenes renamed and reordered based on the list.</color>");
        }
    }
}