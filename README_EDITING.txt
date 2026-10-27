BEFORE WE FIGHT — КАК РЕДАКТИРОВАТЬ ПРОЕКТ

Версия Unity: 6000.2.13f1
Стартовая сцена: Assets/Scenes/Intro.unity

ПОРЯДОК ЗАПУСКА
Intro — анимация руки на чёрном фоне, затем переход в Title.
Title — крупный логотип; через 2 секунды появляется PLAY.
Arena — персонаж на земле, движение и четыре анимации.
В File > Build Profiles > Scene List включены Intro, Title, Arena именно в этом порядке.
Game.unity сохранена как исходная сцена-основа и исключена из запуска сборки.

Сцены созданы и настроены в редакторе, объекты не создаются игровыми скриптами.
Три небольших компонента отвечают за переходы, задержку кнопки и управление.
Их параметры можно менять в Inspector без редактирования кода.

Где менять основные элементы:
• Фон: Assets/Prefabs/Ground.prefab
  Зелёная трава: Assets/Art/Environment/forest_grass.aseprite (исходник)
  и forest_grass.png (спрайт Unity). Размер пола меняется у Ground в Arena.
  Tools > Before We Fight > Use Brown Soil Ground возвращает старую землю;
  Use Green Grass Ground снова включает траву.
• Игровой персонаж: Arena / MODE - Arena / Player (editable prefab instance).
  Здесь находятся актуальные Animator, Top Down Player, Rigidbody2D и коллайдер.
  Move Speed — скорость; Hit Key / Jump Key — клавиши; четыре поля clips — анимации.
  Базовый Player.prefab сохранён; настройки управления добавлены на экземпляр в Arena.
• Заставка: Intro / MODE - Main Menu / Intro Hand (animated).
  Transform — положение и размер; Animator — Hand_Intro.controller;
  Intro Sequence — следующая сцена и пауза после анимации.
• Альтернативная анимация: Assets/Generated/Animations/Hand_Slash.controller.
  Её можно назначить в Animator руки вместо Hand_Intro.controller.
• Название: Title / MODE - Main Menu / Main Menu Canvas / Game Title Logo.
  Rect Transform — размер и положение, Image — изображение логотипа.
• PLAY: тот же Canvas / PLAY Button / Delayed Scene Button.
  Show After Seconds = 2; Next Scene = Arena.
• Интерфейс арены: Arena / MODE - Arena / Arena HUD Canvas.
  Внутри Health and Shift HUD (editable) находятся две отдельные полосы:
  Health Bar (editable) и Shift Bar (editable). Их размер и положение можно
  менять через Rect Transform, не затрагивая размер персонажа или деревьев.
  Исходники полос: Assets/Art/Source/UserHud_2026-10-13.
  Импортированные кадры: Assets/Art/Imported/UserHud.
  В Top Down Player: Health Segments = 62, Health Per Segment = 10
  (стартовый максимум 620 HP), Health Per Segment Per Level = 1.
  Dash Cooldown = 3 — время заполнения после рывка; повторный Shift доступен
  только после полной зарядки. Старые числа HP и полоса опыта в HUD скрыты.
• MENU: Arena HUD Canvas / MENU Button; Next Scene = Title, задержка = 0.
• Точки появления: MODE - Arena / Spawn Points
• Границы арены: MODE - Arena / Arena Bounds
• Враги: Arena / Enemy Camps / Squad 01–05. В каждой группе два InvokerMage
  и два RingCreep. Передвиньте Transform группы, чтобы перенести всю четвёрку;
  позиции отдельных врагов внутри группы тоже редактируются. Группы стоят
  далеко от стартовой позиции героя; враги реагируют только при приближении.
  Новое заклинание мага: Assets/Prefabs/InvokerMage.prefab — Marker Frames
  (7 кадров) и Beam Frames (5 кадров). Исходник и PNG-кадры находятся в
  Assets/Art/Source/Enemies/InvokerMage/NewSpell_2026-10-13.

Исходники Aseprite сохранены в Assets/Art/Source.
Экспортированные PNG и JSON находятся в Assets/Art/Sprites и Assets/Art/Animations.
Готовые кадры находятся в Assets/Generated/Sprites.
AnimationClip и обычные Animator Controller — в Assets/Generated/Animations.
Player.controller содержит Player_Idle, Player_Walk, Player_Hit, Player_Jump.
Кадры можно редактировать в окне Animation, контроллер — в окне Animator.
Для листов руки отключено масштабирование Non Power of 2; фильтр Point.
Старые компоненты Animation оставлены для сохранности старой сцены;
в новых игровых сценах анимации воспроизводит Animator.

Для просмотра:
1. Откройте Assets/Scenes/Intro.unity двойным щелчком в Project.
2. Нажмите Play в верхней панели Unity.
3. Дождитесь завершения заставки и появления кнопки PLAY, затем нажмите её.
4. Щёлкните по Game, чтобы игра получала ввод с клавиатуры.
5. WASD или стрелки — движение; J или левая кнопка мыши — анимация удара;
   Space — анимация прыжка; Esc или MENU — возврат в Title.
6. Для повторения заставки остановите Play и запустите Intro заново.

Перед изменениями сцены остановите Play: изменения объектов во время игры
обычно не сохраняются после выхода из Play Mode.

В сцене уже подготовлены Rigidbody2D, CapsuleCollider2D, границы арены и точки появления.
Движение, поворот персонажа и переключение анимаций работают.
Враги наносят 30–40 HP урона в зависимости от типа. Опыт продолжает повышать
уровень, но не отображается в HUD; каждый уровень добавляет по 1 HP к каждому
из 62 делений. Монеты и старые счётчики в HUD оставлены отключёнными.
Прыжок сейчас воспроизводит нарисованную анимацию, но не меняет физику столкновений.
