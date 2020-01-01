using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;


[WinPathAttribute("Prefabs/Windows/WarningPopup")]
public class WarningPopup : Window {

    public Text title;
    public Text info;
    public Text btnText;

    Action action;

    public static WarningPopup push(string titleStr, string infoStr, string btnStr, Action action )
    {
        WarningPopup wp = SceneManager.Instance.PushWin<WarningPopup>().GetComponent<WarningPopup>();

        wp.title.text = titleStr;
        wp.info.text = infoStr;
        wp.btnText.text = btnStr;
        wp.action = action;
        
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
