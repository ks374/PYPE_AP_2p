/* title:   openiris_server.c

** author:  chenghang zhang
** created: Mon Nov 18 2003
** info:    pype interface with openiris eye tracker; modified from comedi_server by Mazer
** history:
** 
** I'm supposed to write an openiris server that contains a mainloop() to keep updating DACQINFO data. 
** When PYPE boots, it will call dacq_start() to create a subprocess of this server. The server will keep fetching data from the eyetracker (or analog input), 
** while other dacq functions fetch data from DACQINFO for the task file. 
** 
**
** For now, when initiated, DACQINFO will keep updating the following data: 
**  - A flag showing that the server is ON. 
**  - A flag showing that the calibration function is OFF. 
**  - Eye raw positions will be kept updated. These include C0 and C4 left/right positions as well as pupil area. 
**  - Calibrated X/Y positions will be updated if the flag is ON. 
**
** In the older version comedi_server.py, the server initiate a bunch of NI device as well as DACQINFO and eye tracker. 
** I'd assume it achieves DACQINFO updating using NI/eth32/eyetracker. 
** However, dacq.c will be called by the task file and a new DACQINFO will be initiated. Then dacq_server will also call the proper device to update DACQINFO. 
** So this part is possibly and very likey probably, almost absolutely, not necessary... 
** 
** However pype wants to call a server anyway, so I'll use this chance to test the connection to the hardware and release the memory afterward. 
*/

/*
Ther server should at least have the following function: 
init(): get ETH32 and Openiris client ready. It will also get a buffer for EyesData pointer, storing current EyesData. 
Shared memory will be claimed for DACQ_INFO to store all the handles. 
halt(): shutdown ETH32 connection and Openiris client. Clean the EyesData pointer buffer. 

*/

#include "openiris_server.h"

#include <sys/types.h>
#include <sys/ipc.h>
#include <sys/shm.h>
#include <stdio.h>

DACQINFO *dacq_data=NULL;

int init(){
  //First I need to claim the shared memory segment of DACQ_INFO. 
  printf("Testing hardware connections");
  int shmid;
  if ((shmid = shmget((key_t)SHMKEY,
		      sizeof(DACQINFO), 0666 | IPC_CREAT)) < 0) {
    perror2("shmget", __FILE__, __LINE__);
    fprintf(stderr, "openiris_client init -- kernel compiled with SHM/IPC?\n");
    exit(1);
  }else{
    dacq_data->shmid = shmid;
  }
  if ((dacq_data = (DACQINFO*)shmat(shmid, NULL, 0)) < 0) {
    perror2("shmat", __FILE__, __LINE__);
    fprintf(stderr, "openiris_client init -- kernel compiled with SHM/IPC?\n");
    exit(1);
  }
  printf("DACQ_INFO connection succeed.");
  atexit(halt(shmid));

  char* IP_address = getenv("EYETRACKER_ENV");
  if (IP_address==NULL){
    printf("Warning: Eyetracker address not found. Set it up in config file EYETRACKER_ENV");
    return(0);
  }else{
    dacq_data->client = OpenIrisClient_init(IP_address,9003,10.0);
    printf("Eyetracker connection okay... \n");
    dacq_data->eth32_handler = Eth32_init();
    printf("eth32 init okay...");
    if (Eth32_verify(dacq_data->eth32_handler)!=0){
      printf("However, eth32 writing failed... \n");
    }else{
      printf("eth32 test writing success. \n");
    }
    printf("eth32 connection okay...");
    server_shutdown(shmid);
    return(1);
  }  
}

static int halt(shmid){
  if (shmdt(dacq_data) == -1) {
    perror("shmdt");
    printf("Could not detach dacq_data from shared memory segment. \n");
    exit(1);
  }

  if (shmctl(shmid, IPC_RMID, NULL) == -1) {
    perror("shmctl");
    printf("Could not remove shared memory segment. \n");
    exit(1);
  }

  atexit(printf("Consider using ipcrm to remove dacq_data. \n"));
}

static int server_shutdown(shmid){
  OpenIrisClient_close(dacq_data->client);
  Eth32_shutdown(dacq_data->eth32_handler);
  halt(shmid);
}