using NUnit.Framework;
using RushOps.Combat;
using UnityEngine;

namespace RushOps.Tests.EditMode
{
    public class WeaponSlotsTests
    {
        private WeaponDefinition _pistol;
        private WeaponDefinition _shotgun;
        private WeaponDefinition _rifle;

        [SetUp]
        public void CreateWeapons()
        {
            _pistol = ScriptableObject.CreateInstance<WeaponDefinition>();
            _shotgun = ScriptableObject.CreateInstance<WeaponDefinition>();
            _rifle = ScriptableObject.CreateInstance<WeaponDefinition>();
        }

        [TearDown]
        public void DestroyWeapons()
        {
            Object.DestroyImmediate(_pistol);
            Object.DestroyImmediate(_shotgun);
            Object.DestroyImmediate(_rifle);
        }

        [Test]
        public void New_HasOnlySelectedWeapon_Active()
        {
            var slots = new WeaponSlots(_pistol);

            Assert.AreSame(_pistol, slots.Active);
            Assert.IsNull(slots.Picked);
            Assert.IsNull(slots.Inactive);
            Assert.IsFalse(slots.HasTwo);
        }

        [Test]
        public void Swap_WithOneWeapon_DoesNothing()
        {
            var slots = new WeaponSlots(_pistol);
            int changes = 0;
            slots.Changed += () => changes++;

            Assert.IsFalse(slots.Swap());
            Assert.AreSame(_pistol, slots.Active);
            Assert.AreEqual(0, changes);
        }

        [Test]
        public void PickUp_FillsSecondSlot_AndMakesItActive()
        {
            var slots = new WeaponSlots(_pistol);
            int changes = 0;
            slots.Changed += () => changes++;

            Assert.IsTrue(slots.PickUp(_shotgun));

            Assert.AreSame(_pistol, slots.Selected);
            Assert.AreSame(_shotgun, slots.Picked);
            Assert.AreSame(_shotgun, slots.Active);
            Assert.AreSame(_pistol, slots.Inactive);
            Assert.IsTrue(slots.HasTwo);
            Assert.AreEqual(1, changes);
        }

        [Test]
        public void Swap_WithTwoWeapons_TogglesActive()
        {
            var slots = new WeaponSlots(_pistol);
            slots.PickUp(_shotgun);

            Assert.IsTrue(slots.Swap());
            Assert.AreSame(_pistol, slots.Active);
            Assert.AreEqual(WeaponSlots.SelectedIndex, slots.ActiveIndex);

            Assert.IsTrue(slots.Swap());
            Assert.AreSame(_shotgun, slots.Active);
            Assert.AreEqual(WeaponSlots.PickedIndex, slots.ActiveIndex);
        }

        [Test]
        public void PickUp_NewWeapon_ReplacesSecondSlotOnly()
        {
            var slots = new WeaponSlots(_pistol);
            slots.PickUp(_shotgun);
            slots.Swap();

            Assert.IsTrue(slots.PickUp(_rifle));

            Assert.AreSame(_pistol, slots.Selected, "the chosen weapon stays");
            Assert.AreSame(_rifle, slots.Picked);
            Assert.AreSame(_rifle, slots.Active);
        }

        [Test]
        public void PickUp_CarriedOrNullWeapon_ChangesNothing()
        {
            var slots = new WeaponSlots(_pistol);
            slots.PickUp(_shotgun);
            slots.Swap();
            int changes = 0;
            slots.Changed += () => changes++;

            Assert.IsFalse(slots.PickUp(_pistol));
            Assert.IsFalse(slots.PickUp(_shotgun));
            Assert.IsFalse(slots.PickUp(null));

            Assert.AreSame(_pistol, slots.Active);
            Assert.AreSame(_shotgun, slots.Picked);
            Assert.AreEqual(0, changes);
        }

        [Test]
        public void Reset_DropsPickedWeapon()
        {
            var slots = new WeaponSlots(_pistol);
            slots.PickUp(_shotgun);

            slots.Reset(_rifle);

            Assert.AreSame(_rifle, slots.Active);
            Assert.IsNull(slots.Picked);
            Assert.IsFalse(slots.HasTwo);
        }
    }
}
