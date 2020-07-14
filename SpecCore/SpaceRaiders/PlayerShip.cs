namespace SpecCore.SpaceRaiders
{
    using System.Windows.Input;
    using System.Windows.Media;
    using SpecCoreLib;

    /// <summary>
    /// Responsible for drawing and controlling the friendly ship.
    /// </summary>
    public class PlayerShip
    {
        private readonly SpeccyEngine _speccy;
        private readonly int _row;

        public int Col { get; private set; }

        public PlayerMissile Missile { get; private set; }

        public PlayerShip(SpeccyEngine speccy, int row)
        {
            _speccy = speccy;
            _row = row;
            Col = 30;
        }

        public void Draw()
        {
            _speccy.Print(_row, Col - 1, "   ");

            // Handle keypress (remember the "ship" is three characters wide).
            if (_speccy.LastKeyPress == Key.Left && Col > 1)
            {
                Col--;
            }
            else if (_speccy.LastKeyPress == Key.Right && Col < 58)
            {
                Col++;
            }
            else if (_speccy.LastKeyPress == Key.Space && Missile == null)
            {
                // Launch a new missile if there isn't already one in flight.
                Missile = new PlayerMissile(_speccy, _row, Col);
            }

            _speccy.Pen = Colors.Cyan;
            _speccy.Print(_row, Col - 1, "¬J¬K¬I");

            Missile?.Draw();
            if (Missile?.Row == -1)
            {
                Missile = null;
            }
        }

        public void StopMissile()
        {
            Missile = null;
        }

        public ProjectileHitTestResult HitTest(EnemyBomb bomb)
        {
            if (bomb == null || bomb.Row != _row)
            {
                return new ProjectileHitTestResult();
            }

            if (bomb.Col >= Col - 1 && bomb.Col <= Col + 1)
            {
                return new ProjectileHitTestResult { PlayerHit = true };
            }

            return new ProjectileHitTestResult();
        }
    }
}
