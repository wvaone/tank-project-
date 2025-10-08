using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TankController : MonoBehaviour
{
    [Header("Components")]
    public Transform hull;
    public Transform turret;
    public Transform gunEnd;
    public Transform recoilTransform;
    [Tooltip("Pivot that tilts the gun barrel up and down.")]
    public Transform gunPivot;

    [Header("Cameras")]
    public Camera driverCamera;
    public Camera gunnerCamera;
    public Camera commanderCamera;

    [Header("Movement")]
    public float moveSpeed = 10f;
    public float rotationSpeed = 50f;

    [Header("Turret")]
    public float turretRotationSpeed = 30f;
    public float gunElevationSpeed = 20f;
    public float minGunPitch = -10f;
    public float maxGunPitch = 20f;

    [Header("Shooting")]
    public GameObject shellPrefab;
    public GameObject muzzleFlashPrefab;
    public float fireForce = 1000f;
    public float reloadTime = 1.5f;
    public AudioClip fireSound;
    public AudioClip reloadSound;
    private bool canFire = true;
    public int maxAmmo = 10;
    private int currentAmmo;
    public Slider reloadSlider;
    public Text ammoText;

    [Header("UI & Roles")]
    public Text roleText;
    public Button driverBtn, gunnerBtn, commanderBtn;
    public Text killsText;

    [Header("Recoil")]
    public float recoilDistance = 0.3f;
    public float recoilDuration = 0.1f;
    private Vector3 originalGunPosition;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip engineSound;

    [Header("References")]
    public Rigidbody rb;

    private Camera activeCamera;
    private string currentPlayer = "driver";
    private int killCount = 0;
    private float currentGunPitch;
    private float commanderYaw;
    private float commanderPitch;

    void Start()
    {
        currentAmmo = maxAmmo;
        originalGunPosition = recoilTransform.localPosition;
        SwitchCamera("driver");
        SetupButtons();
        audioSource.clip = engineSound;
        audioSource.loop = true;
        audioSource.Play();

        if (gunPivot != null)
        {
            var pivotEuler = gunPivot.localEulerAngles;
            currentGunPitch = pivotEuler.x > 180f ? pivotEuler.x - 360f : pivotEuler.x;
        }

        if (commanderCamera != null)
        {
            var euler = commanderCamera.transform.localEulerAngles;
            commanderYaw = euler.y;
            commanderPitch = euler.x > 180f ? euler.x - 360f : euler.x;
        }
    }

    void SetupButtons()
    {
        driverBtn.onClick.AddListener(() => SwitchCamera("driver"));
        gunnerBtn.onClick.AddListener(() => SwitchCamera("gunner"));
        commanderBtn.onClick.AddListener(() => SwitchCamera("commander"));
    }

    void Update()
    {
        HandleRoleHotkeys();
        HandleDriverInput();
        HandleGunnerInput();
        HandleCommanderInput();
        UpdateUI();

        if ((currentPlayer == "gunner" || currentPlayer == "commander") && Input.GetMouseButtonDown(0) && canFire && currentAmmo > 0)
        {
            Fire();
        }
    }

    void HandleRoleHotkeys()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            SwitchCamera("driver");
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            SwitchCamera("gunner");
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            SwitchCamera("commander");
    }

    void HandleDriverInput()
    {
        if (currentPlayer != "driver") return;

        float move = Input.GetAxis("Vertical");
        float turn = Input.GetAxis("Horizontal");

        rb.velocity = transform.forward * move * moveSpeed;
        float rotation = turn * rotationSpeed * Time.deltaTime;
        hull.Rotate(0, rotation, 0);
    }

    void HandleGunnerInput()
    {
        if (currentPlayer != "gunner") return;

        float turretHorizontal = Input.GetAxisRaw("Mouse X") * turretRotationSpeed * Time.deltaTime;
        float turretVertical = Input.GetAxisRaw("Mouse Y") * gunElevationSpeed * Time.deltaTime;

        turret.Rotate(0f, turretHorizontal, 0f, Space.Self);

        if (gunPivot != null)
        {
            currentGunPitch = Mathf.Clamp(currentGunPitch - turretVertical, minGunPitch, maxGunPitch);
            gunPivot.localRotation = Quaternion.Euler(currentGunPitch, 0f, 0f);
        }
    }

    void HandleCommanderInput()
    {
        if (currentPlayer != "commander") return;

        float mouseX = Input.GetAxis("Mouse X") * 2f;
        float mouseY = Input.GetAxis("Mouse Y") * 2f;

        commanderYaw += mouseX;
        commanderPitch = Mathf.Clamp(commanderPitch - mouseY, -80f, 80f);

        commanderCamera.transform.localRotation = Quaternion.Euler(commanderPitch, commanderYaw, 0f);
    }

    void Fire()
    {
        if (muzzleFlashPrefab != null)
        {
            GameObject flash = Instantiate(muzzleFlashPrefab, gunEnd.position, gunEnd.rotation);
            Destroy(flash, 0.5f);
        }

        GameObject shell = Instantiate(shellPrefab, gunEnd.position, gunEnd.rotation);
        Rigidbody shellRb = shell.GetComponent<Rigidbody>();
        shellRb.AddForce(gunEnd.forward * fireForce, ForceMode.Impulse);

        StartCoroutine(Recoil());
        AudioSource.PlayClipAtPoint(fireSound, gunEnd.position, 0.8f);

        currentAmmo--;
        StartCoroutine(ReloadTimer());
        Destroy(shell, 5f);
    }

    IEnumerator Recoil()
    {
        Vector3 targetPos = originalGunPosition - recoilTransform.forward * recoilDistance;
        float timer = 0;
        while (timer < recoilDuration)
        {
            timer += Time.deltaTime;
            recoilTransform.localPosition = Vector3.Lerp(originalGunPosition, targetPos, timer / recoilDuration);
            yield return null;
        }
        timer = 0;
        while (timer < recoilDuration)
        {
            timer += Time.deltaTime;
            recoilTransform.localPosition = Vector3.Lerp(targetPos, originalGunPosition, timer / recoilDuration);
            yield return null;
        }
    }

    IEnumerator ReloadTimer()
    {
        canFire = false;
        audioSource.PlayOneShot(reloadSound);

        float timer = 0;
        while (timer < reloadTime)
        {
            timer += Time.deltaTime;
            reloadSlider.value = timer / reloadTime;
            yield return null;
        }

        canFire = true;
        reloadSlider.value = 0;

        if (currentAmmo < maxAmmo)
        {
            yield return new WaitForSeconds(3f);
            currentAmmo++;
        }
    }

    public void AddKill()
    {
        killCount++;
    }

    void SwitchCamera(string role)
    {
        currentPlayer = role;

        driverCamera.enabled = false;
        gunnerCamera.enabled = false;
        commanderCamera.enabled = false;

        ToggleAudioListener(driverCamera, false);
        ToggleAudioListener(gunnerCamera, false);
        ToggleAudioListener(commanderCamera, false);

        switch (role)
        {
            case "driver": activeCamera = driverCamera; break;
            case "gunner": activeCamera = gunnerCamera; break;
            case "commander": activeCamera = commanderCamera; break;
            default: activeCamera = driverCamera; break;
        }

        if (activeCamera != null)
        {
            activeCamera.enabled = true;
            ToggleAudioListener(activeCamera, true);
        }
    }

    void UpdateUI()
    {
        roleText.text = $"ROLE: {currentPlayer.ToUpper()}";
        ammoText.text = $"AMMO: {currentAmmo}/{maxAmmo}";
        killsText.text = $"KILLS: {killCount}";
    }

    void ToggleAudioListener(Camera cam, bool enabled)
    {
        if (cam == null) return;

        var listener = cam.GetComponent<AudioListener>();
        if (listener != null)
        {
            listener.enabled = enabled;
        }
    }
}
