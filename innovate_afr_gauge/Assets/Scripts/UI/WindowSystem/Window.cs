using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public enum eTranslation
{
    TranslationNone,
    TranslationTop,
    TranslationBottom,
    TranslationLeft,
    TranslationRight,
    TranslationCustom,
}

public class Window : MonoBehaviour {

    public enum eWinState
    {
        WinStateAppear,
        WinStateDisappear,
        WinStateWork,
    }

    public class WindowFSM : FiniteStateMachine<eWinState> { }

    protected WindowFSM windowFSM;

    protected eTranslation apearTranslation = eTranslation.TranslationLeft;
    protected eTranslation disapearTranslation = eTranslation.TranslationRight;

    protected eTranslation translation
    {
        get {
            if (windowFSM.GetState() == eWinState.WinStateAppear)
                return apearTranslation;
            return disapearTranslation;
        }
    }
    protected float animDeltaTime = 0;
    protected float apearTime = 0.3f;

    protected Image image;
    protected bool inited = false;

    public virtual void Start()
    {
        image = GetComponent<Image>();
        windowFSM = new WindowFSM();

        windowFSM.AddTransition(eWinState.WinStateAppear, eWinState.WinStateWork, null);
        windowFSM.AddTransition(eWinState.WinStateWork, eWinState.WinStateDisappear, null);

        windowFSM.Initialise(eWinState.WinStateAppear);

        startAnimateTranslation();

        inited = true;
    }

    virtual public void Update()
    {
        if (windowFSM.GetState() == eWinState.WinStateAppear ||
            windowFSM.GetState() == eWinState.WinStateDisappear)
        {
            animateTranslation();
        }
    }

    public void hide()
    {
        windowFSM.Advance(eWinState.WinStateDisappear);
        startAnimateTranslation();
    }
    public virtual void active()
    {
    }
    public virtual void disactive()
    {
    }
    public virtual void willAppear()
    {
    }
    public virtual void appear()
    {
    }
    public virtual void disappear()
    {
    }

    public virtual void esqapeClicked()
    {
        if (windowFSM.GetState() == eWinState.WinStateWork)
        {
            backClicked();
        }
    }

    protected virtual void startAnimateTranslation()
    {
        //Debug.Log("startAnimateTranslation");
        animDeltaTime = 0;
        if (windowFSM.GetState() == eWinState.WinStateAppear)
        {
            Vector2 pos = image.rectTransform.anchoredPosition;
            switch (translation)
            {
                case eTranslation.TranslationBottom:
                    pos.y = 0;
                    break;
                case eTranslation.TranslationTop:
                    pos.y = -image.rectTransform.rect.height;
                    break;

                case eTranslation.TranslationRight:
                    pos.x = -image.rectTransform.rect.width;
                    break;
                case eTranslation.TranslationLeft:
                    pos.x = image.rectTransform.rect.width;
                    break;
            }
            image.rectTransform.anchoredPosition = pos;
        }
    }
    protected virtual void animateTranslation()
    {
        animDeltaTime += Time.deltaTime;
        if (apearTime > 0.0001f)
        {
            float t = 0;
            if (windowFSM.GetState() == eWinState.WinStateAppear)
                t = apearTime <= 0.05f ? 0 : (1 - (apearTime - animDeltaTime) / apearTime);
            else
                t = apearTime <= 0.05f ? 1 : (apearTime - animDeltaTime) / apearTime;

            t = 1 - Mathf.Clamp(t, 0, 1);

            Vector2 pos = image.rectTransform.anchoredPosition;
            switch (translation)
            {
                case eTranslation.TranslationBottom:
                    pos.y = -image.rectTransform.rect.height * t;
                    break;

                case eTranslation.TranslationTop:
                    pos.y = -image.rectTransform.rect.height * t;
                    break;

                case eTranslation.TranslationRight:
                    pos.x = -image.rectTransform.rect.width * t;
                    break;

                case eTranslation.TranslationLeft:
                    pos.x = image.rectTransform.rect.width * t;
                    break;

                case eTranslation.TranslationNone:
                    endTranslation();
                    break;
            }

            image.rectTransform.anchoredPosition = pos;

        }
        if (animDeltaTime >= apearTime)
        {
            animDeltaTime = apearTime;
            endTranslation();
        }
    }
    protected virtual void endTranslation()
    {
        if (windowFSM.GetState() == eWinState.WinStateAppear)
        {
            windowFSM.Advance(eWinState.WinStateWork);
            appear();
        }
        else
        {
            disappear();
            Destroy(gameObject);

            //SceneManager.instance.ActiveCurrent();
        }
    }

    public virtual void backClicked()
    {
        SceneManager.Instance.PullWin();
    }
}
