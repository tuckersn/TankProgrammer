extends PopupMenu

signal new_node(node, position)
signal shown()

func _input(event):
	if (event is InputEventMouseButton) and event.pressed:
		var evLocal = make_input_local(event)
		if !Rect2(Vector2(0,0),rect_size).has_point(evLocal.position):
			self.hide()

func _on_GraphEditorPopup_id_pressed(id):
	print("PRESS ", id)


func _on_GraphEditorPopup_id_focused(id):
	print("FOCUS ", id)

func emit_node(node):
	var pos = self.rect_global_position;
	emit_signal("new_node", node, pos);


func show_wrapper():
	self.show();
	emit_signal("shown");
