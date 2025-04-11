Example Program: Buttons and LEDs

This folder contains an example application for the Winford Engineering
ETH32 ethernet I/O device.  

OVERVIEW:
This is a simple program that provides an example of how to 
connect to the ETH32 and use some of its features.  This program 
provides a simple example of how to use the event monitoring 
feature of the ETH32.

EXTERNAL HARDWARE:
This example uses two external pushbuttons that must be wired 
to the ETH32's I/O pins.  See the schematic.pdf file for details.  
Each pushbutton connects an I/O pin to ground when it is pressed.  
We enable the internal pullups on the pushbutton lines, so when 
the pushbuttons are not pressed, the lines float high, and when 
the pushbuttons are pressed, the lines are connected to ground.


OPERATION:
When you run the example, you must enter the IP address or host name 
of the ETH32 device you would like to connect to.  Once connected, 
the example application sets up event monitoring on the device to 
monitor the status of the external pushbuttons.  When a pushbutton 
is held down, the appropriate built-in LED on the ETH32 is lit and the 
application screen is updated to show the button is pressed.  When 
the pushbutton is released, the LED is turned off and the screen is 
updated to show the button has been released.

PROGRAMMING LANGUAGES:
This sample application is included in these programming languages:
  * Visual Basic 6
  * Visual Basic .NET
  * Visual C# .NET
  * C  (compiled binaries included for Windows and Linux)



Winford Engineering
www.winford.com