using RushOps.Audio;
using RushOps.Levels;
using UnityEngine;
using UnityEngine.UI;

namespace RushOps.UI
{
    /// <summary>
    /// Mission tracker top right (F-HUD-03): follows the exit gate of the zone the player is in. For a kill gate it
    /// shows „Tor gesperrt: 4/10 Kills“; when the gate opens it switches to „Tor offen!“ with a chime and hides
    /// after a moment. Gates with other conditions are not shown yet.
    /// </summary>
    public class MissionTracker : MonoBehaviour
    {
        [SerializeField] private HudConfig _config;
        [SerializeField] private Text _label;
        [Tooltip("Zones of the level; the tracker follows the exit gate of the zone activated last.")]
        [SerializeField] private Zone[] _zones = System.Array.Empty<Zone>();
        [SerializeField] private SfxPlayer _sfx;
        [Tooltip("Played when the tracked gate opens.")]
        [SerializeField] private AudioClip _doneClip;

        private Gate _gate;
        private KillGateCondition _kills;
        private int _shownKills = -1;
        private float _doneTime = -1f;

        /// <summary>Text currently shown; empty while hidden.</summary>
        public string Text => _label != null && _label.enabled ? _label.text : string.Empty;

        private void OnEnable()
        {
            foreach (Zone zone in _zones)
            {
                if (zone != null)
                {
                    zone.Activated += Track;
                    if (zone.State == ZoneState.Active)
                    {
                        Track(zone);
                    }
                }
            }

            if (_gate == null)
            {
                Hide();
            }
        }

        private void OnDisable()
        {
            foreach (Zone zone in _zones)
            {
                if (zone != null)
                {
                    zone.Activated -= Track;
                }
            }

            SetGate(null);
        }

        private void Update()
        {
            if (_doneTime >= 0f)
            {
                _doneTime += Time.deltaTime;
                if (_doneTime >= (_config != null ? _config.MissionDoneDuration : 0f))
                {
                    _doneTime = -1f;
                    Hide();
                }

                return;
            }

            if (_kills != null && _gate.State == GateState.Closed && _kills.CurrentKills != _shownKills)
            {
                _shownKills = _kills.CurrentKills;
                Show(MissionText.KillGate(_shownKills, _kills.RequiredKills));
            }
        }

        private void Track(Zone zone)
        {
            Gate gate = zone.ExitGate;
            var kills = gate != null ? gate.Condition as KillGateCondition : null;
            if (kills == null || gate.State != GateState.Closed)
            {
                // Keep a running „Tor offen!“ until it times out; otherwise nothing to show in this zone.
                SetGate(null);
                if (_doneTime < 0f)
                {
                    Hide();
                }

                return;
            }

            SetGate(gate);
            _kills = kills;
            _shownKills = -1;
            _doneTime = -1f;
        }

        private void SetGate(Gate gate)
        {
            if (_gate != null)
            {
                _gate.Opening -= OnGateOpening;
            }

            _gate = gate;
            _kills = null;
            if (_gate != null)
            {
                _gate.Opening += OnGateOpening;
            }
        }

        private void OnGateOpening(Gate gate)
        {
            SetGate(null);
            Show(MissionText.GateOpen);
            _doneTime = 0f;
            if (_sfx != null && _config != null)
            {
                _sfx.Play(_doneClip, _config.MissionDoneVolume, 0f);
            }
        }

        private void Show(string text)
        {
            if (_label != null)
            {
                _label.text = text;
                _label.enabled = true;
            }
        }

        private void Hide()
        {
            if (_label != null)
            {
                _label.enabled = false;
            }
        }
    }
}
