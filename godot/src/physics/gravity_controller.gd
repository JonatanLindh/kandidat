@tool
extends GravityController

@export var run_simulation: bool:
	set(val):
		self.simulate_trajectory()

@export var ClearTrajectories: bool:
	set(val):
		self.clear_trajectories()
