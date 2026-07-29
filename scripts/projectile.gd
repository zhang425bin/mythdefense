extends Area2D

var speed: float = 1200.0
var damage: float = 1.0
var direction: Vector2 = Vector2.UP
var lifetime: float = 3.0
var _life: float = 0.0

@onready var _sprite: Sprite2D = $Sprite2D

func _ready() -> void:
	body_entered.connect(_on_body_entered)

func setup(tex: Texture2D, dmg: float, spd: float, dir: Vector2) -> void:
	_sprite.texture = tex
	damage = dmg
	speed = spd
	direction = dir.normalized()
	rotation = direction.angle() + PI / 2.0
	_life = 0.0

func _physics_process(delta: float) -> void:
	position += direction * speed * delta
	_life += delta
	if _life >= lifetime:
		_game_controller().return_projectile(self)

func _on_body_entered(body: Node2D) -> void:
	if body.is_in_group("enemies"):
		body.take_damage(damage)
		_game_controller().return_projectile(self)

func _game_controller() -> Node:
	return get_tree().current_scene
