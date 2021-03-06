﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Threading;
using System.Net;

public class MainMenu : MonoBehaviour {
    [SerializeField]
    private InputField connectionStringInput = default;
    [SerializeField]
    private Button connectButton = default;
    [SerializeField]
    private Text connectButtonText = default;

    protected void Start() {
        KarmanNet.Networking.ThreadManager.Activate();
        connectionStringInput.text = PlayerPrefs.GetString(ClientFlow.CONNECTION_STRING_PLAYER_PREFS_KEY, "localhost");
    }

    public void CreateServer() {
        SceneManager.LoadScene("Server");
    }

    public void Connect() {
        string connectionString = connectionStringInput.text;
        connectButton.interactable = false;
        connectButtonText.text = "Loading...";
        connectionStringInput.interactable = false;
        Thread thread = new Thread(() => {
            try {
                Debug.Log(string.Format("Parsing of connection string '{0}'", connectionString), this);
                IPEndPoint endpoint = KarmanNet.Networking.ConnectionString.Parse(connectionString, ServerFlow.DEFAULT_SERVER_PORT);
                Debug.Log(string.Format("Parsing connection string '{0}' resulted in {1}", connectionString, endpoint.ToString()), this);
                KarmanNet.Networking.ThreadManager.ExecuteOnMainThread(() => {
                    PlayerPrefs.SetString(ClientFlow.CONNECTION_STRING_PLAYER_PREFS_KEY, connectionStringInput.text);
                    PlayerPrefs.Save();
                    SceneManager.LoadScene("Client");
                });
            } catch (Exception exception) {
                Debug.LogError(string.Format("Parsing connection string '{0}' failed: {1}", connectionString, exception.ToString()), this);
            } finally {
                KarmanNet.Networking.ThreadManager.ExecuteOnMainThread(() => {
                    connectButton.interactable = true;
                    connectButtonText.text = "Connect";
                    connectionStringInput.interactable = true;
                });
            }
        });
        thread.Start();
    }
}
