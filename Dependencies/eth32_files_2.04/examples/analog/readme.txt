Example Program: Analog Readings and Events

This folder contains an example application for the Winford Engineering
ETH32 ethernet I/O device.  

OVERVIEW:
This is a simple program that provides an example of how to 
connect to the ETH32 and use some of its features.  This program 
provides a simple example of how to use the analog channels of 
the device, including obtaining analog readings as well as 
configuring and receiving analog events.

EXTERNAL HARDWARE:
This example uses an external potentiometer that must be wired 
to the ETH32's pins.  See the schematic.pdf file for details.  
The potentiometer is used as a voltage divider with ground on 
one side and 5V (provided by the ETH32) on the other.  The 
potentiometer will allow you to alter the analog input voltage 
from 0V to 5V.

OPERATION:
When you run the example, you must enter the IP address or host name 
of the ETH32 device you would like to connect to.  Once connected, 
the program configures the Analog to Digital Convertor and configures 
and enables analog event monitoring on the analog signal.

Aside from the analog event, the program can periodically read the 
analog channel and visually update the screen to reflect the reading.
This is done by periodically polling the ETH32 device.  This polling 
may be disabled without affecting the analog event.

You can best see how analog events behave by enabling polling and 
adjusting the potentiometer to see at which points the events occur.
You can also adjust the lo-mark and hi-mark thresholds.


PROGRAMMING LANGUAGES:
This sample application is included in these programming languages:
  * Visual Basic 6
  * Visual C# .NET


Winford Engineering
www.winford.com