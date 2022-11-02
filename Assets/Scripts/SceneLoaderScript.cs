using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneLoaderScript : MonoBehaviour
{
    [SerializeField] Animator animator;

    void Start(){
        // clear player prefs on menu screen
        if (GetSceneName().Equals("MenuScene") || GetSceneName().Equals("GameOverScene")){
            PlayerPrefs.DeleteAll();
        }
    }

    public IEnumerator TriggerScene(string sceneName){
        animator.SetTrigger("start");

        yield return new WaitForSeconds(1);

        SceneManager.LoadScene(sceneName);
    }

    public void LoadScene(string sceneName){
        StartCoroutine(TriggerScene(sceneName));
    }

    public void RestartCurrentScene(){
        LoadScene(GetSceneName());
    }

    public void LoadGameOverScreen(){
        LoadScene("GameOverScene");
    }

    public void ExitGame() {
        Application.Quit();
    }

    public string GetSceneName() { return SceneManager.GetActiveScene().name; }
}
