# PYPE_AP_2p
Visual stimulus control for awake monkey 2-photon imaging experiment

### History
Pype2: Original software was developed by Gallant lab at Berkeley. The original repo can be found [here](https://github.com/mazerj/pype2).  
Pype2_AP: The software was used in ShapeLab at UW. It was modified for specific hardware and experimental design. e.g. eye tracking hardware, recording system, network communication.  
Pype3: Gallant lab update the software for Python 3 support. The repo is [here](https://github.com/mazerj/pype3).  
Pype3_AP: Pype2_AP was also updated for python 3 support, but the development path diverge from PYPE3. 

### PYPE3_AP_2p: 
Starting with Pype3_AP, the software is modified for experiment using a homemade 2p microscope.  
It also supports a customized eye-tracking system based on Openris ([here](https://github.com/ocular-motor-lab/OpenIris) for the base software and [here](https://github.com/ryan-ressmeyer/OpenIrisDPI) for DPI plugin)

### NOV2024 Version: 
PYPE3_AP was successfully installed on a modern machine. Check the change log for installation guide. 

### Feb2025 Version: 
PYPE3_AP_2P was successfully installled on a modern machine. Openiris eye data can be successfully read. 

### Notes: 
1: The development of this software diverge from PYPE3_AP. We will try to merge them together. It's likely in the end, if you install dacq folder it will be PYPE3 using EyeLink/ISCAN/ANALOG, and dacq-2p is for Openiris. 
2: You need to open PYPE first and then it can correctly retrive data through Openiris through UPS. Reason unknown. It doesn't worth it to debug. 