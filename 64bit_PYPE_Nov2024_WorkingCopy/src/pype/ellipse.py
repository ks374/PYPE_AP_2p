#!/bin/env pypenv
# -*- Mode: Python; tab-width: 4; py-indent-offset: 4; -*-

#
# run this by typing:
#   sh% pypenv ./ellipse.py
#  or something similar..
#

from sprite import *
from numpy import *

fb = quickinit(dpy=":0.0", w=200, h=200, bpp=32, flags=0)
t = []
fb.clear((128,128,128))
t.append(Sprite(200, 200, fb=fb, centerorigin=1))
t[0].fill((255,255,255))
##these are vertices to draw a diamond -- use withs.polygon
verts1 = [[70,0],[0,40],[-70,0],[0,-40]]
verts0 = [[0,0],[40,0],[60,40],[20,40]]

## vertices to draw a star shaped stimulus that has verts
## based on an ellipse of sides h and w
## for this stimulus always use centerorigin = 1


h = 100 #height of encompassing rectangle
w = 100 #width
pixres = 1 #computed every pixel


#### let's try b8 stimuli
numstim = 2
nvrt =[14,18]                               #number of vertices for each object
Xvrt = zeros((numstim, max(nvrt)+3), Float) #this is from vertices2.txt
Yvrt = zeros((numstim, max(nvrt)+3), Float)

Xvrt[0,0:nvrt[0]+3] = asarray([-0.397,-0.533,-0.397,-0.176,0.0,0.122,0.468,0.988,1.6,0.988,0.468,0.122,0.0,-0.176,-0.397,-0.533,-0.397])*25.0
# these vertices were orginally made for RF dia of 4.0;
#also overlap vertices are directly input here. i.e. if there are n vertices,
#then vertices 1 and 2 are repeated after n and vertex n is repeated before 1
Yvrt[0,0:nvrt[0]+3] = asarray([-0.843,0.0,0.843,1.333,1.6,0.988,0.468,0.122,0.0,-0.122,-0.468,-0.988,-1.6,-1.333,-0.843,0.0,0.843])*25.0
Xvrt[1,0:nvrt[1]+3] = asarray([-0.397,-0.533,-0.397,-0.176,0.0,0.1,0.351,0.74,1.2,1.483,1.6,1.483,1.2,0.74,0.351,0.1,0.0,-0.176,-0.397,-0.533,-0.397])*25.0 
Yvrt[1,0:nvrt[1]+3] = asarray([-0.843,0.0,0.843,1.333,1.6,1.14,0.751,0.5,0.4,0.283,0.0,-0.283,-0.4,-0.5,-0.751,-1.14,-1.6,-1.333,-0.843,0.0,0.843])*25.0

i = 0

ip = arange(50)/50.0
incr = zeros((4,50), Float)
incr[0,:] = -ip*ip*ip+3*ip*ip-3*ip+1
incr[1,:] = 3*ip*ip*ip-6*ip*ip+4
incr[2,:] = -3*ip*ip*ip+3*ip*ip+3*ip+1
incr[3,:] = ip*ip*ip
Xarr = zeros((numstim,max(nvrt)*50), Float)
Yarr = zeros((numstim,max(nvrt)*50), Float)
xvrtid = zeros((4,50), Float)
yvrtid = zeros((4,50), Float)
while i < numstim:
    vtx = []
    vty = []
    j = 0
    while j < nvrt[i]:
        k=0
        while k < 4:
            xvrtid[k,:] = Xvrt[i,j+k]
            yvrtid[k,:] = Yvrt[i,j+k]
            k = k+1
        xb = list(sum(xvrtid*incr)/6.0)
        vtx.append(xb)
        yb = list(sum(yvrtid*incr)/6.0)
        vty.append(yb)
        j = j+1
    vtx1 = reshape(array(vtx),(nvrt[i]*50,))
    vty1 = reshape(array(vty),(nvrt[i]*50,))
    Xarr[i,0:nvrt[i]*50] = vtx1
    Yarr[i,0:nvrt[i]*50] = vty1
    i = i+1
pointslist1 = transpose(array([Xarr[1,:],Yarr[1,:]]))
#segment 1 - this is the x>0y>0 quadrant

def yvertcalc1 (x,):
    return (h/2.0)-(h/2.0)*sqrt(1-pow((x-(w/2.0))/(w/2.0),2))

x1 = arange(w/2,-1,-pixres,Float)#so that both ends are included
y = fromfunction(yvertcalc1,((w/2)+1,)) 
y1 = y[::-pixres]

#segment 4 is just the same with negative y in reverse order
x4 = arange(0,(w/2)+1,pixres,Float)
y4 =-y[::pixres]

#segment 2 - this is same as segment 1 in reverse order 
x2 = -arange(0,(w/2)+1,pixres,Float)
y2 = y[::pixres]

#segment 3 is the same with negative y in reverse order
x3 = -x1
y3 = -y[::-pixres]

x = concatenate((x1,x2,x3,x4),1)
y = concatenate((y1,y2,y3,y4),1)
pointslist = transpose(array([x,y]))
print(pointslist); 

#s.polygon(CYAN,pointslist,0)#star-shape
t[0].polygon(CYAN,pointslist1,0)

#s.ellipse(RED,200, 100, x=0, y=0) #draws an ellipse
#t[0].circlefill(RED,r=50, x=0, y=0, width=0)#filled circle
#s.arc(BLUE,150, 75,x=0, y=0, ang_st=0, ang_end=pi, width=1)#arc
#s.polygon(CYAN, verts0, 0)#diamond

t[0].blit()
fb.flip()
    
sys.stdout.write('>>'); sys.stdin.readline()


