extends Area2D

var max_hp: float = 10.0
var hp: float = 10.0
var damage: float = 1.0
var fire_rate: float = 2.0
var projectile_count: int = 1
var multi_shot: int = 1
var _fire_timer: float = 0.0

@onready var _sprite: Sprite2D = $Sprite2D
@onready var _muzzle: Marker2D = $Muzzle

func _ready() -> void:
	add_to_group("player")

func _process(delta: float) -> void:
	_fire_timer += delta
	if _fire_timer >= 1.0 / fire_rate:
		_fire_timer = 0.0
		_shoot()

func _shoot() -> void:
	var controller = get_tree().current_scene
	var target = _find_nearest_enemy()
	var base_dir = Vector2.UP
	if target != null:
		base_dir = (target.global_position - global_position).normalized()
	
	var angle_step = 15.0 * PI / 180.0
	var start_angle = -(multi_shot - 1) * angle_step / 2.0
	for m in range(multi_shot):
		var dir = base_dir.rotated(start_angle + m * angle_step)
		for p in range(projectile_count):
			controller.spawn_projectile(_muzzle.global_position, dir, damage)

func _find_nearest_enemy() -> Node2D:
	var enemies = get_tree().get_nodes_in_group("enemies")
	var nearest: Node2D = null
	var nearest_dist: float = 100000.0
	for enemy in enemies:
		if not enemy.visible:
			continue
		var d = global_position.distance_squared_to(enemy.global_position)
		if d < nearest_dist:
			nearest_dist = d
			nearest = enemy
	return nearest

func take_damage(amount: float) -> void:
	hp -= amount
	if hp <= 0.0:
		hp = 0.0
	get_tree().current_scene.on_player_damaged()

func heal(amount: float) -> void:
	hp = min(hp + amount, max_hp)

func reset_stats() -> void:
	hp = max_hp
	damage = 1.0
	fire_rate = 2.0
	projectile_count = 1
	multi_shot = 1
