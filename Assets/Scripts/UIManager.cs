using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Button submitButton;
    public Button clearLastButton;
    public Button clearAllButton;

    public GameManager gameManager;

    void Start()
    {
        submitButton.onClick.AddListener(() => gameManager.OnSubmitButtonPressed());
        clearLastButton.onClick.AddListener(() => gameManager.OnClearLastButtonPressed());
        clearAllButton.onClick.AddListener(() => gameManager.OnClearAllButtonPressed());
    }
}
