# https://github.com/kaizenengine/kaizen/issues/43503

var test_var = null


func test():
	print(test_var.x)


func _init():
	test_var = Vector3()
