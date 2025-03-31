# -*- Mode: Python; tab-width: 4; py-indent-offset: 4; -*-
# $Id$

"""Events and result codes

Event definitions, based on the encode tags used by the
ORTEX program from Bob Desimone's lab at NIMH.

Author -- James A. Mazer (james.mazer@yale.edu)

Jul 30 2006 -- Anitha Pasupathy

Change the tuple codes to numbers, to output to plexon
Behavioral codes and task codes all are associated with
numbers

July 16 2007 -- Phil Harding

Added the following codes to  pype_plex_code_dict:
           'color' : 42
           'plexStimIDOffset' : 200
           'plexRotOffset' : 3736

I also changed start (code 10) and stop code 11) to start_trial and
stop_trial respectively.

July 19 2007 -- Phil Harding

Added the following codes to  pype_plex_code_dict:
                         'rfx' : 43,
                'rfy' : 44,
                'iti' : 45,
                'stim_time' : 46,
                'isi' : 47,
                'stim_num' : 48,
                'pause' : 100,
                'unpause' : 101,

I also removed code 32 fix_acuired as it is a duplicate of
code 31, fix_acquired

Feb 06, 2009 Anitha Pasupathy

Added ambiguous_info -

**Revision History**

"""

####################################################################
# Result Codes -- Trial Outcome
####################################################################

# standard trial result codes (try to use these if possible!!)
# result codes are single letter -- additional (task-specfic) info
# can be appended, if required.

ANY_RESPONSE			= None
CORRECT_RESPONSE		= 'C'
USER_ABORT				= 'A'
UNINITIATED_TRIAL		= 'U'
MAXRT_EXCEEDED			= 'M'
EARLY_RELEASE			= 'E'

### Additional behavioral codes -- AP

LATE_RESP = 'L'
BREAK_FIX = 'B'
WRONG_RESP = 'W'
NO_RESP = 'N'


rcodes = {
    'C': 'CORRECT_RESPONSE',
    'A': 'USER_ABORT',
    'U': 'UNITIATED_TRIAL',
    'M': 'MAXRT_EXCEEDED',
    'E': 'EARLY_RELEASE'
}

def iscorrect(code):
    if code[0] == CORRECT_RESPONSE:
        return 1
    else:
        return 0

def isabort(code):
    if code[0] == ABORT:
        return 1
    else:
        return 0

def isui(code):
    if code[0] == UNINITIATED_TRIAL:
        return 1
    else:
        return 0

def ismaxrt_exceeded(code):
    if code[0] == MAXRT_EXCEEDED:
        return 1
    else:
        return 0

def isearly_release(code):
    if code[0] == EARLY_RELEASE:
        return 1
    else:
        return 0

####################################################################
# Event Codes -- timestamped and put in the encode buffer
####################################################################

# for internal use:
START					= 'start'
STOP					= 'stop'

# for general (task) use:

START_ITI				= 'start_iti'
END_ITI					= 'end_iti'

EYE_START				= 'eye_start'
EYE_STOP				= 'eye_stop'

START_PRE_TRIAL			= 'start_pre_trial'
END_PRE_TRIAL			= 'end_pre_trial'

START_POST_TRIAL		= 'start_post_trial'
END_POST_TRIAL			= 'end_post_trial'

START_WAIT_FIXATION		= 'start_wait_fixation'
END_WAIT_FIXATION		= 'end_wait_fixation'
FIXATION_OCCURS			= 'fixation_occurs'

START_WAIT_BAR			= 'start_wait_bar'
END_WAIT_BAR			= 'end_wait_bar'
BAR_UP					= 'bar_up'
BAR_DOWN				= 'bar_down'

TEST_ON					= 'test_on'
TEST_OFF				= 'test_off'

FIX_ON					= 'fix_on'
FIX_OFF					= 'fix_off'

FIX_ACQUIRED			= 'fix_acquired'
OLD_FIX_ACQUIRED		= 'fix_acuired'
FIX_LOST				= 'fix_lost'
FIX_DONE				= 'fix_done'

START_SPONT				= 'start_spont'
STOP_SPONT				= 'stop_spont'

REWARD					=  'reward'

SPIKE1					= '_s1'
SPIKE2					= '_s2'
SPIKE3					= '_s3'
SPIKE4					= '_s4'
SPIKE5					= '_s5'
SPIKE6					= '_s6'
SPIKE7					= '_s7'
SPIKE8					= '_s8'
SPIKE9					= '_s9'
SPIKE10					= '_s10'
SPIKE11					= '_s11'
SPIKE12					= '_s12'
SPIKE13					= '_s13'
SPIKE14					= '_s14'
SPIKE15					= '_s15'
SPIKE16					= '_s16'

# DMTS constants
SAMPLE_ON				= 'sample_on'
SAMPLE_OFF				= 'sample_off'
TARGETS_ON				= 'targets_on'
TARGETS_OFF				= 'targets_off'
mod_MATCH				= ' match'		# modifier only
mod_NON_MATCH			= ' non-match'	# modifier only


# new events for non-cortex-like stuff... add below:
FLIP					= 'flip'

# debugging mark
MARK					= 'mark'

####################################################################
# Dictonary definition for plexon pype communication - AP
####################################################################

def pype_plex_code_dict(code):

    strtonum = {CORRECT_RESPONSE: 0,
                USER_ABORT: 1,
                UNINITIATED_TRIAL: 2,
                MAXRT_EXCEEDED: 3,
                EARLY_RELEASE: 4,
                LATE_RESP: 5,
                BREAK_FIX: 6,
                WRONG_RESP: 7,
                NO_RESP: 8,
                'start_trial': 10,
                'stop_trial': 11,
                'start_iti': 12,
                'end_iti': 13,
                'eye_start': 14,
                'eye_stop': 15,
                'start_pre_trial': 16,
                'end_pre_trial': 17,
                'start_post_trial': 18,
                'end_post_trial': 19,
                'start_wait_fixation': 20,
                'end_wait_fixation': 21,
                'fixation_occurs': 22,
                'start_wait_bar': 23,
                'end_wait_bar': 24,
                'bar_up': 25,
                'bar_down': 26,
                'test_on': 27,
                'test_off': 28,
                'fix_on': 29,
                'fix_off': 30,
                'fix_acquired': 31,
                'fix_lost': 33,
                'fix_done': 34,
                'start_spont': 35,
                'stop_spont': 36,
                'reward': 37,
                # DMTS constants
                'sample_on': 38,
                'sample_off': 39,
                'targets_on': 40,
                'targets_off': 41,
                'color' : 42,
                'rfx' : 43,
                'rfy' : 44,
                'iti' : 45,
                'stim_time' : 46,
                'isi' : 47,
                'numstim' : 48,
                'stimid'  : 49,
                'rotid'   : 50,
                'stimdur' : 51,
                'occlmode' :52,
                'occl_info' : 53,
                'mask_info' : 54,
                'mask_on' : 55,
                'mask_off' : 56,
                'position' : 57,
                'stimWidth' : 58,
                'stimHeight' : 59,
                'stimShape' : 60,
                'perispace' : 61,
                'occlshape' : 62,
                'dot_rad' : 63,
                'line_width' : 64,
                'gen_mode' : 65,
                'gen_submode' : 66,
                'add_extra_isi' : 67,
                'midground_info' : 68,
                'foreground_info' : 69,
                'background_info' : 70,
                'onset_time' : 71,
                'second_stimuli' : 72,
                'location_flip_info' : 73,
                'extra' : 74,
                'ambiguous_info' : 75,
                'fix_x' : 76,
                'fix_y' : 77,
                'mon_ppd' : 78,
                'radius' : 80,
                'pause' : 100,
                'unpause' : 101,
                'plexStimIDOffset' : 200,
                'plexYOffset' : 600,
                'plexFloatMult' : 1000,
                'plexRotOffset' : 3736}
    if isinstance(code, int) or isinstance(code, int): 
        ret_val = code
    else:
        ret_val = strtonum[code]
    return ret_val

####################################################################
# label tags for pickled datafiles
####################################################################

ENCODE = 'ENCODE'
NOTE = 'NOTE'
WARN = 'WARN'
ANNOTATION = 'ANNOTATION'


if __name__=='__main__' :
    pass
else:
    try:
        from pype import loadwarn
        loadwarn(__name__)
    except ImportError:
        pass
        
