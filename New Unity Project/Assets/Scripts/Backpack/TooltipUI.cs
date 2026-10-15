
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Items;

namespace Game.UI
{
    /// <summary>
    /// 悬停提示框：鼠标停在道具槽位上时显示道具名 + 描述。
    /// 使用 TextMeshPro 显示文字，并跟随鼠标移动。
    /// </summary>
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
        [Tooltip("鼠标右下方偏移（屏幕像素）")]
        [SerializeField] private Vector2 _offset = new Vector2(16f, -16f);

        [Header("样式")]
        [SerializeField] private int _nameFontSize = 18;
        [SerializeField] private int _descriptionFontSize = 13;
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
            Vector2 target = screenPos + _offset;

            // 屏幕右侧越界时，将提示框移到鼠标左侧。
            float halfW = _panel.rect.width * 0.5f;
            float halfH = _panel.rect.height * 0.5f;

            if (target.x + halfW > Screen.width)
                target.x = screenPos.x - _offset.x - _panel.rect.width;

            // 屏幕底部越界时，将提示框移到鼠标上方。
            if (target.y - halfH < 0)
                target.y = screenPos.y + Mathf.Abs(_offset.y)
                    + _panel.rect.height;

            // 保留原有的 Screen Space - Overlay 定位方式。
            _panel.position = new Vector3(target.x, target.y, 0f);
        }
    }
}
