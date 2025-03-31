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

#include <sys/types.h>
#include <sys/time.h>
#include <sys/resource.h>
#include <sys/errno.h>
#include <unistd.h>
#include <stdio.h>
#include <stdlib.h>
#include <fcntl.h>
#include <sys/ioctl.h>
#include <string.h>
#include <signal.h>
#include <sys/ipc.h>
#include <sys/shm.h>
#include <sys/mman.h>

#include "openiris_server.h"

static char *progname = NULL;
static DACQINFO *dacq_data = NULL;
//static int mem_locked = 0;
// static open = NULL;
static int semid = -1;

/*Openiris data zone*/
OpenIrisClient *client = NULL;
 

static void openiris_read(double *x, double *y, int *pa){
    Eyesdata *eyesdata;
    OpenIrisClient_fetch_data(eyesdata, client, 0);
    Eyedata *temp;
    temp = eyesdata -> left; //you might want to change it to right if needed. 
    
    /*Variable to store current eyedata content*/
    
    x = (temp -> cr.x) - (temp -> p4.x);
    y = (temp -> cr.y) - (temp -> p4.y);
    pa = (int)(temp -> pupil_area);
    
    return(1);
}
static void halt(void){
    if (shmdt(dacq_data) == -1) {
        perror("shmdt");
        fprintf(stderr, "Failed to detach shared memory\n");
        exit(1);
    }
    OpenIrisClient_close(client);
}
static void init(char *dev,char *server_address, int port, double timeout){
  //First I need to claim the shared memory segment of DACQ_INFO. 
  printf("Testing hardware connections");
  int shmid;
  if ((shmid = shmget((key_t)SHMKEY,
		      sizeof(DACQINFO), 0666 | IPC_CREAT)) < 0) {
    perror("shmget");
    fprintf(stderr, "openiris_client init -- kernel compiled with SHM/IPC?\n");
    exit(1);
  }
  if ((dacq_data = shmat(shmid, NULL, 0)) == NULL) {
    perror("shmat");
    fprintf(stderr, "openiris_client init -- kernel compiled with SHM/IPC?\n");
    exit(1);
  }
  
  if ((semid = psem_init(SEMKEY)) < 0) {
    perror("psem_init");
    fprintf(stderr, "%s: can't init semaphore\n", progname);
    exit(1);
  }
  printf("DACQ_INFO connection succeed.");
  
  client = OpenIrisClient_init(server_address, port, timeout);
  
  fprintf(stderr, "openiris: attached to %s\n", dev);
  atexit(halt());
  catch_signals(progname); //Don't know what it does. May not need it. 

}
int main(int ac, char **av){
    char *p = rindex(av[0], '/');
    char *dev;
    
    if (p) {
        progname = p+1;
    } else {
        progname = av[0];
    }
    fprintf(stderr, "%s: started up\n", progname);
    
    if (ac < 2) {
        fprintf(stderr, "usage: %s serialdev\n", progname);
        exit(1);
    }
    dev=av[1];
    if (ac < 4) {
        fprintf(stderr,"not enought arguments passed to openiris_server\n");
    } else if (ac < 5) {
        double timeout = 10;
    } else {
        char *server_address = av[2];
        int port = av[3];
        double timeout = av[4];
    }
    init(dev,server_address,port,timeout);
    fprintf(stderr,"%s: entering aminloop\n",progname);
    mainloop(server_address,port,timeout);
    exit(0);
}

static void mainloop(char *server_address,int port,double timeout)
{
  double x,y;
  int i,pa;
  int lastpri, setpri;

  errno = 0;
  LOCK(semid);
  i = dacq_data->openiris_pri;
  UNLOCK(semid);
  if (setpriority(PRIO_PROCESS, 0, i) == 0 && errno == 0) {
    fprintf(stderr, "%s:init -- bumped priority %d\n", progname, i);
    lastpri = i;
    setpri = 1;
  } else {
    fprintf(stderr, "%s:init -- failed to change priority\n", progname);
    lastpri = 0;
    setpri = 0;
  }


  fprintf(stderr, "%s: about to set ready flag\n", progname);
  LOCK(semid);
  //dacq_data->iscan_x = dacq_data->iscan_x = x = y = 1;
  dacq_data->openiris_x = x = y = 1;  
  dacq_data->openiris_ready = 1;
  UNLOCK(semid);

  do {
    if (openiris_read(&x, &y, &pa)) {
      LOCK(semid);
      dacq_data->openiris_x = x;
      dacq_data->openiris_y = y;
      UNLOCK(semid);
    }
    
    /* possibly bump up or down priority on the fly */
    LOCK(semid);
    i = dacq_data->openiris_pri;
    UNLOCK(semid);
    if (setpri && lastpri != i) {
      lastpri = i;
      errno = 0;
      if (setpriority(PRIO_PROCESS, 0, i) == -1 && errno) {
	/* disable future priority changes */
	setpri = 0;
      }
    }
    LOCK(semid);
    i = dacq_data->iscan_terminate;
    UNLOCK(semid);
  } while (! i);

  if (dacq_data->iscan_terminate) {
    fprintf(stderr, "%s: exiting\n", progname);
  }
  dacq_data->openiris_ready = 0;
}