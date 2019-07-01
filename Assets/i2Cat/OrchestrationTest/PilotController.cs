﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OrchestratorWrapping;
using UnityEngine.Video;
using System;

public enum Actions { VIDEO_1_START, VIDEO_1_PAUSE, VIDEO_2_START, VIDEO_2_PAUSE, WAIT }

abstract public class PilotController : MonoBehaviour {

    public static int my_ID = 0;

    public PlayerManager[] players;
    public VideoPlayer[] videos;

    //[HideInInspector]
    public OrchestrationBridge test;
    //[HideInInspector]
    public OrchestratorGui orchestrator;

    [HideInInspector]
    public GameObject mainPanel;
    [HideInInspector]
    public GameObject background;

    #region Sync
    public float timer = 0.0f;
    public float delay = 0.0f;
    public Actions todoAction = Actions.WAIT;
    #endregion

    #region Utils
    [HideInInspector]
    public Color playerCol = new Color(0.15f, 0.78f, 0.15f); // Green
    [HideInInspector]
    public Color otherCol = new Color(1.0f, 1.0f, 1.0f); // White
    [HideInInspector]
    public Color offlineCol = new Color(1.0f, 0.5f, 0.5f); // Red
    #endregion

    // Start is called before the first frame update
    public virtual void Start() {
        mainPanel = GameObject.Find("MainPanel");
        background = GameObject.Find("Background");
        test = GameObject.Find("ManagerTest").GetComponent<OrchestrationBridge>();
        orchestrator = GameObject.Find("MainWindow").GetComponent<OrchestratorGui>();
    }

    public virtual void Update() {
        SyncTool.UpdateTimes();
    }

    public void ActivateVoiceChat(VoicePlayer voicePlayer, int id) {
        voicePlayer.gameObject.SetActive(true);
        SocketIOServer.player[id] = voicePlayer;
        voicePlayer.Init();
    }

    public abstract void MessageActivation(string message);
}

