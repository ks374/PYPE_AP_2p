/* title:   dacq.c
** author:  Chenghang Zhang
** created: Fri Nov 22
** info:    task file connector to hardware
** history:
**
** File modified from old dacq.c
*/

#include <sys/types.h>
#include <sys/time.h>
#include <unistd.h>
#include <sys/resource.h>
#include <errno.h>
#include <stdio.h>
#include <stdlib.h>
#include <fcntl.h>
#include <sys/ioctl.h>
#include <string.h>
#include <signal.h>
#include <sys/ipc.h>
#include <sys/shm.h>
#include <sys/wait.h>
#include <sys/errno.h>
#include <math.h>
#include <sys/io.h>
#include <sched.h>
#include <sys/sem.h>

#include "dacq.h"

/* channels for X and Y eye positions */
//#define EYE_X 0
//#define EYE_Y 1

static DACQINFO *dacq_data = NULL; //This should be static. Don't even think about manipulating dacq_data directly. 
int semid = -1; //It will be broadcast as global. Majorly becasue you need to free the space later from task file. 
int shmid; //Same as above. Honestly for fixation task, we are not using any subprocess. I'm having it for future expansion potential. 
//Likely when we want to update the data, continuously write gaze position, and let the task file determin fixation at the same time. 
static EyesData *eyes_data;//This is static and defined here because we don't want to claim/destroy an EyesData everytime we do update. 

static unsigned long timestamp(int init)
{
  struct timeval now, delta;
  struct timezone tz;
  static struct timeval first;

  if (init) {
    gettimeofday(&first, &tz);

    return(0);
  } else {
    gettimeofday(&now, &tz);
    timersub(&now, &first, &delta);

    return((long)(1 + ((1000 * delta.tv_sec) + delta.tv_usec/1000)));
  }
}
static void dacq_sigchld_handler(int signum)
{
  int e, tflag;

  LOCK(semid);
  tflag = dacq_data->terminate;
  UNLOCK(semid);

  if ((e = kill(dacq_server_pid, 0)) < 0) {
    fprintf(stderr, "dacq: server probably gone root, not monitoring\n");
    return;			/* don't bother reseting handler.. */
  } else if (e == 0) {
    /* some non-dacq process exited, just ignore it.. */
    /* fprintf(stderr, "dacq: some other child died.\n"); */
  } else if (! tflag) {
    /* dacq process did exit, but it was unexpected */
    fprintf(stderr, "dacq: dacq_server (%d) died prematurely.\n",
	    dacq_server_pid);
    exit(1);
  } else {
    /* dacq process did exit, but it was supposed to!! */
  }
  /* reset signal handler */
  signal(SIGCHLD, dacq_sigchld_handler);
}

int dacq_start(int boot, int testmode, char *tracker_type,
	       char *dacq_server, char *arg1, char *arg2)
{
  /* 
  Init everything. Timestamp function not included so far. 
  Some variables are kept but not used, such as testmode. 
  */
  timestamp(1);
  fprintf(stderr, "dacq: sizeof dacqinfo ::%d\n", (int)sizeof(DACQINFO)); /*Changed 11/14*/

  if ((shmid =
       shmget((key_t)SHMKEY, sizeof(DACQINFO), 0666 | IPC_CREAT)) < 0) {
    perror("shmget");
    fprintf(stderr, "SHM error was %s\n", strerror(shmid));
    fprintf(stderr, "dacq_start: kernel compiled with SHM/IPC?\n");
    return(0);
  }
  if ((dacq_data = shmat(shmid, NULL, 0)) < 0) {
    perror("shmat");
    fprintf(stderr, "dacq_start: kernel compiled with SHM/IPC?\n");
    return(0);
  }
  if (semid = semget((key_t) SEMKEY,1,0666|IPC_CREAT)<0){
    error("semget of dacq_data");
    fprintf(stderr, "dacq_start: can't init semaphore\n");
    return(0);
  }else{
    if (semctl(semid, 0, SETVAL, 1) < 0) {
      perror("cannot set shmid value to 1.\n");
      return(-1);
    }
  }
  //Initialize dacq_data values
  if (boot){
    dacq_data->shmid = shmid;
    dacq_data->eye_x = 0.0;
    dacq_data->eye_y = 0.0;
    dacq_data->eye_pa = 0.0;
    dacq_data->juice_drop_num = 1;
    dacq_data->juice_drop_size = 1;
    if (strcmp(tracker_type, "OPENIRIS")==0){
      char* IP_address = getenv("EYETRACKER_ENV");
      if (IP_address==NULL){
        printf("Warning: Eyetracker address not found. Set it up in config file EYETRACKER_ENV");
        return(0);
      }else{
        dacq_data->client = OpenIrisClient_init(IP_address,9003,10.0);
        printf("Eyetracker connection okay... \n");
        dacq_data->eth32_handler = Eth32_init();
        printf("eth32 init okay...");
      }
    }else{
      //OpenIris can also use ANALOG input. But we'll skip this for now. 
      //Currently we will setup the software to run for just ethernet communication. 
      printf("Set the tracker_type to OPENIRIS! \n");
    }
  }
  return(1);
}

void dacq_stop(void){
  OpenIrisClient_close(dacq_data->client);
  Eth32_shutdown(dacq_data->eth32_handler);
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
int dacq_release(void)
{
  /* 
  Have no idea why there is a dacq_release and a dacq_stop. When you stop, you release. When you release, you are very likely stopping. 
  But keeping it in case pype want it. 
  */
 return(1);
}

/*-----------------------------------------------------------------------------------
The following functiosn are now void because we are no more concerned about digital/analog input/output. 
---------------------------------------------------------------------------------------*/
int dacq_dig_in(int n)
{
  // int i;

  // LOCK(semid);
  // i = dacq_data->din[n];
  // UNLOCK(semid);

  // return(i);
  return(1);
}
void dacq_dig_out(int n, int val)
{
  // int i;
  // /* wait for strobe and strobed word to be clear */
  // do {
  //   LOCK(semid);
  //   i = dacq_data->dout_strobe | dacq_data->dout_strword;
  //   UNLOCK(semid);
  //   usleep(100);
  // } while (i);

  // LOCK(semid);  
  // dacq_data->dout[n] = val ? 1 : 0;

  // /* signal server digital output pending */
  // dacq_data->dout_strobe = 1;
  // UNLOCK(semid);  
}
void dacq_str_word(int val)
{
  // int i;
  // int res_int;

  //   do {
  //   LOCK(semid);
  //   i = dacq_data->dout_strword | dacq_data->dout_strobe;
  //   UNLOCK(semid);
  //   usleep(100);
  // } while (i);

  // LOCK(semid);

  // //convert val to binary; fill the appropriate bits
  // //make sure strobe pin is low; below this is set to the
  // //the high pin on pORT C(see
  // //comedi-server.c. So in this setup pins 3-7 (port B) & 0-6 of port C
  // //input a 12 bit word to plexon and pin 7 of port C is strobe to plexon

  // dacq_data->doutc[7] = 0;
  // res_int = val;
  // for(i = 3; i < 8; i++){
  //   dacq_data->dout[i] = res_int%2;
  //   res_int = res_int/2;
  // }
  // // Now fill bits for Port C
  //  for(i = 0; i < 7; i++){
  //   dacq_data->doutc[i] = res_int%2;
  //   res_int = res_int/2;
  // }
  // //strobe word pending
  // dacq_data->dout_strword = 1;
  // UNLOCK(semid);
}

static void dacq_eyesdata_update(void){
  /*
  Update the eyes_data pointer. You need this everytime when you want to get eye_x or eye_y.
  */
 OpenIrisClient_fetch_data(eyes_data,dacq_data->client,0);
}
double dacq_eye_x(int left,int type)
{
  /*
  Instead of return int adc value, this value now returns Pupil poisiton or C4 position. 
  You need to specifiy the type as 0 or 4. 
  */
  if (type==0){
    return(eyes_data)
  }else if(type==4){

  }else{

  }
  double i = eyes_data
  int i;
  LOCK(semid);
  i = dacq_data->adc[EYE_X];
  UNLOCK(semid);
  return(i);
}

int dacq_eye_y(void)
{
  int i;
  LOCK(semid);
  i = dacq_data->adc[EYE_Y];
  UNLOCK(semid);
  return(i);
}

int dacq_eye_params(float xgain, float ygain,
		    int xoff, int yoff)
{
  /*
  Give calibration values to dacq_data. Don't understand why off values are int. Pype had it this way so I'll just use it. 
  */
  LOCK(semid);
  dacq_data->eye_xgain = xgain;
  dacq_data->eye_ygain = ygain;
  dacq_data->eye_xoff = xoff;
  dacq_data->eye_yoff = yoff;
  UNLOCK(semid);
  return(1);
}

int dacq_eye_read(int which)
{
  int i;

  LOCK(semid);
  if (which == 0) {
    i = dacq_data->eye_x;
  } else {
    i = dacq_data->eye_y;
  }
  UNLOCK(semid);
  return(i);
}

//void dacq_open_eyefile(char *name)
//{
//  open_data_file(name);
//}


//void dacq_close_eyefile()
//{
//  close_data_file();
//}


int dacq_ad_n(int n)
{
  int i;

  /* read the nth analog input.. */
  LOCK(semid);
  i = dacq_data->adc[n];
  UNLOCK(semid);
  return(i);
}

unsigned long dacq_ts(void)
{
  unsigned long i;
  /* If a dacq driver is loaded and running, get current das_server
     timestamp, otherwise, use the system clock and get it yourself..
     NOTE: it's in msec
  */
  LOCK(semid);
//  if (dacq_data->das_ready) {
//    i = dacq_data->timestamp;
//  } else {
    i = timestamp(0);
//  }
  UNLOCK(semid);
  return(i);
}

int dacq_bar(void)
{
  /* note: digital 1 (high) is pressed */
  if (dacq_dig_in(0)) {
    return(1);
  } else {
    return(0);
  }
}

int dacq_bar_genint(int b)
{
  int i;

  //LOCK(semid);
  i = dacq_data->din_intmask[0];
  dacq_data->din_intmask[0] = b;
  //LOCK(semid);
  return(i);
}

int dacq_bar_transitions(int reset)
{
  int i;
  LOCK(semid);
  i = dacq_data->din_changes[0];
  if (reset) {
    dacq_data->din_changes[0] = 0;
  }
  UNLOCK(semid);
  return(i);
}

int dacq_sw1(void)
{
  return (dacq_dig_in(1));
  //return (dacq_dig_in(3));
}

int dacq_sw2(void)
{
  return (dacq_dig_in(2));
  //return (dacq_dig_in(2));
}

void dacq_juice(int on, int dobeep, int dojuice) // dobeep is isolated secondary reinforcement
{
  if (on) {
    dacq_dig_out(1, dobeep);
    dacq_dig_out(0, dojuice);
  } else {
    dacq_dig_out(1, 0);
    dacq_dig_out(0, 0);
  }
}

int dacq_juice_drip(int ms, int dobeep, int dojuice) // dobeep is isolated secondary reinforcement
{
  dacq_juice(1, dobeep,dojuice);
  usleep(ms * 1000);
  dacq_juice(0, 0, 0);
  return(1);
}

int dacq_fixbreak_tau(int n)
{
  /*
   * set time period (in ms/sampling ticks) the eye must be outside
   * the fixation window before it counts as a break
   */
  LOCK(semid);
  dacq_data->fixbreak_tau = n;
  UNLOCK(semid);
  return(1);   /*Changed 11/14*/
}
  
int dacq_fixwin(int n, int cx, int cy, int radius, float vbias)
{
  if (n < 0) {
    return(NFIXWIN);
  } else if (n > NFIXWIN) {
    return(0);
  } else {
    LOCK(semid);
    if (radius > 0) {
      dacq_data->fixwin[n].active = 0;

      dacq_data->fixwin[n].xchn = EYE_X;
      dacq_data->fixwin[n].ychn = EYE_Y;
      dacq_data->fixwin[n].cx = cx;
      dacq_data->fixwin[n].cy = cy;
      dacq_data->fixwin[n].rad2 = (radius * radius);
      dacq_data->fixwin[n].vbias = vbias;
      dacq_data->fixwin[n].state = 0;
      dacq_data->fixwin[n].broke = 0;
      dacq_data->fixwin[n].genint = 0;
      dacq_data->fixwin[n].break_time = 0;
      dacq_data->fixwin[n].fcount = 0;
      dacq_data->fixwin[n].nout = 0;

      dacq_data->fixwin[n].active = 1;
    } else {
      dacq_data->fixwin[n].active = 0;
    }
    UNLOCK(semid);
    return(1);
  }
}

int dacq_fixwin_genint(int n, int b)
{
  int i = -1;
  if (n >= 0) {  
    LOCK(semid);
    i = dacq_data->fixwin[n].genint;
    if (b >= 0) {
        dacq_data->fixwin[n].genint = b;
    }
    UNLOCK(semid);
  }
  return(i);
}

int dacq_fixwin_reset(int n)
{
  if (n >= 0) {
    LOCK(semid);
    dacq_data->fixwin[n].active = 0;

    dacq_data->fixwin[n].state = 0;
    dacq_data->fixwin[n].broke = 0;
    dacq_data->fixwin[n].genint = 0;
    dacq_data->fixwin[n].break_time = 0;
    dacq_data->fixwin[n].fcount = 0;
    dacq_data->fixwin[n].nout = 0;

    dacq_data->fixwin[n].active = 1;
    UNLOCK(semid);
  }
  return(1);
}

int dacq_fixwin_state(int n)
{
  int s;

  /* if eye gets inside window, then reset broke flag */
  LOCK(semid);
  s = dacq_data->fixwin[n].state;
  if (s) {
    dacq_data->fixwin[n].broke = 0;
    // dacq_data->fixwin[n].genint = 0;
  }
  UNLOCK(semid);
  return(s);
}
  
int dacq_fixwin_broke(int n)
{
  int i;
  LOCK(semid);
  i = dacq_data->fixwin[n].broke;
  UNLOCK(semid);
  return(i);
}

long dacq_fixwin_break_time(int n)
{
  long i;

  LOCK(semid);
  i = dacq_data->fixwin[n].break_time;
  UNLOCK(semid);
  return(i);
}

int dacq_adbuf_toggle(int on)
{
  int i;

  LOCK(semid);
  dacq_data->adbuf_on = 0;
  UNLOCK(semid);
  if (on) {
    dacq_adbuf_clear();
    LOCK(semid);
    dacq_data->adbuf_on = 1;
    UNLOCK(semid);
    return(1);
  } else {
    LOCK(semid);
    i = dacq_data->adbuf_overflow;
    UNLOCK(semid);
    return(i);
  }
}

void dacq_adbuf_clear()
{
  int i;

  LOCK(semid);
  dacq_data->adbuf_on = 0;		/* turn off sampling */
  dacq_data->adbuf_ptr = 0;		/* reset pointer beginning */
  dacq_data->adbuf_overflow = 0;	/* reset overflow flag */
  for (i = 0; i < ADBUFLEN; i++) {	/* clear buffers. */
    dacq_data->adbuf_t[i] = 0;
    dacq_data->adbuf_x[i] = 0;
    dacq_data->adbuf_y[i] = 0;
    dacq_data->adbuf_c0[i] = 0;
    dacq_data->adbuf_c1[i] = 0;
    dacq_data->adbuf_photo[i] = 0;
    dacq_data->adbuf_spikes[i] = 0;
    dacq_data->adbuf_c4[i] = 0;
  }
  UNLOCK(semid);
}

int dacq_adbuf_size()
{
  int i;

  LOCK(semid);
  i = dacq_data->adbuf_ptr;
  UNLOCK(semid);
  return(i);
}

unsigned long dacq_adbuf_t(int ix)
{
  unsigned long i;

  LOCK(semid);
  i = dacq_data->adbuf_t[ix];
  UNLOCK(semid);
  return(i);
}

int dacq_adbuf_x(int ix)
{
  int i;

  LOCK(semid);
  i = dacq_data->adbuf_x[ix];
  UNLOCK(semid);
  return(i);
}

int dacq_adbuf_y(int ix)
{
  int i;

  LOCK(semid);
  i = dacq_data->adbuf_y[ix];
  UNLOCK(semid);
  return(i);
}

int dacq_adbuf_pa(int ix)
{
  int i;

  LOCK(semid);
  i = dacq_data->adbuf_pa[ix];
  UNLOCK(semid);
  return(i);
}

int dacq_adbuf_c0(int ix)
{
  int i;

  LOCK(semid);
  i = dacq_data->adbuf_c0[ix];
  UNLOCK(semid);
  return(i);
}

int dacq_adbuf_c1(int ix)
{
  int i;

  LOCK(semid);
  i = dacq_data->adbuf_c1[ix];
  UNLOCK(semid);
  return(i);
}

int dacq_adbuf_c2(int ix)
{
  int i;

  LOCK(semid);
  i = dacq_data->adbuf_photo[ix];
  UNLOCK(semid);
  return(i);
}

int dacq_adbuf_c3(int ix)
{
  int i;

  LOCK(semid);
  i = dacq_data->adbuf_spikes[ix];
  UNLOCK(semid);
  return(i);
}

int dacq_adbuf_c4(int ix)
{
  int i;

  LOCK(semid);
  i = dacq_data->adbuf_c4[ix];	/* 04-feb-2003: fixed, was _c1... */
  UNLOCK(semid);
  return(i);
}

int dacq_adbuf_photo(int ix) {return dacq_adbuf_c2(ix);}
int dacq_adbuf_spikes(int ix) {return dacq_adbuf_c3(ix);}


int dacq_eye_smooth(int kn)
{
  int i;

  LOCK(semid);
  dacq_data->eye_smooth = kn;
  i = dacq_data->eye_smooth;
  UNLOCK(semid);
  return(i);
}

void dacq_set_pri(int dacq_pri, int iscan_pri)
{
  LOCK(semid);
  dacq_data->dacq_pri = dacq_pri;
  dacq_data->iscan_pri = iscan_pri;
  UNLOCK(semid);
}


/***********************************************************************
 * Sun Dec 16 16:16:01 2001 mazer 
 *
 * Parallel Port I/O --> must be root to init!
 * 
 ***********************************************************************/


//#define BASE	0x3bc		/* /dev/lp0 */
#define BASE	0x378		/* /dev/lp1 or /dev/lp0 on ritalin*/
// #define BASE	0x278		/* /dev/lp2 */

static int _base = BASE;

int pp_init(int base)
{
  if (base == 0) {
    /* Should be: 0x3bc, 0x378 or 0x278
    ** Look at dmesg to figure this out.  For example, try:
    **    dmesg | grep ^parport
    ** under RedHat 7.2
    */
    _base = 0x378;		/* this works for ritalin.. */
  }
  fprintf(stderr, "pp_init: port 0x%3x\n", _base);
  if (iopl(3) != 0) {
    perror("iopl");
    return(0);
  } else {
    return(1);
  }
}

int pp_bar(void)		/* 1 is down, 0 is up */
{
  return(inb(_base + 1) & 0x80);
}


int pp_sw1(void)		/* 1 is down, 0 is up */
{
  return(inb(_base + 1) & 0x08);
}

int pp_sw2(void)		/* 1 is down, 0 is up */
{
  return(inb(_base + 1) & 0x10);
}

int pp_sw3(void)		/* 1 is down, 0 is up */
{
  return(inb(_base + 1) & 0x20);
}

void pp_juice(int on)
{
  unsigned char state;

  state = inb(_base + 0);
  if (on) {
    outb(state | 0x01, _base+0);
  } else {
    outb(state & ~0x01, _base+0);
  }
}

int pp_juice_drip(int ms)
{
  pp_juice(1);
  usleep(ms * 1000);
  pp_juice(0);
  return(1);
}

void pp_out(int x)
{
  outb((unsigned char) (0xff & x), _base + 0);
}


int dacq_seteuid(int uid)
{
  return(seteuid((uid_t) uid));
}

int dacq_set_rt(int rt)
{
  struct sched_param p;
  /* change scheduler priority from OTHER to RealTime/RR or vice versa */

  if (sched_getparam(0, &p) >= 0) {
    if (rt) {
      p.sched_priority = SCHED_RR;
      sched_setscheduler(0, SCHED_RR, &p);
    } else {
      p.sched_priority = SCHED_OTHER;
      sched_setscheduler(0, SCHED_OTHER, &p);
    }
    return(1);
  }
  return(0);
}

int dacq_set_mypri(int pri)
{
  uid_t old = geteuid();
  int result;

  if (seteuid((uid_t)0) == 0) {
    errno = 0;
    if (setpriority(PRIO_PROCESS, 0, pri) == 0 && errno == 0) {
      result = 1;
    } else {
      result = 0;
    }
    (void)seteuid(old); /*Changed 11/14*/
  } else {
    result = 0;
  }
  return(result);
}

int dacq_int_class(void)
{
  int i;
  LOCK(semid);
  i = dacq_data->int_class;
  UNLOCK(semid);
  return(i);
}

int dacq_int_arg(void)
{
  int i;
  LOCK(semid);
  i = dacq_data->int_arg;
  UNLOCK(semid);
  return(i);
}
