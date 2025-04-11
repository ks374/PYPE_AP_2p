Attribute VB_Name = "eth32_mod"
' Winford Engineering ETH32 Declarations Library
' Copyright 2005
' www.winford.com
'
' This module is intended for use with Visual Basic 5/6 to provide
' constants and function declarations for the ETH32 API (eth32api.dll)
' (Note the "api" on the end of the file name, which avoids conflicts
'  with the Visual Studio .NET class library, which is eth32.dll)


Option Explicit

Public Const ETH32_PORT As Integer = 7152
Public Const DIR_INPUT As Long = 0
Public Const DIR_OUTPUT As Long = &HFF
Public Const ETH32_PRODUCT_ID As Byte = 105

Public Enum Eth32EventType
    EVENT_DIGITAL = 0
    EVENT_ANALOG = 1
    EVENT_COUNTER_ROLLOVER = 2
    EVENT_COUNTER_THRESHOLD = 3
    EVENT_HEARTBEAT = 4
End Enum


' Used internally by the class only
Public Const HANDLER_NONE As Long = 0
Public Const HANDLER_CALLBACK As Long = 1
Public Const HANDLER_MESSAGE As Long = 2


' Analog channel assignments
' Single ended channels

Public Enum Eth32AnalogChannel
    ANALOG_SE0 = &H0              ' Channel 0
    ANALOG_SE1 = &H1
    ANALOG_SE2 = &H2
    ANALOG_SE3 = &H3
    ANALOG_SE4 = &H4
    ANALOG_SE5 = &H5
    ANALOG_SE6 = &H6
    ANALOG_SE7 = &H7

    ' Differential channels
    ANALOG_DI00X10 = &H8           ' Difference of channel 0 and 0 (for calibration), with 10X gain
    ANALOG_DI10X10 = &H9          ' Difference of channel 1 and 0, with 10X gain
    ANALOG_DI00X200 = &HA
    ANALOG_DI10X200 = &HB
    ANALOG_DI22X10 = &HC
    ANALOG_DI32X10 = &HD
    ANALOG_DI22X200 = &HE
    ANALOG_DI32X200 = &HF
    ANALOG_DI01X1 = &H10
    ANALOG_DI11X1 = &H11
    ANALOG_DI21X1 = &H12
    ANALOG_DI31X1 = &H13
    ANALOG_DI41X1 = &H14
    ANALOG_DI51X1 = &H15
    ANALOG_DI61X1 = &H16
    ANALOG_DI71X1 = &H17
    ANALOG_DI02X1 = &H18
    ANALOG_DI12X1 = &H19
    ANALOG_DI22X1 = &H1A
    ANALOG_DI32X1 = &H1B
    ANALOG_DI42X1 = &H1C
    ANALOG_DI52X1 = &H1D
    ANALOG_122V = &H1E            ' 1.22V
    ANALOG_0V = &H1F              ' 0V
End Enum

Public Enum Eth32CounterState
    COUNTER_DISABLED = 0
    COUNTER_FALLING = 1
    COUNTER_RISING = 2
End Enum

Public Enum Eth32AnalogState
    ADC_DISABLED = 0
    ADC_ENABLED = 1
End Enum

Public Enum Eth32AnalogReference
    REF_EXTERNAL = 0
    REF_INTERNAL = 1
    REF_256 = 3
End Enum

Public Enum Eth32PulseEdge
    PULSE_FALLING = 0
    PULSE_RISING = 1
End Enum

Public Enum Eth32AnalogEvtDef
    ANEVT_DEFAULT_LOW = 0
    ANEVT_DEFAULT_HIGH = 1

End Enum

Public Enum Eth32PwmClock
    PWM_CLOCK_DISABLED = 0
    PWM_CLOCK_ENABLED = 1
End Enum

Public Enum Eth32PwmChannel
    PWM_CHANNEL_DISABLED = 0
    PWM_CHANNEL_NORMAL = 1
    PWM_CHANNEL_INVERTED = 2
End Enum

Public Enum Eth32ConnectionFlag
    CONN_FLAG_NONE = &H0
    CONN_FLAG_RESPONSE = &H1
    CONN_FLAG_DIGITAL_EVENT = &H2
    CONN_FLAG_ANALOG_EVENT = &H4
    CONN_FLAG_COUNTER_EVENT = &H8
End Enum


' Settings for handling a full queue condition
'  DISCARD_NEW prevents the new events from being added to the queue,
'              so the actual queue remains unchanged
'  DISCARD_OLD shifts an old event off the queue to make room for the new
Public Enum Eth32QueueMode
    QUEUE_DISCARD_NEW = 0
    QUEUE_DISCARD_OLD = 1
End Enum

' Event information structure
Public Type eth32_event
  id As Long
  type As Long
  port As Long
  bit As Long
  prev_value As Long
  value As Long
  direction As Long
End Type


' Event handler definition structure
Public Type eth32_handler
  type As Long
  maxqueue As Long
  fullqueue As Long
  eventfn As Long ' Address of callback function
  extra As Long
  window As Long ' Window to send messages to
  msgid As Long
  wparam As Long
  lparam As Long
End Type

Public Declare Function eth32_open Lib "eth32api.dll" (ByVal address As String, ByVal port As Integer, ByVal Timeout As Long, ByRef Result As Long) As Long
Public Declare Function eth32_set_timeout Lib "eth32api.dll" (ByVal handle As Long, ByVal Timeout As Long) As Long
Public Declare Function eth32_get_timeout Lib "eth32api.dll" (ByVal handle As Long, ByRef Timeout As Long) As Long
Public Declare Function eth32_verify_connection Lib "eth32api.dll" (ByVal handle As Long) As Long
Public Declare Function eth32_output_byte Lib "eth32api.dll" (ByVal handle As Long, ByVal port As Long, ByVal value As Long) As Long
Public Declare Function eth32_output_bit Lib "eth32api.dll" (ByVal handle As Long, ByVal port As Long, ByVal bit As Long, ByVal value As Long) As Long
Public Declare Function eth32_pulse_bit Lib "eth32api.dll" (ByVal handle As Long, ByVal port As Long, ByVal bit As Long, ByVal edge As Long, ByVal count As Long) As Long
Public Declare Function eth32_set_led Lib "eth32api.dll" (ByVal handle As Long, ByVal Led As Long, ByVal value As Long) As Long
Public Declare Function eth32_input_byte Lib "eth32api.dll" (ByVal handle As Long, ByVal port As Long, ByRef value As Long) As Long
Public Declare Function eth32_input_successive Lib "eth32api.dll" (ByVal handle As Long, ByVal port As Long, ByVal max As Long, ByRef value As Long, ByRef status As Long) As Long
Public Declare Function eth32_input_bit Lib "eth32api.dll" (ByVal handle As Long, ByVal port As Long, ByVal bit As Long, ByRef value As Long) As Long
Public Declare Function eth32_readback Lib "eth32api.dll" (ByVal handle As Long, ByVal port As Long, ByRef value As Long) As Long
Public Declare Function eth32_get_led Lib "eth32api.dll" (ByVal handle As Long, ByVal Led As Long, ByRef value As Long) As Long
Public Declare Function eth32_set_direction Lib "eth32api.dll" (ByVal handle As Long, ByVal port As Long, ByVal direction As Long) As Long
Public Declare Function eth32_get_direction Lib "eth32api.dll" (ByVal handle As Long, ByVal port As Long, ByRef direction As Long) As Long
Public Declare Function eth32_set_direction_bit Lib "eth32api.dll" (ByVal handle As Long, ByVal port As Long, ByVal bit As Long, ByVal direction As Long) As Long
Public Declare Function eth32_get_direction_bit Lib "eth32api.dll" (ByVal handle As Long, ByVal port As Long, ByVal bit As Long, ByRef direction As Long) As Long
Public Declare Function eth32_set_analog_state Lib "eth32api.dll" (ByVal handle As Long, ByVal state As Long) As Long
Public Declare Function eth32_get_analog_state Lib "eth32api.dll" (ByVal handle As Long, ByRef state As Long) As Long
Public Declare Function eth32_set_analog_reference Lib "eth32api.dll" (ByVal handle As Long, ByVal reference As Long) As Long
Public Declare Function eth32_get_analog_reference Lib "eth32api.dll" (ByVal handle As Long, ByRef reference As Long) As Long
Public Declare Function eth32_input_analog Lib "eth32api.dll" (ByVal handle As Long, ByVal channel As Long, ByRef value As Long) As Long
Public Declare Function eth32_set_analog_eventdef Lib "eth32api.dll" (ByVal handle As Long, ByVal bank As Long, ByVal channel As Long, ByVal lomark As Long, ByVal himark As Long, ByVal defaultval As Long) As Long
Public Declare Function eth32_get_analog_eventdef Lib "eth32api.dll" (ByVal handle As Long, ByVal bank As Long, ByVal channel As Long, ByRef lomark As Long, ByRef himark As Long) As Long
Public Declare Function eth32_set_analog_assignment Lib "eth32api.dll" (ByVal handle As Long, ByVal channel As Long, ByVal source As Long) As Long
Public Declare Function eth32_get_analog_assignment Lib "eth32api.dll" (ByVal handle As Long, ByVal channel As Long, ByRef source As Long) As Long
Public Declare Function eth32_reset Lib "eth32api.dll" (ByVal handle As Long) As Long
Public Declare Function eth32_get_serialnum Lib "eth32api.dll" (ByVal handle As Long, ByRef batch As Long, ByRef unit As Long) As Long
Public Declare Function eth32_get_serialnum_string Lib "eth32api.dll" (ByVal handle As Long, ByVal serial As String, ByVal bufsize As Long) As Long
Public Declare Function eth32_get_product_id Lib "eth32api.dll" (ByVal handle As Long, ByRef prodid As Long) As Long
Public Declare Function eth32_get_firmware_release Lib "eth32api.dll" (ByVal handle As Long, ByRef major As Long, ByRef minor As Long) As Long
Public Declare Function eth32_connection_flags Lib "eth32api.dll" (ByVal handle As Long, ByVal reset As Long, ByRef flags As Long) As Long

Public Declare Function eth32_set_event_queue_config Lib "eth32api.dll" (ByVal handle As Long, ByVal maxsize As Long, ByVal fullqueue As Long) As Long
Public Declare Function eth32_get_event_queue_status Lib "eth32api.dll" (ByVal handle As Long, ByRef maxsize As Long, ByRef fullqueue As Long, ByRef cursize As Long) As Long
Public Declare Function eth32_dequeue_event Lib "eth32api.dll" (ByVal handle As Long, ByRef event_info As eth32_event, ByVal Timeout As Long) As Long
Public Declare Function eth32_empty_event_queue Lib "eth32api.dll" (ByVal handle As Long) As Long
Public Declare Function eth32_enable_event Lib "eth32api.dll" (ByVal handle As Long, ByVal evtype As Long, ByVal port As Long, ByVal bit As Long, ByVal id As Long) As Long
Public Declare Function eth32_disable_event Lib "eth32api.dll" (ByVal handle As Long, ByVal evtype As Long, ByVal port As Long, ByVal bit As Long) As Long
Public Declare Function eth32_set_event_handler Lib "eth32api.dll" (ByVal handle As Long, ByRef handler As eth32_handler) As Long
Public Declare Function eth32_get_event_handler Lib "eth32api.dll" (ByVal handle As Long, ByRef handler As eth32_handler) As Long

Public Declare Function eth32_set_counter_state Lib "eth32api.dll" (ByVal handle As Long, ByVal counter As Long, ByVal state As Long) As Long
Public Declare Function eth32_get_counter_state Lib "eth32api.dll" (ByVal handle As Long, ByVal counter As Long, ByRef state As Long) As Long
Public Declare Function eth32_set_counter_value Lib "eth32api.dll" (ByVal handle As Long, ByVal counter As Long, ByVal value As Long) As Long
Public Declare Function eth32_get_counter_value Lib "eth32api.dll" (ByVal handle As Long, ByVal counter As Long, ByRef value As Long) As Long
Public Declare Function eth32_set_counter_rollover Lib "eth32api.dll" (ByVal handle As Long, ByVal counter As Long, ByVal rollover As Long) As Long
Public Declare Function eth32_get_counter_rollover Lib "eth32api.dll" (ByVal handle As Long, ByVal counter As Long, ByRef rollover As Long) As Long
Public Declare Function eth32_set_counter_threshold Lib "eth32api.dll" (ByVal handle As Long, ByVal counter As Long, ByVal threshold As Long) As Long
Public Declare Function eth32_get_counter_threshold Lib "eth32api.dll" (ByVal handle As Long, ByVal counter As Long, ByRef threshold As Long) As Long

Public Declare Function eth32_set_pwm_clock_state Lib "eth32api.dll" (ByVal handle As Long, ByVal state As Long) As Long
Public Declare Function eth32_get_pwm_clock_state Lib "eth32api.dll" (ByVal handle As Long, ByRef state As Long) As Long
Public Declare Function eth32_set_pwm_base_period Lib "eth32api.dll" (ByVal handle As Long, ByVal period As Long) As Long
Public Declare Function eth32_get_pwm_base_period Lib "eth32api.dll" (ByVal handle As Long, ByRef period As Long) As Long
Public Declare Function eth32_set_pwm_channel Lib "eth32api.dll" (ByVal handle As Long, ByVal channel As Long, ByVal state As Long) As Long
Public Declare Function eth32_get_pwm_channel Lib "eth32api.dll" (ByVal handle As Long, ByVal channel As Long, ByRef state As Long) As Long
Public Declare Function eth32_set_pwm_duty_period Lib "eth32api.dll" (ByVal handle As Long, ByVal channel As Long, ByVal period As Long) As Long
Public Declare Function eth32_get_pwm_duty_period Lib "eth32api.dll" (ByVal handle As Long, ByVal channel As Long, ByRef period As Long) As Long

Public Declare Function eth32_set_pwm_parameters Lib "eth32api.dll" (ByVal handle As Long, ByVal channel As Long, ByVal state As Long, ByVal freq As Single, ByVal duty As Single) As Long
Public Declare Function eth32_get_pwm_parameters Lib "eth32api.dll" (ByVal handle As Long, ByVal channel As Long, ByRef state As Long, ByRef freq As Single, ByRef duty As Single) As Long

Public Declare Function eth32_get_eeprom Lib "eth32api.dll" (ByVal handle As Long, ByVal address As Long, ByVal length As Long, ByRef buffer As Byte) As Long
Public Declare Function eth32_set_eeprom Lib "eth32api.dll" (ByVal handle As Long, ByVal address As Long, ByVal length As Long, ByRef buffer As Byte) As Long

Public Declare Function eth32_error_string Lib "eth32api.dll" (ByVal errorcode As Long) As Long

Public Declare Function eth32_close Lib "eth32api.dll" (ByVal handle As Long) As Long

'************************************************
' Functions/Definitions for ETH32 Configuration functionality
'************************************************
Public Type eth32cfg_mac
    buf(5) As Byte
End Type

Public Type eth32cfg_ip
    buf(3) As Byte
End Type


Public Type eth32cfg_data
    product_id As Byte
    firmware_major As Byte
    firmware_minor As Byte
    config_enable As Byte
    mac As eth32cfg_mac
    pad1 As Byte ' mac needs to occupy 8 bytes in structure, so add two padding bytes
    pad2 As Byte
    serialnum_batch As Integer
    serialnum_unit As Integer
    config_ip As eth32cfg_ip
    config_gateway As eth32cfg_ip
    config_netmask As eth32cfg_ip
    active_ip As eth32cfg_ip
    active_gateway As eth32cfg_ip
    active_netmask As eth32cfg_ip
    dhcp As Byte
End Type



Public Enum Eth32ConfigPluginType
    ETH32CFG_PLUG_NONE = 0
    ETH32CFG_PLUG_SYS = 1
    ETH32CFG_PLUG_PCAP = 2
End Enum

Public Enum Eth32ConfigInterfaceName
    ETH32CFG_IFACENAME_STANDARD = 0
    ETH32CFG_IFACENAME_FRIENDLY = 1
    ETH32CFG_IFACENAME_DESCRIPTION = 2
End Enum

Public Enum Eth32ConfigInterfaceType
    ETH32CFG_IFTYPE_NONE = 0
    ETH32CFG_IFTYPE_OTHER = 1
    ETH32CFG_IFTYPE_ETHERNET = 6
    ETH32CFG_IFTYPE_TOKENRING = 9
    ETH32CFG_IFTYPE_FDDI = 15
    ETH32CFG_IFTYPE_PPP = 23
    ETH32CFG_IFTYPE_LOOPBACK = 24
    ETH32CFG_IFTYPE_SLIP = 28
End Enum

Public Enum Eth32ConfigFilter
    ETH32CFG_FILTER_NONE = 0
    ETH32CFG_FILTER_MAC = 1
    ETH32CFG_FILTER_SERIAL = 2
End Enum

Public Type Eth32ConfigPluginInterface
    ip As eth32cfg_ip
    netmask As eth32cfg_ip
    InterfaceType As Eth32ConfigInterfaceType
    StandardName As String
    FriendlyName As String
    Description As String
End Type

Public Declare Function eth32cfg_ip_to_string Lib "eth32api.dll" (ipbinary As eth32cfg_ip, ByVal ipstring As String) As Long
Public Declare Function eth32cfg_string_to_ip Lib "eth32api.dll" (ByVal ipstring As String, ipbinary As eth32cfg_ip) As Long

Public Declare Function eth32cfg_query Lib "eth32api.dll" (bcastaddr As eth32cfg_ip, ByRef number As Long, ByRef Result As Long) As Long
Public Declare Function eth32cfg_discover_ip Lib "eth32api.dll" (bcastaddr As eth32cfg_ip, ByVal flags As Long, macaddr As eth32cfg_mac, ByVal product_id As Byte, ByVal serialnum_batch As Integer, ByVal serialnum_unit As Integer, ByRef number As Long, ByRef Result As Long) As Long
Public Declare Function eth32cfg_get_config Lib "eth32api.dll" (ByVal handle As Long, ByVal index As Long, ByRef data As eth32cfg_data) As Long
Public Declare Function eth32cfg_set_config Lib "eth32api.dll" (bcastaddr As eth32cfg_ip, data As eth32cfg_data) As Long
Public Declare Function eth32cfg_serialnum_string Lib "eth32api.dll" (ByVal product_id As Byte, ByVal batch As Integer, ByVal unit As Integer, ByVal serialstring As String, ByVal bufsize As Long) As Long
Public Declare Sub eth32cfg_free Lib "eth32api.dll" (ByVal handle As Long)

Public Declare Function eth32cfg_plugin_load Lib "eth32api.dll" (ByVal pluginoption As Long) As Long
Public Declare Function eth32cfg_plugin_interface_list Lib "eth32api.dll" (ByRef numd As Long, ByRef Result As Long) As Long
Public Declare Function eth32cfg_plugin_interface_address Lib "eth32api.dll" (ByVal handle As Long, ByVal index As Long, ByRef ipaddr As eth32cfg_ip, ByRef netmask As eth32cfg_ip) As Long
Public Declare Function eth32cfg_plugin_interface_type Lib "eth32api.dll" (ByVal handle As Long, ByVal index As Long, ByRef devtype As Long) As Long
Public Declare Function eth32cfg_plugin_interface_name Lib "eth32api.dll" (ByVal handle As Long, ByVal index As Long, ByVal nametype As Long, ByVal name As String, ByRef length As Long) As Long
Public Declare Function eth32cfg_plugin_choose_interface Lib "eth32api.dll" (ByVal handle As Long, ByVal index As Long) As Long
Public Declare Sub eth32cfg_plugin_interface_list_free Lib "eth32api.dll" (ByVal handle As Long)



'************************************************
' Windows API function declarations
'************************************************
Public Declare Function mod_eth32_lstrcpy Lib "kernel32" Alias "lstrcpyA" (ByVal lpString1 As String, ByVal lpString2 As Long) As Long
Public Declare Function mod_eth32_lstrlen Lib "kernel32" Alias "lstrlenA" (ByVal lpString As Long) As Long
Public Declare Function mod_eth32_GetWindowLong Lib "user32" Alias "GetWindowLongA" (ByVal hwnd As Long, ByVal nIndex As Long) As Long
Public Declare Sub mod_eth32_copymemory Lib "kernel32" Alias "RtlMoveMemory" (dest As Any, src As Any, ByVal length As Long)

' Windows API constants
Public Const mod_eth32_WM_COMMAND = &H111
Public Const mod_eth32_GWL_ID = (-12)


Public Const ETH_SUCCESS    As Long = 0                  ' No error
Public Const ETH_GENERAL_ERROR   As Long = -1            ' Unknown or other error
Public Const ETH_CLOSING        As Long = -2             ' Function aborted since the device is being closed
Public Const ETH_NETWORK_ERROR    As Long = -10          ' unable to open, read, or write to the network socket
Public Const ETH_THREAD_ERROR    As Long = -11           ' General error ocurred in the threads library used by this API.
Public Const ETH_NOT_SUPPORTED    As Long = -12          ' returned if an API function is not supported by a device
Public Const ETH_PIPE_ERROR       As Long = -13          ' Internal API error
Public Const ETH_RTHREAD_ERROR     As Long = -14         ' Internal API error
Public Const ETH_ETHREAD_ERROR    As Long = -15          ' Internal API error
Public Const ETH_MALLOC_ERROR   As Long = -16            ' Problem involving allocating memory
Public Const ETH_WINDOWS_ERROR    As Long = -17          ' Internal API error - specific to Windows platform
Public Const ETH_WINSOCK_ERROR   As Long = -18           ' Internal API error - specific to Windows sockets
Public Const ETH_NETWORK_INTR    As Long = -19           ' Network read/write operation was interrupted.
Public Const ETH_WRONG_MODE     As Long = -20            ' Something is not configured correctly in order to allow this functionality.
Public Const ETH_BCAST_OPT      As Long = -21            ' Error setting SO_BROADCAST option on socket
Public Const ETH_REUSE_OPT      As Long = -22            ' Error setting SO_REUSEADDR option on socket
Public Const ETH_CFG_NOACK      As Long = -23            ' Really a warning - no acknowledgement after configuring IP settings of device
Public Const ETH_CFG_REJECT     As Long = -24            ' The device refused to set its IP configuration settings
Public Const ETH_LOADLIB        As Long = -25            ' Error loading an external DLL library
Public Const ETH_PLUGIN         As Long = -26            ' General error with plugin being used (for device discovery, sniffing, etc)
Public Const ETH_BUFSIZE        As Long = -27            ' A buffer provided is either invalid size or too small
Public Const ETH_INVALID_HANDLE As Long = -101         ' Invalid device handle was passed in
Public Const ETH_INVALID_PORT   As Long = -104          ' Port specified is invalid for the device model
Public Const ETH_INVALID_BIT    As Long = -109           ' Value passed identifying bit is out of range
Public Const ETH_INVALID_CHANNEL As Long = -111       ' Invalid channel number specified
Public Const ETH_INVALID_POINTER As Long = -112          ' Invalid pointer given as a parameter to an API function
Public Const ETH_INVALID_OTHER   As Long = -113          ' Some parameter passed to an API function was invalid or out of range
Public Const ETH_INVALID_VALUE   As Long = -114          ' Value out of possible range for that I/O port
Public Const ETH_INVALID_IP      As Long = -115          ' Invalid IP address was provided
Public Const ETH_INVALID_NETMASK As Long = -116          ' Invalid netmask was provided
Public Const ETH_INVALID_INDEX   As Long = -117          ' Invalid index value
Public Const ETH_TIMEOUT       As Long = -201            ' Timeout ocurred. Most likely communication has been lost w/ device


' ===========================
' Definitions to support the VB Class

' When raising errors, VB requires an offset to be added to the error numbers
' to avoid conflicts.  Define the offset here.
Public Const EthErrorOffset As Long = vbObjectError + 512

' Define the error codes used in the VB Class.
' In order to work better with the VB Error system, positive error codes
' are used instead of the negative codes used by the ETH32 API, so notice
' the minus sign on all these.  After the API-defined error codes, we also
' define any codes that are internal to the VB class
Public Enum EthError
    EthErrorNone = 0  ' No Error
    EthErrorGeneral = EthErrorOffset - ETH_GENERAL_ERROR
    EthErrorClosing = EthErrorOffset - ETH_CLOSING
    EthErrorNetwork = EthErrorOffset - ETH_NETWORK_ERROR
    EthErrorThread = EthErrorOffset - ETH_THREAD_ERROR
    EthErrorNotSupported = EthErrorOffset - ETH_NOT_SUPPORTED
    EthErrorPipe = EthErrorOffset - ETH_PIPE_ERROR
    EthErrorRthread = EthErrorOffset - ETH_RTHREAD_ERROR
    EthErrorEthread = EthErrorOffset - ETH_ETHREAD_ERROR
    EthErrorMalloc = EthErrorOffset - ETH_MALLOC_ERROR
    EthErrorWindows = EthErrorOffset - ETH_WINDOWS_ERROR
    EthErrorWinsock = EthErrorOffset - ETH_WINSOCK_ERROR
    EthErrorNetworkIntr = EthErrorOffset - ETH_NETWORK_INTR
    EthErrorWrongMode = EthErrorOffset - ETH_WRONG_MODE
    EthErrorBcastOpt = EthErrorOffset - ETH_BCAST_OPT
    EthErrorReuseOpt = EthErrorOffset - ETH_REUSE_OPT
    EthErrorConfigNoack = EthErrorOffset - ETH_CFG_NOACK
    EthErrorConfigReject = EthErrorOffset - ETH_CFG_REJECT
    EthErrorLoadlib = EthErrorOffset - ETH_LOADLIB
    EthErrorPlugin = EthErrorOffset - ETH_PLUGIN
    EthErrorBufsize = EthErrorOffset - ETH_BUFSIZE
    EthErrorInvalidHandle = EthErrorOffset - ETH_INVALID_HANDLE
    EthErrorInvalidPort = EthErrorOffset - ETH_INVALID_PORT
    EthErrorInvalidBit = EthErrorOffset - ETH_INVALID_BIT
    EthErrorInvalidChannel = EthErrorOffset - ETH_INVALID_CHANNEL
    EthErrorInvalidPointer = EthErrorOffset - ETH_INVALID_POINTER
    EthErrorInvalidOther = EthErrorOffset - ETH_INVALID_OTHER
    EthErrorInvalidValue = EthErrorOffset - ETH_INVALID_VALUE
    EthErrorInvalidIp = EthErrorOffset - ETH_INVALID_IP
    EthErrorInvalidNetmask = EthErrorOffset - ETH_INVALID_NETMASK
    EthErrorInvalidIndex = EthErrorOffset - ETH_INVALID_INDEX
    EthErrorTimeout = EthErrorOffset - ETH_TIMEOUT
    
    ' ========================
    ' Error codes for the VB class - these are not defined by the ETH32 API
    EthErrorAlreadyConnected = EthErrorOffset + 1000
    EthErrorNotConnected = EthErrorOffset + 1001
    
End Enum


' The number at which the VB Class error codes begin
Public Const EthErrorVBClass As Long = 1000


Public Function Eth32ErrorStringAPI(ByVal errorcode As Long) As String
    ' Converts an API error code into an error message string
    Dim str_ptr As Long
    Dim err_str_len As Long
    Dim tempstr As String
    
    
    str_ptr = eth32_error_string(errorcode)
    'Get string length
    err_str_len = mod_eth32_lstrlen(str_ptr)
    'Create buffer to hold error string
    tempstr = Space(err_str_len + 1)
        
    'Copy error string into buffer
    mod_eth32_lstrcpy tempstr, str_ptr
    Eth32ErrorStringAPI = Left(tempstr, err_str_len)
End Function

Public Function Eth32ErrorString(errorcode As EthError) As String
    ' Translate an EthError error code given by the Err object into string
    
    Dim str As String           'holds error string
    

    If (errorcode < EthErrorOffset + EthErrorVBClass) Then
        str = Eth32ErrorStringAPI(-(errorcode - EthErrorOffset))
    Else
        Select Case errorcode
            Case EthErrorAlreadyConnected
                str = "EthErrorAlreadyConnected: A connection is already open in this class instance.  Please disconnect first before reconnecting to another device."
            Case EthErrorNotConnected
                str = "EthErrorNotConnected: This class instance is not yet connected to a device."
            Case Else
                str = "Unrecognized error code"
        End Select
    End If
    
    Eth32ErrorString = str
End Function



