/* title:   dacqinfo.h
** author:  Chenghang Zhang
** created: Fri Nov 22 2024
** info:    generic dacq interface structure. Modified from old mazer version. 
** history:
**
*/

#include "Eth32_interface.h"
#include "Openiris_client.h"

#define SHMKEY	0xDA01
#define SEMKEY	0xF0F0

/*
A WIP section
*/
// typedef struct {
//   /* raw data (input and output) */
//   eth32* eth32_handler;
//   OpenIrisClient* client;
//   int shmid;

//   double	eye_x;			/* current eye position: X position */
//   double	eye_y;			/* current eye position: Y position */
//   int	eye_pa;			/* current pupil area, if available */
// } DACQINFO;

/*Needed for OpenIris*/
#define NDIGOUT	1         /*Number of digital output. For Openiris we have 1*/
#define NFIXWIN	1         /*Not sure why we need fixwin in dacqinfo*/
#define ADBUFLEN (1000 * 60)    /*Eyepos buffer. Note it's a ring buffer*/

/*Not in use, kept for back compatibility*/
#define NDIGIN	1         /*Number of digitial input. For Openiris we don't have it*/
#define NDIGOUTC 1        /*digital output on port C - Anitha, Possibly don't need it for Openiris*/
#define NADC	4           /**/
#define NDAC	2
#define MAXSMOOTH 25            /*Don't know what this is*/
/* pseudo-interupt codes */
#define INT_DIN		1
#define INT_FIXWIN	2

typedef struct {
  int active;			/* active or idle flag */
  int xchn, ychn;		/* channels to read */
  int cx, cy;			/* center of window */
  float vbias;			/* vertical elongation factor */
  int rad2;			/* window radius ^ 2 (squared for speed) */
  int state;			/* current state (1 for inside) */
  int broke;			/* latched state */
  long break_time;		/* timestamp for last break */
  int fcount;			/* internal.. */
  int nout;			/* internal.. */
  int genint;			/* generate an SIGUSR2 on break?? */
} FIXWIN;

typedef struct {
  /*Openiris client info*/
  OpenIrisClient *openiris_client
  
  
  /*Needed for Openiris*/
  char	dout[NDIGOUT];		/* status of digital output lines */
  /*Calibrated positions*/
  int	eye_x;			/* current eye position: X position */
  int	eye_y;			/* current eye position: Y position */
  int	eye_pa;			/* current pupil area, if available */
  /*Raw positions*/
  /*Do we need these? */
  double openiris_p0_x;
  double openiris_p0_y;
  double openiris_p4_x;
  double openiris_p4_y;
  /*Calibration values*/
  float eye_xgain, eye_ygain;	/* mult. gain for x/y eye position */
  int	eye_xoff, eye_yoff;	/* additive offset in pixels */

  /* Kept for back-compatibility */
  char	din[NDIGIN];		/* status of digital input lines */
  char	din_changes[NDIGIN];	/* # of changes since last reset */
  char  din_intmask[NDIGIN];	/* mask for SIGUSR1 digital inputs */
  char  doutc[NDIGOUTC];         /* status of Port C lines */
  char	dout_strobe;		/* software strobe (force digital output) */
  char  dout_strword;           /* this is to force output of strobed word */
  int	iscan_x;		/* last iscan value: X position */
  int	iscan_y;		/* last iscan value: Y position */
  int	adc[NADC];		/* current values for ADC channels */
  int	dac[NDAC];		/* current values for DAC channels */
  int	dac_strobe;		/* software strobe (force DA output) */
  float eye_smooth;		/* NOT IMPLEMENTED NOW */


  

  /* housekeeping flags */
  unsigned long timestamp;	/* timestamp of last update (ms resolution) */
  int	iscan_terminate;	/* set flag to force iscan termination */
  int	terminate;		/* set flag to force termination */
  int	das_ready;		/* sync flag -- when true, dacq proc ready */
  int	openiris_ready;		/* sync flag -- when true, iscan proc ready */

  /* used only once.. */
  int	dacq_pri;
  int	iscan_pri;

  /* d/a buffers */
  unsigned int	adbuf_on;	/* flag to trigger a/d collect */
  unsigned int	adbuf_ptr;	/* current point in ring buffers */
  unsigned int	adbuf_overflow;	/* overflow flag (INDICATES ERROR!!) */

  unsigned long adbuf_t[ADBUFLEN];	/* timestamps (ms) */
  int		adbuf_x[ADBUFLEN];	/* eye x position trace */
  int		adbuf_y[ADBUFLEN];	/* eye y position trace */
  int		adbuf_pa[ADBUFLEN];	/* pupil area, if available */
  
  int		adbuf_c0[ADBUFLEN];	/* channel 0/x */
  int		adbuf_c1[ADBUFLEN];	/* channel 1/y */
  int		adbuf_c2[ADBUFLEN];	/* channel 2/photo diode*/
  int		adbuf_c3[ADBUFLEN];	/* channel 3/spike input)*/
  int		adbuf_c4[ADBUFLEN];	/* channel 4/extra analog chanel */

  /* automatic fixation windows */
  FIXWIN fixwin[NFIXWIN];
  int		fixbreak_tau;		/* number of samples outside */
					/* before it counts as a break */

  /* 'interrupt' classes & arguments */
  int int_class;
  int int_arg;
} DACQINFO;

/* these are for backwards compatibility: */
#define adbuf_photo	adbuf_c2
#define adbuf_spikes	adbuf_c3
