tool
extends OptionButton

signal set_operator

func set_item(label, index):
	self.set_item_id(index, index)
	self.set_item_text(index, label)

func update():
	self.set_item("Greater than", 0)
	self.set_item("Less than", 1)
	self.set_item("Equals", 2)
	self.set_item("Greater or equals", 3)
	self.set_item("Less or equals", 4)

func _ready():
	for i in range(0,5):
		self.add_item("TEST", i);
	self.update();
	self.connect("item_selected", self, "item_selection")
	
	
func item_selection(selection):
	if selected == 0:
		self.emit_signal("set_operator", ">")
	elif selected == 1:
		self.emit_signal("set_operator", "<")
	elif selected == 2:
		self.emit_signal("set_operator", "=")
	elif selected == 3:
		self.emit_signal("set_operator", ">=")
	elif selected == 4:
		self.emit_signal("set_operator", "<=")
