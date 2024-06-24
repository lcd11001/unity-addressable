using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class CaptureScreenshot : MonoBehaviour, IPointerClickHandler
{
    public Canvas canvas;
    public ScreenshotEvent OnScreenshotTaken;

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Taking screenshot...");
        StartCoroutine(TakeScreenshot());
    }

    private IEnumerator TakeScreenshot()
    {
        // Hide UI
        canvas.enabled = false;
        yield return new WaitForEndOfFrame();
        var screenshot = ScreenCapture.CaptureScreenshotAsTexture();
        // Show UI
        canvas.enabled = true;
        OnScreenshotTaken?.Invoke(screenshot);
    }
}

[Serializable]
public class ScreenshotEvent : UnityEvent<Texture2D>
{

}
