public static class GameLayers
{
    public const int Player = 3;
    public const int Enemy = 6;
    public const int NeutralActor = 7;
    public const int Terrain = 8;
    public const int Obstacle = 9;
    public const int Interactable = 10;
    public const int PlayerProjectile = 11;
    public const int EnemyProjectile = 12;
    public const int Sensor = 13;

    public const int PlayerMask = 1 << Player;
    public const int EnemyMask = 1 << Enemy;
    public const int NeutralActorMask = 1 << NeutralActor;
    public const int TerrainMask = 1 << Terrain;
    public const int ObstacleMask = 1 << Obstacle;
    public const int InteractableMask = 1 << Interactable;
    public const int PlayerProjectileMask = 1 << PlayerProjectile;
    public const int EnemyProjectileMask = 1 << EnemyProjectile;
    public const int SensorMask = 1 << Sensor;
}
