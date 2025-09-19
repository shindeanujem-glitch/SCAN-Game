using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

public class FlowerNode : MonoBehaviour
{
    public string label;
    public TextMeshProUGUI labelTMP;
    public event Action<FlowerNode> OnSelected;
    public Vector3 targetPosition;
    public Button button;
    public Transform butterfly;
    public List<Sprite> sprites = new List<Sprite>();
    public Image image;
    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        button.onClick.AddListener(()=>{
            OnMouseDown();
        });

        int randomIndex = UnityEngine.Random.Range(0, sprites.Count-1);
        image.sprite = sprites[randomIndex];
    }
    public void SetLabel(string text)
    {
        if (labelTMP != null)
            labelTMP.text = text;
    }

    public void OnMouseDown()
    {
        OnSelected?.Invoke(this);

        RectTransform rt = button.GetComponent<RectTransform>();
        Vector3 screenPoint = RectTransformUtility.WorldToScreenPoint(null, rt.position);
        screenPoint.z = Mathf.Abs(Camera.main.transform.position.z - butterfly.transform.position.z);
        targetPosition = Camera.main.ScreenToWorldPoint(screenPoint);
        targetPosition.z = butterfly.transform.position.z;
    }
}
