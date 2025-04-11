#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <sys/time.h>
#include<math.h>

#include "Openiris_client.h"

#define MAX_BUFFER_SIZE 8192

/*
This clien is used by Openiris server. It will initialize the socket communication with the tracker PC, define eye data structure (in json format), 
and return desired eye data (such as the gaze position). 
So far, the calibration will be handled by pype utility. File saving will be handled by OpenIris PC. 

You also need the header file Openiris_client.h. 

11/19/2024 Chenghang Zhang
*/

// Function to initialize the OpenIrisClient
OpenIrisClient* OpenIrisClient_init(char* server_address, int port, double timeout) {
    //timeout in microsecond. 
    OpenIrisClient* client = (OpenIrisClient*)malloc(sizeof(OpenIrisClient));
	if (!client) {
        fprintf(stderr, "Memory allocation failed for Openirsclient\n");
        return NULL;
    }
    client->server_address.sin_family = AF_INET; //Use IPV4 address type. 
    client->server_address.sin_port = htons(port); //assign the port number. 
    if (inet_pton(AF_INET, server_address, &client->server_address.sin_addr) <= 0) {
        fprintf(stderr, "Invalid server address: %s\n", server_address);
        free(client);
        return NULL;
    }
    if ((client->sock = socket(AF_INET, SOCK_DGRAM, 0)) < 0) {
        perror("Socket creation failed");
        free(client);
        return NULL;
    }
    client->timeout = timeout;
    
    // Set socket timeout
    int timeout_usec = (int)round((timeout-(int)timeout)*1000000);
    int timeout_sec = (int)timeout;
    struct timeval tv;
    tv.tv_sec = timeout_sec;
    tv.tv_usec = timeout_usec;
    if (setsockopt(client->sock, SOL_SOCKET, SO_RCVTIMEO, &tv, sizeof(tv)) < 0) {
        perror("Failed to set socket timeout");
        close(client->sock);
        free(client);
        return NULL;
    }
	
	// Test connection by sending a handshake
	const char* handshake = "INIT";
    socklen_t addr_len = sizeof(client->server_address);
    if (sendto(client->sock, handshake, strlen(handshake), 0,
              (struct sockaddr*)&client->server_address, addr_len) < 0) {
        perror("Connection test failed");
        OpenIrisClient_close(client);
        return NULL;
    }
	/*
	// Verify server response
    char ack_buffer[16];
    int bytes_received = recvfrom(client->sock, ack_buffer, sizeof(ack_buffer), 0, NULL, NULL);
    if (bytes_received <= 0) {
        fprintf(stderr, "No response from server\n");
        OpenIrisClient_close(client);
        return NULL;
    }
	*/
    return client;
}

// Function to send a request and receive raw data
static char* OpenIrisClient_fetch_data_raw(OpenIrisClient* client, int debug) {
    //fprintf(stderr,"Openiris_server: calling fetch_data_raw. \n");
	//fprintf(stderr, "Openiris_server: fetch data IP address: %s, Port: %d\n", 
    //    inet_ntoa(client->server_address.sin_addr), ntohs(client->server_address.sin_port));
	
	const char* message = "WAITFORDATA";
	socklen_t len = sizeof(client->server_address);
    
	char* data = (char*)malloc(MAX_BUFFER_SIZE);
	if (!data) {
        if (debug) perror("Malloc failed");
        return NULL;
    }
	//fprintf(stderr,"OpenirisClient: Entering fetch data raw looping: \n");
    while (1){
		ssize_t sent_bytes = sendto(client->sock, message, strlen(message), 0, (struct sockaddr*)&client->server_address, sizeof(client->server_address));
		if (sent_bytes < 0){
			perror("Error sending message");
			continue;
		}
		int bytes_received = recvfrom(client->sock, data, MAX_BUFFER_SIZE, 0, (struct sockaddr*)&client->server_address, &len);
		//fprintf(stderr,"bytes_received = %d\n",bytes_received);
		if (bytes_received < 0) {
			fprintf(stderr,"No data. ");
			if (debug) {
				perror("Error receiving data");
			}
			continue;
		}
    //fprintf(stderr,"receiving actual data\n");
    data[bytes_received] = '\0';  // Null terminate the received string
    return data;
	}
}

// Function to fetch data as a JSON object
static json_object* OpenIrisClient_fetch_data_json(OpenIrisClient* client, int debug) {
    //fprintf(stderr,"Openiris_server: calling fetch_data json. \n");
	char* raw_data = OpenIrisClient_fetch_data_raw(client, debug);
    if (strcmp(raw_data, "{}") == 0){
        perror("Error receiving data");
        return NULL;
    } else {
        json_object* json_data = json_tokener_parse(raw_data);
        free(raw_data);  // Free the raw data string after parsing
        return json_data;
    } 
	return NULL;
}

/*
Now it's time to think about how to obtain the gaze position. 
In the original python version of this code, EyesData class was created once every time you fetch json data. It's not efficient in terms of memory as well as time. 

First, I have a json_update() function that send request to update a temperory EyesData pointer in openiris_client. 
The update function will write to data buffer, which is DACQ_INFO file, with gaze positions. DACQ_INFO should contain the pointer as well as some X,Y positions. 
I have one thread to update the data buffer. Either in real-time or when requested. 
I have another thread to fetch data from the buffer when required. The buffer will be locked during fetching. 
*/
EyesData* OpenIrisClient_Data_buffer_init(){
    EyesData* eyesdata = (EyesData*)malloc(sizeof(EyesData)); //Here is the EyesData Pointer Buffer. 
    if (eyesdata!=NULL) {
        return eyesdata;
    }else{
        printf("Memory allocation failed. No EyesData inited. \n");
		return NULL;
    }
}
void OpenIrisClient_fetch_data(EyesData* eyesdata, OpenIrisClient* client, int debug) {
	//fprintf(stderr,"Openiris_server: calling fetch_data. \n");
    EyesData_init(OpenIrisClient_fetch_data_json(client, debug),eyesdata);
}

// Function to close the client (similar to __exit__ in Python)
void OpenIrisClient_close(OpenIrisClient* client) {
    close(client->sock);
    free(client);  // Free the memory for the client
}
