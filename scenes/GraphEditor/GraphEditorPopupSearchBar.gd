extends LineEdit

# Inseucre but whatever for this
var regex = RegEx.new();
# Declare member variables here. Examples:
# var a = 2
# var b = "text"

class Sorter:
	static func alpha(a,b):
		return a[0] < b[0]

# Called when the node enters the scene tree for the first time.
func _ready():
	# Sorting the listings
	var list = $"../ScrollContainer/NodeListContainer";
	var listings = []
	
	for listing in list.get_children():
		listings.append([listing.text, listing]);
	
	listings.sort_custom(Sorter, "alpha");
	
	for i in len(listings):
		list.move_child(listings[i][1], i)

func _search(text):
	regex.compile(text.to_lower());
	
	var listings = $"../ScrollContainer/NodeListContainer".get_children();
	
	for listing in listings:
		var results = regex.search_all(listing.text.to_lower())
		print("LISTING: ", (results.size() > 0), " | ", listing, " | ", listing.text, " | ", listing.text.to_lower(), " | ", results)
		if results.size() > 0:
			listing.visible = true
		else:
			listing.visible = false
			


func _on_GraphEditorPopup_shown():
	self.grab_focus();
