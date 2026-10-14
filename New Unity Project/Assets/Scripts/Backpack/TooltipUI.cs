using UnityEngine;
using UnityEngine.UI;
using Game.Items;

namespace Game.UI
{
    /// <summary>
    /// 悬停提示框：鼠标停在道具槽位上时显示道具名 + 描述。
    /// 跟随鼠标移动。
    ///
    /// 搭建方式：
    ///   1. Canvas 下挂 Panel "TooltipPanel"（带半透明背景的 Image）
    ///   2. Panel 下挂两个 Text（道具名 + 描述）
    ///   3. Panel 挂本组件，拖入两个 Text
    ///   4. Panel 物体必须保持【勾选/激活】状态（不要手动禁用！）
    ///      Awake() 会自己调用 Hide() 把面板藏起来；
    ///      手动禁用物体会导致 Awake 不运行、Instance 为 null，tooltip 永远不显示。
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
        [SerializeField] private Text _nameText;
        [SerializeField] private Text _descriptionText;

        [Header("偏移")]
        [Tooltip("鼠标右下方偏移（屏幕像素）")]
        [SerializeField] private Vector2 _offset = new Vector2(16f, -16f);

        [Header("样式")]
        [SerializeField] private int _nameFontSize = 18;
        [SerializeField] private int _descriptionFontSize = 13;
        [SerializeField] private Color _nameColor = new Color(1f, 0.95f, 0.7f, 1f);
        [SerializeField] private Color _descriptionColor = new Color(0.9f, 0.9f, 0.9f, 1f);

        private Canvas _canvas;
        private RectTransform _canvasRect;

        private void Awake()
        {
            Instance = this;

            if (_panel == null) _panel = transform as RectTransform;
            _canvas = GetComponentInParent<Canvas>();
            if (_canvas != null) _canvasRect = _canvas.transform as RectTransform;

            // 关键：禁用 panel 和所有子物体的射线检测
            // 否则 tooltip 会拦截鼠标事件，闪烁就是由此产生的
            foreach (var g in GetComponentsInChildren<Graphic>(true))
                g.raycastTarget = false;

            if (_nameText != null) _nameText.fontSize = _nameFontSize;
            if (_descriptionText != null) _descriptionText.fontSize = _descriptionFontSize;
            if (_nameText != null) _nameText.color = _nameColor;
            if (_descriptionText != null) _descriptionText.color = _descriptionColor;

            Hide();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void Show(ItemData item)
        {
            if (item == null) return;
            if (_panel == null) _panel = transform as RectTransform;

            if (_nameText != null) _nameText.text = item.itemName;
            if (_descriptionText != null) _descriptionText.text = item.description ?? "";

            if (_panel != null) _panel.gameObject.SetActive(true);

            UpdatePosition();
        }

        public void Hide()
        {
            if (_panel != null) _panel.gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            if (_panel != null && _panel.gameObject.activeSelf)
            {
                UpdatePosition();
            }
        }

        private void UpdatePosition()
        {
            if (_panel == null) return;

            Vector2 screenPos = Input.mousePosition;
            Vector2 target = screenPos + _offset;

            // 屏幕越界时翻转偏移方向
            float halfW = _panel.rect.width * 0.5f;
            float halfH = _panel.rect.height * 0.5f;
            if (target.x + halfW > Screen.width) target.x = screenPos.x - _offset.x - _panel.rect.width;
            if (target.y - halfH < 0) target.y = screenPos.y + Mathf.Abs(_offset.y) + _panel.rect.height;

            // Screen Space - Overlay 下，world position = screen pixels；这个写法跟随鼠标有效
            _panel.position = new Vector3(target.x, target.y, 0f);
        }
    }
}
