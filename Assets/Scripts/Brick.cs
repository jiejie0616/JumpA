using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Brick : MonoBehaviour {
    private bool isPress;       // 是否按压
    private bool isPlayerOn;    // 玩家是否在上面

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            isPress = true;         // 鼠标按下
        }
        if (Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject())
        {   
            isPress = false;        // 鼠标未按下
        }
        if (isPress && isPlayerOn)  // 鼠标按下，且玩家在上面，按压
        {
            Vector3 pos = transform.position;
            if (pos.y > 0)
            {
                pos.y -= 0.01f;
            }
            transform.position = pos;

        }
        else                        // 否则恢复原状
        {
            Vector3 pos = transform.position;
            if (pos.y < 1)
            {
                pos.y += 0.02f;
            }
            transform.position = pos;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.gameObject.tag == "player")
        {
            // 玩家到达该砖块
            isPlayerOn = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.transform.gameObject.tag == "player")
        {
            // 玩家离开该砖块
            isPlayerOn = false;
        }
    }
}
