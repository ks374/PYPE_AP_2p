import sys, types,os
from pype import *
import pygame as pg

"""
	The purpose of this file is to train Book so that he can get used to sit in the booth and drink juice. 
	
	Read this file and you'll learn immediately how to write your own task. 
	
	You click a button on the keyboard and Book gets reward with defined number of drops and duration, instead of hitting
	the "reward" button where Book gets only 1 drop. 

	11/14/2024
	Chenghang
"""
def RunSet(app):
    app.taskObject.RunSet(app)

def cleanup(app):
    app.taskObject.cleanup(app)
    
def main(app):
    app.taskObject = fixationTask(app)
    app.globals = Holder()
    app.idlefb()
    app.startfn = RunSet
	



class fixationTask:

    def __init__(self,app):
        self.app = app

    def cleanup(self):
        #delete parameter table and anything else we created
        self.myTaskParams.destroy()

    def RunSet(self,app):
    	"""
	This is what is run when you hit the 'start' button (set as such in
	the 'main' function, defined at the end of this file).
	"""
	con(app,"Experiment start...\n")
	while True:
		UserInput = input("Num_drop = ")
		if UserInput > 0:
			for i in range(UserInput):
				app.reward(multiplier=5)
				app.idlefn(50)#time between juice drops
			UserInput = 0
		else:
			break;

