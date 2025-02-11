#ifndef ETH32_INTERFACE_H
#define ETH32_INTERFACE_H

#include <eth32.h>

eth32 Eth32_init();
//Initialize eth32 device and set the pins of PORT0-2 as input or output depending on environmental variables ETH32_PORT#_DIR

int Eth32_write_byte(eth32 handle, int port, int value);
//Write the value to a port. 

int Eth32_verify(eth32 handle);
//Return 0 if the connection is okay. 

int Eth32_shutdown(eth32 handle);
//Shutdown the device. 


#endif 