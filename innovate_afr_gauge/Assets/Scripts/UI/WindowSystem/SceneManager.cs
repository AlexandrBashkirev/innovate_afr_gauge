using System;
using UnityEngine;
using System.Collections.Generic;


public class SceneManager : MonoBehaviour {

    #region Fields

    private List<Window> windowsQueue = new List<Window>();
    private Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();

    private static SceneManager instance = null;

    [SerializeField] private RectTransform winContainer;

    #endregion



    #region Properties

    public static SceneManager Instance => instance;

    #endregion
    
    
    
    #region Unity lifecicle

    void Awake()
    {
        instance = this;
    }

    #endregion

    
    
    #region Methods
    
    public T PushWin<T>() where T : Window
    {
        WinPathAttribute pathAttr = (WinPathAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(WinPathAttribute));

        if (pathAttr == null)
            return null;

        Window win = CreateWin(pathAttr.path);

        PushWin(win);

        return win.GetComponent<T>();
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

    
    void PushWin(Window win)
    {
        if (windowsQueue.Count > 0)
            windowsQueue[windowsQueue.Count - 1].disappear();

        win.willAppear();
        win.transform.SetParent(winContainer.transform, false);
        windowsQueue.Add(win);
    }
    
    
    GameObject GetPrefab(string str)
    {
        if (!prefabs.ContainsKey(str))
        {
            prefabs.Add(str, (GameObject)Resources.Load(str));
        }

        return prefabs[str];
    }

    
    Window CreateWin(string path)
    {
        GameObject mapPrefab = GetPrefab(path);
        GameObject go = GameObject.Instantiate(mapPrefab);
        return go.GetComponent<Window>();
    }

    #endregion
}
