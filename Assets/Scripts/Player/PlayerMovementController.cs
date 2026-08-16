using UnityEngine;

/// <summary>
/// First-person movement for the Player capsule. The view pivot is deliberately
/// separate so the body only rotates around Y while the camera handles pitch.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public sealed class PlayerMovementController : MonoBehaviour
{
    [SerializeField] private Transform playerView;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float acceleration = 45f;
    [SerializeField] private float deceleration = 55f;
    [SerializeField] private float mouseSensitivity = 2.2f;
    [SerializeField] private float bobSpeed = 8f;
    [SerializeField] private float bobAmount = 0.015f;
    [SerializeField] private float crouchHeight = 1.1f;
    [SerializeField] private float crouchCameraOffset = 0.4f;
    [SerializeField] private float crouchTransitionSpeed = 12f;
    [SerializeField] private float walkStepDistance = 2.5f;
    [SerializeField] private float sprintStepDistance = 2.25f;
    [SerializeField] private float crouchStepDistance = 1.55f;

    private Rigidbody body;
    private CapsuleCollider capsule;
    private Vector3 horizontalVelocity;
    private Vector2 moveInput;
    private Vector3 viewRestPosition;
    private Vector3 bobOffset;
    private float standingHeight;
    private Vector3 standingCenter;
    private float pitch;
    private float bobTimer;
    private bool isCrouching;
    private float footstepDistance;
    private readonly RaycastHit[] groundHits = new RaycastHit[8];

    private void Awake()
    {
        mouseSensitivity = GameSettings.MouseSensitivity;
        body = GetComponent<Rigidbody>();
        body.freezeRotation = true;
        capsule = GetComponent<CapsuleCollider>();

        if (capsule != null)
        {
            standingHeight = capsule.height;
            standingCenter = capsule.center;
        }

        if (playerView == null)
            playerView = transform.Find("PlayerView");

        if (playerView != null)
            viewRestPosition = playerView.localPosition;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (GameplayUiController.IsTerminalOpen || DailyStoryController.IsPlaying || StoryIntroController.IsPlaying || GameplayTutorialController.IsBlockingGameplay)
        {
            moveInput = Vector2.zero;
            return;
        }

        // Read player input every frame. FixedUpdate can run less often and makes
        // short presses or quick WASD direction changes feel delayed.
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        isCrouching = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C);
        UpdateCrouch();
        UpdateViewPosition();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (Cursor.lockState != CursorLockMode.Locked || playerView == null)
            return;

        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
        transform.Rotate(Vector3.up * mouseX);

        pitch = Mathf.Clamp(pitch - mouseY, -85f, 85f);
        playerView.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void FixedUpdate()
    {
        // Normalize diagonal input so W+A / W+D moves at the same speed as W.
        Vector3 direction = (transform.right * moveInput.x + transform.forward * moveInput.y).normalized;
        float speed = isCrouching ? crouchSpeed : Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed;
        Vector3 targetVelocity = direction * speed;
        float rate = moveInput.sqrMagnitude > 0.01f ? acceleration : deceleration;
        horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetVelocity, rate * Time.fixedDeltaTime);

        body.linearVelocity = new Vector3(horizontalVelocity.x, body.linearVelocity.y, horizontalVelocity.z);
        UpdateHeadBob(moveInput.sqrMagnitude > 0.01f);
        UpdateFootsteps(horizontalVelocity.magnitude);
    }

    private void UpdateFootsteps(float horizontalSpeed)
    {
        if (horizontalSpeed < .2f || moveInput.sqrMagnitude < .01f || !IsGrounded())
        {
            footstepDistance = 0f;
            return;
        }

        footstepDistance += horizontalSpeed * Time.fixedDeltaTime;
        float stride = isCrouching
            ? crouchStepDistance
            : Input.GetKey(KeyCode.LeftShift) ? sprintStepDistance : walkStepDistance;
        if (footstepDistance < stride) return;

        footstepDistance -= stride;
        SoundManager.Instance?.PlayMetalFootstep();
    }

    private bool IsGrounded()
    {
        if (capsule == null) return true;

        float distance = capsule.bounds.extents.y + .22f;
        int hitCount = Physics.RaycastNonAlloc(capsule.bounds.center, Vector3.down, groundHits, distance, ~0, QueryTriggerInteraction.Ignore);
        for (int index = 0; index < hitCount; index++)
        {
            Collider hit = groundHits[index].collider;
            if (hit != null && hit != capsule && hit.attachedRigidbody != body)
                return true;
        }
        return false;
    }

    private void UpdateHeadBob(bool isMoving)
    {
        if (playerView == null)
            return;

        if (isMoving)
        {
            bobTimer += Time.fixedDeltaTime * bobSpeed;
            bobOffset = Vector3.up * Mathf.Sin(bobTimer) * bobAmount;
        }
        else
        {
            bobTimer = 0f;
            bobOffset = Vector3.Lerp(bobOffset, Vector3.zero, Time.fixedDeltaTime * 8f);
        }
    }

    private void UpdateCrouch()
    {
        if (capsule == null)
            return;

        float targetHeight = isCrouching ? crouchHeight : standingHeight;
        capsule.height = Mathf.MoveTowards(capsule.height, targetHeight, crouchTransitionSpeed * Time.deltaTime);
        float bottom = standingCenter.y - standingHeight * 0.5f;
        capsule.center = new Vector3(standingCenter.x, bottom + capsule.height * 0.5f, standingCenter.z);
    }

    private void UpdateViewPosition()
    {
        if (playerView == null)
            return;

        Vector3 target = viewRestPosition + (isCrouching ? Vector3.down * crouchCameraOffset : Vector3.zero) + bobOffset;
        playerView.localPosition = Vector3.Lerp(playerView.localPosition, target, crouchTransitionSpeed * Time.deltaTime);
    }
}
