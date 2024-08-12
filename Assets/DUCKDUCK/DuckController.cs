using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Windows.Speech;
using System.Linq;



public class DuckController : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public float moveSpeed = 5f;
    public AudioClip duckCall;
    public AudioClip quackQuack;
    public Vector2 minBoundary = new Vector2(-1600, -900);
    public Vector2 maxBoundary = new Vector2(1600, 900);
    public GameObject duckPrefab;
    public Transform parentTransform;
    [SerializeField] private float jumpForce = 60f; 
    private int maxDucks = 100; 

    private Animator animator;
    private AudioSource audioSource;
    private Rigidbody2D rb;
    public bool IsSitting = false; 
    public bool isDragging = false;
    public bool isJumping;
    public bool isManagingDucks = true;
    public List<GameObject> ducks = new List<GameObject>();

    private PhraseRecognizer phraseRecognizer;
    private Dictionary<string, Action> commands;


    private void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody2D>();

        if (audioSource == null)
        {
            Debug.LogError("AudioSource component is missing.");
        }

        // Ensure the Rigidbody2D component is configured
        rb.gravityScale = 1f; // Adjust this value to control the falling speed
        rb.mass = 1f; // Adjust the mass as needed

        StartCoroutine(RandomMovement());
        StartCoroutine(DuckCallRoutine());

        commands = new Dictionary<string, Action>
        {
            { "sit", SitDown },
            { "stand", StandUp },
            { "jump", () => StartCoroutine(Jump()) },
            { "management", ToggleDuckManagement },
            { "add", AddDuck },
            { "remove", RemoveDuck }
        };

        phraseRecognizer = new KeywordRecognizer(commands.Keys.ToArray());
        phraseRecognizer.OnPhraseRecognized += OnPhraseRecognized;
        phraseRecognizer.Start();
    }

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        // Check for sit input
        if (Input.GetKeyDown(KeyCode.S) && !IsSitting)
        {
            SitDown();
        }

        // Check for stand up input
        if (Input.GetKeyDown(KeyCode.W) && IsSitting)
        {
            StandUp();
        }

        // Check for jump input
        if (Input.GetKeyDown(KeyCode.Space) && !isJumping && !IsSitting)
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

    private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
{
    Debug.Log($"Recognized phrase: {args.text}");
    
    if (commands.ContainsKey(args.text.ToLower()))
    {
        commands[args.text.ToLower()].Invoke();
    }
}
private void OnDestroy()
{
    if (phraseRecognizer != null && phraseRecognizer.IsRunning)
    {
        phraseRecognizer.OnPhraseRecognized -= OnPhraseRecognized;
        phraseRecognizer.Stop();
        phraseRecognizer.Dispose();
    }
}



private void SitDown()
{
    if (!IsSitting) // Ensure this action only happens once
    {
        IsSitting = true;
        animator.SetBool("IsSitting", IsSitting); // Set the sitting animation
        rb.velocity = Vector2.zero; // Stop all movement
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation; // Freeze x-axis movement
    }
}


    private void StandUp()
    {
        IsSitting = false;
        animator.SetBool("IsSitting", IsSitting);
        animator.SetTrigger("StandUp"); // Optionally trigger a stand-up animation
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Unfreeze x-axis movement
    }


    private void ToggleDuckManagement()
    {
        isManagingDucks = !isManagingDucks;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!IsSitting)
        {
            isDragging = true;
            if (quackQuack != null && audioSource != null)
            {
                audioSource.PlayOneShot(quackQuack);
            }

            // Disable gravity while dragging
            rb.gravityScale = 0;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging && !IsSitting)
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

        // Re-enable gravity when dragging stops
        rb.gravityScale = 1f;
    }

    private IEnumerator Jump()
    {
        animator.SetTrigger("Jump");
        isJumping = true;

        // Adding upward force for the jump
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        // Assuming the jump animation takes 1 second
        yield return new WaitForSeconds(1f);

        isJumping = false;
    }

    private void AddDuck()
    {
        if (ducks.Count >= maxDucks)
        {
            // Log a message to indicate that no more ducks can be added
            Debug.Log("Maximum number of ducks reached. Cannot add more.");
            return;
        }

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
        // Adjust this to your scene setup
        return new Vector2(UnityEngine.Random.Range(minBoundary.x, maxBoundary.x), UnityEngine.Random.Range(minBoundary.y, maxBoundary.y));
    }

    private IEnumerator RandomMovement()
    {
        while (true)
        {
            if (!IsSitting && !isDragging)
            {
                float horizontal = UnityEngine.Random.Range(-1f, 1f);

                Vector2 direction = new Vector2(horizontal, 0).normalized;

                float moveDuration = UnityEngine.Random.Range(1f, 3f);
                float moveTime = 0f;

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

                float stopDuration = UnityEngine.Random.Range(1f, 3f);
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
            if (!IsSitting && !isDragging && duckCall != null && audioSource != null)
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
