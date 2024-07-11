@tool
extends TextureRect

@export var enable_center_pivot: bool = false

func _ready() -> void:
	resized.connect(update_pivot.bind(enable_center_pivot))

func update_pivot(state: bool) -> void:
	if state and texture != null:
		pivot_offset = size / 2
	elif !state:
		pivot_offset = Vector2.ZERO
