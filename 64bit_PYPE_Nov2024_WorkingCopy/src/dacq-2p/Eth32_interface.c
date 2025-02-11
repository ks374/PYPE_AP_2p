/* title:   Eth32_interface.c

** author:  Mariana Varotto
** created: Tue Oct 20 9:42:15 2009 mvarotto 
** info:    interface to ETH32 Winford Ethernet I/O Device
** history:
**
**CHenghang 11/19/2024
**  These functions are modified to include a header file for better maintenance. 
*/


#define PORT0	0
#define PORT1	1
#define PORT2	2

#include <stdio.h>
#include <ctype.h>
#include "Eth32_interface.h"

eth32 Eth32_init()
{
	int eth32result, port_dir;
	char *ip_addr;
	char *port_spec;
	eth32 handle;

    ip_addr = getenv("ETH32_IP");
	if (ip_addr == NULL) {
  		fprintf(stderr, "ETH32 IP Address not specified. Add ETH32_IP to pype config file.\n");
		return (NULL);
	}
	fprintf(stderr, "Test, IP address is: %s\n", ip_addr);
	
        // Open a connection to the ETH32
	handle=eth32_open(ip_addr, ETH32_PORT, 5000, &eth32result);
	if(handle==0)
	{
  		fprintf(stderr, "Error connecting to ETH32: %s\n", eth32_error_string(eth32result));
		return(NULL);
	}

	// Configure ports 0 to 2
        //ETH32_PORT0_DIR: 0xFF
		//ETH32_PORT1_DIR: 0xFF
		//ETH32_PORT2_DIR: 0x00
		port_spec = getenv("ETH32_PORT0_DIR");
        port_dir = 0;
        sscanf(port_spec, "%X", &port_dir);	
	if (port_dir) 
	{
		eth32result=eth32_set_direction(handle, PORT0, port_dir);
		if(eth32result)
		{
			fprintf(stderr, "Error configuring ETH32 PORT0: %s\n", eth32_error_string(eth32result));
			return(NULL);
		}
        	fprintf(stderr, "Configured port 0 as: %d\n", port_dir);
	}
        port_spec = getenv("ETH32_PORT1_DIR");
        port_dir = 0;
        sscanf(port_spec, "%X", &port_dir);
	if (port_dir) 
	{
		eth32result=eth32_set_direction(handle, PORT1, port_dir);
		if(eth32result)
		{
			fprintf(stderr, "Error configuring ETH32 PORT1: %s\n", eth32_error_string(eth32result));
			return(NULL);
		}
       		fprintf(stderr, "Configured port 1 as: %d\n", port_dir);
	}
        port_spec = getenv("ETH32_PORT2_DIR");
        port_dir = 0;
        sscanf(port_spec, "%X", &port_dir);
	if (port_dir) 
	{
		eth32result=eth32_set_direction(handle, PORT2, port_dir);
		if(eth32result)
		{
			fprintf(stderr, "Error configuring ETH32 PORT2: %s\n", eth32_error_string(eth32result));
			return(NULL);
		}
      		fprintf(stderr, "Configured port 2 as: %d\n", port_dir);
	}
	fprintf(stderr, "Successfully connected to the ETH32 device. \n\n");
	return(handle);
}

int Eth32_write_byte(eth32 handle, int port, int value)
{

	int eth32result;

	eth32result=eth32_output_byte(handle, port, value);
	if(eth32result)
	{
		fprintf(stderr, "Error writing to port: %s\n", eth32_error_string(eth32result));
		return(1);
	}
//	else fprintf(stderr, "Writing %X to Eth32\n", value);
 	return (0);
}

int Eth32_verify(eth32 handle)
{
	eth32_verify_connection(handle);
 	return (0);
}

int Eth32_shutdown(eth32 handle)
{

	int eth32result;

	eth32result=eth32_close(handle);
	if(eth32result)
	{
		fprintf(stderr, "Error closing Eth32 connection: %s\n", eth32_error_string(eth32result));
		return(1);
	}
	else fprintf(stderr, "Successfully closed Eth32 connection\n");
 	return (0);
}

