#!/bin/env pypenv
# -*- Mode: Python; tab-width: 4; py-indent-offset: 4; -*-

#
# run this by typing:
#   sh% pypenv ./ellipse.py
#  or something similar..
#

from sprite import *
#from numpy import *
from numpy import *

fb = quickinit(dpy=":0.0", w=200, h=200, bpp=32, flags=0)
fb.clear((128,128,128))
s = Sprite(200, 200, fb=fb, centerorigin=1)
s.fill((255,255,255))
s.ellipse(RED,200, 100, x=0, y=0)
s.blit()
fb.flip()
#d = 1.0 - ((hypot(s.ax, s.ay) - 50) / (50 - 100))
#alpha = clip(d, 0.0, 1.0)
#i = pygame.surfarray.pixels3d(s.im)
#alpha = transpose(array((alpha,alpha,alpha)), axes=[1,2,0])
#if _is_seq(bg):
#    bgi = zeros(i.shape)
#    bgi[:,:,0] = bg[0]
#    bgi[:,:,1] = bg[1]
#    bgi[:,:,2] = bg[2]
#else:
#    bgi = bg
#i[:] = ((alpha * i.astype(Float))+((1.0-alpha) * bgi)).astype(UnsignedInt8)
#s.alpha[:] = 255;
s.blit()
fb.flip()
