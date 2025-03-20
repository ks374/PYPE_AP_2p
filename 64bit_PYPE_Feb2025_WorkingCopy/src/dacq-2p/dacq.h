/* title:   dacq.h
** author:  Chenghang Zhang
** created: Fri Nov 22
** info:    task file connector to hardware
** history:
**
** File modified from old dacq.c
*/
#pragma once

#include "dacqinfo.h"

extern int shmid;
extern int semid;

int dacq_start(int boot, int testmode, char *tracker_type,
	       char *dacq_server, char *arg1, char *arg2);
void dacq_stop(void);
int dacq_release(void);

int dacq_dig_in(int n);
void dacq_dig_out(int n, int val);
void dacq_str_word(int val);