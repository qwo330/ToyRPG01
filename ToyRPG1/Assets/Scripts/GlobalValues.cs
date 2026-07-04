public static class GlobalValues
{
    public const float Gravity = -9.8f;
    public const float GroundedForce = -2f;
    public const float IgnoreDistance = 0.001f;
    
    public const float RelocationThresholdSqr = 25f; // 5f * 5f
    public const float HMoveThresholdSqr = 0.0025f; // 0.05f * 0.05f
    public const float VMoveThreshold = 5f;
    public const float RotationDotThreshold = 0.999f; // 0.998f ~ 0.9995f
    public const float RotationAngleThreshold = 7f; // 5f ~ 10f
}
