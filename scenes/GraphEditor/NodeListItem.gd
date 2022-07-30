extends ToolButton


export(PackedScene) var graph_node = null;
export(String) var call_method = null;
export(String) var call_method_string_arg = null;

func _pressed():
	var gn = graph_node.instance();
	
	if call_method != null:
		if call_method_string_arg != null:
			gn.call(call_method, call_method_string_arg)
		else:
			gn.call(call_method)
	
	var popup = $"../../../../../../../";
	popup.emit_node(gn);
	popup.hide();
	

