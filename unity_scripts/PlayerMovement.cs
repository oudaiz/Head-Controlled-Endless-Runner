using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public AudioSource audioSource;

    public AudioClip jumpSound;
    public AudioClip coinSound;
    public AudioClip hitSound;

    public float laneDistance = 3f;
    public float laneMoveDuration = 0.2f;

    public float jumpHeight = 2f;
    public float jumpDuration = 0.8f;

    public float slideHoldDuration = 0.7f;
    public float slideY = 0.25f;
    public float slideTransitionDuration = 0.12f;

    public float score = 0f;
    public int coinCount = 0;

    public float gameSpeed = 1f;
    public float speedIncreaseRate = 0.3f;
    public float maxSpeed = 3f;

    private bool allowKeyboardInput = true;
    private bool isGameOver = false;

    private bool isJumping = false;
    private bool isSliding = false;
    private bool canStartRunning = false;

    public bool IsJumping => isJumping;
    public bool IsSliding => isSliding;
    public bool CanStartRunning => canStartRunning;

    private int currentLane = 1;

    private float[] laneXPositions;
    private float targetX;

    private float standingY;
    private float baseY;
    private float currentJumpOffset = 0f;

    private int lastPrintedScore = -1;

    private Coroutine jumpCoroutine;
    private Coroutine slideCoroutine;

    private GameUIManager uiManager;
    private Animator animator;

    void Start()
    {
        allowKeyboardInput = GameLaunchSettings.SelectedInputMode == InputMode.Keyboard;

        laneXPositions = new float[]
        {
            -laneDistance,
            0f,
            laneDistance
        };

        currentLane = 1;
        targetX = laneXPositions[currentLane];

        standingY = transform.position.y;
        baseY = standingY;

        uiManager = FindAnyObjectByType<GameUIManager>();
        animator = GetComponentInChildren<Animator>();

        if (animator != null)
            animator.SetBool("isRunning", false);

        if (uiManager != null)
            StartCoroutine(uiManager.ShowCountdown(StartRunning));
    }

    void StartRunning()
    {
        canStartRunning = true;

        if (animator != null)
            animator.SetBool("isRunning", true);
    }

    void Update()
    {
        if (isGameOver) return;

        if (!canStartRunning)
        {
            UpdateLaneMovement();
            UpdateVerticalPosition();
            return;
        }

        if (allowKeyboardInput)
            HandleKeyboardInput();

        UpdateLaneMovement();
        UpdateVerticalPosition();

        gameSpeed += Time.deltaTime * speedIncreaseRate;
        gameSpeed = Mathf.Clamp(gameSpeed, 1f, maxSpeed);

        score += Time.deltaTime * gameSpeed;
        int currentScore = Mathf.FloorToInt(score);

        if (currentScore != lastPrintedScore)
            lastPrintedScore = currentScore;

        if (uiManager != null)
            uiManager.UpdateHUD(currentScore, coinCount);
    }

    void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            CommandMoveRight();

        if (Input.GetKeyDown(KeyCode.LeftArrow))
            CommandMoveLeft();

        if (Input.GetKeyDown(KeyCode.Space))
            CommandJump();

        if (Input.GetKeyDown(KeyCode.DownArrow))
            CommandSlide();
    }

    public void CommandMoveRight()
    {
        if (!canStartRunning || isGameOver) return;

        if (currentLane < 2)
        {
            currentLane++;
            targetX = laneXPositions[currentLane];
        }
    }

    public void CommandMoveLeft()
    {
        if (!canStartRunning || isGameOver) return;

        if (currentLane > 0)
        {
            currentLane--;
            targetX = laneXPositions[currentLane];
        }
    }

    public void CommandJump()
    {
        if (!canStartRunning || isGameOver) return;

        if (isSliding)
        {
            CancelSlide();
            StartJump();
        }
        else
        {
            StartJump();
        }
    }

    public void CommandSlide()
    {
        if (!canStartRunning || isGameOver) return;

        if (isJumping)
        {
            CancelJump();
            StartSlide();
        }
        else if (!isSliding)
        {
            StartSlide();
        }
    }

    void UpdateLaneMovement()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Lerp(pos.x, targetX, Time.deltaTime * (1f / laneMoveDuration));
        transform.position = pos;
    }

    void UpdateVerticalPosition()
    {
        Vector3 pos = transform.position;
        pos.y = baseY + currentJumpOffset;
        transform.position = pos;
    }

    void StartJump()
    {
        if (isJumping) return;

        isJumping = true;

        if (audioSource && jumpSound)
            audioSource.PlayOneShot(jumpSound);

        if (animator)
        {
            animator.ResetTrigger("Slide");
            animator.SetTrigger("Jump");
        }

        jumpCoroutine = StartCoroutine(JumpRoutine());
    }

    void StartSlide()
    {
        if (isSliding) return;

        isSliding = true;

        if (animator)
        {
            animator.ResetTrigger("Jump");
            animator.SetTrigger("Slide");
        }

        slideCoroutine = StartCoroutine(SlideRoutine());
    }

    void CancelJump()
    {
        if (jumpCoroutine != null)
            StopCoroutine(jumpCoroutine);

        isJumping = false;
        currentJumpOffset = 0f;
    }

    void CancelSlide()
    {
        if (slideCoroutine != null)
            StopCoroutine(slideCoroutine);

        isSliding = false;
        baseY = standingY;
    }

    IEnumerator JumpRoutine()
    {
        float elapsed = 0f;

        while (elapsed < jumpDuration)
        {
            float t = elapsed / jumpDuration;
            currentJumpOffset = 4f * jumpHeight * t * (1f - t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        currentJumpOffset = 0f;
        isJumping = false;
    }

    IEnumerator SlideRoutine()
    {
        float elapsed = 0f;

        while (elapsed < slideTransitionDuration)
        {
            float t = elapsed / slideTransitionDuration;
            baseY = Mathf.Lerp(standingY, slideY, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        baseY = slideY;

        yield return new WaitForSeconds(slideHoldDuration);

        elapsed = 0f;

        while (elapsed < slideTransitionDuration)
        {
            float t = elapsed / slideTransitionDuration;
            baseY = Mathf.Lerp(slideY, standingY, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        baseY = standingY;
        isSliding = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Coin"))
        {
            if (audioSource && coinSound)
                audioSource.PlayOneShot(coinSound);

            coinCount++;
            Destroy(other.gameObject);
            return;
        }

        ObstacleType obstacle = other.GetComponent<ObstacleType>();

        if (obstacle == null) return;

        bool survived =
            (obstacle.obstacleKind == ObstacleType.ObstacleKind.JumpOver && isJumping) ||
            (obstacle.obstacleKind == ObstacleType.ObstacleKind.SlideUnder && isSliding);

        if (survived) return;

        isGameOver = true;

        if (audioSource && hitSound)
            audioSource.PlayOneShot(hitSound);

        if (uiManager != null)
            uiManager.ShowGameOver(Mathf.FloorToInt(score), coinCount);
    }
}