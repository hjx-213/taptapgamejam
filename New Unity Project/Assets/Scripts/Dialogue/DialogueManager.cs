using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Game.Audio;

namespace Game.Dialogue
{
    public class DialogueManager : MonoBehaviour
    {
        [System.Serializable]
        public class AvatarEntry
        {
            public string characterName; // 角色名，例如：主角
            public string expression;    // 表情，例如：惊恐；默认表情留空
            public Sprite avatar;        // 对应头像图片
        }

        [Header("剧情数据")]
        [SerializeField] private DialogueDataReader dataReader;
        [SerializeField] private DialogueUI dialogueUI;

        [Header("人物头像表")]
        [SerializeField] private AvatarEntry[] avatarEntries;

        [Header("剧情入口")]
        [SerializeField] private int startId = 0;

        private int currentId;
        private bool isFinished;
        private AudioDatabase audioDatabase;

        private void Start()
        {
            if (dataReader == null || dialogueUI == null)
            {
                Debug.LogError(
                    "[DialogueManager] 请绑定 DialogueDataReader 和 DialogueUI。");
                enabled = false;
                return;
            }

            if (!dataReader.Load())
            {
                enabled = false;
                return;
            }

            if (SaveManager.Instance != null &&
                SaveManager.Instance.TryGetDialogueId(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
                    out int savedId))
            {
                currentId = savedId;
            }
            else
            {
                currentId = startId;
            }

            audioDatabase = FindObjectOfType<AudioDatabase>();
            if (audioDatabase == null)
                Debug.LogWarning("[DialogueManager] 场景中没有找到 AudioDatabase，剧情仍可播放，但音频列不会生效。");

            RestoreSavedBGM();
            ShowCurrentDialogue();
        }

        private void Update()
        {
            if (isFinished)
                return;

            if (!dataReader.TryGetRecord(
                    currentId, out DialogueRecord record))
                return;

            // 选项阶段只允许点击选项按钮。
            if (record.flag == "&")
                return;

            bool spacePressed = Input.GetKeyDown(KeyCode.Space);
            bool mousePressed = Input.GetMouseButtonDown(0);

            if (!spacePressed && !mousePressed)
                return;

            // 空格始终可用；鼠标点击 UI 时不推进剧情。
            if (spacePressed)
            {
                HandleAdvanceInput();
            }
            else if (mousePressed)
            {
                if (EventSystem.current != null &&
                    EventSystem.current.IsPointerOverGameObject())
                    return;

                HandleAdvanceInput();
            }
        }

        private void HandleAdvanceInput()
        {
            // 打字期间：先显示完整台词，不推进下一句。
            if (dialogueUI.IsTyping)
            {
                dialogueUI.CompleteTyping();
                return;
            }

            AdvanceDialogue();
        }

        private Sprite GetAvatar(string characterName)
        {
            string roleName = characterName.Trim();
            string expression = "";

            // 识别中文括号或英文括号中的表情。
            int left = roleName.LastIndexOf('（');
            int right = roleName.LastIndexOf('）');

            if (left >= 0 && right == roleName.Length - 1 && right > left)
            {
                expression = roleName.Substring(
                    left + 1, right - left - 1).Trim();

                roleName = roleName.Substring(0, left).Trim();
            }
            else
            {
                left = roleName.LastIndexOf('(');
                right = roleName.LastIndexOf(')');

                if (left >= 0 && right == roleName.Length - 1 && right > left)
                {
                    expression = roleName.Substring(
                        left + 1, right - left - 1).Trim();

                    roleName = roleName.Substring(0, left).Trim();
                }
            }

            // 优先查找角色名和表情都匹配的头像。
            foreach (AvatarEntry entry in avatarEntries)
            {
                if (entry.characterName == roleName &&
                    entry.expression == expression)
                {
                    return entry.avatar;
                }
            }

            // 找不到指定表情时，退回该角色的默认头像。
            foreach (AvatarEntry entry in avatarEntries)
            {
                if (entry.characterName == roleName &&
                    string.IsNullOrWhiteSpace(entry.expression))
                {
                    Debug.LogWarning(
                        $"没有找到 {roleName} 的“{expression}”表情，使用默认头像。");

                    return entry.avatar;
                }
            }

            Debug.LogWarning($"没有找到人物头像配置：{characterName}");
            return null;
        }

        private void ShowCurrentDialogue()
        {
            dialogueUI.ClearOptions();

            if (!dataReader.TryGetRecord(
                    currentId, out DialogueRecord record))
            {
                Debug.LogError(
                    $"[DialogueManager] 找不到剧情 ID：{currentId}");
                FinishDialogue();
                return;
            }

            if (record.flag == "END")
            {
                FinishDialogue();
                return;
            }

            if (record.flag == "&")
            {
                ShowOptionGroup();
                return;
            }

            // 每条普通台词只在显示时触发一次音频；空白字段不改变当前音频。
            HandleAudio(record);

            string displayName = record.character.Trim();

            int bracketIndex = displayName.LastIndexOf('（');
            if (bracketIndex < 0)
                bracketIndex = displayName.LastIndexOf('(');

            if (bracketIndex >= 0)
                displayName = displayName.Substring(0, bracketIndex).Trim();

            dialogueUI.ShowDialogue(
                displayName,
                record.content,
                record.position,
                GetAvatar(record.character)
            );
        }

        private void HandleAudio(DialogueRecord record)
        {
            if (string.IsNullOrWhiteSpace(record.bgm) &&
                string.IsNullOrWhiteSpace(record.sfx))
                return;

            if (AudioManager.Instance == null)
            {
                Debug.LogWarning("[DialogueManager] 找不到 AudioManager，无法播放音频。");
                return;
            }

            if (!string.IsNullOrWhiteSpace(record.bgm))
            {
                string bgmId = record.bgm.Trim();

                if (string.Equals(
                    bgmId, "STOP",
                    System.StringComparison.OrdinalIgnoreCase))
                {
                    AudioManager.Instance.StopBGM();

                    if (SaveManager.Instance != null)
                        SaveManager.Instance.SaveBGMState("", true);
                }
                else if (audioDatabase != null &&
                        audioDatabase.TryGetBGM(
                            bgmId, out AudioClip bgmClip))
                {
                    AudioManager.Instance.PlayBGM(bgmClip);

                    if (SaveManager.Instance != null)
                        SaveManager.Instance.SaveBGMState(bgmId, false);
                }
                else
                {
                    Debug.LogWarning(
                        $"[DialogueManager] 找不到背景音乐 ID：{bgmId}");
                }
            }

            if (!string.IsNullOrWhiteSpace(record.sfx))
            {
                if (audioDatabase != null && audioDatabase.TryGetSFX(record.sfx, out AudioClip sfxClip))
                {
                    AudioManager.Instance.PlaySFX(sfxClip);
                }
                else
                {
                    Debug.LogWarning($"[DialogueManager] 找不到音效 ID：{record.sfx}");
                }
            }
        }

        private void RestoreSavedBGM()
        {
            if (SaveManager.Instance == null)
                return;

            SaveManager.SaveData save =
                SaveManager.Instance.CurrentSave;

            if (save == null)
                return;

            // 存档明确要求停止音乐
            if (save.bgmStopped)
            {
                if (AudioManager.Instance != null)
                    AudioManager.Instance.StopBGM();

                return;
            }

            // 没有记录过音乐，不进行恢复
            if (string.IsNullOrWhiteSpace(save.bgmId))
                return;

            if (AudioManager.Instance == null)
            {
                Debug.LogWarning(
                    "[DialogueManager] 找不到 AudioManager，无法恢复背景音乐。");
                return;
            }

            if (audioDatabase != null &&
                audioDatabase.TryGetBGM(
                    save.bgmId, out AudioClip bgmClip))
            {
                AudioManager.Instance.PlayBGM(bgmClip);
                Debug.Log("[DialogueManager] 已恢复背景音乐：" + save.bgmId);
            }
            else
            {
                Debug.LogWarning(
                    "[DialogueManager] 无法恢复背景音乐，找不到 ID：" + save.bgmId);
            }
        }

        private void ShowOptionGroup()
        {
            List<DialogueRecord> options =
                dataReader.GetConsecutiveOptions(currentId);

            if (options.Count == 0)
            {
                Debug.LogError(
                    $"[DialogueManager] ID {currentId} 没有找到有效选项。");
                return;
            }

            dialogueUI.ShowOptions(options, OnOptionSelected);
        }

        private void SaveCurrentProgress()
        {
            if (SaveManager.Instance == null)
                return;

            string sceneName =
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            SaveManager.Instance.SaveDialogueProgress(sceneName, currentId);
        }

        private void OnOptionSelected(DialogueRecord option)
        {
            if (isFinished)
                return;

            currentId = option.nextId;

            SaveCurrentProgress();
            ShowCurrentDialogue();
        }

        private void AdvanceDialogue()
        {
            if (!dataReader.TryGetRecord(
                    currentId, out DialogueRecord record))
                return;

            if (record.flag == "&" || record.flag == "END")
                return;

            currentId = record.nextId;

            SaveCurrentProgress();
            ShowCurrentDialogue();
        }

        private void FinishDialogue()
        {
            isFinished = true;
            dialogueUI.HideDialogue();
            Debug.Log("[DialogueManager] 当前剧情结束。");
        }
    }
}
