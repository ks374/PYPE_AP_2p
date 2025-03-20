/* title:   das_common.c

** author:  jamie mazer
** created: Mon Mar  4 16:41:26 2002 mazer 
** info:    dasXX_server.c common functions
** history:
**
** Thu Apr  4 14:06:25 2002 mazer 
**   - changed calls to setpriority to also bump scheduler priority up to
**	realtime (SCHED_RR)
**
** Fri Aug 23 16:53:54 2002 mazer 
**   - Modified timestamp() to use the RDTSC for speed.  At
**     1 gHz, the 8byte (64bit) counter would overflow in:
**       (2^64) / (1e9) secs = 1.8e10s, or more than 500 years..
**     so, I'm assuming overflow is NOT a problem right now..
**
** Thu Dec 19 14:03:32 2002 mazer 
**   added EYELINK_TEST mode
**
** Wed Apr 16 10:41:16 2003 mazer 
**   added parsing of $XXEYELINK_OPTS to allow setting of eyelink
**   parameters in the pyperc config file...
**
** Sun Nov  6 10:06:36 2005 mazer 
**   added $EYELINK_FILE to save native EDF file during run.
**
** Tue Jan 17 11:37:56 2006 mazer 
**   - added $(CWD)/eyelink.ini file --> supplemental commands for the
**     eyelink
**   - made sure stderr messages all contain progname..
**
** Mon Jan 23 10:01:22 2006 mazer 
**   Added handling of FIXWIN.vbias for vertical elongation of the
**   fixation window.
**
** Fri Aug-18-2006 Anitha
**   Added option to use eyelink calibrated gaze output instead of the
**   raw signal. Set EYELINK_OUT to CALIBRATED in Config file for this
**
** Thu 24 Aug 2006 Anitha
**   Added output of strobed words directly in mainloop
**
** Mon 17 Mar 2025 Chenghang 
**   Modified for Openiris. Based on Taekjun's version 2019-2023. 
*/

#include <unistd.h>
#include <sys/time.h>
#include <sys/errno.h>
#include <sys/resource.h>
#include <sys/types.h>
#include <signal.h>
#include <sched.h>
#include <string.h>

#include "das_common.h"

#include "psems.h"
#ifdef CHANGE_NAME
# include "procname.h"
#endif

/* these are eyelink API header files */
//#include <eyelink.h> // edited Feb 2023, Taekjun
//#include <eyetypes.h> // edited Feb 2023, Taekjun

/* this has set_eyelink_address() etc.. */
//#include <core_expt.h> // edited May 2019, Taekjun

static char *_tmodes[] = { "ANALOG", "ISCAN", "EYELINK", "EYELINK_TEST", "Openiris" };
#define ANALOG		0
#define ISCAN		1
#define EYELINK		2
#define EYELINK_TEST	3
#define OPENIRIS 4

#define INSIDE		1
#define OUTSIDE		0


static int tracker_mode = ANALOG; //Default tracker mode. 
static int semid = -1;
static unsigned long long ticks_per_ms = 0;
static int eyelink_camera = -1;
static int swap_xy = 0;

static double find_clockfreq()	/* get clock frequency in Hz. Should be public method */
{
  FILE *fp;
  char buf[100];
  double khz;

  /* Original code
  ** if ((fp = fopen("/sys/devices/system/cpu/cpu0/cpufreq/cpuinfo_max_freq", "r")) == NULL) {
  **   fprintf(stderr, "%s: Can't open /sys/devices/system/cpu/cpu0/cpufreq/cpuinfo_max_freq\n", progname);
  **   exit(1);
  ** }
  ** khz = -1.0;
  ** fgets(buf, sizeof(buf), fp);
  ** sscanf(buf, "%lf", &khz);
  ** return(khz * 1.0e3);
  */

  // revised for a newer ubuntu machine (Aug 2023, Taekjun)
  if ((fp = fopen("/proc/cpuinfo", "r")) == NULL) {
      fprintf(stderr, "Can't open /proc/cpuinfo\n");
      exit(1); 
  }
  khz = -1.0; 
  while (fgets(buf, sizeof(buf), fp)) {
      if (sscanf(buf, "cpu MHz : %lf", &khz) == 1) {
          khz *= 1.0e3; // Convert MHz to khz
      }
  }
  fclose(fp); 
  return(khz * 1.0e3); 
}

static void iscan_init(char *server, char *dev) /*Kept same for Openiris version*/
{
  int pid, k;

  LOCK(semid);
  dacq_data->iscan_terminate = 0;
  dacq_data->iscan_ready = 0;
  UNLOCK(semid);
  if ((pid = fork()) == 0) {
    execlp(server, server, dev, NULL);
  } else {
    fprintf(stderr, "%s: waiting for iscan_ready\n", progname);
    do {
      LOCK(semid);
      k = dacq_data->iscan_ready;
      UNLOCK(semid);
      usleep(100);
    } while (! k);
  }
  tracker_mode = ISCAN;
}

static void iscan_halt() /*Kept same for Openiris version*/
{
  int k;

  fprintf(stderr, "%s: in iscan_halt\n", progname);

  if (tracker_mode == ISCAN) {
    fprintf(stderr, "%s: terminating iscan\n", progname);
    LOCK(semid);
    dacq_data->iscan_terminate = 1;
    UNLOCK(semid);
    do { 
      LOCK(semid);
      k = dacq_data->iscan_ready;
      UNLOCK(semid);
      usleep(100);
    } while (k);
    tracker_mode = ANALOG;
  }
  fprintf(stderr, "%s: leaving iscan_halt\n", progname);
}

static void eyelink_init(char *ip_address) /*Kept same for Openiris version*/
{
  char *p, *q, *opts, buf[100];
  extern char *__progname;
  char *saved;
  FILE *fp;
  
  tracker_mode = OPENIRIS;

  fprintf(stderr, "%s/eyelink_init: trying %s\n", progname, ip_address);

  saved = malloc(strlen(__progname) + 1);
  strcpy(saved, __progname);
#ifdef CHANGE_NAME
  set_proc_title("eyelink_thread");
#endif

  //begin_realtime_mode();
  set_eyelink_address(ip_address);
  
  if (open_eyelink_connection(0)) {
    fprintf(stderr, "\n%s/eyelink_init: can't open connection to tracker\n",
	    progname);
    return;
  }
  set_offline_mode();

  /* 16-apr-2003: step through the XXEYELINK_OPTS env var (commands
   * separated by :'s) and set each command to the eyelink, while
   * echoing to the console..  This variable is setup by pype before
   * dacq_start() gets called..
   */
  opts = getenv("XXEYELINK_OPTS");
  for (q = p = opts; *p; p++) {
    if (*p == ':') {
      *p = 0;
      eyecmd_printf(q);
      fprintf(stderr, "%s: eyelink_opt=<%s>\n", progname, q);
      *p = ':';
      q = p + 1;
    }
  }

  /* this should be "0" or "1", default to 1 */
  p = getenv("XXEYELINK_CAMERA");
  if (p == NULL || sscanf(p, "%d", &eyelink_camera) != 1) {
    eyelink_camera = 1;
  }
  sprintf(buf, "eyelink_camera = %d", eyelink_camera);
  fprintf(stderr, "%s: %s\n", progname, buf);
  eyecmd_printf(buf);

  /* Tue Jan 17 11:37:48 2006 mazer 
   * if file "eyelink.ini" exists in the current directory, send
   * it as a series of commands to the eyelink over the network.
   */

  if ((fp = fopen("eyelink.ini", "r")) != NULL) {
    while (fgets(buf, sizeof(buf), fp) != NULL) {
      if ((p = index(buf, '\n')) != NULL) {
	*p = 0;
      }
      fprintf(stderr, "%s: %s\n", progname, buf);
      eyecmd_printf(buf);
    }
  }

  // start recording & tell EL to send samples but
  // not events through link
  p = getenv("EYELINK_FILE");
  if (p != NULL) {
    open_data_file(p);
    if (start_recording(1,1,1,0)) {
      fprintf(stderr, "%s/eyelink_init: can't start recording\n", progname);
      return;
    }
    fprintf(stderr, "%s/eyelink_init: saving data to '%s'\n", progname, p);
  } else {
    if (start_recording(0,0,1,0)) {
      fprintf(stderr, "%s/eyelink_init: can't start recording\n", progname);
      return;
    }
  }

  if (eyelink_wait_for_block_start(10,1,0)==0) {
    fprintf(stderr, "%s/eyelink_init: can't get block start\n", progname);
    return;
  }

  fprintf(stderr, "%s/eyelink_init: connected ok\n", progname);
  tracker_mode = EYELINK;
#ifdef CHANGE_NAME
  set_proc_title(saved);
#endif
}
static void eyelink_halt() /*Kept same for Openiris version*/
{
  char *p;

  if (tracker_mode == EYELINK || tracker_mode == EYELINK_TEST) {
    stop_recording();
    set_offline_mode();
    tracker_mode = ANALOG;

    p = getenv("EYELINK_FILE");
    if (p != NULL) {
      pump_delay(500);
      eyecmd_printf("close_data_file");
      fprintf(stderr, "%s/eyelink_halt: requesting '%s'\n", progname, p);
      if (receive_data_file(p, p, 0) > 1) {
	fprintf(stderr, "%s/eyelink_halt: received.\n", progname);
      } else {
	fprintf(stderr, "%s/eyelink_halt: error receiving.\n", progname);
      }
    }
  }
}

/*Openiris init and halt are based on eyelink functions. */
static void openiris_init(char *ip_address,int port, double timeout) 
{
  //char *p, *q, *opts, buf[100];
  extern char *__progname;
  char *saved;
  int pid;
  char *server
  //FILE *fp;
  
  fprintf(stderr, "%s/openiris_init: trying %s\n", progname, ip_address);

  saved = malloc(strlen(__progname) + 1);
  strcpy(saved, __progname);
#ifdef CHANGE_NAME
  set_proc_title("openiris_thread");
#endif

  //begin_realtime_mode();
  //set_eyelink_address(ip_address);
  
  /*Attemp to connect to Openiris client. */
  /*Establish the client and return the pointer to the client. */

	OpenIrisClient* client = OpenIrisClient_init(ip_address, port, timeout);
	if (!client) {
		fprintf(stderr, "%s/openiris_init: failed %s\n", progname, ip_address);
		return NULL;
	}
    LOCK(semid);
      dacq_data->openiris_client = client;
    UNLOCK(semid);
    server = "openiris_server"
    if ((pid = fork()) == 0) {
        execlp(server, server, NULL);
  } else {
    fprintf(stderr, "%s: waiting for iscan_ready\n", progname);
    do {
      LOCK(semid);
      k = dacq_data->openiris_ready;
      UNLOCK(semid);
      usleep(100);
    } while (! k);
  }
  tracker_mode = OPENIRIS;
  
  
#ifdef CHANGE_NAME
  set_proc_title(saved);
#endif
}
static void openiris_halt(OpenIrisClient* client) /*Kept same for Openiris version*/
{
	OpenIrisClient_close(OpenIrisClient* client);
    LOCK(semid);
    dacq_data->openiris_ready = 0;
    UNLOCK(semid);
	fprintf(stdout,"Openiris client shut down");
}
static int eyelink_read(float *x, float *y,  float *p,
			unsigned int *t, int *new)
{
  static FSAMPLE sbuf;
  int e;
  char *calib_val;

  if ((e = eyelink_newest_float_sample(&sbuf)) < 0) {
    return(0);
  } else {
#ifdef DEBUGGING
    fprintf(stderr, "ti = %d\n.", sbuf.time);
    fprintf(stderr, " px = [%f %f]\n", sbuf.px[0], sbuf.px[eyelink_camera]);
    fprintf(stderr, " py = [%f %f]\n", sbuf.py[0], sbuf.py[eyelink_camera]);
    fprintf(stderr, " pa = [%f %f]\n", sbuf.pa[0], sbuf.pa[eyelink_camera]);
#endif /* DEBUGGING */
    // if Eyelink output is calibrated use gaze, if not use raw
    calib_val = getenv("EYE_OUT");
    if (strcmp(calib_val,"CALIBRATED") == 0) {
       *t = (unsigned int) sbuf.time;
       *x = sbuf.gx[eyelink_camera];		/* xpos, RIGHT/LEFT */
       *y = sbuf.gy[eyelink_camera];		/* ypos, RIGHT/LEFT */
       *p = sbuf.pa[eyelink_camera];		/* pupil area, RIGHT/LEFT */
    } else {
    /* there are new data about eye positions */
       *t = (unsigned int) sbuf.time;
       *x = sbuf.px[eyelink_camera];		/* xpos, RIGHT/LEFT */
       *y = sbuf.py[eyelink_camera];		/* ypos, RIGHT/LEFT */
       *p = sbuf.pa[eyelink_camera];		/* pupil area, RIGHT/LEFT */
    }
    *new = (e == 1);
    return(1);
  }
}


#define RDTSC(x) __asm__ __volatile__ ( ".byte 0x0f,0x31" \
					:"=a" (((unsigned long*)&x)[0]), \
					 "=d" (((unsigned long*)&x)[1]))

static unsigned long timestamp(int init)
{
  static unsigned long long timezero;
  unsigned long long now;

  RDTSC(now);			/* get cycle counter from hardwareTSC */
  if (init) {
    timezero = now;
    return(0);
  } else {
    /* use precalibrated ticks_per_ms to convert to real time.. */
    return((unsigned long)((now - timezero) / ticks_per_ms));
  }
}

static void perror2(char *s, char *file, int line)
{
  char *p = (char *)malloc(strlen(progname)+strlen(s)+25);

  sprintf(p, "%s (file=%s, line=%d):%s", progname, file, line, s);
  perror(p);
  free(p);
}

static void resched(int rt)
{
#ifdef ALLOW_RESCHED
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
  }
#endif
}

static void mainloop(void)
{
  register int i, lastpri, setpri;
  register float x, y, z, pa, tmp, calx, caly;
  float tx, ty, tp;
  unsigned long last_ts = 0, ts;
  unsigned int eyelink_t;
  int eyelink_new;
  int k;
  long accum[NADC], naccum;

  register float sx=0, sy=0;
  int si, sn;
  float sbx[MAXSMOOTH], sby[MAXSMOOTH];

  /*
   * calx/caly are the gain+offset adjusted eye position values
   * x/y are the raw values
   */
  calx = caly = x = y = pa = -1;

  for (si = 0; si < MAXSMOOTH; si++) {
    sbx[si] = sby[si] = 0.0;
  }
  si = 0;

  errno = 0;
  LOCK(semid);
  k = dacq_data->dacq_pri;
  UNLOCK(semid);

  if (setpriority(PRIO_PROCESS, 0, k) == 0 && errno == 0) {
    fprintf(stderr, "%s: bumped priority %d\n", progname, k);
    lastpri = k;
    if (lastpri < 0) {
      resched(1);
    }
    setpri = 1;
  } else {
    fprintf(stderr, "%s: failed to change priority\n", progname);
    setpri = 0;
    lastpri = 0;
  }

  timestamp(1);			/* initialize the timestamp to 0 */

  fprintf(stderr, "%s: tracker_mode=%s (%d)\n", progname,
	  _tmodes[tracker_mode], tracker_mode);

  /* signal client we're ready */
  LOCK(semid);
  dacq_data->das_ready = 1;
  fprintf(stderr, "%s: ready\n", progname);
  UNLOCK(semid);

  do {
    /* sample converters as fast as possible and accumulate
     * into temp buffer for averaging at the end of the sample
     * period (1ms).  This replaces spin-locking code.
     */
    naccum = 0;
    do {
      /* sample the converters & acculumate values */
      for (i = 0; i < NADC; i++) {
	if (naccum == 0) {
	  accum[i] = ad_in(i);
	} else {
	  accum[i] += ad_in(i);
	}
      }
      naccum += 1;
    } while (((ts = timestamp(0)) - last_ts) < 1);
    last_ts = ts;

    /* adjust for # of acculumated values */
    for (i = 0; i < NADC; i++) {
      LOCK(semid);
      dacq_data->adc[i] = (int)(accum[i] / naccum);
      UNLOCK(semid);
    }

    if (tracker_mode == ISCAN) {
      LOCK(semid);
      x = dacq_data->iscan_x;
      y = dacq_data->iscan_y;
      UNLOCK(semid);
      pa = -1;
    } else if (tracker_mode == EYELINK) {
      /* try reading from eyelink */
      if (eyelink_read(&tx, &ty, &tp, &eyelink_t, &eyelink_new) != 0) {
	x = tx;
	y = ty;
	pa = tp;
      }
    } else if (tracker_mode == EYELINK_TEST) {
      /* try reading from eyelink */
      if (eyelink_read(&tx, &ty, &tp, &eyelink_t, &eyelink_new) == 0) {
	tx = ty = tp = -1;
      }
      LOCK(semid);
      x = dacq_data->adc[0];
      y = dacq_data->adc[1];
      UNLOCK(semid);
    } else {
      x = dacq_data->adc[0];
      y = dacq_data->adc[1];
      pa = -1;
    }

    if (swap_xy) {
      tmp = x; x = y; y = tmp;
    }

    /* smooth (if necessary) raw eye position trace */
    LOCK(semid);
    sn = dacq_data->eye_smooth;
    if (sn > MAXSMOOTH) {
      sn = MAXSMOOTH;
    }
    UNLOCK(semid);

    if (sn > 1) {
      /* remove old point, add new point to smoothing sum */
      sx = sx - sbx[si] + x;
      sy = sy - sby[si] + y;

      /* add new (unsmoothed data points) to smoothing buffer */
      sbx[si] = x;
      sby[si] = y;
      si = (si + 1) % sn;

      /* calc smoothed point */
      x = sx / sn;
      y = sy / sn;
    }

    /* convert from raw to pixel domain and save in eye_x/eye_y */
    LOCK(semid);
    calx = (dacq_data->eye_xgain * x) - dacq_data->eye_xoff;
    caly = (dacq_data->eye_ygain * y) - dacq_data->eye_yoff;
    dacq_data->eye_x = (int)((calx > 0) ? (calx+0.5) : (calx-0.5));
    dacq_data->eye_y = (int)((caly > 0) ? (caly+0.5) : (caly-0.5));
    dacq_data->eye_pa = pa;
    UNLOCK(semid);
    
    /* read digital input lines */
    dig_in();
    
    /* set digital output lines, only if the strobe's been set */
    LOCK(semid);
    k = dacq_data->dout_strobe;
    UNLOCK(semid);
    if (k) {
      dig_out();
      /* reset the strobe (as if it were a latch */
      LOCK(semid);
      dacq_data->dout_strobe = 0;
      UNLOCK(semid);
    }
    /* or if the strword is high -- Anitha*/
    LOCK(semid);
    k = dacq_data->dout_strword;
    UNLOCK(semid);
    if (k) {
      dig_str_out(); /* write the strobed word */
      LOCK(semid);
      dacq_data->dout_strword = 0;
      UNLOCK(semid);
    }

    LOCK(semid);
    dacq_data->timestamp = ts;
    k = dacq_data->adbuf_on;
    UNLOCK(semid);

    /* Stash the data, if recording is on:
     *  adbuf_t,x,y <- calibrated eye signal
     *  adbuf_pa <- pupil area, if available (eyelink only)
     *  adbuf_c[01234] <- raw data streams; in eyelink test mode
     *    these are:
     *     c0 <- eyelink x
     *     c1 <- eyelink y
     *     c2 <- coil raw x
     *     c3 <- coil raw y
     *     c4 <- eyelink pupil area
     */
    if (k) {
      LOCK(semid);
      k = dacq_data->adbuf_ptr;
      dacq_data->adbuf_t[k] = ts;
      dacq_data->adbuf_x[k] = dacq_data->eye_x;
      dacq_data->adbuf_y[k] = dacq_data->eye_y;
      dacq_data->adbuf_pa[k] = dacq_data->eye_pa;

      if (tracker_mode == EYELINK_TEST) {
	/* in test mode, analog channels 0,1,4 are filled with
	 * the eyelink data (x,y,pupil area)
	 */
	dacq_data->adbuf_c0[k] = (int)(tx > 0 ? tx+0.5 : tx-0.5);
	dacq_data->adbuf_c1[k] = (int)(ty > 0 ? ty+0.5 : ty-0.5);
	dacq_data->adbuf_c2[k] = (int)((x > 0) ? (x+0.5) : (x-0.5));
	dacq_data->adbuf_c3[k] = (int)((y > 0) ? (y+0.5) : (y-0.5));
	dacq_data->adbuf_c4[k] = (int)(tp > 0 ? tp+0.5 : tp-0.5);
      } else {
	/* otherwise, the raw analog values are stuffed in, which
	 * are usually raw x,y values off the coil, unless you're
	 * using them for something else (and have iscan/eyelink)
	 */
	dacq_data->adbuf_c0[k] = dacq_data->adc[0];
	dacq_data->adbuf_c1[k] = dacq_data->adc[1];
	dacq_data->adbuf_c2[k] = dacq_data->adc[2];
	dacq_data->adbuf_c3[k] = dacq_data->adc[3];

	/* Mon Jan 16 09:25:34 2006 mazer 
	 *  set up saving EDF-time to c4 channel for debugging

	 dacq_data->adbuf_c4[k] = eyelink_t;

	 */
      }
      if (++dacq_data->adbuf_ptr > ADBUFLEN) {
	dacq_data->adbuf_overflow++;
	dacq_data->adbuf_ptr = 0;
      }
      UNLOCK(semid);
    }

    /* check fixwins for in/out events */
    for (i = 0; i < NFIXWIN; i++) {
      LOCK(semid);
      k = dacq_data->fixwin[i].active;
      UNLOCK(semid);
      if (k) {
	LOCK(semid);
	x = dacq_data->eye_x - dacq_data->fixwin[i].cx;
	y = (dacq_data->eye_y - dacq_data->fixwin[i].cy) /
	  dacq_data->fixwin[i].vbias;
	UNLOCK(semid);
	
	z = (x * x) + (y * y);
	
	LOCK(semid);
	if (z < dacq_data->fixwin[i].rad2) {
	  /*
	   * eye is now INSIDE the fixation window -- stop counting
	   * transient breaks
	   */
	  dacq_data->fixwin[i].state = INSIDE;
	  dacq_data->fixwin[i].fcount = 0;
	} else {
	  /*
	   * eye is outside the fixation window, but could be shot noise..
	   */
	  if (dacq_data->fixwin[i].state == INSIDE) {
	    /*
	     * eye was inside last sample, so the break just happened
	     * reset the break counter and start counting # samples
	     * outside fixation window
	     */
	    dacq_data->fixwin[i].fcount = 1;
	    dacq_data->fixwin[i].nout = 0;
	  }
	  dacq_data->fixwin[i].state = OUTSIDE;
	  if (dacq_data->fixwin[i].fcount) {
	    dacq_data->fixwin[i].nout += 1;
	    if (dacq_data->fixwin[i].nout > dacq_data->fixbreak_tau) {
	      /* number of samples the eye's been out of the window
	       * has exceeded the limit defined by fixbreak_tau, count
	       * this as a real fixation break.
	       */
	      if (dacq_data->fixwin[i].broke == 0) {
		/* stash time if it's the first break */
		dacq_data->fixwin[i].break_time =  dacq_data->timestamp;
	      }
	      dacq_data->fixwin[i].broke = 1;
	      if (dacq_data->fixwin[i].genint) {
		/* send interupt to parent */
		dacq_data->int_class = INT_FIXWIN;
		dacq_data->int_arg = 0;
		dacq_data->fixwin[i].genint = 0;
		kill(getppid(), SIGUSR1);
		/* fprintf(stderr,"das: sent int, disabled\n"); */
	      }
	    }
	  }
	}
	UNLOCK(semid);
      }
    }

    /* possibly bump up or down priority on the fly */
    LOCK(semid);
    k = dacq_data->dacq_pri;
    UNLOCK(semid);
    if (setpri && lastpri != k) {
      lastpri = k;
      errno = 0;
      if (setpriority(PRIO_PROCESS, 0, k) == -1 && errno) {
	/* disable future priority changes */
	setpri = 0;
      }
      if (lastpri < 0) {
	resched(1);
      }
    }
    LOCK(semid);
    k = dacq_data->terminate;
    UNLOCK(semid);
  } while (! k);

  fprintf(stderr, "%s: terminate signaled\n", progname);
  iscan_halt();
  eyelink_halt();

  /* no longer ready */
  LOCK(semid);
  dacq_data->das_ready = 0;
  UNLOCK(semid);

  // this isn't needed, halt() gets called automatically via atexit()
  //halt();
}

static void mainloop_openiris()
{
  register int i, lastpri, setpri;
  //register float x, y, z, pa, tmp, calx, caly;
  register float x,y,z,pa,temp;
  register double calx_0,calx_4,caly_0,caly_4;
  register double x_0,y_0,x_4,y_4;
  float tx, ty, tp;
  unsigned long last_ts = 0, ts;
  unsigned int eyelink_t;
  int eyelink_new;
  int k;
  long accum[NADC], naccum;
  OpenIrisClient *client;

  register float sx=0, sy=0;
  register double sx_0=0, sy_0=0, sx_4=0, sy_4=0;
  int si, sn;
  double sbx_0[MAXSMOOTH], sby_0[MAXSMOOTH], sbx_4[MAXSMOOTH], sby_4[MAXSMOOTH];

  /*
   * calx/caly are the gain+offset adjusted eye position values
   * x/y are the raw values
   */
  calx = caly = x = y = pa = -1;

  for (si = 0; si < MAXSMOOTH; si++) {
    sbx_0[si] = sby_0[si] = sbx_4[si] = sby_4[si] = 0.0;
  }
  si = 0;

  errno = 0;
  LOCK(semid);
  client = dacq_data->openiris_client;
  k = dacq_data->dacq_pri;
  UNLOCK(semid);

  if (setpriority(PRIO_PROCESS, 0, k) == 0 && errno == 0) {
    fprintf(stderr, "%s: bumped priority %d\n", progname, k);
    lastpri = k;
    if (lastpri < 0) {
      resched(1);
    }
    setpri = 1;
  } else {
    fprintf(stderr, "%s: failed to change priority\n", progname);
    setpri = 0;
    lastpri = 0;
  }

  timestamp(1);			/* initialize the timestamp to 0 */

  fprintf(stderr, "%s: tracker_mode=%s (%d)\n", progname,
	  _tmodes[tracker_mode], tracker_mode);

  /* signal client we're ready */
  LOCK(semid);
  dacq_data->das_ready = 1;
  fprintf(stderr, "%s: ready\n", progname);
  UNLOCK(semid);

  do {
    /* sample converters as fast as possible and accumulate
     * into temp buffer for averaging at the end of the sample
     * period (1ms).  This replaces spin-locking code.
     */
    naccum = 0;
    /*
	//These code is for daq/das16_server. Openiris doesn't need them. 
	do {
      //sample the converters & acculumate values
      for (i = 0; i < NADC; i++) {
	if (naccum == 0) {
	  accum[i] = ad_in(i);
	} else {
	  accum[i] += ad_in(i);
	}
      }
      naccum += 1;
    } while (((ts = timestamp(0)) - last_ts) < 1);

    last_ts = ts;

    // adjust for # of acculumated values
    for (i = 0; i < NADC; i++) {
      LOCK(semid);
      dacq_data->adc[i] = (int)(accum[i] / naccum);
      UNLOCK(semid);
    }
    */
    ts = timestamp(0);
    last_ts = ts;
 {
      LOCK(semid);
      x = dacq_data->iscan_x;
      y = dacq_data->iscan_y;
      UNLOCK(semid);
      pa = -1;
    } 
	LOCK(semid);
      x_0 = dacq_data->openiris_p0_x;
      y_0 = dacq_data->openiris_p0_y;
	  x_4 = dacq_data->openiris_p4_x;
	  y_4 = dacq_data->openiris_p4_y;
      UNLOCK(semid);
    if (swap_xy) {
      tmp = x_0; x_0 = y_0; y_0 = tmp;
	  temp = x_4;x_4 = y_4;y_4 = temp;
    }

    /* smooth (if necessary) raw eye position trace */
    LOCK(semid);
    sn = dacq_data->eye_smooth;
    if (sn > MAXSMOOTH) {
      sn = MAXSMOOTH;
    }
    UNLOCK(semid);

    if (sn > 1) {
      /* remove old point, add new point to smoothing sum */
      sx_0 = sx_0 - sbx_0[si] + x_0;
      sy_0 = sy_0 - sby_0[si] + y_0;
	  sx_4 = sx_4 - sbx_4[si] + x_4;
      sy_4 = sy_4 - sby_4[si] + y_4;

      /* add new (unsmoothed data points) to smoothing buffer */
      sbx_0[si] = x_0;
      sby_0[si] = y_0;
	  sbx_4[si] = x_4;
      sby_4[si] = y_4;
      si = (si + 1) % sn;

      /* calc smoothed point */
      x_0 = sx_0 / sn;
      y_0 = sy_0 / sn;
	  x_4 = sx_4 / sn;
      y_4 = sy_4 / sn;
    }

    /* convert from raw to pixel domain and save in eye_x/eye_y */
    LOCK(semid);
	x = (float)(x4-x0);
    y = (float)(y4-y0);
    calx = (dacq_data->eye_xgain * x) - dacq_data->eye_xoff;
    caly = (dacq_data->eye_ygain * y) - dacq_data->eye_yoff;
    dacq_data->eye_x = (int)((calx > 0) ? (calx+0.5) : (calx-0.5));
    dacq_data->eye_y = (int)((caly > 0) ? (caly+0.5) : (caly-0.5));
    dacq_data->eye_pa = pa;
    UNLOCK(semid);
    
    /*Following lines are for digital signal or strobe. No need for Openiris.
    // read digital input lines 
    dig_in();
    
    //  set digital output lines, only if the strobe's been set 
    LOCK(semid);
    k = dacq_data->dout_strobe;
    UNLOCK(semid);
    if (k) {
      dig_out();
       //  reset the strobe (as if it were a latch 
      LOCK(semid);
      dacq_data->dout_strobe = 0;
      UNLOCK(semid);
    }
    // or if the strword is high -- Anitha
    LOCK(semid);
    k = dacq_data->dout_strword;
    UNLOCK(semid);
    if (k) {
      dig_str_out();  // write the strobed word 
      LOCK(semid);
      dacq_data->dout_strword = 0;
      UNLOCK(semid);
    }
    */
    
    //Check eye position buffer. 
    LOCK(semid);
    dacq_data->timestamp = ts;
    k = dacq_data->adbuf_on;
    UNLOCK(semid);
    
    /* Stash the data, if recording is on:
     *  adbuf_t,x,y <- calibrated eye signal
     *  adbuf_pa <- pupil area, if available (eyelink only)
     *  adbuf_c[01234] <- raw data streams; in eyelink test mode
     *    these are:
     *     c0 <- eyelink x
     *     c1 <- eyelink y
     *     c2 <- coil raw x
     *     c3 <- coil raw y
     *     c4 <- eyelink pupil area
     */
     
    if (k) {
      LOCK(semid);
      k = dacq_data->adbuf_ptr;
      dacq_data->adbuf_t[k] = ts;
      dacq_data->adbuf_x[k] = dacq_data->eye_x;
      dacq_data->adbuf_y[k] = dacq_data->eye_y;
      dacq_data->adbuf_pa[k] = dacq_data->eye_pa;
        
    /* Fill the c channels buffer, we don't have adc so fill channel 0,1,4
    with x,y,pupil area, but pa will be convereted as int*/
    if (tracker_mode == OPENIRIS) {
        dacq_data->adbuf_c0[k] = (int)((calx > 0) ? (calx+0.5) : (calx-0.5));
        dacq_data->adbuf_c1[k] = (int)((caly > 0) ? (caly+0.5) : (caly-0.5));
        dacq_data->adbuf_c4[k] = (int) pa;
    }
    
      if (++dacq_data->adbuf_ptr > ADBUFLEN) {
	dacq_data->adbuf_overflow++;
	dacq_data->adbuf_ptr = 0;
      }
      UNLOCK(semid);
    }

    /* check fixwins for in/out events */
    for (i = 0; i < NFIXWIN; i++) {
      LOCK(semid);
      k = dacq_data->fixwin[i].active;
      UNLOCK(semid);
      if (k) {
	LOCK(semid);
	x = dacq_data->eye_x - dacq_data->fixwin[i].cx;
	y = (dacq_data->eye_y - dacq_data->fixwin[i].cy) /
	  dacq_data->fixwin[i].vbias;
	UNLOCK(semid);
	
	z = (x * x) + (y * y);
	
	LOCK(semid);
	if (z < dacq_data->fixwin[i].rad2) {
	  /*
	   * eye is now INSIDE the fixation window -- stop counting
	   * transient breaks
	   */
	  dacq_data->fixwin[i].state = INSIDE;
	  dacq_data->fixwin[i].fcount = 0;
	} else {
	  /*
	   * eye is outside the fixation window, but could be shot noise..
	   */
	  if (dacq_data->fixwin[i].state == INSIDE) {
	    /*
	     * eye was inside last sample, so the break just happened
	     * reset the break counter and start counting # samples
	     * outside fixation window
	     */
	    dacq_data->fixwin[i].fcount = 1;
	    dacq_data->fixwin[i].nout = 0;
	  }
	  dacq_data->fixwin[i].state = OUTSIDE;
	  if (dacq_data->fixwin[i].fcount) {
	    dacq_data->fixwin[i].nout += 1;
	    if (dacq_data->fixwin[i].nout > dacq_data->fixbreak_tau) {
	      /* number of samples the eye's been out of the window
	       * has exceeded the limit defined by fixbreak_tau, count
	       * this as a real fixation break.
	       */
	      if (dacq_data->fixwin[i].broke == 0) {
		/* stash time if it's the first break */
		dacq_data->fixwin[i].break_time =  dacq_data->timestamp;
	      }
	      dacq_data->fixwin[i].broke = 1;
	      if (dacq_data->fixwin[i].genint) {
		/* send interupt to parent */
		dacq_data->int_class = INT_FIXWIN;
		dacq_data->int_arg = 0;
		dacq_data->fixwin[i].genint = 0;
		kill(getppid(), SIGUSR1);
		/* fprintf(stderr,"das: sent int, disabled\n"); */
	      }
	    }
	  }
	}
	UNLOCK(semid);
      }
    }

    /* possibly bump up or down priority on the fly */
    LOCK(semid);
    k = dacq_data->dacq_pri;
    UNLOCK(semid);
    if (setpri && lastpri != k) {
      lastpri = k;
      errno = 0;
      if (setpriority(PRIO_PROCESS, 0, k) == -1 && errno) {
	/* disable future priority changes */
	setpri = 0;
      }
      if (lastpri < 0) {
	resched(1);
      }
    }
    LOCK(semid);
    k = dacq_data->terminate;
    UNLOCK(semid);
  } while (! k);

  fprintf(stderr, "%s: terminate signaled\n", progname);
  //iscan_halt();
  //eyelink_halt();
  openiris_halt(client);

  /* no longer ready */
  LOCK(semid);
  dacq_data->das_ready = 0;
  UNLOCK(semid);

  // this isn't needed, halt() gets called automatically via atexit()
  //halt();
}

int main(int ac, char **av, char **envp)
{
  char *p;
  //char buf[100];
  float mhz;
  OpenIrisClient *client;
  int port;
  double timeout;
  
  if (ac == 1){
	port = 5000;
	timeout = 10;
  }
  if (ac == 2){
	  port = av[1];
	  timeout = 10;
  }
  if (ac == 3){
	  port = av[1];
	  timeout = av[2];
  }
  

#ifdef CHANGE_NAME
  init_set_proc_title(ac, av, envp);
#endif

  /*
  fprintf(stderr, "**** %s (pid=%d ppid=%d)****\n",
	  av[0], getpid(), getppid());
  */


  p = rindex(av[0], '/');
  progname = p ? (p + 1) : av[0];

  ticks_per_ms = (unsigned long long)(0.5 +
				      ((mhz = find_clockfreq()) / 1000.0));

  //fprintf(stderr, "%s: started up\n", progname);
  //fprintf(stderr, "%s: <%s>\n", progname, getcwd(buf, sizeof(buf)));
  //fprintf(stderr, "%s: cpu/rdtsc @ %f mHz\n", progname, mhz/1e6);
  //fprintf(stderr, "%s: MAXSMOOTH=%d\n", progname, MAXSMOOTH);
  //fprintf(stderr, "%s: ADBUFLEN = ~ %.1f s\n", progname, ADBUFLEN/1000.0);

  if ((semid = psem_init(SEMKEY)) < 0) {
    perror("psem_init");
    fprintf(stderr, "%s: can't init semaphore\n", progname);
    exit(1);
  }

  //client = openiris_init(char *ip_address,int port, double timeout);
  fprintf(stderr, "%s: initted\n", progname);
  

  //fprintf(stderr, "av[1]=<%s>\n", av[1]);
  //fprintf(stderr, "av[2]=<%s>\n", av[2]);

  if (av[1] && (strcmp(av[1], "-eyelink") == 0)) {
    eyelink_init(av[2]);
  } else if (getenv("EYELINK_TEST") != NULL) {
    eyelink_init(getenv("EYELINK_TEST"));
    if (tracker_mode == EYELINK) {
      tracker_mode = EYELINK_TEST;
      fprintf(stderr, "*********************************\n");
      fprintf(stderr, "*** eyelink test mode SUCCESS ***\n");
      fprintf(stderr, "*********************************\n");
    } else {
      fprintf(stderr, "*********************************\n");
      fprintf(stderr, "*** eyelink test mode FAILED  ***\n");
      fprintf(stderr, "*********************************\n");
    }
  } else if (ac == 2) {
    iscan_init(av[1], av[2]);
  } else if (ac == 3) {
	openiris_init(char *ip_address,int port, double timeout);
  }

  if (getenv("XXSWAP_XY")) {
    swap_xy = 1;
    fprintf(stderr, "%s: swapping X and Y\n", progname);
  }
  if (tracker_mode == OPENIRIS){
	  mainloop_openiris();
  } else {
	mainloop();
  }
  fprintf(stderr, "%s: bye bye\n", progname);
  exit(0);
}

