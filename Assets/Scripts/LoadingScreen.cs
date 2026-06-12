using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class LoadingScreen : MonoBehaviour
{
    [Header("UI References")]
    public Image loadingCircleImage;  // assign any one of the circle images
    public TextMeshProUGUI loadingText;

    [Header("Loading Circle Frames")]
    [Tooltip("Assign all 12 frames in order: Loading_Circle_1 to Loading_Circle_12")]
    public Sprite[] circleFrames;              // 12 frames

    [Header("Settings")]
    public float minimumLoadTime = 1.5f;
    public float dotAnimSpeed = 0.4f;
    public float frameRate = 0.08f;     // seconds per frame — adjust for speed

    void Start()
    {
        string sceneToLoad = PlayerPrefs.GetString("SceneToLoad", "SampleScene");
        StartCoroutine(AnimateCircle());
        StartCoroutine(AnimateDots());
        StartCoroutine(LoadGameScene(sceneToLoad));
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Cycle through 12 circle frames
    // ─────────────────────────────────────────────────────────────────────────
    IEnumerator AnimateCircle()
    {
        if (circleFrames == null || circleFrames.Length == 0) yield break;

        int index = 0;
        while (true)
        {
            if (loadingCircleImage == null) yield break;
            if (circleFrames[index] != null)
                loadingCircleImage.sprite = circleFrames[index];

            index = (index + 1) % circleFrames.Length;
            yield return new WaitForSeconds(frameRate);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Animate dots: Loading. → Loading.. → Loading...
    // ─────────────────────────────────────────────────────────────────────────
    IEnumerator AnimateDots()
    {
        string[] dotStates = { "Loading.", "Loading..", "Loading..." };
        int index = 0;

        while (true)
        {
            if (loadingText == null) yield break;
            loadingText.text = dotStates[index];

            index = (index + 1) % dotStates.Length;
            yield return new WaitForSeconds(dotAnimSpeed);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Async load game scene
    // ─────────────────────────────────────────────────────────────────────────
    IEnumerator LoadGameScene(string sceneName)
    {
        float elapsedTime = 0f;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            elapsedTime += Time.deltaTime;

            if (asyncLoad.progress >= 0.9f && elapsedTime >= minimumLoadTime)
            {
                yield return new WaitForSeconds(0.3f);
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}