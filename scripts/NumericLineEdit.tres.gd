tool
extends LineEdit

signal change(number)

export var number = 0;
var regex = RegEx.new()

func _ready():
	self.text = str(number);
	regex.compile("^(-|)[0-9]+(\\.|)[0-9]*")
	connect("text_changed", self, "_on_text_changed")

func _on_text_changed(new_text):
	var results = regex.search_all(new_text);
	var new_str = "";
	var caret_pos = self.caret_position;
	for result in results:
		new_str += result.strings[0]
	self.text = new_str
	self.caret_position = caret_pos
	
	emit_signal("change", float(self.text));
	
