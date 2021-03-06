﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Random = System.Random;

public enum TaskType
{
    PaperShred,
    PaperFax
}
public class TaskTypeEvent : UnityEvent<TaskType> {
}
public class MinigameController : MonoBehaviour
{
    public float startTime;
    public float gameAccelerator;
    public float startAccelerator = 1;
    public float maxAccelerator;
    public float GAME_SECONDS = 30;
    public Pool pool;
    public Queue<TaskType> paperQueue = new Queue<TaskType>();
    
    public GameObject fax;
    public GameObject shredder;
    private Collider2D faxCol;
    private Collider2D shredCol;
    public int completed=0;
    public float gameTimer = 0;

    public float[] MAX_ACCEL_PER_LEVEL = new float[3] { 1, 4, 2 };
    public int[] QUOTA_PER_LEVEL = new int[3];
    public float PAPERS_PER_SEC = 1;
    public float PAPER_FREQ;

    public int level = 0;
    public bool active = false;

    public Camera horizontalCamera, verticalCamera;
    public Scene gameScene;
    public DetectOrientation orientation;

    private TaskType nextTask;

    public UnityEvent<TaskType> spawnTimer;

    // Start is called before the first frame update
    void Start()
    {
        faxCol = fax.GetComponent<Collider2D>();
        shredCol = shredder.GetComponent<Collider2D>();
        DontDestroyOnLoad(this);
        spawnTimer = new TaskTypeEvent();
        SceneManager.sceneLoaded += OnSceneLoaded;
        gameScene = SceneManager.GetSceneByName("Minigame");
        orientation.onRotateLandscape.AddListener(landscapeCamera);
        orientation.onRotatePortrait.AddListener(portraitCamera);
        for (int i = 0; i < MAX_ACCEL_PER_LEVEL.Length; i++)
            QUOTA_PER_LEVEL[i] = Mathf.FloorToInt(PAPERS_PER_SEC * GAME_SECONDS * ((MAX_ACCEL_PER_LEVEL[i] - 1) / 2 + 1)) - (2 * (int)MAX_ACCEL_PER_LEVEL[i]);
        PAPER_FREQ = 1.0f / PAPERS_PER_SEC;
        nextTask = UnityEngine.Random.value > .5f ? TaskType.PaperShred : TaskType.PaperFax;
        OnSceneLoaded(gameScene, LoadSceneMode.Single);
    }

    void portraitCamera()
    {
        verticalCamera.enabled = true;
        verticalCamera.GetComponent<AudioListener>().enabled = true;
        horizontalCamera.enabled = false;
        horizontalCamera.GetComponent<AudioListener>().enabled = false;
    }

    void landscapeCamera()
    {
        verticalCamera.enabled = false;
        verticalCamera.GetComponent<AudioListener>().enabled = false;
        horizontalCamera.enabled = true;
        horizontalCamera.GetComponent<AudioListener>().enabled = true;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene == gameScene)
        {
            portraitCamera();
            startTime = Time.time;
            gameAccelerator = startAccelerator;
            maxAccelerator = MAX_ACCEL_PER_LEVEL[level];
            gameTimer = 0;

            active = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if ((Time.time - startTime >= GAME_SECONDS || completed == QUOTA_PER_LEVEL[level]) && active)
        {
            level++;
            active = false;
            //end game
            //switch scene
            SwitchScene();
        }
        else if (active)
        {
            gameAccelerator = (Time.time - startTime) / GAME_SECONDS * (maxAccelerator - startAccelerator) +
                              startAccelerator;
            gameTimer += Time.deltaTime * gameAccelerator;
            if (gameTimer >= PAPER_FREQ)
            {
                //spawnTimer.Invoke(nextTask);
                paperQueue.Enqueue(nextTask);
                gameTimer -= PAPER_FREQ;
                //write a better spawn system here
                nextTask = UnityEngine.Random.value > .5f ? TaskType.PaperShred : TaskType.PaperFax;
            }
            if (paperQueue.Count > 0 && pool.onScreen() == 0)
            {
                spawnTimer.Invoke(paperQueue.Dequeue());
            }
        }
    }

    private void SwitchScene()
    {
        SceneManager.LoadScene("Dialogue");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //check the TaskType Paper
        Paper paper = other.GetComponent<Paper>();
        if (paper.getType() == TaskType.PaperFax) {
            completed++;
        }
        else if(paper.getType() == TaskType.PaperShred) {
            completed++;
        }
            Debug.Log(completed);
    }


}
