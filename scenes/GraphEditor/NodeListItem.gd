extends ToolButton


export(PackedScene) var graph_node = null;

func _pressed():
	var gn = graph_node.instance();
	var popup = $"../../../../../../../";
	popup.emit_node(gn);
	popup.hide();
