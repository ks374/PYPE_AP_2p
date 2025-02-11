include ../../make.defs

#DEBUG = -DDEBUG=1
#DEBUG = -DDEBUG=0

# for beta/1st version of eyelink api tools:
#   LIBEYELINK = ./Eyelink/libeyelink.a -lpthread
#   INCEYELINK = 

# for new version (summer 2003):
LIBEYELINK = -leyelink_core -lpthread -lm #edited in May 2019
INCEYELINK = /usr/include/EyeLink   # edited in Dec 2022. to work with a new product key
#INCEYELINK = /usr/include

LIBETH32 = -leth32 -lm -lpthread #edited in May 2019
#Previous: INCETH32=/usr/ETH32 11/14modified
INCETH32 = /usr/include/

CFLAGS = -O6 -g -Wall -I$(INCEYELINK) -I$(INCETH32) -I./include \
		$(GLOBAL_CFLAGS) $(PYTHONINC) $(DEBUG) -fPIC #edited in May 2019
CC = gcc

all: build install

build: das16_server das08_server _dacqmodule.so iscan_server comedi

sigs.o: sigs.c sigs.h
	$(CC) $(CFLAGS) -c sigs.c #edited in May 2019

psems.o: psems.c psems.h
	$(CC) $(CFLAGS) -c psems.c #edited in May 2019

procname.o: procname.c procname.h
	$(CC) $(CFLAGS) -c procname.c #edited in May 2019

psems: psems.c psems.h
	$(CC) $(CFLAGS) -o psems -DTEST psems.c #edited in May 2019

cmdeyelink: cmdeyelink.o
	$(CC) -o $@ $< $(LIBEYELINK)

das16_server.o: das16_server.c das_common.c das_common.h \
	procname.h procname.c

das16_server: das16_server.o dacq.h sigs.o psems.o procname.o
	$(CC) -o $@ $< sigs.o psems.o procname.o $(LIBEYELINK)

comedi:
	if [ -f /usr/include/comedi.h -o -f /usr/local/include/comedi.h ]; then \
		make comedi_server; \
	fi;

comedi_server.o: comedi_server.c das_common.c das_common.h \
	procname.h procname.c

comedi_server: comedi_server.o dacq.h sigs.o psems.o procname.o
	$(CC) -o $@ $< sigs.o  psems.o procname.o $(LIBEYELINK) $(LIBETH32) -lrt -lcomedi -lm

das08_server.o: das08_server.c das_common.c das_common.h \
	procname.h procname.c

das08_server: das08_server.o dacq.h sigs.o psems.o procname.o
	$(CC) -o $@ $< sigs.o psems.o procname.o $(LIBEYELINK)

#iscan_server: iscan_server.o dacq.h sigs.o  psems.o procname.o libez
#	@echo "NOTE: can't tell if libez is fresh, will relink iscan_server"
#	$(CC) -o $@ $< sigs.o psems.o procname.o libezV24-0.0.3/libezV24-0.a

iscan_server: iscan_server.o dacq.h sigs.o  psems.o procname.o 
	@echo "NOTE: can't tell if libez is fresh, will relink iscan_server"
	$(CC) -o $@ $< sigs.o psems.o procname.o -lezV24

#libez:
#	(cd libezV24-0.0.3 ; make static; cd ..)


dacq.c: dacq.h

dacq.i: dacq.c dacq.h dacq.i.pre
	@cat dacq.i.pre dacq.h >dacq.i
	@echo "Made dacq.i"

dacq_wrap.c: dacq.i
	swig -python dacq.i

dacq_wrap.o: dacq_wrap.c
	$(CC) $(CFLAGS) $(PYTHONINC) -c dacq_wrap.c

_dacq.so: dacq_wrap.o dacq.o psems.o
	$(CC) -shared dacq_wrap.o dacq.o psems.o -o $@

install: das16_server das08_server _dacq.so iscan_server comedi
	cp das16_server $(PYPEDIR)/bin
	cp das08_server $(PYPEDIR)/bin
	cp iscan_server $(PYPEDIR)/bin
	cp dacq.py $(PYPEDIR)/lib
	cp _dacq.so $(PYPEDIR)/lib
	#cp libeyelinkcore.so $(PYPEDIR)/lib
	if [ -x comedi_server ]; then cp comedi_server $(PYPEDIR)/bin; fi;

#############################################################

dastest_poll: dastest_poll.o
	$(CC) -o $@ $<

dastest_cont: dastest_cont.o
	$(CC) -o $@ $<

#############################################################


clean:
	@rm -f *.o _dacq.so *.i *.pyc _dacq \
		*_server *_wrap.* .*~ dacq.py
	#(cd libezV24-0.0.3 ; make clean; cd ..)
	#@(rm libezV24-0.0.3/libezV24-0.a || \
	#	echo libezV24-0.a not built so not cleaning)
	@rm -f das16_server comedi_server das08_server _dacq.so 

