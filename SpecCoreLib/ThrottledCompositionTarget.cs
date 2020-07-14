namespace SpecCoreLib
{
    using System;
    using System.Diagnostics;
    using System.Windows.Media;

    /// <summary>
    /// Provides throttling of the CompositionTarget.Rendering event.
    /// </summary>
    internal static class ThrottledCompositionTarget
    {
        private static readonly Stopwatch sw = new Stopwatch();
        private static event EventHandler<RenderingEventArgs> FrameUpdatingInstance;

        private static long previous;
        private static long lag;

        /// <summary>
        /// Gets or sets the FPS.
        /// </summary>
        internal static int Fps { get; set; }

        /// <summary>
        /// A throttled version of the CompositionTarget.Rendering event.
        /// </summary>
        public static event EventHandler<RenderingEventArgs> FrameUpdating
        {
            add
            {
                if (FrameUpdatingInstance == null)
                {
                    CompositionTarget.Rendering += CompositionTargetRendering;
                    FrameUpdatingInstance += value;
                    sw.Start();
                }
            }
            remove
            {
                FrameUpdatingInstance -= value;
                if (FrameUpdatingInstance == null)
                {
                    CompositionTarget.Rendering -= CompositionTargetRendering;
                }
            }
        }

        /// <summary>
        /// Handles the CompositionTarget.Rendering event and throttles it based on the configured FPS.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private static void CompositionTargetRendering(object sender, EventArgs e)
        {
            var current = sw.ElapsedMilliseconds;
            var elapsed = current - previous;
            previous = current;
            lag += elapsed;
            var frameMs = 1000 / Fps;
            while (lag >= frameMs)
            {
                FrameUpdatingInstance?.Invoke(sender, null);
                lag -= frameMs;
            }
        }
    }
}
