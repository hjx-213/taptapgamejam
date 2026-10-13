using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Game.Dialogue
{
    /// <summary>
    /// 一条 CSV 剧情记录。
    /// 对应：标志、ID、人物、位置、内容、跳转、BGM、SFX。
    /// </summary>
    [Serializable]
    public class DialogueRecord
    {
        public string flag;
        public int id;
        public string character;
        public string position;
        public string content;
        public int nextId;
        public string bgm;
        public string sfx;
    }

    /// <summary>
    /// 读取并解析 CSV，不负责显示 UI 或控制剧情。
    /// </summary>
    public class DialogueDataReader : MonoBehaviour
    {
        [Header("CSV 剧情文件")]
        [SerializeField] private TextAsset dialogueCsv;

        private readonly Dictionary<int, DialogueRecord> records =
            new Dictionary<int, DialogueRecord>();

        private readonly List<DialogueRecord> orderedRecords =
            new List<DialogueRecord>();

        public int RecordCount => records.Count;

        public bool TryGetRecord(int id, out DialogueRecord record)
        {
            return records.TryGetValue(id, out record);
        }

        public List<DialogueRecord> GetConsecutiveOptions(int startId)
        {
            List<DialogueRecord> options = new List<DialogueRecord>();

            int startIndex = orderedRecords.FindIndex(
                record => record.id == startId);

            if (startIndex < 0)
                return options;

            for (int i = startIndex; i < orderedRecords.Count; i++)
            {
                DialogueRecord record = orderedRecords[i];

                if (record.flag != "&")
                    break;

                options.Add(record);
            }

            return options;
        }

        public bool Load()
        {
            records.Clear();
            orderedRecords.Clear();

            if (dialogueCsv == null)
            {
                Debug.LogError("[DialogueDataReader] 尚未绑定 CSV 文件。");
                return false;
            }

            List<List<string>> rows;

            try
            {
                rows = ParseCsv(dialogueCsv.text);
            }
            catch (Exception e)
            {
                Debug.LogError(
                    $"[DialogueDataReader] CSV 解析失败：{e.Message}");
                return false;
            }

            int loadedCount = 0;

            foreach (List<string> row in rows)
            {
                if (row.Count == 0)
                    continue;

                string flag = row[0].Trim();

                // 跳过空行和表头。
                if (string.IsNullOrWhiteSpace(flag) ||
                    flag == "标志" ||
                    row.Count > 1 && row[1].Trim() == "ID")
                    continue;

                if (row.Count < 6)
                {
                    Debug.LogError(
                        $"[DialogueDataReader] 某行不足六列：{string.Join(",", row)}");
                    records.Clear();
                    return false;
                }

                string idText = row[1].Trim();

                if (!int.TryParse(idText, out int id))
                {
                    Debug.LogError(
                        $"[DialogueDataReader] ID 无效：{idText}");
                    records.Clear();
                    return false;
                }

                int nextId = -1;
                string nextText = row[5].Trim();

                if (flag != "END" &&
                    !int.TryParse(nextText, out nextId))
                {
                    Debug.LogError(
                        $"[DialogueDataReader] ID {id} 的跳转值无效：{nextText}");
                    records.Clear();
                    return false;
                }

                if (flag != "#" && flag != "&" && flag != "END")
                {
                    Debug.LogError(
                        $"[DialogueDataReader] ID {id} 的标志无效：{flag}");
                    records.Clear();
                    return false;
                }

                DialogueRecord record = new DialogueRecord
                {
                    flag = flag,
                    id = id,
                    character = row[2].Trim(),
                    position = row[3].Trim(),
                    content = row[4],
                    nextId = nextId,
                    // 兼容旧版六列表格：未提供的音频列默认留空。
                    bgm = row.Count > 6 ? row[6].Trim() : "",
                    sfx = row.Count > 7 ? row[7].Trim() : ""
                };

                if (records.ContainsKey(id))
                {
                    Debug.LogError(
                        $"[DialogueDataReader] ID 重复：{id}");
                    records.Clear();
                    return false;
                }

                records.Add(id, record);
                orderedRecords.Add(record);
                loadedCount++;
            }

            // 检查跳转目标是否存在。
            foreach (DialogueRecord record in records.Values)
            {
                if (record.flag == "END")
                    continue;

                if (!records.ContainsKey(record.nextId))
                {
                    Debug.LogError(
                        $"[DialogueDataReader] ID {record.id} " +
                        $"跳转到不存在的 ID：{record.nextId}");
                    records.Clear();
                    return false;
                }
            }

            Debug.Log(
                $"[DialogueDataReader] CSV 读取成功，共 {loadedCount} 条记录。");

            return true;
        }

        private static List<List<string>> ParseCsv(string text)
        {
            var result = new List<List<string>>();
            var row = new List<string>();
            var cell = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (c == '"')
                {
                    if (inQuotes &&
                        i + 1 < text.Length &&
                        text[i + 1] == '"')
                    {
                        cell.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    row.Add(cell.ToString());
                    cell.Clear();
                }
                else if ((c == '\n' || c == '\r') && !inQuotes)
                {
                    if (c == '\r' &&
                        i + 1 < text.Length &&
                        text[i + 1] == '\n')
                        i++;

                    row.Add(cell.ToString());
                    result.Add(row);

                    row = new List<string>();
                    cell.Clear();
                }
                else
                {
                    cell.Append(c);
                }
            }

            if (inQuotes)
                throw new FormatException("存在没有闭合的双引号。");

            if (cell.Length > 0 || row.Count > 0)
            {
                row.Add(cell.ToString());
                result.Add(row);
            }

            return result;
        }
        
        private void Start()
        {
            if (Load() && TryGetRecord(0, out DialogueRecord record))
            {
                Debug.Log(
                    $"测试成功：ID={record.id}，人物={record.character}，内容={record.content}");
            }
        }
    }
}
