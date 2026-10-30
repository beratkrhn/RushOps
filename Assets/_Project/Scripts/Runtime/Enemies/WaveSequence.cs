using System;

namespace RushOps.Enemies
{
    /// <summary>
    /// Runs the waves of one zone (F-GEG-02): hands out the enemies of the current wave one per spawn interval,
    /// waits until all of them died, pauses and starts the next wave. After the last wave it repeats or ends as
    /// set in the <see cref="WaveDefinition"/>. It only asks for enemies; limit and spawn points are up to the caller.
    /// </summary>
    public class WaveSequence
    {
        private readonly WaveDefinition _definition;
        private readonly int[] _handedOut;
        private int _cursor;
        private int _remaining;
        private float _timer;

        /// <summary>Creates an idle sequence over the waves of <paramref name="definition"/>.</summary>
        public WaveSequence(WaveDefinition definition)
        {
            _definition = definition;
            int maxEntries = 0;
            if (definition != null)
            {
                foreach (Wave wave in definition.Waves)
                {
                    maxEntries = Math.Max(maxEntries, wave != null ? wave.Entries.Length : 0);
                }
            }

            _handedOut = new int[maxEntries];
        }

        /// <summary>Current phase.</summary>
        public WavePhase Phase { get; private set; } = WavePhase.Idle;

        /// <summary>Index of the current wave, or of the next one during a pause.</summary>
        public int WaveIndex { get; private set; }

        /// <summary>Enemies handed out by this sequence that have not died yet.</summary>
        public int Outstanding { get; private set; }

        /// <summary>Raised for every enemy the current wave asks for.</summary>
        public event Action<EnemyDefinition> SpawnRequested;

        /// <summary>Raised when a wave starts, with its index.</summary>
        public event Action<int> WaveStarted;

        /// <summary>Starts the first wave; its first enemy comes with the next <see cref="Tick"/>. Does nothing unless idle.</summary>
        public void Start()
        {
            if (Phase != WavePhase.Idle)
            {
                return;
            }

            if (_definition == null || _definition.Waves.Length == 0)
            {
                Phase = WavePhase.Finished;
                return;
            }

            BeginWave(0);
        }

        /// <summary>Ends the sequence for good, e.g. when the zone was completed. Asks for no more enemies.</summary>
        public void Stop()
        {
            Phase = WavePhase.Finished;
        }

        /// <summary>Reports that one of the enemies handed out by this sequence died.</summary>
        public void NotifyKilled()
        {
            if (Outstanding > 0)
            {
                Outstanding--;
            }
        }

        /// <summary>Advances the sequence by <paramref name="deltaTime"/> seconds.</summary>
        public void Tick(float deltaTime)
        {
            if (Phase == WavePhase.Pause)
            {
                _timer -= deltaTime;
                if (_timer > 0f)
                {
                    return;
                }

                BeginWave(WaveIndex);
            }

            if (Phase == WavePhase.Spawning)
            {
                _timer -= deltaTime;
                while (_timer <= 0f && _remaining > 0)
                {
                    HandOutNext();
                    _timer += _definition.SpawnInterval;
                }

                if (_remaining == 0)
                {
                    Phase = WavePhase.Clearing;
                }
            }

            if (Phase == WavePhase.Clearing && Outstanding == 0)
            {
                EndWave();
            }
        }

        private void BeginWave(int index)
        {
            WaveIndex = index;
            Wave wave = _definition.Waves[index];
            Array.Clear(_handedOut, 0, _handedOut.Length);
            _cursor = 0;
            _remaining = 0;
            if (wave != null)
            {
                foreach (WaveEntry entry in wave.Entries)
                {
                    _remaining += entry.Enemy != null ? entry.Count : 0;
                }
            }

            _timer = 0f;
            Phase = WavePhase.Spawning;
            WaveStarted?.Invoke(index);
        }

        private void EndWave()
        {
            int count = _definition.Waves.Length;
            int next = WaveIndex + 1;
            if (next >= count)
            {
                switch (_definition.AfterLastWave)
                {
                    case AfterLastWave.End:
                        Phase = WavePhase.Finished;
                        return;
                    case AfterLastWave.RepeatLast:
                        next = count - 1;
                        break;
                    default:
                        next = 0;
                        break;
                }
            }

            WaveIndex = next;
            _timer = _definition.PauseBetweenWaves;
            Phase = WavePhase.Pause;
        }

        // Takes the entries in turn, so mixed waves alternate between enemy types.
        private void HandOutNext()
        {
            WaveEntry[] entries = _definition.Waves[WaveIndex].Entries;
            for (int k = 0; k < entries.Length; k++)
            {
                int e = (_cursor + k) % entries.Length;
                if (entries[e].Enemy == null || _handedOut[e] >= entries[e].Count)
                {
                    continue;
                }

                _handedOut[e]++;
                _cursor = e + 1;
                _remaining--;
                Outstanding++;
                SpawnRequested?.Invoke(entries[e].Enemy);
                return;
            }

            _remaining = 0;
        }
    }
}
