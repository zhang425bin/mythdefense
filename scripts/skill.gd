class_name Skill

var id: String
var name: String
var description: String
var icon: Texture2D

func _init(p_id: String, p_name: String, p_description: String, p_icon: Texture2D) -> void:
	id = p_id
	name = p_name
	description = p_description
	icon = p_icon

func apply(player: Node2D) -> void:
	match id:
		"attack_speed":
			player.fire_rate *= 1.2
		"damage_up":
			player.damage += 0.5
		"multi_shot":
			player.multi_shot += 1
		"projectile_count":
			player.projectile_count += 1
		"heal":
			player.heal(3.0)
		"barrier_repair":
			var barrier = player.get_tree().get_first_node_in_group("barrier")
			if barrier != null:
				barrier.heal(5.0)
