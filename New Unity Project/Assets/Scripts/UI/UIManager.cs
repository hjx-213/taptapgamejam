using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Sample;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [System.Serializable]
    public class UIPanel
    {
        public string panelName;
        public GameObject panelObject;
    }

    [Header("主面板")]
    [SerializeField] private GameObject backpackPanel;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject journalPanel;

    public UIPanel[] panels;
    [SerializeField] private TMPro.TMP_InputField passwordInput;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SubmitPassword(PasswordBox puzzle)
    {
        if (passwordInput == null || puzzle == null)
        {
            Debug.LogWarning("UIManager：密码输入框或密码谜题没有绑定！");
            return;
        }

        puzzle.OnCodeSubmit(passwordInput.text);
    }
}