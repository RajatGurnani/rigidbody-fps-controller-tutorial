using System.Collections;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GunData gunData;
    [SerializeField] private Transform cam;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Animator _animator;

    [Header("Bullet")]
    [SerializeField] private string bulletKey = "Bullet";

    private bool reloading = false;
    private int currentAmmo = 0;
    private PlayerMovement playerMovement;

    float timeSinceLastShot;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        currentAmmo = gunData.magSize;

        playerMovement = FindFirstObjectByType<PlayerMovement>();
    }

    private void OnEnable()
    {
        PlayerShoot.shootInput += Shoot;
        PlayerShoot.reloadInput += StartReload;
    }

    private void OnDisable()
    {
        PlayerShoot.shootInput -= Shoot;
        PlayerShoot.reloadInput -= StartReload;
        reloading = false;
    }

    public void StartReload()
    {
        if (!reloading && this.gameObject.activeSelf)
            StartCoroutine(Reload());
    }

    private IEnumerator Reload()
    {
        reloading = true;
        _animator.SetTrigger("Reload");
        yield return new WaitForSeconds(gunData.reloadTime);

        currentAmmo = gunData.magSize;
        reloading = false;
    }

    private bool CanShoot() => !reloading && timeSinceLastShot > 1f / (gunData.fireRate / 60f);

    private void Shoot()
    {
        if (currentAmmo <= 0 || !CanShoot()) return;

        // Determine direction — toward raycast hit or fall back to firePoint forward
        Vector3 direction;
        if (Physics.Raycast(cam.position, cam.forward, out RaycastHit hitInfo, gunData.maxDistance))
        {
            hitInfo.transform?.GetComponent<IDamageable>()?.TakeDamage(gunData.damage);
            direction = (hitInfo.point - firePoint.position).normalized;
        }
        else
        {
            direction = firePoint.forward;
        }

        Quaternion bulletRotation = Quaternion.FromToRotation(Vector3.up, direction);

        // Kickback on Shogun-type guns
        if (!string.IsNullOrWhiteSpace(gunData.name) && gunData.name.ToLower().Contains("shotgun") && playerMovement != null)
        {
            // apply opposite impulse to movement rig
            playerMovement.ApplyKickback(-direction * gunData.recoilPerShot);
        }

        BulletPool.Instance.Get(bulletKey, firePoint.position, firePoint.rotation);

        currentAmmo--;
        if (currentAmmo <= 0)
        {
            StartReload();
        }
        timeSinceLastShot = 0;
        OnGunShot();
    }

    private void Update()
    {
        timeSinceLastShot += Time.deltaTime;
        Debug.DrawRay(cam.position, cam.forward * gunData.maxDistance);
    }

    private void OnGunShot()
    {
        _animator.Play("Shoot");
    }

    private void OnDestroy()
    {
        PlayerShoot.shootInput -= Shoot;
        PlayerShoot.reloadInput -= StartReload;
    }

    public string WeaponName => gunData != null ? gunData.name : "Unknown";
    public int CurrentAmmo => currentAmmo;
    public int MaxAmmo => gunData != null ? gunData.magSize : 0;
}