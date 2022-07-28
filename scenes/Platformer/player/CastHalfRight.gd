extends RayCast2D


# Declare member variables here. Examples:
# var a = 2
# var b = "text"


# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


var counter = 0
# Called every frame. 'delta' is the elapsed time since the previous frame.
func _physics_process(delta):
	counter = counter + 1
	if counter % 5 == 0:
		if is_colliding():
			#print("THING:", get_collision_point().distance_to(self.global_position))
			pass
