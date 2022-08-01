tool
extends OptionButton

signal set_operator

func set_item(label, index):
	self.set_item_id(index, index)
	self.set_item_text(index, label)

func update():
	self.set_item("Add", 0)
	self.set_item("Subtract", 1)
	self.set_item("Multiply", 2)
	self.set_item("Divide", 3)
	self.set_item("Exponent", 4)
	self.set_item("Log", 5)
	self.set_item("Abs", 6)
	self.set_item("Negate", 7)

func _ready():
	for i in range(0,5):
		self.add_item("TEST", i);
	self.update();
	self.connect("item_selected", self, "item_selection")
	
	
func item_selection(selection):
	self.emit_signal("set_operator", selected);
