int get_EyesData(){
while (1) {
        EyesData* data = OpenIrisClient_fetch_data(client, 1);
        if (data != NULL) {
            // Process the data (for example, print the raw JSON data)
            printf("Received data: %s\n", json_object_to_json_string(data->json_data));
            free(data);  // Don't forget to free the data
        } else {
            break;
        }
        sleep(1);
    }
    
    OpenIrisClient_close(client);
    return 0;
}





int main() {
	const char *eye_ip = getenv("EYETRACKER_DEV");
	if (eye_ip!=NULL){
		printf("Fetching eye tracker at %s\n", eye_ip);
	}else{
		printf("Error: No eye tracker found. Setting EYETRACKER_DEV in the config file. ");
	}
    
	
	
	
	
	
	

    if (strlen(ip) == 0) {
        strcpy(ip, "localhost"); // Set default if input is empty
    }

    OpenIrisClient *client = OpenIrisClient_init(ip);
    if (client == NULL) {
        fprintf(stderr, "Failed to connect to OpenIris server\n");
        return 1;
    }

    while (1) {
        void *data = OpenIrisClient_fetch_data(client, 1); // Assuming fetch_data takes a boolean-like parameter
        if (data != NULL) {
            printf("Received data: %p\n", data); // Modify as needed to print actual data
        } else {
            break;
        }
        sleep(1); // Equivalent to time.sleep(1) in Python
    }

    OpenIrisClient_cleanup(client); // Assuming cleanup or close function is available
    return 0;
}