extends Camera3D
class_name GalaxyFreeCamera

const SAVE_PATH := "user://exploration.cfg"

@export var orbit_sensitivity := 0.22
@export var zoom_sensitivity := 0.12
@export var pan_sensitivity := 0.012
@export var fly_speed := 18.0
@export var min_distance := 0.6
@export var max_distance := 260.0

var focus_point := Vector3.ZERO
var yaw := 0.0
var pitch := -24.0
var distance := 34.0
var target_distance := 34.0
var dragging := false
var last_pointer := Vector2.ZERO
var last_pinch_distance := 0.0
var last_touch_center := Vector2.ZERO
var active_touches: Dictionary = {}
var save_clock := 0.0

func _ready() -> void:
	load_exploration_state()
	position = focus_point + _orbit_rotation() * Vector3(0.0, 0.0, distance)
	look_at(focus_point, Vector3.UP)

func _process(delta: float) -> void:
	_read_keyboard(delta)
	distance = lerp(distance, target_distance, 1.0 - exp(-8.0 * delta))
	var rotation_basis := _orbit_rotation()
	var desired_position := focus_point + rotation_basis * Vector3(0.0, 0.0, distance)
	position = position.lerp(desired_position, 1.0 - exp(-10.0 * delta))
	look_at(focus_point, Vector3.UP)
	save_clock += delta
	if save_clock >= 5.0:
		save_clock = 0.0
		save_exploration_state()

func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventMouseButton:
		if event.button_index == MOUSE_BUTTON_LEFT:
			dragging = event.pressed
			last_pointer = event.position
		elif event.button_index == MOUSE_BUTTON_WHEEL_UP:
			target_distance = clamp(target_distance * (1.0 - zoom_sensitivity), min_distance, max_distance)
		elif event.button_index == MOUSE_BUTTON_WHEEL_DOWN:
			target_distance = clamp(target_distance * (1.0 + zoom_sensitivity), min_distance, max_distance)
	elif event is InputEventMouseMotion:
		if dragging:
			_orbit_by(event.relative)
		elif Input.is_mouse_button_pressed(MOUSE_BUTTON_RIGHT):
			_pan_by(event.relative)
	elif event is InputEventScreenTouch:
		if event.pressed:
			active_touches[event.index] = event.position
			last_pointer = event.position
		else:
			active_touches.erase(event.index)
			if active_touches.size() < 2:
				last_pinch_distance = 0.0
	elif event is InputEventScreenDrag:
		active_touches[event.index] = event.position
		var points := _touch_points()
		if points.size() >= 2:
			var current_distance: float = points[0].distance_to(points[1])
			var center: Vector2 = (points[0] + points[1]) * 0.5
			if last_pinch_distance > 0.0:
				target_distance = clamp(target_distance * (1.0 - (current_distance - last_pinch_distance) * 0.002), min_distance, max_distance)
				_pan_by(center - last_touch_center)
			last_pinch_distance = current_distance
			last_touch_center = center
		else:
			_orbit_by(event.relative)

func _touch_points() -> Array[Vector2]:
	var result: Array[Vector2] = []
	for point in active_touches.values():
		result.append(point)
	return result

func _orbit_by(delta: Vector2) -> void:
	yaw -= delta.x * orbit_sensitivity * 0.12
	pitch = clamp(pitch - delta.y * orbit_sensitivity * 0.12, -85.0, 85.0)

func _pan_by(delta: Vector2) -> void:
	var scale := pan_sensitivity * maxf(distance, 2.0)
	var right := global_transform.basis.x
	var up := global_transform.basis.y
	focus_point += (-right * delta.x + up * delta.y) * scale

func _read_keyboard(delta: float) -> void:
	var direction := Vector3.ZERO
	if Input.is_key_pressed(KEY_W): direction -= global_transform.basis.z
	if Input.is_key_pressed(KEY_S): direction += global_transform.basis.z
	if Input.is_key_pressed(KEY_A): direction -= global_transform.basis.x
	if Input.is_key_pressed(KEY_D): direction += global_transform.basis.x
	if Input.is_key_pressed(KEY_E): direction += Vector3.UP
	if Input.is_key_pressed(KEY_Q): direction -= Vector3.UP
	if direction.length_squared() > 0.0:
		var speed := fly_speed * (4.0 if Input.is_key_pressed(KEY_SHIFT) else 1.0)
		focus_point += direction.normalized() * speed * delta

func _orbit_rotation() -> Quaternion:
	return Quaternion(Vector3.UP, deg_to_rad(yaw)) * Quaternion(Vector3.RIGHT, deg_to_rad(pitch))

func save_exploration_state() -> void:
	var config := ConfigFile.new()
	config.set_value("camera", "focus", focus_point)
	config.set_value("camera", "yaw", yaw)
	config.set_value("camera", "pitch", pitch)
	config.set_value("camera", "distance", target_distance)
	config.save(SAVE_PATH)

func load_exploration_state() -> void:
	var config := ConfigFile.new()
	if config.load(SAVE_PATH) != OK:
		return
	focus_point = config.get_value("camera", "focus", Vector3.ZERO)
	yaw = config.get_value("camera", "yaw", yaw)
	pitch = config.get_value("camera", "pitch", pitch)
	target_distance = clamp(float(config.get_value("camera", "distance", 34.0)), min_distance, max_distance)
	distance = target_distance

func _notification(what: int) -> void:
	if what == NOTIFICATION_APPLICATION_PAUSED or what == NOTIFICATION_WM_CLOSE_REQUEST:
		save_exploration_state()
