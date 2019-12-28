using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Loader : Window {

    public Text text;
    public Loader()
    {
        apearTime = 0.0f;
        apearTranslation = eTranslation.TranslationNone;
        disapearTranslation = eTranslation.TranslationNone;
    }

    public void setText(string str)
    {
        text.text = str;
    }
}
