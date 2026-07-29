extends Area2D

var max_hp: float = 3.0
var hp: float = 3.0
var speed: float = 150.0
var damage: float = 1.0
var xp_value: float = 1.0
var _active: bool = false

@onready var _sprite: Sprite2D = $Sprite2D

func _ready() -> void:
	body_entered.connect(_on_body_entered)

func setup(tex: Texture2D, max_health: float, move_speed: float, atk: float, xp: float) -> void:
	_sprite.texture = tex
	max_hp = max_health
	hp = max_health
	speed = move_speed
	damage = atk
	xp_value = xp
	_active = true

func _physics_process(delta: float) -> void:
	if not _active:
		return
	position.y += speed * delta
	var viewport_size = get_viewport_rect().size
	if position.y > viewport_size.y + 100.0:
		_game_controller().return_enemy(self)

func take_damage(amount: float) -> void:
	hp -= amount
	if hp <= 0.0:
		_game_controller().on_enemy_died(self)
		_game_controller().return_enemy(self)

func _on_body_entered(body: Node2D) -> void:
	if not _active:
		return
	if body.is_in_group("player"):
		body.take_damage(damage)
		_game_controller().return_enemy(self)
	elif body.is_in_group("barrier"):
		body.take_damage(damage)
		_game_controller().return_enemy(self)

func _game_controller() -> Node:
	return get_tree().current_scene
