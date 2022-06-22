using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Singleton;
using System;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private Slider slider = null;
    private CanvasGroup canvasGroup;
    private Camera loadingScreenCamera;

    public static LoadingScreen instance { get; private set; }

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        canvasGroup = GetComponentInChildren<CanvasGroup>();
        loadingScreenCamera = GetComponent<Camera>();

        Utility.SetCanvasGroupEnabled(canvasGroup, false);
        loadingScreenCamera.enabled = false;

        // PlayerPrefs.SetInt("IsTutorial", 0);
       // PlayerPrefs.SetInt("TutorialCurrentScreen", 17);
       // PlayerPrefs.SetInt("LevelsUnlocked", 20);
    }

    IEnumerator LoadingSceneProcess(string _nameScene)
    {
        Utility.SetCanvasGroupEnabled(canvasGroup, true);
        loadingScreenCamera.enabled = true;

        AsyncOperation operation = SceneManager.LoadSceneAsync(_nameScene);

        while (!operation.isDone)
        {
            slider.value = operation.progress;
            yield return null;
        }

        Utility.SetCanvasGroupEnabled(canvasGroup, false);
        loadingScreenCamera.enabled = false;

    }

    public void LoadScene(string _nameScene)
    {
        StartCoroutine(LoadingSceneProcess(_nameScene));
    }

    //private void OnApplicationPause(bool pause)
    //{
    //    if (pause)
    //    {
    //        GameSettings.Instance.currentMissionIndex = 0;
    //    }
    //}

    private void OnApplicationQuit()
    {
        GameSettings.Instance.currentMissionIndex = 0;
    }
}
