namespace SpecCore.SpaceRaiders
{
    public class ProjectileHitTestResult
    {
        public bool ProjectileStopped { get; set; }
        public int Score { get; set; }
        public bool HitEnemyReactor { get; set; }
        public bool PlayerHit { get; set; }
    }
}
