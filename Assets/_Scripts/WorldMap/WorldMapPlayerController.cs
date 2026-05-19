using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class WorldMapPlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Vector2 minBounds = new Vector2(-100f, -100f);
    [SerializeField] private Vector2 maxBounds = new Vector2(100f, 100f);

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 moveInput;
    private Vector3 baseScale;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody2D>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        CircleCollider2D playerCollider = GetComponent<CircleCollider2D>();
        if (playerCollider == null)
        {
            playerCollider = gameObject.AddComponent<CircleCollider2D>();
            playerCollider.radius = 0.32f;
            playerCollider.offset = new Vector2(0f, -0.08f);
        }

        baseScale = transform.localScale;
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        moveInput = ReadMoveInput();

        if (Mathf.Abs(moveInput.x) > 0.01f)
        {
            float facing = moveInput.x < 0f ? -1f : 1f;
            transform.localScale = new Vector3(Mathf.Abs(baseScale.x) * facing, baseScale.y, baseScale.z);
        }

        // Actualizar animación
        if (animator != null)
            animator.SetFloat("Speed", moveInput.sqrMagnitude);
    }

    private void FixedUpdate()
    {
        Vector2 nextPosition = rb.position + moveInput.normalized * moveSpeed * Time.fixedDeltaTime;
        nextPosition.x = Mathf.Clamp(nextPosition.x, minBounds.x, maxBounds.x);
        nextPosition.y = Mathf.Clamp(nextPosition.y, minBounds.y, maxBounds.y);
        rb.MovePosition(nextPosition);
    }

    private Vector2 ReadMoveInput()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            Vector2 input = Vector2.zero;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) input.x -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) input.x += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) input.y -= 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) input.y += 1f;
            return input;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#else
        return Vector2.zero;
#endif
    }
}
