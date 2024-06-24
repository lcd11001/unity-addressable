using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.Events;

public class FirebaseInit : MonoBehaviour
{
    public UnityEvent OnFirebaseInitialized;
    // Start is called before the first frame update
    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError($"Failed to initialize Firebase with {task.Exception.Message}");
                return;
            }
            if (task.Result == DependencyStatus.Available)
            {
                Debug.Log("Firebase initialized");
                OnFirebaseInitialized?.Invoke();
            }
            else
            {
                Debug.LogError($"Failed to initialize Firebase with {task.Result}");
            }
        });
    }
}
