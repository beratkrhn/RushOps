using System;

namespace RushOps.Story
{
    /// <summary>
    /// The rules of playing a cutscene (issue #331, F-STO-02), without Unity objects: each panel fades in from black,
    /// its boxes appear one after another, and a tap either shows the next box at once or, once all boxes show,
    /// fades the panel out and moves on. Skipping ends at once. <see cref="CutscenePlayer"/> drives it every frame.
    /// No allocations after construction.
    /// </summary>
    public sealed class CutscenePlayback
    {
        private readonly int[] _boxCounts;
        private readonly CutsceneTiming _timing;
        private readonly float[] _shownAt;
        private float _phaseTime;
        private float _sinceBox;

        /// <summary>Creates the playback and starts with the first panel.</summary>
        /// <param name="boxCounts">Number of boxes of every panel in order.</param>
        /// <param name="timing">The durations.</param>
        public CutscenePlayback(int[] boxCounts, CutsceneTiming timing)
        {
            _boxCounts = boxCounts != null ? (int[])boxCounts.Clone() : Array.Empty<int>();
            _timing = timing;
            int max = 0;
            for (int i = 0; i < _boxCounts.Length; i++)
            {
                _boxCounts[i] = Math.Max(0, _boxCounts[i]);
                max = Math.Max(max, _boxCounts[i]);
            }

            _shownAt = new float[max];
            if (_boxCounts.Length == 0)
            {
                Phase = CutscenePhase.Finished;
            }
            else
            {
                EnterPanel(0);
            }
        }

        /// <summary>Raised once when the cutscene ends, by playing through or by <see cref="Skip"/>.</summary>
        public event Action Finished;

        /// <summary>Number of panels.</summary>
        public int PanelCount => _boxCounts.Length;

        /// <summary>The current panel; stays on the last one once finished.</summary>
        public int PanelIndex { get; private set; }

        /// <summary>Where the playback is.</summary>
        public CutscenePhase Phase { get; private set; }

        /// <summary>True once the cutscene is over.</summary>
        public bool IsFinished => Phase == CutscenePhase.Finished;

        /// <summary>Boxes of the current panel that have appeared, in reading order.</summary>
        public int VisibleBoxes { get; private set; }

        /// <summary>Number of boxes of the current panel.</summary>
        public int BoxCount => PanelCount > 0 ? _boxCounts[PanelIndex] : 0;

        /// <summary>True once every box of the current panel has appeared.</summary>
        public bool AllBoxesShown => VisibleBoxes >= BoxCount;

        /// <summary>True when the panel is complete and a tap moves on; the player shows its hint then.</summary>
        public bool WaitsForTap => Phase == CutscenePhase.Showing && AllBoxesShown;

        /// <summary>Opacity of the black cover over the panel: 1 hides it, 0 shows it.</summary>
        public float CoverAlpha
        {
            get
            {
                switch (Phase)
                {
                    case CutscenePhase.FadingIn:
                        return 1f - Fraction(_phaseTime, _timing.PanelFade);
                    case CutscenePhase.Showing:
                        return 0f;
                    case CutscenePhase.FadingOut:
                        return Fraction(_phaseTime, _timing.PanelFade);
                    default:
                        return 1f;
                }
            }
        }

        /// <summary>Opacity of box <paramref name="index"/> of the current panel, 0 before it appears.</summary>
        public float BoxAlpha(int index)
        {
            if (index < 0 || index >= VisibleBoxes)
            {
                return 0f;
            }

            if (Phase != CutscenePhase.Showing)
            {
                return 1f;
            }

            return Fraction(_phaseTime - _shownAt[index], _timing.BoxFade);
        }

        /// <summary>Advances the time; call once per frame with the unscaled frame time.</summary>
        public void Tick(float deltaTime)
        {
            if (IsFinished || deltaTime <= 0f)
            {
                return;
            }

            _phaseTime += deltaTime;
            switch (Phase)
            {
                case CutscenePhase.FadingIn:
                    if (_phaseTime >= _timing.PanelFade)
                    {
                        EnterShowing();
                    }

                    break;
                case CutscenePhase.Showing:
                    _sinceBox += deltaTime;
                    while (!AllBoxesShown && _sinceBox >= NextBoxDelay)
                    {
                        _sinceBox -= NextBoxDelay;
                        ShowNextBox();
                    }

                    break;
                case CutscenePhase.FadingOut:
                    if (_phaseTime >= _timing.PanelFade)
                    {
                        NextPanel();
                    }

                    break;
            }
        }

        /// <summary>
        /// A tap: ends the fade-in, shows the next box at once, or, when the panel is complete, moves on to the next
        /// panel (the last one ends the cutscene).
        /// </summary>
        public void Tap()
        {
            switch (Phase)
            {
                case CutscenePhase.FadingIn:
                    EnterShowing();
                    break;
                case CutscenePhase.Showing:
                    if (AllBoxesShown)
                    {
                        EnterFadingOut();
                    }
                    else
                    {
                        ShowNextBox();
                        _sinceBox = 0f;
                    }

                    break;
            }
        }

        /// <summary>Ends the cutscene at once („Überspringen“).</summary>
        public void Skip()
        {
            Finish();
        }

        private float NextBoxDelay => VisibleBoxes == 0 ? _timing.FirstBoxDelay : _timing.BoxDelay;

        private void EnterPanel(int index)
        {
            PanelIndex = index;
            VisibleBoxes = 0;
            _phaseTime = 0f;
            _sinceBox = 0f;
            Phase = _timing.PanelFade > 0f ? CutscenePhase.FadingIn : CutscenePhase.Showing;
            if (Phase == CutscenePhase.Showing && _timing.FirstBoxDelay <= 0f && BoxCount > 0)
            {
                ShowNextBox();
            }
        }

        private void EnterShowing()
        {
            Phase = CutscenePhase.Showing;
            _phaseTime = 0f;
            _sinceBox = 0f;
        }

        private void EnterFadingOut()
        {
            Phase = CutscenePhase.FadingOut;
            _phaseTime = 0f;
        }

        private void ShowNextBox()
        {
            _shownAt[VisibleBoxes] = _phaseTime;
            VisibleBoxes++;
        }

        private void NextPanel()
        {
            if (PanelIndex + 1 >= PanelCount)
            {
                Finish();
            }
            else
            {
                EnterPanel(PanelIndex + 1);
            }
        }

        private void Finish()
        {
            if (Phase == CutscenePhase.Finished)
            {
                return;
            }

            Phase = CutscenePhase.Finished;
            Finished?.Invoke();
        }

        private static float Fraction(float time, float duration)
        {
            if (duration <= 0f)
            {
                return 1f;
            }

            float f = time / duration;
            return f < 0f ? 0f : f > 1f ? 1f : f;
        }
    }
}
