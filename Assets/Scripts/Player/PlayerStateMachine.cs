using UnityEngine;

/// <summary>
/// Tracks the player's high-level state (Idle / Running / Shooting / Hurt / Dead)
/// for the on-screen debug panel and any future animation hooks.
/// PlayerController feeds Speed and Direction in every frame; the machine then
/// picks the active state by simple priority.
/// </summary>
public class PlayerStateMachine : MonoBehaviour
{
    public enum State { Idle, Running, Shooting, Hurt, Dead }

    public State CurrentState { get; private set; } = State.Idle;

    // Filled in by PlayerController each frame.
    public float Speed;
    public Vector2 Direction = Vector2.right;
    public bool Grounded = true;     // always true for this top-down game; kept for the UI

    // Hold timers so brief states (Shooting / Hurt) stay visible long enough to read.
    float shootTimer;
    float hurtTimer;
    const float SHOOT_HOLD = 0.12f;
    const float HURT_HOLD  = 0.25f;

    void Update()
    {
        if (CurrentState == State.Dead) return;

        if (shootTimer > 0f) shootTimer -= Time.deltaTime;
        if (hurtTimer  > 0f) hurtTimer  -= Time.deltaTime;

        // Priority: Hurt > Shooting > Running > Idle.
        if (hurtTimer > 0f)        CurrentState = State.Hurt;
        else if (shootTimer > 0f)  CurrentState = State.Shooting;
        else if (Speed > 0.05f)    CurrentState = State.Running;
        else                       CurrentState = State.Idle;
    }

    public void TriggerShoot() { if (CurrentState != State.Dead) shootTimer = SHOOT_HOLD; }
    public void TriggerHurt()  { if (CurrentState != State.Dead) hurtTimer  = HURT_HOLD;  }
    public void TriggerDeath() { CurrentState = State.Dead; }
    public void Revive()       { CurrentState = State.Idle; shootTimer = hurtTimer = 0f; }

    /// <summary>Aim direction as Up / Down / Left / Right for the UI label.</summary>
    public string DirectionString
    {
        get
        {
            float angle = Mathf.Atan2(Direction.y, Direction.x) * Mathf.Rad2Deg;
            if (angle > -45f && angle <=  45f) return "Right";
            if (angle >  45f && angle <= 135f) return "Up";
            if (angle > 135f || angle <= -135f) return "Left";
            return "Down";
        }
    }

    /// <summary>Aim angle in 0..360 degrees, for the UI label.</summary>
    public float AimAngle
    {
        get
        {
            float a = Mathf.Atan2(Direction.y, Direction.x) * Mathf.Rad2Deg;
            return a < 0f ? a + 360f : a;
        }
    }
}
