using UnityEngine;
using System.Collections;
using TMPro;

// ─────────────────────────────────────────────────────────────────────────────
//  FloatingText
//  Floating text that rises and fades out.
//  childMode = true means parent FloatingRoot handles rising — text only fades.
// ─────────────────────────────────────────────────────────────────────────────
public class FloatingText : MonoBehaviour
{
    private TextMeshPro tmp;

    public void Initialize(string text, Color color, float fontSize,
                           float riseSpeed, float duration, bool childMode = false)
    {
        tmp = gameObject.AddComponent<TextMeshPro>();
        tmp.text = text;
        tmp.color = color;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        tmp.sortingOrder = 20;
        tmp.outlineWidth = 0.2f;
        tmp.outlineColor = new Color32(0, 0, 0, 255);

        StartCoroutine(Animate(riseSpeed, duration, childMode));
    }

    IEnumerator Animate(float riseSpeed, float duration, bool childMode)
    {
        float timer = 0f;
        Color startColor = tmp.color;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            // Rise upward only if not child mode
            if (!childMode)
                transform.position += Vector3.up * riseSpeed * Time.deltaTime;

            // Fade out in second half
            if (t > 0.5f)
            {
                float fadeT = (t - 0.5f) / 0.5f;
                Color c = startColor;
                c.a = Mathf.Lerp(1f, 0f, fadeT);
                tmp.color = c;
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
//  FloatingRoot
//  Animates icon + text group together — rises and fades as one unit.
// ─────────────────────────────────────────────────────────────────────────────
public class FloatingRoot : MonoBehaviour
{
    public void Initialize(float riseSpeed, float duration)
    {
        StartCoroutine(Animate(riseSpeed, duration));
    }

    IEnumerator Animate(float riseSpeed, float duration)
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            // Rise upward
            transform.position += Vector3.up * riseSpeed * Time.deltaTime;

            // Fade out sprite renderers in second half
            if (t > 0.5f)
            {
                float fadeT = (t - 0.5f) / 0.5f;
                float alpha = Mathf.Lerp(1f, 0f, fadeT);

                foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>())
                {
                    Color c = sr.color;
                    c.a = alpha;
                    sr.color = c;
                }
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}