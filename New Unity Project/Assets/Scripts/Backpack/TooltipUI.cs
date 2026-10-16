
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Items;

namespace Game.UI
{
    // 悬停提示框：鼠标停在道具槽位上时显示道具名 + 描述。
    // 使用 TextMeshPro 显示文字，并跟随鼠标移动。
    public class TooltipUI : MonoBehaviour
    {
        private static TooltipUI _instance;

        public static TooltipUI Instance
        {
            get
            {
                if (_instance == null)
                    _instance = Object.FindObjectOfType<TooltipUI>(true);

                return _instance;
            }
            private set { _instance = value; }
        }

        [Header("UI 引用")]
        [SerializeField] private RectTransform _panel;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _descriptionText;

        [Header("偏移")]
        [Tooltip("鼠标正上方偏移（屏幕像素）")]
        [SerializeField] private Vector2 _offset = new Vector2(0f, 30f);

        [Header("样式")]
        [SerializeField] private int _nameFontSize;
        [SerializeField] private int _descriptionFontSize;
        [SerializeField] private Color _nameColor =
            new Color(1f, 0.95f, 0.7f, 1f);
        [SerializeField] private Color _descriptionColor =
            new Color(0.9f, 0.9f, 0.9f, 1f);

        private Canvas _canvas;
        private RectTransform _canvasRect;

        private void Awake()
        {
            Instance = this;

            if (_panel == null)
                _panel = transform as RectTransform;

            _canvas = GetComponentInParent<Canvas>();

            if (_canvas != null)
                _canvasRect = _canvas.transform as RectTransform;

            // 禁用提示框及其子物体的射线检测，避免挡住鼠标事件。
            foreach (var graphic in GetComponentsInChildren<Graphic>(true))
                graphic.raycastTarget = false;

            if (_nameText != null)
            {
                _nameText.fontSize = _nameFontSize;
                _nameText.color = _nameColor;
            }

            if (_descriptionText != null)
            {
                _descriptionText.fontSize = _descriptionFontSize;
                _descriptionText.color = _descriptionColor;
            }

            Hide();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void Show(ItemData item)
        {
            if (item == null)
                return;

            if (_panel == null)
                _panel = transform as RectTransform;

            // 仍然直接读取 ItemData 中的名称和描述。
            if (_nameText != null)
                _nameText.text = item.itemName;

            if (_descriptionText != null)
                _descriptionText.text = item.description ?? "";

            if (_panel != null)
                _panel.gameObject.SetActive(true);

            UpdatePosition();
        }

        public void Hide()
        {
            if (_panel != null)
                _panel.gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            if (_panel != null && _panel.gameObject.activeSelf)
                UpdatePosition();
        }

        private void UpdatePosition()
        {
            if (_panel == null)
                return;

            Vector2 screenPos = Input.mousePosition;
            float halfW = _panel.rect.width * 0.5f;
            float halfH = _panel.rect.height * 0.5f;

            // 默认在鼠标正上方，保留可配置的横向偏移。
            Vector2 target = screenPos + new Vector2(_offset.x, Mathf.Abs(_offset.y));

            // 左右越界时，自动贴住屏幕边缘。
            if (target.x - halfW < 0f)
                target.x = halfW + 8f;
            else if (target.x + halfW > Screen.width)
                target.x = Screen.width - halfW - 8f;

            // 顶部/底部越界时，仍然保持在鼠标上方附近。
            if (target.y + halfH > Screen.height)
                target.y = Screen.height - halfH - 8f;
            else if (target.y - halfH < 0f)
                target.y = halfH + 8f;

            _panel.position = new Vector3(target.x, target.y, 0f);
        }
    }
}
