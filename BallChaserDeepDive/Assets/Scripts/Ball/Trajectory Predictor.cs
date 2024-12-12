using UnityEngine;

public class TrajectoryPredictor : MonoBehaviour
{
    /*
        This script is attached to the same place as ProjectileThrow.cs and is to make a raycast of the ball's throw prediction.
    */
    #region Members
    LineRenderer trajectoryLine;
    [SerializeField, Range(10, 100), Tooltip("The maximum number of points the LineRenderer can have")]
    int maxPoints = 50;
    [SerializeField, Range(0.01f, 0.5f), Tooltip("The time increment used to calculate the trajectory")]
    float increment = 0.025f;
    [SerializeField, Range(1.05f, 2f), Tooltip("The raycast overlap between points in the trajectory, this is a multiplier of the length between points. 2 = twice as long")]
    float rayOverlap = 1.1f;
    #endregion

    private void Start()
    {
        if (trajectoryLine == null)
            trajectoryLine = GetComponent<LineRenderer>();

        //SetTrajectoryVisible(true);
    }
    public void PredictTrajectory(ProjectileProperties projectile)
    {
        // Use the corrected initial speed
        Vector3 velocity = projectile.direction * projectile.initialSpeed;
        Vector3 position = projectile.initialPosition;
        Vector3 nextPosition;

        UpdateLineRender(maxPoints, (0, position));

        for (int i = 1; i < maxPoints; i++)
        {
            velocity = CalculateNewVelocity(velocity, projectile.drag, Time.fixedDeltaTime);
            nextPosition = position + velocity * Time.fixedDeltaTime;

            float overlap = Vector3.Distance(position, nextPosition) * rayOverlap;

            if (Physics.Raycast(position, velocity.normalized, out RaycastHit hit, overlap))
            {
                UpdateLineRender(i, (i - 1, hit.point));
                break;
            }

            position = nextPosition;
            UpdateLineRender(maxPoints, (i, position));
        }
    }

    /// <summary>
    /// Allows us to set line count and an induvidual position at the same time
    /// </summary>
    /// <param name="count">Number of points in our line</param>
    /// <param name="pointPos">The position of an induvidual point</param>
    private void UpdateLineRender(int count, (int point, Vector3 pos) pointPos)
    {
        trajectoryLine.positionCount = count;
        trajectoryLine.SetPosition(pointPos.point, pointPos.pos);
    }

    private Vector3 CalculateNewVelocity(Vector3 velocity, float drag, float increment)
    {
        velocity += Physics.gravity * increment;
        velocity *= Mathf.Clamp01(1f - drag * increment);
        return velocity;
    }

    public void SetTrajectoryVisible(bool visible)
    {
        trajectoryLine.enabled = visible;
    }
}