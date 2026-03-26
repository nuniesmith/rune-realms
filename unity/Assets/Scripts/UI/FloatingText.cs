using UnityEngine;
using TMPro;

namespace RuneRealms.UI
{
    /// <summary>
    /// Floating text that animates upward and fades out.
    /// Spawned dynamically by game events (XP gains, damage numbers, etc.)
    /// </summary>
    public class FloatingText : MonoBehaviour
    {
        private static GameObject prefab;

        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private float moveSpeed = 60f;
        [SerializeField] private float fadeSpeed = 2f;
        [SerializeField] private float lifetime = 1f;

        private float timer;
        private Color originalColor;

        /// <summary>
        /// Spawn floating text at the given world/screen position.
        /// If no prefab is set, creates a simple text dynamically.
        /// </summary>
        public static void Spawn(string message, Vector3 position, Color color)
        {
            // Find or create canvas
            var canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            var go = new GameObject("FloatingText");
            go.transform.SetParent(canvas.transform, false);
            go.transform.position = position;

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = message;
            tmp.color = color;
            tmp.fontSize = 18;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            tmp.raycastTarget = false;

            // Set size
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(200, 40);

            var ft = go.AddComponent<FloatingText>();
            ft.text = tmp;
            ft.originalColor = color;
        }

        private void Update()
        {
            timer += Time.deltaTime;

            // Move upward
            transform.position += Vector3.up * moveSpeed * Time.deltaTime;

            // Fade out
            if (text != null)
            {
                float alpha = Mathf.Lerp(1f, 0f, timer / lifetime);
                text.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            }

            // Destroy when done
            if (timer >= lifetime)
            {
                Destroy(gameObject);
            }
        }
    }
}
