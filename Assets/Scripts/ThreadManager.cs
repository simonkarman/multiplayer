﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class ThreadManager : MonoBehaviour {
    private static ThreadManager instance;
    private readonly List<Action> requestedActions = new List<Action>();
    private readonly List<Action> currentActions = new List<Action>();

    public static void Activate() {
        if (instance == null) {
            instance = new GameObject("Thread Manager").AddComponent<ThreadManager>();
        }
    }

    public static void ExecuteOnMainThread(Action _action) {
        lock (instance.requestedActions) {
            instance.requestedActions.Add(_action);
        }
    }

    protected void Update() {
        lock (requestedActions) {
            currentActions.AddRange(requestedActions);
            requestedActions.Clear();
        }
        for (int i = 0; i < currentActions.Count; i++) {
            currentActions[i]();
        }
        currentActions.Clear();
    }
}