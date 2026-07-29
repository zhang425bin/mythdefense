extends Area2D

var max_hp: float = 20.0
var hp: float = 20.0

@onready var _sprite: Sprite2D = $Sprite2D

func _ready() -> void:
	add_to_group("barrier")

func setup(max_health: float) -> void:
	max_hp = max_health
	hp = max_health

func take_damage(amount: float) -> void:
	hp -= amount
	if hp <= 0.0:
		hp = 0.0
		get_tree().current_scene.on_game_over(false)

func heal(amount: float) -> void:
	hp = min(hp + amount, max_hp)
