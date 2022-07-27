extends Control


# Declare member variables here. Examples:
# var a = 2
# var b = "text"


# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
#func _process(delta):
#	pass


func _on_GraphEdit_duplicate_nodes_request():
	print("DUPLICATE");


func _on_GraphEdit_copy_nodes_request():
	print("COPY");


func _on_GraphEdit_paste_nodes_request():
	print("PASTE");


func _on_GraphEdit_connection_from_empty(to, to_slot, release_position):
	print("FROM EMPTY: ", to, " | ", to_slot, " | ", release_position);


func _on_GraphEdit_connection_to_empty(from, from_slot, release_position):
	print("TO EMPTY: ", from, " | ", from_slot, " | ", release_position);


func _on_GraphEdit_connection_request(from, from_slot, to, to_slot):
	print("CONN REQ: ", from, " | ", from_slot, " | ", to, " | ", to_slot);
	print(typeof(to));
