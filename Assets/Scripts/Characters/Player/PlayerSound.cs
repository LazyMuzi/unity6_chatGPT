using UnityEngine;

/// <summary>
/// 플레이어 캐릭터 오디오 재생을 전담합니다.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class PlayerSound : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip touchClip;
    [SerializeField] private AudioClip footstepClip;
    [SerializeField] private float footstepInterval = 0.45f;

    private float footstepTimer;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    public void Tick(Vector3 moveDirection, bool isInputLocked)
    {
        if (isInputLocked || moveDirection.sqrMagnitude < 0.01f)
        {
            footstepTimer = 0f;
            return;
        }

        if (audioSource == null || footstepClip == null)
        {
            return;
        }

        footstepTimer += Time.deltaTime;
        if (footstepTimer >= footstepInterval)
        {
            footstepTimer = 0f;
            audioSource.PlayOneShot(footstepClip);
        }
    }

    public void PlayTouchedSound()
    {
        if (audioSource == null || touchClip == null)
        {
            return;
        }

        audioSource.PlayOneShot(touchClip);
    }
}
