using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [Serializable]
    public class SaveData
    {
        public string sceneName;
        public int dialogueId;
        public bool hasDialogueProgress;

        public string bgmId = "";
        public bool bgmStopped = false;

        // 当前周目已经解锁的手记线索
        public List<string> unlockedClueIds = new List<string>();
    }

    private string SavePath =>
        Path.Combine(Application.persistentDataPath, "game_save.json");

    public SaveData CurrentSave { get; private set; }

    public bool HasSave =>
        CurrentSave != null &&
        !string.IsNullOrEmpty(CurrentSave.sceneName);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadFromDisk();
    }

    public void SaveDialogueProgress(string sceneName, int dialogueId)
    {
        if (CurrentSave == null)
            CurrentSave = new SaveData();

        CurrentSave.sceneName = sceneName;
        CurrentSave.dialogueId = dialogueId;
        CurrentSave.hasDialogueProgress = true;

        WriteToDisk();
    }

    // 保存当前背景音乐状态，不覆盖已有的剧情进度
    public void SaveBGMState(string bgmId, bool stopped)
    {
        if (CurrentSave == null)
        {
            CurrentSave = new SaveData();
            CurrentSave.sceneName =
                UnityEngine.SceneManagement.SceneManager
                    .GetActiveScene().name;
        }

        CurrentSave.bgmId = bgmId ?? "";
        CurrentSave.bgmStopped = stopped;

        WriteToDisk();
    }

    // 保存当前已解锁的手记线索
    public void SaveUnlockedClues(List<string> clueIds)
    {
        if (CurrentSave == null)
            CurrentSave = new SaveData();

        CurrentSave.unlockedClueIds =
            clueIds != null
                ? new List<string>(clueIds)
                : new List<string>();

        WriteToDisk();
    }

    // 读取当前存档中的手记线索
    public List<string> LoadUnlockedClues()
    {
        if (CurrentSave == null ||
            CurrentSave.unlockedClueIds == null)
        {
            return new List<string>();
        }

        return new List<string>(CurrentSave.unlockedClueIds);
    }

    public bool TryGetDialogueId(string sceneName, out int dialogueId)
    {
        dialogueId = 0;

        if (CurrentSave == null ||
            !CurrentSave.hasDialogueProgress ||
            CurrentSave.sceneName != sceneName)
            return false;

        dialogueId = CurrentSave.dialogueId;
        return true;
    }

    public void ContinueGame()
    {
        LoadFromDisk();

        if (!HasSave)
        {
            Debug.LogWarning("[SaveManager] 当前没有可继续的存档。");
            return;
        }

        SceneManager.LoadScene(CurrentSave.sceneName);
    }

    public void StartNewGame(string firstSceneName)
    {
        DeleteSave();
        if (JournalManager.Instance != null)
        {
            JournalManager.Instance.ResetClues();
        }
        CurrentSave = new SaveData
        {
            sceneName = firstSceneName,
            hasDialogueProgress = false
        };

        WriteToDisk();
        SceneManager.LoadScene(firstSceneName);
    }

    public void DeleteSave()
    {
        CurrentSave = null;

        if (File.Exists(SavePath))
            File.Delete(SavePath);
    }

    private void LoadFromDisk()
    {
        if (!File.Exists(SavePath))
        {
            CurrentSave = null;
            return;
        }

        try
        {
            string json = File.ReadAllText(SavePath);
            CurrentSave = JsonUtility.FromJson<SaveData>(json);
        }
        catch (Exception e)
        {
            Debug.LogError("[SaveManager] 读取存档失败：" + e.Message);
            CurrentSave = null;
        }
    }

    private void WriteToDisk()
    {
        try
        {
            string json = JsonUtility.ToJson(CurrentSave, true);
            File.WriteAllText(SavePath, json);
        }
        catch (Exception e)
        {
            Debug.LogError("[SaveManager] 保存失败：" + e.Message);
        }
    }
}
