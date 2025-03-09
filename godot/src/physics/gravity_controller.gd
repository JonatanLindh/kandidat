@tool
extends GravityController

@export var run_simulation: bool:
	set(val):
		self.simulate_trajectories()

@export var ClearTrajectories: bool:
	set(val):
		self.clear_trajectories()

var _t := 0.;
var show_trajectories_ingame := false :
	set(val):
		show_trajectories_ingame = val
		if !val:
			self.clear_trajectories()
		else:
			self.simulate_trajectories()
			_t = 0;

func _process(delta: float) -> void:
	_t += delta;
	#FIXME: Should NOT block main thread
	if _t >= 1  && show_trajectories_ingame:
		_t = 0.;
		self.simulate_trajectories()
