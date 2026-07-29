class_name ObjectPool

var _scene: PackedScene
var _container: Node
var _capacity: int
var _available: Array[Node] = []
var _active: Array[Node] = []

func initialize(scene: PackedScene, container: Node, capacity: int) -> void:
	_scene = scene
	_container = container
	_capacity = capacity
	for i in range(capacity):
		var obj = _scene.instantiate()
		obj.process_mode = Node.PROCESS_MODE_DISABLED
		obj.visible = false
		_container.add_child(obj)
		_available.append(obj)

func get_object() -> Node:
	if _available.is_empty():
		return null
	var obj = _available.pop_back()
	obj.process_mode = Node.PROCESS_MODE_INHERIT
	obj.visible = true
	_active.append(obj)
	return obj

func return_object(obj: Node) -> void:
	if obj == null:
		return
	obj.process_mode = Node.PROCESS_MODE_DISABLED
	obj.visible = false
	obj.position = Vector2.ZERO
	_active.erase(obj)
	_available.append(obj)

func get_active() -> Array[Node]:
	return _active.duplicate()

func clear() -> void:
	for obj in _active:
		return_object(obj)
