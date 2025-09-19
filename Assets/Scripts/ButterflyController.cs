using UnityEngine;
using System.Collections;

public class ButterflyController : MonoBehaviour
{
    // public Animator animator;
    public float flightTime = 1.2f;
    public float arcHeight = 1.5f;
    private Vector3 idlePos;
    private bool isMoving = false;

    void Start()
    {
        idlePos = transform.position;
        // if (animator == null) animator = GetComponent<Animator>();
        // animator.Play("WingFlap"); // Ensure this is looped in the animator
        StartCoroutine(IdleHover());
    }

    public void FlyTo(Vector3 target)
    {
        if (isMoving) StopAllCoroutines();
        StartCoroutine(FlyToPosition(target));
    }

    private IEnumerator FlyToPosition(Vector3 target)
    {
        isMoving = true;
        // animator.Play("WingFlap");
        Vector3 start = transform.position;
        Vector3 mid = (start + target) * 0.5f + Vector3.up * arcHeight;

        float t = 0f;
        while (t < flightTime)
        {
            t += Time.deltaTime;
            float perc = t / flightTime;
            Vector3 pos = Mathf.Pow(1 - perc, 2) * start +
                          2 * (1 - perc) * perc * mid +
                          Mathf.Pow(perc, 2) * target;
            transform.position = pos;
            yield return null;
        }
        transform.position = target;
        // animator.Play("WingFlap");
        idlePos = target;
        isMoving = false;
        StartCoroutine(IdleHover());
    }

    private IEnumerator IdleHover()
    {
        float hoverRange = 0.1f;
        float hoverSpeed = 2.0f;
        while (true)
        {
            if (!isMoving)
            {
                float offset = Mathf.Sin(Time.time * hoverSpeed) * hoverRange;
                transform.position = idlePos + Vector3.up * offset;
            }
            yield return null;
        }
    }
}
