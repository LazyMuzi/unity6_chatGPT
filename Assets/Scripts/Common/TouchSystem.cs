using UnityEngine;
using System;

public class TouchSystem : MonoBehaviour
{
    public static TouchSystem Instance;

    [SerializeField] private LayerMask characterLayer;
    [SerializeField] private float maxRayDistance = 100f;

    private Camera mainCam;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        mainCam = Camera.main;
    }

    private void Update()
    {
        if (!TryGetInputPosition(out Vector3 screenPos)) return;

        Ray ray = mainCam.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, characterLayer))
        {
            var character = hit.collider.GetComponent<CharacterBase>();
            if (character != null)
            {
                character.OnCharacterTouched();
            }
        }
    }

    private bool TryGetInputPosition(out Vector3 position)
    {
        position = Vector3.zero;

#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButtonDown(0))
        {
            position = Input.mousePosition;
            return true;
        }
#else
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            position = Input.GetTouch(0).position;
            return true;
        }
#endif

        return false;
    }
}
