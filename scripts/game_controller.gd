extends Node2D

@export var enemy_scenes: Array[PackedScene] = []
@export var projectile_scene: PackedScene

@onready var player: Node2D = $Player
@onready var barrier: Node2D = $Barrier
@onready var enemy_container: Node2D = $EnemyContainer
@onready var projectile_container: Node2D = $ProjectileContainer
@onready var background: TextureRect = $Background
@onready var hud: CanvasLayer = $HUD
@onready var skill_panel: Panel = $HUD/SkillPanel
@onready var game_over_panel: Panel = $HUD/GameOverPanel

var enemy_textures: Array[Texture2D] = []
var projectile_texture: Texture2D
var barrier_texture: Texture2D
var bg_texture: Texture2D
var skill_icons: Dictionary = {}

var enemy_pool: ObjectPool
var projectile_pool: ObjectPool

var wave: int = 1
var max_waves: int = 10
var game_time: float = 0.0
var xp: float = 0.0
var xp_to_level: float = 10.0
var level: int = 1
var is_paused: bool = false
var is_game_over: bool = false

var _spawn_timer: float = 0.0
var _wave_timer: float = 0.0
var _wave_interval: float = 15.0
var _enemies_spawned_in_wave: int = 0
var _enemies_per_wave: int = 10

var _skill_options: Array[Skill] = []
var _all_skills: Array[Skill] = []

func _ready() -> void:
	_load_assets()
	_setup_scene()
	_setup_pools()
	_build_skills()
	_update_hud()
	skill_panel.visible = false
	game_over_panel.visible = false

func _load_assets() -> void:
	enemy_textures.append(preload("res://assets/sprites/enemyBlack1.png"))
	enemy_textures.append(preload("res://assets/sprites/enemyBlue2.png"))
	enemy_textures.append(preload("res://assets/sprites/enemyGreen3.png"))
	enemy_textures.append(preload("res://assets/sprites/enemyRed4.png"))
	projectile_texture = preload("res://assets/sprites/laserBlue16.png")
	barrier_texture = preload("res://assets/sprites/laserRed01.png")
	bg_texture = preload("res://assets/sprites/darkPurple.png")
	
	skill_icons["attack_speed"] = preload("res://assets/sprites/bolt_gold.png")
	skill_icons["damage_up"] = preload("res://assets/sprites/star.png")
	skill_icons["multi_shot"] = preload("res://assets/sprites/shield.png")
	skill_icons["projectile_count"] = preload("res://assets/sprites/bolt_gold.png")
	skill_icons["heal"] = preload("res://assets/sprites/pill_blue.png")
	skill_icons["barrier_repair"] = preload("res://assets/sprites/playerLife1_red.png")

func _setup_scene() -> void:
	background.texture = bg_texture
	background.size = get_viewport_rect().size
	barrier.get_node("Sprite2D").texture = barrier_texture
	barrier.setup(20.0)
	player.reset_stats()
	
	var viewport_size = get_viewport_rect().size
	player.position = Vector2(viewport_size.x * 0.5, viewport_size.y - 120.0)
	barrier.position = Vector2(viewport_size.x * 0.5, viewport_size.y - 60.0)

func _setup_pools() -> void:
	enemy_pool = ObjectPool.new()
	enemy_pool.initialize(enemy_scenes[0], enemy_container, 40)
	projectile_pool = ObjectPool.new()
	projectile_pool.initialize(projectile_scene, projectile_container, 60)

func _build_skills() -> void:
	_all_skills.append(Skill.new("attack_speed", "疾风剑诀", "攻击速度 +20%", skill_icons["attack_speed"]))
	_all_skills.append(Skill.new("damage_up", "破军之力", "飞剑伤害 +0.5", skill_icons["damage_up"]))
	_all_skills.append(Skill.new("multi_shot", "漫天剑雨", "同时发射方向 +1", skill_icons["multi_shot"]))
	_all_skills.append(Skill.new("projectile_count", "连珠剑", "每次发射飞剑 +1", skill_icons["projectile_count"]))
	_all_skills.append(Skill.new("heal", "灵丹妙药", "主角回血 +3", skill_icons["heal"]))
	_all_skills.append(Skill.new("barrier_repair", "固阵符", "防线回血 +5", skill_icons["barrier_repair"]))

func _process(delta: float) -> void:
	if is_paused or is_game_over:
		return
	game_time += delta
	_wave_timer += delta
	
	_update_spawn(delta)
	_update_hud()
	
	if _wave_timer >= _wave_interval:
		_wave_timer = 0.0
		if wave < max_waves:
			wave += 1
			_enemies_spawned_in_wave = 0
			_enemies_per_wave += 3
		else:
			on_game_over(true)

func _update_spawn(delta: float) -> void:
	_spawn_timer += delta
	var spawn_interval = max(0.3, 1.5 - wave * 0.1)
	if _spawn_timer >= spawn_interval and _enemies_spawned_in_wave < _enemies_per_wave:
		_spawn_timer = 0.0
		_spawn_enemy()

func _spawn_enemy() -> void:
	var enemy = enemy_pool.get_object()
	if enemy == null:
		return
	_enemies_spawned_in_wave += 1
	var viewport_size = get_viewport_rect().size
	var x = randf_range(40.0, viewport_size.x - 40.0)
	enemy.position = Vector2(x, -50.0)
	var tex = enemy_textures[randi() % enemy_textures.size()]
	var hp = 3.0 + wave * 1.5
	var speed = 100.0 + wave * 15.0
	var damage = 1.0 + wave * 0.2
	var xp = 1.0 + wave * 0.1
	enemy.setup(tex, hp, speed, damage, xp)

func spawn_projectile(pos: Vector2, dir: Vector2, dmg: float) -> void:
	var proj = projectile_pool.get_object()
	if proj == null:
		return
	proj.position = pos
	proj.setup(projectile_texture, dmg, 1200.0, dir)

func return_projectile(proj: Node) -> void:
	projectile_pool.return_object(proj)

func return_enemy(enemy: Node) -> void:
	enemy_pool.return_object(enemy)

func on_enemy_died(enemy: Node2D) -> void:
	xp += enemy.xp_value
	if xp >= xp_to_level:
		xp -= xp_to_level
		xp_to_level *= 1.3
		level += 1
		_show_skill_selection()

func on_player_damaged() -> void:
	_update_hud()
	if player.hp <= 0.0:
		on_game_over(false)

func _show_skill_selection() -> void:
	is_paused = true
	get_tree().paused = true
	_skill_options.clear()
	var available = _all_skills.duplicate()
	available.shuffle()
	for i in range(min(3, available.size())):
		_skill_options.append(available[i])
	
	for i in range(3):
		var btn = skill_panel.get_node("VBoxContainer/Button" + str(i + 1)) as Button
		if i < _skill_options.size():
			var sk = _skill_options[i]
			btn.icon = sk.icon
			btn.text = sk.name + "\n" + sk.description
			btn.visible = true
		else:
			btn.visible = false
	skill_panel.visible = true

func _on_skill_selected(index: int) -> void:
	if index < 0 or index >= _skill_options.size():
		return
	var sk = _skill_options[index]
	sk.apply(player)
	skill_panel.visible = false
	get_tree().paused = false
	is_paused = false

func on_game_over(victory: bool) -> void:
	is_game_over = true
	get_tree().paused = true
	var label = game_over_panel.get_node("Label") as Label
	if victory:
		label.text = "妖尽道存！\n你守住了防线\n时间：%1.fs" % game_time
	else:
		label.text = "防线告急…\n存活时间：%1.fs" % game_time
	game_over_panel.visible = true

func _update_hud() -> void:
	var wave_label = hud.get_node("WaveLabel") as Label
	var time_label = hud.get_node("TimeLabel") as Label
	var fps_label = hud.get_node("FPSLabel") as Label
	var xp_bar = hud.get_node("XPBar") as ProgressBar
	var hp_bar = hud.get_node("HPBar") as ProgressBar
	var barrier_bar = hud.get_node("BarrierBar") as ProgressBar
	
	wave_label.text = "波次：%d/%d" % [wave, max_waves]
	time_label.text = "时间：%1.fs" % game_time
	fps_label.text = "FPS：%d" % Engine.get_frames_per_second()
	xp_bar.max_value = xp_to_level
	xp_bar.value = xp
	hp_bar.max_value = player.max_hp
	hp_bar.value = player.hp
	barrier_bar.max_value = barrier.max_hp
	barrier_bar.value = barrier.hp
