using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Managers
{
    public class SceneManager : Singleton<SceneManager>
    {
        [Serializable]
        public enum Scenes
        {
            Title,
            LevelSelect,
            GameScene,
        }

        public static event Action OnSceneLoaded;

        void OnEnable()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneLoaded;
        }

        void OnDisable()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= SceneLoaded;
        }

        private void SceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode)
        {
            OnSceneLoaded?.Invoke();
        }

        /// <summary>
        /// Load given Scenes
        /// </summary>
        /// <param name="scene">Build Index of Scene</param>
        public void LoadScene(int sceneBuildIndex)
        {
            LoadScene((Scenes)sceneBuildIndex);
        }
        

        public void LoadScene(string sceneName)
        {
            sceneName = sceneName.ToLower();
            switch (sceneName)
            {
                case "menu":
                    LoadScene(Scenes.Title);
                    break;
                case "title":
                    LoadScene(Scenes.Title);
                    break;
                case "levelSelect":
                    LoadScene(Scenes.LevelSelect);
                    break;
                case "game":
                    LoadScene(Scenes.GameScene);   
                    break;
                default:
                    Debug.LogWarning($"Invalid Scene To Load: {sceneName}");
                    break;
            }

        }

        public void LoadScene(Scenes scene)
        {
            int sceneIndex = (int)scene;
            Time.timeScale = 1;
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex);
        }

        public void QuitToMenu()
        {
            LoadScene(Scenes.Title);
        }

        public void Quit()
        {
            #if UNITY_EDITOR
            // Code to run only in the Unity Editor
            EditorApplication.isPlaying = false;
            #endif
            Application.Quit();
        }

    }
}

