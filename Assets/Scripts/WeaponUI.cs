using UnityEngine;
using TMPro;

public class WeaponUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WeaponSwitching weaponSwitching;
    [SerializeField] private TMP_Text weaponNameText;
    [SerializeField] private TMP_Text ammoText;

    private Gun currentGun;

    private void Start()
    {
        if (weaponSwitching == null)
            weaponSwitching = FindFirstObjectByType<WeaponSwitching>();
    }

    private void Update()
    {
        if (weaponSwitching == null || weaponNameText == null || ammoText == null)
            return;

        Transform activeWeapon = weaponSwitching.ActiveWeapon;
        if (activeWeapon == null)
            return;

        Gun gun = activeWeapon.GetComponentInChildren<Gun>();
        if (gun == null)
            return;

        currentGun = gun;

        weaponNameText.text = string.IsNullOrWhiteSpace(gun.WeaponName) ? "No Weapon" : gun.WeaponName;
        ammoText.text = $"Ammo: {gun.CurrentAmmo} / {gun.MaxAmmo}";
    }
}
