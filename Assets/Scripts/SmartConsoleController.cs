using System.Collections;
using System.Collections.Generic;
using ED.SC.Components;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SmartConsoleController : MonoBehaviour
{
    [SerializeField] private ConsoleInputHandler consoleInputHandler;
    [SerializeField] private RawImage imageLogo;

    // Start is called before the first frame update
    void Start()
    {
        EventTrigger trigger = imageLogo.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = imageLogo.gameObject.AddComponent<EventTrigger>();
        }

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;

        entry.callback.AddListener((data) =>
        {
            consoleInputHandler.ToggleInputDispatcher();
        });

        trigger.triggers.Add(entry);
    }
}
