using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

internal static class Utils {
    //public async static void Load(Scene scene, Scene currentScene) {
    //    var a = Task.Run(() => { });
    //    a.GetAwaiter().OnCompleted(() => {

    //    });
    //    //StartCoroutine(InternalLoad(scene, currentScene));
    //}

    public static void LoadAsync(Scene scene, Scene currentScene) {
        Utils.LoadAsync(scene, currentScene, () => { });
    }

    public static void LoadAsync(Scene scene, Scene currentScene, Action OnComplete) {
        Task UnloadAndEnable = null;
        AsyncOperation asyncCurScene = null;
        UnloadAndEnable = new(() => {
            while (!asyncCurScene.isDone) {
                //yield return null;
            }
        });
        UnloadAndEnable.GetAwaiter().OnCompleted(() => {
            SceneManager.SetActiveScene(scene);
            OnComplete();
        });
        Task loadtask = null;
        AsyncOperation asyncScene = null;
        if (!scene.isLoaded) {
            asyncScene = SceneManager.LoadSceneAsync(scene.name, LoadSceneMode.Additive);
        }
        loadtask = Task.Run(() => {
            if (!scene.isLoaded) {
                while (!asyncScene.isDone && !scene.isLoaded) {
                    //yield return null;
                }
                loadtask.Dispose();
            }
        });

        loadtask.GetAwaiter().OnCompleted(() => {
            asyncCurScene = SceneManager.UnloadSceneAsync(currentScene);
            UnloadAndEnable.Start();
        });

    }

    public static BattleManager GetBattleManager() {
        return GameObject.FindGameObjectWithTag("BattleManager").GetComponent<BattleManager>();
    }
}
