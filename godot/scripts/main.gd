extends Node3D

const CAMERA_SCRIPT := preload("res://scripts/free_camera.gd")
const PLANETS := [
	{"name":"Mercury","radius":0.22,"orbit":4.0,"period":88.0,"color":Color(0.58,0.55,0.50),"fact":"The smallest planet; its surface is heavily cratered."},
	{"name":"Venus","radius":0.38,"orbit":6.0,"period":225.0,"color":Color(0.88,0.67,0.38),"fact":"A rocky world with a dense carbon-dioxide atmosphere."},
	{"name":"Earth","radius":0.40,"orbit":8.0,"period":365.25,"color":Color(0.20,0.48,0.92),"fact":"Our home world, with stable surface liquid water."},
	{"name":"Mars","radius":0.30,"orbit":10.5,"period":687.0,"color":Color(0.78,0.30,0.18),"fact":"A cold desert world with two small moons."},
	{"name":"Jupiter","radius":1.10,"orbit":15.0,"period":4332.6,"color":Color(0.78,0.61,0.43),"fact":"The largest planet, with powerful atmospheric storms."},
	{"name":"Saturn","radius":0.92,"orbit":20.0,"period":10759.0,"color":Color(0.82,0.75,0.57),"fact":"A gas giant surrounded by a broad ring system."},
	{"name":"Uranus","radius":0.62,"orbit":25.0,"period":30688.0,"color":Color(0.48,0.80,0.83),"fact":"An ice giant with an extreme axial tilt."},
	{"name":"Neptune","radius":0.60,"orbit":30.0,"period":60182.0,"color":Color(0.22,0.34,0.88),"fact":"A distant ice giant with very fast atmospheric winds."}
]

var bodies: Array[Dictionary] = []
var selected_name := "Sun"
var simulation_speed := 1.0
var paused := false
var hud: Label
var info_label: Label
var camera: Camera3D
var discoveries: Array[String] = []

func _ready() -> void:
	_build_lighting()
	_build_starfield()
	_build_sun()
	_build_planets()
	_build_camera()
	_build_hud()
	_load_discoveries()

func _process(delta: float) -> void:
	if not paused:
		for body in bodies:
			var pivot: Node3D = body["pivot"]
			pivot.rotate_y(TAU * delta * simulation_speed * 12.0 / float(body["period"]))
	hud.text = "GALAXY EXPLORER  |  %s discoveries  |  Speed %.1fx  |  %s" % [discoveries.size(), simulation_speed, "PAUSED" if paused else "RUNNING"]

func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventKey and event.pressed and not event.echo:
		match event.keycode:
			KEY_SPACE: paused = not paused
			KEY_EQUAL, KEY_KP_ADD: simulation_speed = minf(simulation_speed * 2.0, 128.0)
			KEY_MINUS, KEY_KP_SUBTRACT: simulation_speed = maxf(simulation_speed * 0.5, 0.125)
			KEY_R:
				simulation_speed = 1.0
				paused = false
	elif event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT:
		_select_from_screen(event.position)
	elif event is InputEventScreenTouch and event.pressed:
		_select_from_screen(event.position)

func _build_lighting() -> void:
	var light := DirectionalLight3D.new()
	light.name = "Sunlight"
	light.rotation_degrees = Vector3(-35.0, -25.0, 0.0)
	light.light_energy = 1.8
	light.shadow_enabled = true
	add_child(light)
	var environment := WorldEnvironment.new()
	var env := Environment.new()
	env.background_mode = Environment.BG_COLOR
	env.background_color = Color(0.004, 0.006, 0.02)
	env.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
	env.ambient_light_color = Color(0.10, 0.12, 0.20)
	env.ambient_light_energy = 0.35
	environment.environment = env
	add_child(environment)

func _build_starfield() -> void:
	var mesh := SphereMesh.new()
	mesh.radius = 0.055
	mesh.height = 0.11
	var material := StandardMaterial3D.new()
	material.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	material.albedo_color = Color(0.72, 0.82, 1.0)
	mesh.material = material
	var multi := MultiMesh.new()
	multi.transform_format = MultiMesh.TRANSFORM_3D
	multi.mesh = mesh
	multi.instance_count = 1400
	var rng := RandomNumberGenerator.new()
	rng.seed = 24112026
	for i in range(multi.instance_count):
		var p := Vector3(rng.randf_range(-180,180),rng.randf_range(-110,110),rng.randf_range(-180,180))
		if p.length() < 45.0:
			p = p.normalized() * 45.0
		multi.set_instance_transform(i, Transform3D(Basis.IDENTITY, p))
	var stars := MultiMeshInstance3D.new()
	stars.name = "Starfield"
	stars.multimesh = multi
	add_child(stars)

func _build_sun() -> void:
	var sun := MeshInstance3D.new()
	sun.name = "Sun"
	var sphere := SphereMesh.new()
	sphere.radius = 1.6
	sphere.height = 3.2
	sun.mesh = sphere
	var mat := StandardMaterial3D.new()
	mat.albedo_color = Color(1.0,0.55,0.12)
	mat.emission_enabled = true
	mat.emission = Color(1.0,0.28,0.04)
	mat.emission_energy_multiplier = 2.0
	sun.material_override = mat
	add_child(sun)
	_add_body_collision(sun, 1.6)
	sun.set_meta("fact", "The star at the centre of our Solar System.")
	bodies.append({"pivot":sun,"period":1.0,"name":"Sun"})

func _build_planets() -> void:
	for data in PLANETS:
		var pivot := Node3D.new()
		pivot.name = str(data.name) + "Orbit"
		add_child(pivot)
		_add_orbit_path(float(data.orbit))
		var planet := MeshInstance3D.new()
		planet.name = str(data.name)
		var sphere := SphereMesh.new()
		sphere.radius = float(data.radius)
		sphere.height = float(data.radius) * 2.0
		sphere.radial_segments = 24
		sphere.rings = 12
		planet.mesh = sphere
		planet.position = Vector3(float(data.orbit), 0.0, 0.0)
		var material := StandardMaterial3D.new()
		material.albedo_color = data.color
		material.roughness = 0.82
		planet.material_override = material
		pivot.add_child(planet)
		_add_body_collision(planet, float(data.radius))
		planet.set_meta("fact", data.fact)
		planet.set_meta("body_name", data.name)
		bodies.append({"pivot":pivot,"period":data.period,"name":data.name})
		if data.name == "Saturn":
			_add_saturn_rings(planet, float(data.radius))

func _add_orbit_path(radius: float) -> void:
	var ring := MeshInstance3D.new()
	var torus := TorusMesh.new()
	torus.inner_radius = radius - 0.012
	torus.outer_radius = radius + 0.012
	ring.mesh = torus
	ring.rotation_degrees.x = 90.0
	var mat := StandardMaterial3D.new()
	mat.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	mat.albedo_color = Color(0.16,0.25,0.42,0.55)
	mat.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
	ring.material_override = mat
	add_child(ring)

func _add_saturn_rings(parent: Node3D, radius: float) -> void:
	var ring := MeshInstance3D.new()
	var torus := TorusMesh.new()
	torus.inner_radius = radius * 1.35
	torus.outer_radius = radius * 2.15
	ring.mesh = torus
	ring.rotation_degrees.x = 90.0
	var mat := StandardMaterial3D.new()
	mat.albedo_color = Color(0.78,0.68,0.49,0.78)
	mat.transparency = BaseMaterial3D.TRANSPARENCY_ALPHA
	ring.material_override = mat
	parent.add_child(ring)

func _add_body_collision(body: Node3D, radius: float) -> void:
	var area := Area3D.new()
	area.name = "SelectionArea"
	var shape := CollisionShape3D.new()
	var sphere := SphereShape3D.new()
	sphere.radius = radius * 1.2
	shape.shape = sphere
	area.add_child(shape)
	body.add_child(area)

func _build_camera() -> void:
	camera = Camera3D.new()
	camera.name = "FreeExplorationCamera"
	camera.set_script(CAMERA_SCRIPT)
	camera.position = Vector3(0,18,36)
	camera.current = true
	camera.fov = 65.0
	add_child(camera)
	camera.look_at(Vector3.ZERO, Vector3.UP)

func _build_hud() -> void:
	var layer := CanvasLayer.new()
	add_child(layer)
	hud = Label.new()
	hud.position = Vector2(22,18)
	hud.add_theme_font_size_override("font_size",18)
	hud.add_theme_color_override("font_color",Color(0.62,0.85,1.0))
	layer.add_child(hud)
	info_label = Label.new()
	info_label.position = Vector2(22,58)
	info_label.size = Vector2(560,110)
	info_label.add_theme_font_size_override("font_size",16)
	info_label.add_theme_color_override("font_color",Color(0.9,0.93,1.0))
	info_label.text = "Tap a planet to inspect it. Drag to look around; pinch to zoom."
	layer.add_child(info_label)

func _select_from_screen(screen_position: Vector2) -> void:
	var origin := camera.project_ray_origin(screen_position)
	var direction := camera.project_ray_normal(screen_position)
	var query := PhysicsRayQueryParameters3D.create(origin, origin + direction * 500.0)
	var result := get_world_3d().direct_space_state.intersect_ray(query)
	if result.is_empty():
		return
	var node: Node = result.collider
	while node != null and not node.has_meta("body_name") and node.name != "Sun":
		node = node.get_parent()
	if node == null:
		return
	selected_name = str(node.get_meta("body_name", "Sun"))
	var fact := str(node.get_meta("fact", "The star at the centre of our Solar System."))
	info_label.text = "%s\n%s" % [selected_name, fact]
	if not discoveries.has(selected_name):
		discoveries.append(selected_name)
		_save_discoveries()

func _save_discoveries() -> void:
	var config := ConfigFile.new()
	config.set_value("progress", "discoveries", discoveries)
	config.save("user://discoveries.cfg")

func _load_discoveries() -> void:
	var config := ConfigFile.new()
	if config.load("user://discoveries.cfg") == OK:
		discoveries = config.get_value("progress", "discoveries", [])
