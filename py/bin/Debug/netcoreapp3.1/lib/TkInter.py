# Tcl/Tk WinForms binding api AP, (C) APR 2020

#WF = LIB("System.Windows.Forms")
py "WinForms dll System.Windows.Forms"
py "Form alias @WinForms->System.Windows.Forms.Form"

class Tk:
	def __init__(self):
		py "0 new @Form"
		py "f variable @Form %f"
		py "+ assign $self "

	def title(self, text):
		py "0 property $f %Text"
		py "+ assign $0 $text"

	def geometry(self, s):
		w, h = s.split('x')
		SET_PROP(self.root, "Width", int(w))
		SET_PROP(self.root, "Height", int(h))

	def mainloop(self):
		py "+ call $f *Form,ShowDialog"

class Label:
	def __init__(self):
		pass

class Button:
	def __init__(self):
		pass

class Entry:
	def __init__(self):
		pass

	def get(self):
		pass