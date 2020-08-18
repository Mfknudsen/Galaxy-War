using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Rescaler : MonoBehaviour
{
    public CanvasScaler CS;
    public int StartSettingIndex = 1;
    public Vector2[] Settings;

    private void Start()
    {
        CS.referenceResolution = Settings[StartSettingIndex];
    }

    public void ChangeScaleSetting(int i)
    {
        CS.referenceResolution = Settings[i];
    }
}
