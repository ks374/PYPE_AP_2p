#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <sys/time.h>

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
OpenIrisClient* OpenIrisClient_init(const char* server_address, int port, double timeout) {
    //timeout in microsecond. 
    OpenIrisClient* client = (OpenIrisClient*)malloc(sizeof(OpenIrisClient));
    client->server_address.sin_family = AF_INET; //Use IPV4 address type. 
    client->server_address.sin_port = htons(port); //assign the port number. 
    client->server_address.sin_addr.s_addr = inet_addr(server_address); //assign the server address. 
    client->sock = socket(AF_INET, SOCK_DGRAM, 0);
    client->timeout = timeout;
    
    // Set socket timeout
    int timeout_usec = (int)round((timeout-(int)timeout)*1000000);
    int timeout_sec = (int)timeout;
    struct timeval tv;
    tv.tv_sec = timeout_sec;
    tv.tv_usec = timeout_usec;
    setsockopt(client->sock, SOL_SOCKET, SO_RCVTIMEO, (const char*)&tv, sizeof tv);
    
    return client;
}

// Function to send a request and receive raw data
static char* OpenIrisClient_fetch_data_raw(OpenIrisClient* client, int debug) {
    const char* message = "WAITFORDATA";
    sendto(client->sock, message, strlen(message), 0, (struct sockaddr*)&client->server_address, sizeof(client->server_address));
    
    char* data = (char*)malloc(MAX_BUFFER_SIZE);
    socklen_t len = sizeof(client->server_address);
    
    int bytes_received = recvfrom(client->sock, data, MAX_BUFFER_SIZE, 0, (struct sockaddr*)&client->server_address, &len);
    if (bytes_received < 0) {
        if (debug) {
            perror("Error receiving data");
        }
        free(data);
        return "{}";  // Return empty JSON object in case of error
    }
    
    data[bytes_received] = '\0';  // Null terminate the received string
    return data;
}

// Function to fetch data as a JSON object
static json_object* OpenIrisClient_fetch_data_json(OpenIrisClient* client, int debug) {
    char* raw_data = OpenIrisClient_fetch_data_raw(client, debug);
    json_object* json_data = json_tokener_parse(raw_data);
    free(raw_data);  // Free the raw data string after parsing
    return json_data;
}

/*
Now it's time to think about how to obtain the gaze position. 
In the original python version of this code, EyesData class was created once every time you fetch json data. It's not efficient in terms of memory as well as time. 

First, I have a json_update() function that send request to update a temperory EyesData pointer in openiris_client. 
The update function will write to data buffer, which is DACQ_INFO file, with gaze positions. DACQ_INFO should contain the pointer as well as some X,Y positions. 
I have one thread to update the data buffer. Either in real-time or when requested. 
I have another thread to fetch data from the buffer when required. The buffer will be locked during fetching. 
*/
EyeData* OpenIrisClient_Data_buffer_init(){
    EyesData* eyesdata = (EyesData*)malloc(sizeof(EyesData)); //Here is the EyesData Pointer Buffer. 
    if (eyesdata!=NULL) {
        return eyesdata;
    }else{
        printf("Memory allocation failed. No EyesData inited. \n");
    }
}
void OpenIrisClient_fetch_data(EyesData* eyesdata, OpenIrisClient* client, int debug) {
    Eyesdata_init(OpenIrisClient_fetch_data_json(client, debug),eyesdata);
}

// Function to close the client (similar to __exit__ in Python)
void OpenIrisClient_close(OpenIrisClient* client) {
    close(client->sock);
    free(client);  // Free the memory for the client
}
