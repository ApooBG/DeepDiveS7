using UnityEngine;

public class ThrowBallManager : MonoBehaviour
{
    [SerializeField, Range(0.0f, 100.0f)]
    float force = 10f;

    [SerializeField, Range(0.0f, 100.0f)]
    float forceIncrementSpeed = 70f; // Speed at which force increases


    [SerializeField]
    public bool isAiming = false;

    [SerializeField]
    public ProjectileThrow projectileThrow;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    private void OnEnable()
    {
        projectileThrow = GetComponent<ProjectileThrow>();
    }

    // Update is called once per frame
    void Update()
    {
        HandleForceChange();
        UpdateValues();
    }

    void HandleForceChange()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel"); // Get the scroll wheel input

        if (scrollInput != 0) // Check if there is any scroll input
        {
            // Adjust the force value based on scroll direction
            force += scrollInput * forceIncrementSpeed;

            // Clamp the force value between 0 and 100
            force = Mathf.Clamp(force, 0.0f, 100.0f);

            Debug.Log($"Scroll Input Detected. New Force: {force}");
        }
        else
        {
            Debug.Log("No Scroll Input Detected");
        }
    }

    void UpdateValues()
    {
        if (projectileThrow != null)
        {
            projectileThrow.force = force;
        }
    }

    public void ChangeIsThrowingState(bool state)
    {
        isAiming = state;
    }

    public void HandleThrow()
    {
        ChangeIsThrowingState(true);
    }

    public void ThrowObject()
    {
        if (isAiming)
        {
            //ChangeIsThrowingState(false);
            projectileThrow.ThrowObject();
        }
    }
}
