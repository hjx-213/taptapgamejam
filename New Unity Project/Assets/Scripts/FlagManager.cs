using UnityEngine;
using System.Collections.Generic;

public class FlagManager : MonoBehaviour
{
    private static FlagManager instance;

    public static FlagManager Instance => instance;

    private Dictionary<string, bool> flags = new Dictionary<string, bool>();

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetFlag(string flagName, bool value)
    {
        flags[flagName] = value;
    }

    public bool GetFlag(string flagName)
    {
        if (flags.ContainsKey(flagName))
        {
            return flags[flagName];
        }

        return false;
    }
}