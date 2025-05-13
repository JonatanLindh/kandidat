@tool
extends GravityController

func _ready():
	HudSignalBus.connect("orbits_visibility", _on_orbit_visibility_change)
	HudSignalBus.emit_signal("query_orbits_visibility")

func _on_orbit_visibility_change(visible):
	show_trajectories_ingame = visible

@export var run_simulation: bool:
	set(val):
		self.simulate_trajectories()

@export var ClearTrajectories: bool:
	set(val):
		self.clear_trajectories()

var show_trajectories_ingame := false:
	set(enabled):
		show_trajectories_ingame = enabled
		if enabled:
			self.enable_trajectories()
		else:
			self.disable_trajectories();

var _dt := 0.
func _process(delta: float) -> void:
	if show_trajectories_ingame:
		self.poll_trajectory_results()
		self.queue_simulate_trajectories()
