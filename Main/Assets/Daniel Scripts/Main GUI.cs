using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainGUI : MonoBehaviour
{

    public UIDocument ui;
    private Button play;
    private Button exit;

    // Start is called before the first frame update
    void Start()
    {
        play = ui.rootVisualElement.Q<Button>("play");
        exit = ui.rootVisualElement.Q<Button>("exit");
        play.clicked += () => 
        {
            SceneManager.LoadScene(0, LoadSceneMode.Single);
        };
        exit.clicked += () =>
        {
            Application.Quit();
        };
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
