tool
extends RichTextLabel

func _ready():
	var parent = get_parent();
	print("SETTING LABEL: ", parent.CommentText);
	parent.LabelReadyCB(self);
	text = parent.CommentText

