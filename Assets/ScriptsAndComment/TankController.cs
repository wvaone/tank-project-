using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Основной скрипт управления танком. Отвечает за:
/// - Переключение между ролями (мехвод, наводчик, командир)
/// - Управление движением корпуса и вращением башни
/// - Стрельбу, перезарядку, откат ствола
/// - Воспроизведение звуков
/// - Обновление HUD-интерфейса
/// </summary>
public class TankController : MonoBehaviour
{
    // ================================ //
    // ========== ПОЛЯ ИНСПЕКТОРА ===== //
    // ================================ //

    [Header("=== ОСНОВНЫЕ КОМПОНЕНТЫ ТАНКА ===")]
    /// <summary>
    /// Ссылка на трансформ корпуса танка. К нему привязана камера мехвода.
    /// Именно корпус поворачивается при нажатии A/D.
    /// </summary>
    public Transform hull;

    /// <summary>
    /// Ссылка на трансформ башни танка. К ней привязана камера наводчика.
    /// Башня вращается независимо от корпуса (стрелки ← →).
    /// </summary>
    public Transform turret;

    /// <summary>
    /// Точка, откуда вылетает снаряд (конец ствола пушки).
    /// Используется для спавна снаряда и вспышки.
    /// </summary>
    public Transform gunEnd;

    /// <summary>
    /// Трансформ, к которому применяется анимация отката ствола.
    /// Обычно это сам ствол или дочерний объект внутри башни.
    /// </summary>
    public Transform recoilTransform;

    [Header("=== КАМЕРЫ ДЛЯ РАЗНЫХ РОЛЕЙ ===")]
    /// <summary>
    /// Камера механика-водителя. Привязана к корпусу танка.
    /// Активируется при выборе роли "driver".
    /// </summary>
    public Camera driverCamera;

    /// <summary>
    /// Камера наводчика. Привязана к башне танка.
    /// Активируется при выборе роли "gunner".
    /// </summary>
    public Camera gunnerCamera;

    /// <summary>
    /// Камера командира. Может свободно вращаться на 360°.
    /// Не привязана жестко к башне или корпусу — вращается мышью.
    /// </summary>
    public Camera commanderCamera;

    [Header("=== НАСТРОЙКИ ДВИЖЕНИЯ ===")]
    /// <summary>
    /// Скорость движения танка вперёд/назад (W/S).
    /// Значение умножается на Time.deltaTime для плавности.
    /// </summary>
    public float moveSpeed = 10f;

    /// <summary>
    /// Скорость поворота корпуса танка (A/D).
    /// Измеряется в градусах в секунду.
    /// </summary>
    public float rotationSpeed = 50f;

    [Header("=== НАСТРОЙКИ БАШНИ ===")]
    /// <summary>
    /// Скорость горизонтального вращения башни (стрелки ← →).
    /// Наводчик использует эту настройку для прицеливания.
    /// </summary>
    public float turretRotationSpeed = 30f;

    [Header("=== НАСТРОЙКИ СТРЕЛЬБЫ ===")]
    /// <summary>
    /// Префаб снаряда, который будет создаваться при выстреле.
    /// Должен содержать Rigidbody и скрипт Shell.cs.
    /// </summary>
    public GameObject shellPrefab;

    /// <summary>
    /// Префаб визуального эффекта вспышки при выстреле (мuzzle flash).
    /// Автоматически уничтожается через 0.5 секунд.
    /// </summary>
    public GameObject muzzleFlashPrefab;

    /// <summary>
    /// Сила, с которой снаряд вылетает из пушки.
    /// Применяется через AddForce в направлении ствола.
    /// </summary>
    public float fireForce = 1000f;

    /// <summary>
    /// Время перезарядки между выстрелами (в секундах).
    /// Во время перезарядки нельзя стрелять.
    /// </summary>
    public float reloadTime = 1.5f;

    /// <summary>
    /// Звуковой клип, воспроизводимый при выстреле.
    /// Воспроизводится через AudioSource.PlayClipAtPoint в позиции ствола.
    /// </summary>
    public AudioClip fireSound;

    /// <summary>
    /// Звуковой клип, воспроизводимый при начале перезарядки.
    /// Проигрывается через основной AudioSource танка.
    /// </summary>
    public AudioClip reloadSound;

    /// <summary>
    /// Флаг, можно ли сейчас стрелять.
    /// Сбрасывается в false при выстреле, восстанавливается после reloadTime.
    /// </summary>
    private bool canFire = true;

    /// <summary>
    /// Максимальное количество боеприпасов в магазине.
    /// </summary>
    public int maxAmmo = 10;

    /// <summary>
    /// Текущее количество боеприпасов. Уменьшается при выстреле.
    /// Автоматически пополняется со временем (если меньше maxAmmo).
    /// </summary>
    private int currentAmmo;

    /// <summary>
    /// Слайдер на HUD, показывающий прогресс перезарядки.
    /// Заполняется от 0 до 1 во время корутины ReloadTimer.
    /// </summary>
    public Slider reloadSlider;

    /// <summary>
    /// Текстовый элемент HUD, отображающий текущий боезапас.
    /// Формат: "AMMO: X/Y"
    /// </summary>
    public Text ammoText;

    [Header("=== ИНТЕРФЕЙС И РОЛИ ===")]
    /// <summary>
    /// Текст на экране, показывающий текущую активную роль.
    /// Обновляется при переключении между водителем, наводчиком и командиром.
    /// </summary>
    public Text roleText;

    /// <summary>
    /// Кнопка UI для переключения на роль механика-водителя.
    /// Нажатие вызывает SwitchCamera("driver").
    /// </summary>
    public Button driverBtn;

    /// <summary>
    /// Кнопка UI для переключения на роль наводчика.
    /// Нажатие вызывает SwitchCamera("gunner").
    /// </summary>
    public Button gunnerBtn;

    /// <summary>
    /// Кнопка UI для переключения на роль командира.
    /// Нажатие вызывает SwitchCamera("commander").
    /// </summary>
    public Button commanderBtn;

    /// <summary>
    /// Текст на экране, отображающий количество уничтоженных врагов.
    /// Обновляется при вызове AddKill().
    /// </summary>
    public Text killsText;

    [Header("=== АНИМАЦИЯ ОТКАТА СТВОЛА ===")]
    /// <summary>
    /// Расстояние, на которое ствол откатывается назад при выстреле.
    /// Применяется к локальной позиции recoilTransform.
    /// </summary>
    public float recoilDistance = 0.3f;

    /// <summary>
    /// Длительность анимации отката и возврата ствола (в секундах).
    /// Разделена на две фазы: откат и возврат.
    /// </summary>
    public float recoilDuration = 0.1f;

    /// <summary>
    /// Исходная локальная позиция ствола (до отката).
    /// Сохраняется в Start() для корректного возврата.
    /// </summary>
    private Vector3 originalGunPosition;

    [Header("=== АУДИО ===")]
    /// <summary>
    /// Основной AudioSource, прикреплённый к танку.
    /// Используется для воспроизведения звука двигателя и перезарядки.
    /// </summary>
    public AudioSource audioSource;

    /// <summary>
    /// Звуковой клип зацикленного звука двигателя.
    /// Проигрывается с самого начала и не останавливается.
    /// </summary>
    public AudioClip engineSound;

    [Header("=== ССЫЛКИ НА КОМПОНЕНТЫ ===")]
    /// <summary>
    /// Rigidbody корпуса танка. Используется для физического перемещения.
    /// Рекомендуется заморозить вращение по X и Z в инспекторе.
    /// </summary>
    public Rigidbody rb;

    // ================================ //
    // ========== ПРИВАТНЫЕ ПОЛЯ ======= //
    // ================================ //

    /// <summary>
    /// Ссылка на активную камеру (для удобства).
    /// Обновляется при переключении ролей.
    /// </summary>
    private Camera activeCamera;

    /// <summary>
    /// Текущая активная роль: "driver", "gunner", "commander".
    /// Определяет, чьи команды управления обрабатываются.
    /// </summary>
    private string currentPlayer = "driver";

    /// <summary>
    /// Счётчик убийств (количество уничтоженных врагов).
    /// Увеличивается при вызове AddKill().
    /// </summary>
    private int killCount = 0;

    // ================================ //
    // ========== СТАРТ =============== //
    // ================================ //

    /// <summary>
    /// Вызывается при старте игры.
    /// Инициализирует боезапас, сохраняет исходную позицию ствола,
    /// включает камеру водителя, настраивает кнопки и запускает звук двигателя.
    /// </summary>
    void Start()
    {
        // Устанавливаем начальное количество боеприпасов
        currentAmmo = maxAmmo;

        // Сохраняем исходную локальную позицию ствола для анимации отката
        originalGunPosition = recoilTransform.localPosition;

        // Переключаемся на камеру водителя по умолчанию
        SwitchCamera("driver");

        // Настраиваем обработчики нажатий на кнопки UI
        SetupButtons();

        // Настраиваем и запускаем звук двигателя
        audioSource.clip = engineSound;
        audioSource.loop = true;  // зацикливаем
        audioSource.Play();       // запускаем воспроизведение
    }

    /// <summary>
    /// Настраивает обработчики нажатий на кнопки переключения ролей.
    /// При нажатии на кнопку вызывается SwitchCamera с соответствующей ролью.
    /// </summary>
    void SetupButtons()
    {
        driverBtn.onClick.AddListener(() => SwitchCamera("driver"));
        gunnerBtn.onClick.AddListener(() => SwitchCamera("gunner"));
        commanderBtn.onClick.AddListener(() => SwitchCamera("commander"));
    }

    // ================================ //
    // ========== ОБНОВЛЕНИЕ ========== //
    // ================================ //

    /// <summary>
    /// Главный метод Update, вызывается каждый кадр.
    /// Обрабатывает ввод для текущей роли, обновляет интерфейс,
    /// и вызывает выстрел при нажатии ЛКМ (если можно стрелять).
    /// </summary>
    void Update()
    {
        // Обработка ввода для каждой роли (только если роль активна)
        HandleDriverInput();
        HandleGunnerInput();
        HandleCommanderInput();

        // Обновление текстов и элементов HUD
        UpdateUI();

        // Если активна роль, позволяющая стрелять (наводчик или командир),
        // и нажата левая кнопка мыши, и можно стрелять, и есть боеприпасы — стреляем
        if ((currentPlayer == "gunner" || currentPlayer == "commander") &&
            Input.GetMouseButtonDown(0) &&
            canFire &&
            currentAmmo > 0)
        {
            Fire();
        }
    }

    // ================================ //
    // ========== УПРАВЛЕНИЕ ========== //
    // ================================ //

    /// <summary>
    /// Обрабатывает ввод для механика-водителя.
    /// Реагирует на W/S (вперёд/назад) и A/D (поворот корпуса).
    /// Работает только если currentPlayer == "driver".
    /// </summary>
    void HandleDriverInput()
    {
        // Если сейчас не водитель — ничего не делаем
        if (currentPlayer != "driver") return;

        // Получаем ось "Vertical" (W/S) для движения вперёд/назад
        float move = Input.GetAxis("Vertical");

        // Получаем ось "Horizontal" (A/D) для поворота корпуса
        float turn = Input.GetAxis("Horizontal");

        // Применяем скорость движения в направлении вперёд относительно корпуса
        rb.velocity = transform.forward * move * moveSpeed;

        // Рассчитываем угол поворота за кадр
        float rotation = turn * rotationSpeed * Time.deltaTime;

        // Поворачиваем корпус вокруг оси Y
        hull.Rotate(0, rotation, 0);
    }

    /// <summary>
    /// Обрабатывает ввод для наводчика.
    /// Реагирует на стрелки ← → для вращения башни.
    /// Работает только если currentPlayer == "gunner".
    /// </summary>
    void HandleGunnerInput()
    {
        // Если сейчас не наводчик — ничего не делаем
        if (currentPlayer != "gunner") return;

        // Получаем ось "HorizontalArrow" (стрелки ← →)
        float turretHorizontal = Input.GetAxis("HorizontalArrow");

        // Получаем ось "VerticalArrow" (стрелки ↑ ↓) — можно использовать для вертикального наведения
        float turretVertical = Input.GetAxis("VerticalArrow");

        // Вращаем башню по горизонтали
        turret.Rotate(0, turretHorizontal * turretRotationSpeed * Time.deltaTime, 0);

        // 🔽 Раскомментируй, если хочешь добавить вертикальное наведение:
        // turret.Rotate(-turretVertical * turretRotationSpeed * 0.5f, 0, 0);
    }

    /// <summary>
    /// Обрабатывает ввод для командира.
    /// Реагирует на движение мыши для свободного вращения камеры на 360°.
    /// Работает только если currentPlayer == "commander".
    /// </summary>
    void HandleCommanderInput()
    {
        // Если сейчас не командир — ничего не делаем
        if (currentPlayer != "commander") return;

        // Получаем движение мыши по X и Y
        float mouseX = Input.GetAxis("Mouse X") * 2f;  // чувствительность
        float mouseY = Input.GetAxis("Mouse Y") * 2f;

        // Вращаем камеру командира:
        // - По вертикали (вокруг оси X) — с учётом инверсии (минус mouseY)
        // - По горизонтали (вокруг оси Y) — в мировых координатах, чтобы не зависеть от поворота танка
        commanderCamera.transform.Rotate(Vector3.right, -mouseY, Space.Self);
        commanderCamera.transform.Rotate(Vector3.up, mouseX, Space.World);
    }

    // ================================ //
    // ========== СТРЕЛЬБА ============ //
    // ================================ //

    /// <summary>
    /// Выполняет выстрел: создаёт снаряд, вспышку, звук, анимацию отката.
    /// Уменьшает боезапас и запускает перезарядку.
    /// </summary>
    void Fire()
    {
        // Создаём вспышку на конце ствола, если префаб задан
        if (muzzleFlashPrefab != null)
        {
            GameObject flash = Instantiate(muzzleFlashPrefab, gunEnd.position, gunEnd.rotation);
            Destroy(flash, 0.5f);  // уничтожаем через полсекунды
        }

        // Создаём снаряд в позиции ствола с его поворотом
        GameObject shell = Instantiate(shellPrefab, gunEnd.position, gunEnd.rotation);

        // Получаем Rigidbody снаряда и придаём ему силу в направлении ствола
        Rigidbody shellRb = shell.GetComponent<Rigidbody>();
        shellRb.AddForce(gunEnd.forward * fireForce, ForceMode.Impulse);

        // Запускаем корутину анимации отката ствола
        StartCoroutine(Recoil());

        // Проигрываем звук выстрела в позиции ствола
        AudioSource.PlayClipAtPoint(fireSound, gunEnd.position, 0.8f);

        // Уменьшаем боезапас
        currentAmmo--;

        // Запускаем корутину перезарядки
        StartCoroutine(ReloadTimer());

        // Уничтожаем снаряд через 5 секунд на всякий случай (если не попал во что-то)
        Destroy(shell, 5f);
    }

    /// <summary>
    /// Корутина анимации отката ствола.
    /// Сначала ствол двигается назад, затем возвращается в исходное положение.
    /// </summary>
    IEnumerator Recoil()
    {
        // Рассчитываем целевую позицию отката (назад по направлению ствола)
        Vector3 targetPos = originalGunPosition - recoilTransform.forward * recoilDistance;

        float timer = 0;

        // Фаза 1: откат назад
        while (timer < recoilDuration)
        {
            timer += Time.deltaTime;
            // Плавно перемещаем ствол к целевой позиции
            recoilTransform.localPosition = Vector3.Lerp(originalGunPosition, targetPos, timer / recoilDuration);
            yield return null;  // ждём следующего кадра
        }

        timer = 0;

        // Фаза 2: возврат в исходное положение
        while (timer < recoilDuration)
        {
            timer += Time.deltaTime;
            recoilTransform.localPosition = Vector3.Lerp(targetPos, originalGunPosition, timer / recoilDuration);
            yield return null;
        }
    }

    /// <summary>
    /// Корутина перезарядки.
    /// Блокирует стрельбу на reloadTime секунд, обновляет слайдер,
    /// проигрывает звук перезарядки, затем разблокирует стрельбу.
    /// Также запускает пополнение боезапаса через 3 секунды (если есть место).
    /// </summary>
    IEnumerator ReloadTimer()
    {
        // Блокируем стрельбу
        canFire = false;

        // Проигрываем звук перезарядки
        audioSource.PlayOneShot(reloadSound);

        float timer = 0;

        // Обновляем слайдер перезарядки от 0 до 1
        while (timer < reloadTime)
        {
            timer += Time.deltaTime;
            reloadSlider.value = timer / reloadTime;
            yield return null;
        }

        // Перезарядка завершена
        canFire = true;
        reloadSlider.value = 0;  // сбрасываем слайдер

        // Если боезапас не полный — через 3 секунды добавляем 1 патрон
        if (currentAmmo < maxAmmo)
        {
            yield return new WaitForSeconds(3f);
            currentAmmo++;
        }
    }

    // ================================ //
    // ========== ВСПОМОГАТЕЛЬНЫЕ ===== //
    // ================================ //

    /// <summary>
    /// Увеличивает счётчик убийств на 1.
    /// Вызывается из EnemyAI при уничтожении врага.
    /// </summary>
    public void AddKill()
    {
        killCount++;
    }

    /// <summary>
    /// Переключает активную камеру и роль.
    /// Отключает все камеры, включает нужную, обновляет currentPlayer.
    /// </summary>
    /// <param name="role">"driver", "gunner" или "commander"</param>
    void SwitchCamera(string role)
    {
        currentPlayer = role;

        // Отключаем все камеры
        driverCamera.enabled = false;
        gunnerCamera.enabled = false;
        commanderCamera.enabled = false;

        // Выбираем активную камеру в зависимости от роли
        switch (role)
        {
            case "driver":
                activeCamera = driverCamera;
                break;
            case "gunner":
                activeCamera = gunnerCamera;
                break;
            case "commander":
                activeCamera = commanderCamera;
                break;
        }

        // Включаем выбранную камеру
        activeCamera.enabled = true;
    }

    /// <summary>
    /// Обновляет текстовые элементы HUD: роль, боезапас, количество убийств.
    /// Вызывается каждый кадр в Update.
    /// </summary>
    void UpdateUI()
    {
        roleText.text = $"ROLE: {currentPlayer.ToUpper()}";
        ammoText.text = $"AMMO: {currentAmmo}/{maxAmmo}";
        killsText.text = $"KILLS: {killCount}";
    }
}