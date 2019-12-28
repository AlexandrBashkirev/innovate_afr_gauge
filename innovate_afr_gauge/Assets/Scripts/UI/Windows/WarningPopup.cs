using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;

public class WarningPopup : Window {

    public Text title;
    public Text info;
    public Text btnText;

    Action action;

    public static WarningPopup push(string titleStr, string infoStr, string btnStr, Action action )
    {
        WarningPopup wp = Instantiate((GameObject)Resources.Load("Prefabs/WarningPopup")).GetComponent<WarningPopup>();

        wp.title.text = titleStr;
        wp.info.text = infoStr;
        wp.btnText.text = btnStr;
        wp.action = action;

        SceneManager.instance.PushWin(wp);
        return wp;
    }

    public WarningPopup()
    {
        apearTranslation = eTranslation.TranslationTop;
        disapearTranslation = eTranslation.TranslationBottom;
    }

    public void btnClick()
    {
        backClicked();
        action();
    }
}
