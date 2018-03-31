using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Player : MonoBehaviour {
    public GameObject firstBrickGo;             // 第一个砖块
    public GameObject brickPreafab;             // 砖块预制体
    public GameObject finishedGo;               // 结束界面

    public Text scoreText;                      // 当前分数组件
    public Text addScoreText;                   // 增加分数组件
    public Text maxScoreText;                   // 最高分组件

    // 0 跳跃 1 失败 2 得分 3 按压
    public AudioClip[] audioClips;              // 声音源文件

    public Transform initBrickPos;                // 初始砖块位置
    public Transform initPlayerPos;                // 初始玩家位置
    public Transform initCameraPos;                // 初始相机位置
    
    private Rigidbody playerRigidBody;          // 玩家刚体

    private GameObject currentBrick;            // 当前砖块
    private GameObject lastBrick;            // 后一砖块

    private ParticleSystem playerPS;            // 玩家粒子效果

    private AudioSource audioSource;            // 声音组件

    private Collision lastCollision;            // 上一次碰撞

    private Vector3 brickDir;                   // 下一砖块方位
    private Vector3 relativeOffestWithCamera;   // 与相机的相对偏移

    private float maxDuration = 2;                  // 最大按压时间
    private float startPressTime;               // 开始按键时间
    private bool isFirst = true;                // 是否为第一次生成方块
    private bool isMoveCamera;                  // 是否移动相机
    private int score = 0;                      // 当前分数
    private int scoreAdd = 1;                   // 当前需要加的分数

    private static Player instance;             // 单例

    public static Player Instance
    {
        get { return Player.instance; }
        set { Player.instance = value; }
    }

    void Awake()
    {
        Instance = this;
    }

	// Use this for initialization
	void Start () {
        playerRigidBody = this.GetComponent<Rigidbody>();       // 获取刚体
        playerPS = GameObject.Find("Player/PlayerPs").GetComponent<ParticleSystem>();       // 获取粒子特效
        audioSource = this.GetComponent<AudioSource>();         // 获取声音组件
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())        // 鼠标左键按下,且未点击到UI上,且未在飞
        {
            startPressTime = Time.time;
            playerPS.Play();                    // 播放按压特效
            audioSource.PlayOneShot(audioClips[3]);     // 播放按压音效
        }
        if (Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            audioSource.Stop();                     // 停止播放按压音频
            playerPS.Stop();                        // 停止粒子效果
            audioSource.PlayOneShot(audioClips[0]);     // 播放跳跃音效
            DoMove();                   // 开始跳跃
        }
	}

    void LateUpdate()
    {
        MoveCamera();               // 移动相机
    }

    // 添加砖块
    public void CreateBrick()
    {
        int randomDir = Random.Range(0, 2);
        brickDir = ((randomDir == 0) ? new Vector3(1, 0, 0) : new Vector3(0, 0, 1));             // 随机生成方向
        float distance = Random.Range(5.0f, 10.0f);                                              // 随机距离
        float scale = Random.Range(2.0f, 5.0f);                                                     // 随机大小
        Color color = new Color(Random.Range(0f, 1), Random.Range(0f, 1), Random.Range(0f, 1));     // 随机颜色
        // 生成砖块
        GameObject itemGo = GameObject.Instantiate(brickPreafab, currentBrick.transform.position + brickDir * distance, Quaternion.identity);
        itemGo.transform.localScale = new Vector3(scale, 2f, scale);
        itemGo.GetComponent<MeshRenderer>().material.color = color;
        lastBrick = itemGo;
    }

    // 玩家移动
    private void DoMove()
    {
        float duration = Time.time - startPressTime;
        if (duration > maxDuration)
        {
            duration = maxDuration;
        }
        Vector3 dir = (lastBrick.transform.position - this.transform.position).normalized;          // 下一个砖块方位
        dir.y = 0;
        // 往上方和下一个砖块中心方向添加力
        playerRigidBody.AddForce(new Vector3(0, 8, 0) + dir * duration * 10, ForceMode.Impulse);
    }

    // 玩家与其他物体碰撞
    void OnCollisionEnter(Collision collision)
    {
        audioSource.Stop();         // 停止播放
        string tag = collision.transform.gameObject.tag;            // 碰撞物体标签
        if (tag == "Brick")             // 碰到砖块
        {    
            if (collision.transform.gameObject == currentBrick)     // 若还在同一个砖块
            {
                return;
            }
            
            if (currentBrick != null)       
            {
                Destroy(currentBrick, 1);       // 销毁之前的砖块
                if (IsOnTheCenter())
                {
                    scoreAdd *= 2;
                    AddScore(scoreAdd);                     // 在中央
                }
                else
                {
                    scoreAdd = 1;
                    AddScore(scoreAdd);                     // 不在中央
                }
            }
            if (isFirst)                // 游戏开始，第一次创建砖块
            {
                relativeOffestWithCamera = Camera.main.transform.position - transform.position;     // 获取相对偏移
                isFirst = false;
            }
            currentBrick = collision.transform.gameObject;          // 更新当前砖块，即玩家所在砖块
            CreateBrick();      // 新添加砖块
            isMoveCamera = true;        // 移动相机
        }
        else if (tag == "Plane")    // 碰到地板
        {
            GameOver();             // 游戏结束
        }
    }

    // 移动相机
    private void MoveCamera()
    {
        if (isMoveCamera)               // 相机跟随玩家移动效果
        {
            Vector3 targetPos = relativeOffestWithCamera + this.transform.position;        // 相机当前位置
            Vector3 startPos = Camera.main.transform.position;
            Camera.main.transform.position = Vector3.Lerp(startPos, targetPos, 0.3f);      // 插值
            if (Vector3.Distance(targetPos, Camera.main.transform.position) < 0.1f)        // 到达目标位置
            {
                isMoveCamera = false;
            }
        }
    }

    // 加分
    private void AddScore(int num = 1)
    {
        audioSource.PlayOneShot(audioClips[2]);             // 播放得分音效
        addScoreText.text = "+" + num;                      // 得分动画
        addScoreText.GetComponent<Animator>().SetTrigger("AddScore");
        score += num;                       // 更新得分标签
        scoreText.text = score + "";
    }

    // 游戏结束
    private void GameOver()
    {
        audioSource.PlayOneShot(audioClips[1]);         // 播放失败音效
        finishedGo.SetActive(true);
        int maxScore = PlayerPrefs.GetInt("MaxScore", 0);       // 最高分
        if (score > maxScore)
        {
            maxScore = score;
            PlayerPrefs.SetInt("MaxScore", maxScore);
        }
        maxScoreText.text = maxScore + "";
    }

    // 点击重新开始按钮
    public void OnRestartBtnClick()
    {
        GameObject[] brickGos = GameObject.FindGameObjectsWithTag("Brick");
        for (int i = 0; i < brickGos.Length; ++i)       // 销毁场景中所有砖块
        {
            Destroy(brickGos[i]);
        }
        finishedGo.SetActive(false);            // 隐藏结束界面
        score = 0;                              // 初始化得分
        scoreText.text = score + "";
        scoreAdd = 1;
        this.transform.position = initPlayerPos.position;           // 玩家回到初始位置
        playerRigidBody.velocity = Vector3.zero;
        // 初始化第一个砖块
        GameObject.Instantiate(brickPreafab, initBrickPos.position, Quaternion.identity);
        Camera.main.transform.position = initCameraPos.position;    // 初始化相机位置                               
    }

    // 检测是否在砖块中央
    private bool IsOnTheCenter()
    {
        float offsetX = this.transform.position.x - lastBrick.transform.position.x;         // 下一个砖块，即碰撞后的当前砖块
        float offsetZ = this.transform.position.z - lastBrick.transform.position.z;
        if (Mathf.Abs(offsetX) < 0.8f && Mathf.Abs(offsetZ) < 0.8f)         // 允许有误差
        {
            return true;
        }
        return false;
    }

    // 点击结束游戏按钮
    public void OnQuitGameBtnClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();      
#endif
    }
}
