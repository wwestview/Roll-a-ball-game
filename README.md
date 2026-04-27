
# 🌌 Roll-a-Ball: Cosmic Edition

Цей проєкт є значно розширеною версією класичного туторіалу **Unity 3D Beginner: Roll-a-Ball**. До базової механіки збору предметів було додано повноцінне візуальне оформлення (Post-Processing, Particles), процедурну генерацію звуків, систему UI (Меню, HUD) та нові ігрові механіки (стрибки, бонуси, перешкоди).

---

## 🛠 Фічі та Реалізація

| Фіча | Реалізація | Скрипт |
| :--- | :--- | :--- |
| **Skybox (захід сонця)** | Процедурний через `Skybox/Procedural` шейдер | `SkyboxController.cs` |
| **Bloom** | URP Volume з Bloom intensity 1.2 | `LevelBuilder.cs` |
| **Color Grading** | ColorAdjustments + ACES Tonemapping + Vignette | `LevelBuilder.cs` |
| **Рампи (Ramps)** | 2 нахилені площини для переходу на другий рівень | `LevelBuilder.cs` |
| **Піднята платформа** | Другий рівень з огородженнями та бонусним PickUp | `LevelBuilder.cs` |
| **Рухомі перешкоди** | 3 кубики, що рухаються між точками (ping-pong) | `MovingObstacle.cs` |
| **Прірва** | Зона з червоними краями, при потраплянні — Game Over | `FallZone.cs`, `GapTrigger` |
| **Trail Renderer** | Cyan → Purple градієнт, що тягнеться за кулькою | `LevelBuilder.cs` |
| **Pickup Effect** | Вибух 30-50 іскор при зборі бонуса | `PickupEffectSpawner.cs` |
| **Фонова музика** | Процедурний ambient drone (30 сек loop) | `GameAudioManager.cs` |
| **Звук "дзинь"** | Rising sine chirp при зборі PickUp | `GameAudioManager.cs` |
| **Звук удару** | Noise burst при зіткненні зі стіною | `GameAudioManager.cs` |
| **Start Menu** | Сцена з UI меню: Заголовок, кнопки Start/Quit | `MainMenuUI.cs` |
| **HUD** | Відображення рахунку у лівому верхньому куті (⭐ X/12) | `GameUIManager.cs` |
| **End Game Popup** | Спливаюче вікно з результатом (Win/Lose) та кнопками | `GameUIManager.cs` |
| **Стрибок (Jump)** | Стрибок на **Пробіл** з перевіркою землі (Raycast) | `PlayerController.cs` |
| **PowerUps (Бонуси)** | Золотий (Speed Boost) та Червоний (Speed Penalty) предмети | `PowerUpItem.cs` |
| **Status Bar** | Індикатор таймера діючого PowerUp | `GameUIManager.cs` |

---

## 🎮 Деталі Механік

### 🖥 Меню та UI
* **MainMenu:** Повністю генерується кодом. Має темно-фіолетовий фон з пульсуючим заголовком *"ROLL-A-BALL: COSMIC EDITION"*.
* **HUD:** Відображає поточний рахунок у лівому верхньому куті екрана. Також показує іконку активного PowerUp та смужку таймера.
* **End Game:** Коли гравець збирає всі 12 предметів, або падає у прірву, ігровий час майже зупиняється (`Time.timeScale = 0.1f`), і з'являється вікно Game Over (Перемога або Поразка) з кнопками **Restart** та **Menu**.

### 🦘 Стрибок
Гравець може стрибнути, натиснувши клавішу **Space (Пробіл)**. Реалізовано надійну перевірку знаходження на землі (Ground Check) за допомогою `Physics.Raycast(Vector3.down)`. Це дозволяє безпечно перестрибувати рухомі перешкоди або прірви, уникаючи подвійних стрибків у повітрі.

### ⚡ Система Бонусів та Штрафів
За бонуси та штрафи відповідає скрипт `PowerUpItem.cs`. У сцену додано два типи спеціальних циліндричних предметів:
* 🟡 **Золотий (Speed Boost):** Збільшує швидкість гравця у 1.5 рази на 5 секунд.
* 🔴 **Червоний (Speed Penalty):** Зменшує швидкість гравця вдвічі на 5 секунд.

При зборі бонуса на екрані з'являється UI-смужка (PowerUpBar), яка плавно зменшується. Це реалізовано за допомогою `Coroutine` у `PlayerController.cs`.

---

## 📁 Структура Проєкту

### ✨ Нові скрипти

| Файл | Опис |
| :--- | :--- |
| `SkyboxController.cs` | Процедурний skybox з фіолетовим небом та помаранчевим горизонтом. |
| `LevelBuilder.cs` | Головний скрипт, що створює всю нову геометрію та налаштовує post-processing. |
| `MovingObstacle.cs` | Ping-pong рух перешкод між двома точками з використанням SmoothStep. |
| `FallZone.cs` | Моніторинг Y-позиції гравця для детекції падіння у прірву. |
| `PickupEffectSpawner.cs`| Процедурні `ParticleSystem` ефекти при зборі бонусів. |
| `GameAudioManager.cs` | Процедурна генерація всіх ігрових звуків через `AudioClip.Create()`. |
| `GameBootstrapper.cs` | Автоматичне створення LevelBuilder та UI через атрибут `RuntimeInitializeOnLoadMethod`. |
| `MainMenuUI.cs` | Процедурна генерація стартового меню. |
| `GameUIManager.cs` | Керування HUD, таймером бонусів та End Game Popup. |
| `PowerUpItem.cs` | Логіка застосування предметів Speed Boost та Speed Penalty. |

### 🔄 Модифіковані файли базового туторіалу

| Файл | Зміни |
| :--- | :--- |
| `PlayerController.cs` | Додано: particle effect при зборі, pickup sound, wall hit sound, Jump механіку, `Coroutine` для модифікатора швидкості, виклики `GameUIManager`. |
| `Pickup.mat` | Увімкнено emission (золотисте світіння для коректної роботи ефекту Bloom). |
| `EditorBuildSettings.asset`| Додано сцени `MainMenu` та `MiniGame` до списку Build Settings. |
