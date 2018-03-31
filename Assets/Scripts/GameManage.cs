using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManage : MonoBehaviour {

    // 开始游戏
    public void OnNewGameBtnClick()
    {
        SceneManager.LoadScene(1);
    }

    // 结束游戏
    public void OnQuitGameBtnClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();      
#endif
    }
}
