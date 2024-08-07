using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DuckController : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public float moveSpeed = 5f;
    public AudioClip duckCall;
    public AudioClip quackQuack;
    public Vector2 minBoundary = new Vector2(-20, -10);
    public Vector2 maxBoundary = new Vector2(20, 10);
    public GameObject duckPrefab;
    public Transform parentTransform;
    [SerializeField] private float jumpForce = 6f;
    private int maxDucks = 4; 

    private Animator animator;
    private AudioSource audioSource;
    private Rigidbody2D rb;
    private bool isSleeping = false;
    public bool isDragging = false;
    public bool isJumping;
    public bool isManagingDucks;
    private List<GameObject> ducks = new List<GameObject>();

    private void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody2D>();

        if (audioSource == null)
        {
            Debug.LogError("AudioSource component is missing.");
        }


        rb.gravityScale = 1f;
        rb.mass = 1f; 

        StartCoroutine(RandomMovement());
        StartCoroutine(DuckCallRoutine());
    }

    private void Update()
    {
        HandleInput();
    }

private void HandleInput()
{
    // Check for sleep input
    if (Input.GetKeyDown(KeyCode.S))
    {
        ToggleSleep();
    }

    // Check for jump input
    if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
    {
        isJumping = true;
        StartCoroutine(Jump());
    }

    // Check for duck management mode toggle
    if (Input.GetKeyDown(KeyCode.Equals))
    {
        ToggleDuckManagement();
    }

    // Check for adding/removing ducks
    if (isManagingDucks)
    {
        if ((Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus)) && ducks.Count < maxDucks)
        {
            AddDuck();
        }
        else if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            RemoveDuck();
        }
    }
}

    private void ToggleSleep()
    {
        isSleeping = !isSleeping;
        animator.SetBool("IsSleeping", isSleeping);
    }

    private void ToggleDuckManagement()
    {
        isManagingDucks = !isManagingDucks;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        if (quackQuack != null && audioSource != null)
        {
            audioSource.PlayOneShot(quackQuack);
        }

        // Disable gravity while dragging
        rb.gravityScale = 0;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging)
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(eventData.position);
            Vector2 clampedPosition = ClampPosition(mousePosition);
            transform.position = clampedPosition;

            animator.SetFloat("MoveX", Mathf.Abs(mousePosition.x - transform.position.x));
            animator.SetBool("IsWalking", true);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
        animator.SetBool("IsWalking", false);

    }

    private IEnumerator Jump()
    {
        animator.SetTrigger("Jump");
        isJumping = true;

        // Adding upward force for the jump
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(1f);

        isJumping = false;
    }


    private void AddDuck()
    {
        // Ensure the number of ducks does not exceed the maximum limit
        if (ducks.Count >= maxDucks)
        {
        Debug.Log("Cannot add more ducks. Maximum limit reached.");
        return;
    }

    // Instantiate and add the new duck
    GameObject newDuck = Instantiate(duckPrefab, GetRandomPosition(), Quaternion.identity, parentTransform);
    ducks.Add(newDuck);
}


    private void RemoveDuck()
    {
        if (ducks.Count > 0)
        {
            GameObject duckToRemove = ducks[ducks.Count - 1];
            ducks.RemoveAt(ducks.Count - 1);
            Destroy(duckToRemove);
        }
    }

    private Vector2 GetRandomPosition()
    {
        return new Vector2(Random.Range(minBoundary.x, maxBoundary.x), Random.Range(minBoundary.y, maxBoundary.y));
    }

    private IEnumerator RandomMovement()
    {
        while (true)
        {
            if (!isSleeping && !isDragging)
            {
                float horizontal = Random.Range(-1f, 1f);

                Vector2 direction = new Vector2(horizontal, 0).normalized;

                float moveDuration = Random.Range(1f, 3f);
                float moveTime = 1f;

                while (moveTime < moveDuration)
                {
                    Vector2 newPosition = (Vector2)transform.position + direction * moveSpeed * Time.deltaTime;
                    transform.position = ClampPosition(newPosition);

                    animator.SetFloat("MoveX", Mathf.Abs(direction.x));
                    animator.SetBool("IsWalking", true);

                    moveTime += Time.deltaTime;
                    yield return null;
                }

                animator.SetFloat("MoveX", 0);
                animator.SetBool("IsWalking", false);

                float stopDuration = Random.Range(1f, 3f);
                yield return new WaitForSeconds(stopDuration);
            }
            else
            {
                yield return new WaitForSeconds(1f);
            }
        }
    }

    private IEnumerator DuckCallRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(30f);
            if (!isSleeping && !isDragging && duckCall != null && audioSource != null)
            {
                audioSource.PlayOneShot(duckCall);
            }
        }
    }

    private Vector2 ClampPosition(Vector2 position)
    {
        float clampedX = Mathf.Clamp(position.x, minBoundary.x, maxBoundary.x);
        float clampedY = Mathf.Clamp(position.y, minBoundary.y, maxBoundary.y);
        return new Vector2(clampedX, clampedY);
    }

}
