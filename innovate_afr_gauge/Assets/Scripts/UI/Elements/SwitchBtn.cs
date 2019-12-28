using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Events;


[System.Serializable]
public class BoolEvent : UnityEvent<bool>
{
}

public class SwitchBtn : MonoBehaviour {

	Image img = null;

	public Sprite onSprite;
	public Sprite offSprite;

	public BoolEvent onClick;

	bool _isActive;
	public bool isActive {
		get{ return _isActive;}
		set{ 
			_isActive = value;
            if (img == null)
                img = GetComponent<Image>();
            img.sprite = _isActive ? onSprite : offSprite;
		}
	}

	void Start()
	{
        if(img == null)
		    img = GetComponent<Image> ();

		Button btn = GetComponent<Button> ();

        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(delegate { click(); });
        }
	}

	void click()
	{
		isActive = !isActive;

		if(onClick != null)
			onClick.Invoke(isActive);
	}
}
