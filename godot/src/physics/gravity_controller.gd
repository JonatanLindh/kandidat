@tool
extends GravityController

@export var run_simulation: bool:
	set(val):
		self.simulate_trajectory()
