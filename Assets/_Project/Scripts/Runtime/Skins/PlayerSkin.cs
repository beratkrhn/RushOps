using RushOps.Characters;
using RushOps.Save;
using UnityEngine;

namespace RushOps.Skins
{
    /// <summary>
    /// Dresses the player in the current skin when the level starts (issue #128). Thin adapter: only the animations
    /// change, never a game value (F-SKN-04).
    /// </summary>
    public class PlayerSkin : MonoBehaviour
    {
        [SerializeField] private SaveGameService _save;
        [SerializeField] private SkinCatalog _catalog;
        [SerializeField] private DirectionalSpriteAnimator _view;

        /// <summary>The skin shown; null until applied or without a catalog.</summary>
        public SkinDefinition Skin { get; private set; }

        /// <summary>Shows <paramref name="skin"/>'s animations; missing ones keep the current animation.</summary>
        public void Apply(SkinDefinition skin)
        {
            if (skin == null || _view == null)
            {
                return;
            }

            Skin = skin;
            _view.SetAnimations(skin.Idle, skin.Walk, skin.Hit, skin.Death, skin.Dash);
        }

        private void Awake()
        {
            if (_save != null && _catalog != null)
            {
                Apply(_catalog.Current(_save.Data));
            }
        }
    }
}
