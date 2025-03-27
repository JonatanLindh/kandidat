@tool
extends GravityController

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
		
		_dt += delta
		if _dt > 2.:
			_dt = 0.;
			self.queue_simulate_trajectories()
