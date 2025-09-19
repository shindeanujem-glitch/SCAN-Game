using TMPro;
using UnityEngine;

public class ErrorMessage : MonoBehaviour
{
    // Assign the object to activate from the inspector
    public GameObject targetObject;
    public TextMeshProUGUI textMeshProUGUI;
    // Call this method to activate the object for x seconds
    public void ActivateObject(float seconds, string error)
    {
        StartCoroutine(ActivateForDuration(seconds));
        textMeshProUGUI.text = error;
    }

    private System.Collections.IEnumerator ActivateForDuration(float seconds)
    {
        targetObject.SetActive(true);
        yield return new WaitForSeconds(seconds);
        targetObject.SetActive(false);
    }
}
