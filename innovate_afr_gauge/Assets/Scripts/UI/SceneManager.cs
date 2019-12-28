using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public enum WindowsType{
    MainWin,
    SettingsWin,
    LoaderWin,
    InfoWindow,
}

public class SceneManager : MonoBehaviour {

    List<Window> windowsQueue = new List<Window>();
    private Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();

    public static SceneManager instance = null;

    public RectTransform winContainer;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        PushWin(WindowsType.MainWin);
    }
    public Window PushWin(WindowsType wt)
    {
        Window win = CreateWin(wt);

        PushWin(win);

        return win;
    }
    public void PushWin(Window win)
    {
        if (windowsQueue.Count > 0)
            windowsQueue[windowsQueue.Count - 1].disappear();

        win.willAppear();
        win.transform.SetParent(winContainer.transform, false);
        windowsQueue.Add(win);
    }
    public void PullWin()
    {
        if (windowsQueue.Count <= 1)
            return;

        Window oldWin = windowsQueue[windowsQueue.Count - 1];
        windowsQueue.RemoveAt(windowsQueue.Count - 1);

        oldWin.disappear();
        oldWin.hide();

        windowsQueue[windowsQueue.Count - 1].willAppear();
    }
    public void PullWin(Window win)
    {
        if(windowsQueue.Contains(win))
        {
            if (windowsQueue[windowsQueue.Count - 1] == win)
                PullWin();
            else
            {
                windowsQueue.Remove(win);
                win.hide();
            }
        }
    }
    static string GetPrefabNameForWinType(WindowsType wt)
    {
        switch (wt)
        {
            case WindowsType.MainWin:
                return "Prefabs/MainWindow";
            case WindowsType.SettingsWin:
                return "Prefabs/SettingsWindow";
            case WindowsType.LoaderWin:
                return "Prefabs/LoaderWin";
            case WindowsType.InfoWindow:
                return "Prefabs/InfoWindow";
        }
        return "";
    }
    GameObject GetPrefab(string str)
    {
        if (!prefabs.ContainsKey(str))
        {
            prefabs.Add(str, (GameObject)Resources.Load(str));
        }

        return prefabs[str];
    }

    Window CreateWin(WindowsType wt)
    {
        GameObject mapPrefab = GetPrefab(GetPrefabNameForWinType(wt));
        GameObject go = GameObject.Instantiate(mapPrefab);
        return go.GetComponent<Window>();
    }
}
