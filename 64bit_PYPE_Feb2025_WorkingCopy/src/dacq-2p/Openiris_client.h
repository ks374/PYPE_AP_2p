#ifndef OPENIRIS_CLIENT_H
#define OPENIRIS_CLIENT_H

#include <arpa/inet.h>
#include <json-c/json.h>

#include "Eye_data.h"

typedef struct {
    struct sockaddr_in server_address;
    int sock;
    int timeout;
} OpenIrisClient;

OpenIrisClient* OpenIrisClient_init(char* server_address, int port, double timeout);
EyeData* OpenIrisClient_Data_buffer_init();
void OpenIrisClient_fetch_data(EyesData* eyesdata, OpenIrisClient* client, int debug);
void OpenIrisClient_close(OpenIrisClient* client);


#endif