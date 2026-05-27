using UnityEngine;

public class FollowCar : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform playerCar;

    [Header("Follow Settings")]
    [SerializeField] private float initialDistance = 40f;
    [SerializeField] private float finalDistance = 8f;
    [SerializeField] private float approachDuration = 25f;
    [SerializeField] private float followHeight = 0f;
    [SerializeField] private float followSmoothness = 3f;

    private float elapsedTime = 0f;
    private bool isActive = false;

    public void StartFollowing()
    {
        isActive = true;
        elapsedTime = 0f;
    }

    void Update()
    {
        if (!isActive || playerCar == null) return;

        elapsedTime += Time.deltaTime;

        float t = Mathf.Clamp01(elapsedTime / approachDuration);
        float targetDistance = Mathf.Lerp(initialDistance, finalDistance, t);

        Vector3 targetPos = playerCar.position
            - playerCar.forward * targetDistance
            + Vector3.up * followHeight;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPos,
            Time.deltaTime * followSmoothness
        );

        transform.LookAt(playerCar);
    }
    public void StopFollowing()
{
    isActive = false;
}
}