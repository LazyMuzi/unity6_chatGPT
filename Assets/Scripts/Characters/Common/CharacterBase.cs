using UnityEngine;
using System.Collections;

/// <summary>
/// 캐릭터 공통 기반 클래스. 이동 방식은 서브클래스에서 결정합니다.
/// </summary>
public abstract class CharacterBase : MonoBehaviour
{
    protected Vector3 moveDirection;
    protected float moveSpeed = 3f;

    public bool IsInputLocked { get; set; }

    public Coroutine LookAtSmooth(Transform target, float duration = 0.4f)
    {
        return StartCoroutine(LookAtSmoothRoutine(target, duration));
    }

    private IEnumerator LookAtSmoothRoutine(Transform target, float duration)
    {
        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f) yield break;

        Quaternion startRot = transform.rotation;
        Quaternion endRot = Quaternion.LookRotation(direction);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }

        transform.rotation = endRot;
    }

    public virtual void OnCharacterTouched()
    {
        Debug.Log("OnCharacterTouched: " + name);
    }
}
