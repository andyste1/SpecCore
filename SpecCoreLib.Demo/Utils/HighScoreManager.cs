using System.Collections.Generic;

namespace SpecCoreLib.Demo.Utils
{
    using System.Linq;

    /// <summary>
    /// Used to manage a high score table.
    /// Note this implementation doesn't persist the high scores when you stop running the program!
    /// </summary>
    public class HighScoreManager
    {
        private readonly List<HighScore> _highScores;

        /// <summary>
        /// Gets the high score table.
        /// </summary>
        public IReadOnlyList<HighScore> HighScores
        {
            get
            {
                return _highScores;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HighScoreManager"/> class.
        /// </summary>
        /// <param name="numScores">The number scores.</param>
        public HighScoreManager(int numScores)
        {
            _highScores = new List<HighScore>(numScores);
            for (var i = 0; i < numScores; i++)
            {
                _highScores.Add(new HighScore { Name = "AAA", Score = 0});
            }
        }

        /// <summary>
        /// Determines whether the given score is a high score.
        /// </summary>
        /// <param name="score">The score.</param>
        /// <returns></returns>
        public bool IsHighScore(int score)
        {
            if (!_highScores.Any())
            {
                return false;
            }

            return score > _highScores.Last().Score;
        }

        /// <summary>
        /// Adds the given details to the high score table.
        /// </summary>
        /// <param name="score">The score.</param>
        /// <param name="name">The name.</param>
        public void AddScore(int score, string name)
        {
            for (var i = 0; i < _highScores.Count; i++)
            {
                if (score >= _highScores[i].Score)
                {
                    _highScores.Insert(i, new HighScore
                        {
                            Name = name,
                            Score = score
                        });
                    _highScores.RemoveAt(_highScores.Count - 1);
                    break;
                }
            }
        }
    }

    public class HighScore
    {
        public string Name { get; set; }
        public int Score { get; set; }
    }
}
