# win10 OS
# AP (C) 2020

from TkInter import *

window = Tk()

window.title("Welcome to LikeGeeks")

window.geometry('350x200')

lbl = Label(window, text="Hello", font=("Arial Bold", 50))

lbl.grid(column=0, row=0)

txt = Entry(window, width=10)

txt.grid(column=1, row=0)

def clicked():
	res = "Welcome to " + txt.get()
	lbl.configure(text=res)

btn = Button(window, text="Click Me", bg="orange", fg="red", command=clicked)

btn.grid(column=2, row=0)

window.mainloop()