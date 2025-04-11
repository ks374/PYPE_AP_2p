VERSION 1.0 CLASS
BEGIN
  MultiUse = -1  'True
  Persistable = 0  'NotPersistable
  DataBindingBehavior = 0  'vbNone
  DataSourceBehavior  = 0  'vbNone
  MTSTransactionMode  = 0  'NotAnMTSObject
END
Attribute VB_Name = "Eth32"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = True
Attribute VB_PredeclaredId = False
Attribute VB_Exposed = False
' Winford Engineering ETH32 VB5/6 Class Library
' Copyright 2005
' www.winford.com
'
' This module is intended for use with Visual Basic 5/6 to provide
' an easy way for Visual Basic 5/6 developers to utilize the functionality
' provided by the ETH32 API (eth32api.dll)

Option Explicit

Private handle As Long         'handle to connection, or 0 if not connected
Private event_window As eth32_form ' reference to event helper form for this class instance
Dim firmware_major As Long ' We cache the firmware version at Connect for compatibility checks
Dim firmware_minor As Long
Private serial_batch As Long
Private serial_unit As Long
Private serial_have As Boolean

' Class events
Public Event EventFired(ByVal id As Long, ByVal eventtype As Long, ByVal port As Long, ByVal bit As Long, ByVal prev_value As Long, ByVal value As Long, ByVal direction As Long)


Private Function handle_error(ByVal ecode As Long, Optional ByVal apierror As Boolean = True) As EthError
    ' If API error is true, then this error code came directly from an API
    ' function and needs to be converted into the VB class error code.
    ' Otherwise, it is already a VB class error code
    
    If apierror Then
        ecode = EthErrorOffset + (-ecode)
    End If
    
    Err.Raise ecode, "eth32", ErrorString(ecode)

    handle_error = ecode
End Function

Private Function precheck() As EthError
    ' This function checks that everything is OK before running user-accessible code
    ' It returns an error code if there is a problem.
    ' It also raises the error if the class is configured to do so
    
    If (handle = 0) Then
        precheck = EthErrorNotConnected
    Else
        precheck = EthErrorNone
    End If
    
    If (precheck <> EthErrorNone) Then
        handle_error precheck, False
    End If
    
End Function

Public Sub CheckEvents()
    ' Check the API event queue to see if any events are waiting
    ' This is meant to be called from the helper form when the button on the
    ' form receives a Click event, but it can also be called directly by the user
    ' to initiate processing any events in the queue.
    ' That can be useful for example if the code is in a long loop that doesn't
    ' allow any windows messages to be processed.
    
    Dim details As eth32_event
    
    ' no precheck necessary at this point
    
    Do While (eth32_dequeue_event(handle, details, 0) = 0)
        RaiseEvent EventFired(details.id, details.type, details.port, details.bit, details.prev_value, details.value, details.direction)
    Loop


End Sub

Public Sub Connect(ByVal address As String, Optional ByVal port As Long = ETH32_PORT, Optional ByVal Timeout As Long = 0)
    ' This function is named connect instead of open because open is reserved in VB
    Dim result As Long
    Dim h As Long
    Dim temp As Long
    Dim handler As eth32_handler
    
    ' See if this instance of the class already has a board connected
    If Connected() Then
        handle_error EthErrorAlreadyConnected, False
        Exit Sub
    End If
    
    
    h = eth32_open(address, port, Timeout, result)
    
    If (h) Then
        handle = h
    Else
        handle_error result
        Exit Sub
    End If
    
    ' Configure event handling
    ' Create a new form
    Set event_window = New eth32_form
    
    ' Set its object reference so it knows how to call us - this isn't a real
    ' VB object reference - it's what some people call a weak reference which allows
    ' us to create a real reference, but doesn't increment the usage count, and therefore
    ' doesn't create a circular reference
    mod_eth32_copymemory temp, Me, 4
    event_window.eth32_object = temp
    
    ' Configure the API's event handler mechanism to send a windows message
    ' emulating a mouse click when a new event arrives
    handler.type = HANDLER_MESSAGE
    handler.window = event_window.hwnd
    handler.msgid = mod_eth32_WM_COMMAND
    handler.wparam = mod_eth32_GetWindowLong(event_window.event_notify.hwnd, mod_eth32_GWL_ID)
    handler.lparam = event_window.event_notify.hwnd

    result = eth32_set_event_handler(handle, handler)
    If (result) Then
        Call eth32_close(handle)
        handle = 0
        handle_error result
        Exit Sub
    End If
    
    ' Now enable the event queue with a default queue size and by default
    ' if the queue were to overrun, discard the new events and leave the
    ' full queue intact.
    result = eth32_set_event_queue_config(handle, 1000, QUEUE_DISCARD_NEW)
    If (result) Then
        Call eth32_close(handle)
        handle = 0
        handle_error result
        Exit Sub
    End If
    
    ' cache the firmware version of the connected device
    ' for any functionality that depends on a certain version.
    firmware_major = FirmwareMajor
    firmware_minor = FirmwareMinor
    
    
End Sub

Public Property Let Timeout(ByVal value As Long)
    Dim result As Long
    
    precheck
    
    result = eth32_set_timeout(handle, value)
    If (result) Then
        handle_error result
        Exit Property
    End If
    
End Property

Public Property Get Timeout() As Long
    Dim result As Long
    Dim value As Long
    
    precheck
    
    result = eth32_get_timeout(handle, value)
    If (result) Then
        handle_error result
        Exit Property
    End If
    
    Timeout = value
End Property

Public Property Get Connected() As Boolean
    ' This property simply indicates whether a connection has been opened
    ' with this object instance.  It does not actually send any traffic
    ' to verify the connection is still good.
    
    If (handle) Then
        Connected = True
    Else
        Connected = False
    End If
    
End Property
Public Sub VerifyConnection()
    Dim result As Long
    
    precheck
    
    result = eth32_verify_connection(handle)
    
    If (result) Then
        handle_error result
        Exit Sub
    End If
    
End Sub

Public Sub OutputByte(ByVal port As Long, ByVal value As Long)
    Dim result As Long
    
    precheck
    
    result = eth32_output_byte(handle, port, value)
    
    If (result) Then
        handle_error result
        Exit Sub
    End If
    
End Sub

Public Sub OutputBit(ByVal port As Long, ByVal bit As Long, ByVal value As Long)
    Dim result As Long
    
    precheck
    
    result = eth32_output_bit(handle, port, bit, value)
    
    If (result) Then
        handle_error result
        Exit Sub
    End If
    
End Sub

Public Sub PulseBit(ByVal port As Long, ByVal bit As Long, ByVal edge As Eth32PulseEdge, ByVal count As Long)
    Dim result As Long
    
    precheck
    
    result = eth32_pulse_bit(handle, port, bit, edge, count)
    
    If (result) Then
        handle_error result
        Exit Sub
    End If

End Sub

Public Property Let Led(ByVal lednum As Long, ByVal state As Boolean)
    Dim result As Long
    Dim value As Long
   
    precheck
    
    If state Then
        value = 1
    Else
        value = 0
    End If
    result = eth32_set_led(handle, lednum, value)
    
    If (result) Then
        handle_error result
        Exit Property
    End If

End Property

Public Property Get Led(ByVal lednum As Long) As Boolean
    Dim result As Long
    Dim value As Long
    
    precheck
    
    result = eth32_get_led(handle, lednum, value)
    
    If (result) Then
        handle_error result
        Exit Property
    End If

    Led = value
End Property

Public Function InputByte(ByVal port As Long) As Long
    Dim result As Long
    Dim value As Long
    
    precheck
    
    result = eth32_input_byte(handle, port, value)
    
    If (result) Then
        handle_error result
        Exit Function
    End If

    InputByte = value
    
End Function

Public Function InputSuccessive(ByVal port As Long, ByVal maxcount As Long, ByRef status As Long) As Long
    ' Passes the status (whether or not a consistent value was found and how many
    ' reads it took) out by reference in the status parameter
    Dim result As Long
    Dim value As Long
    
    precheck
    
    result = eth32_input_successive(handle, port, maxcount, value, status)
    
    If (result) Then
        handle_error result
        Exit Function
    End If

    InputSuccessive = value
    
End Function

Public Function InputBit(ByVal port As Long, ByVal bit As Long) As Long
    Dim result As Long
    Dim value As Long
    
    precheck
    
    result = eth32_input_bit(handle, port, bit, value)
    
    If (result) Then
        handle_error result
        Exit Function
    End If

    InputBit = value
    
End Function

Public Function Readback(ByVal port As Long) As Long
    Dim result As Long
    Dim value As Long
    
    precheck
    
    result = eth32_readback(handle, port, value)
    
    If (result) Then
        handle_error result
        Exit Function
    End If

    Readback = value
    
End Function

Public Sub SetDirection(ByVal port As Long, ByVal direction As Long)
    Dim result As Long
    
    precheck
    
    result = eth32_set_direction(handle, port, direction)
    
    If (result) Then
        handle_error result
        Exit Sub
    End If

    
End Sub


Public Function GetDirection(ByVal port As Long) As Long
    Dim result As Long
    Dim value As Long
    
    precheck
    
    result = eth32_get_direction(handle, port, value)
    
    If (result) Then
        handle_error result
        Exit Function
    End If

    GetDirection = value
    
End Function

Public Sub SetDirectionBit(ByVal port As Long, ByVal bit As Long, ByVal direction As Long)
    Dim result As Long
    
    precheck
    
    result = eth32_set_direction_bit(handle, port, bit, direction)
    
    If (result) Then
        handle_error result
        Exit Sub
    End If

    
End Sub

Public Function GetDirectionBit(ByVal port As Long, ByVal bit As Long) As Long
    Dim result As Long
    Dim value As Long
    
    precheck
    
    result = eth32_get_direction_bit(handle, port, bit, value)
    
    If (result) Then
        handle_error result
        Exit Function
    End If

    GetDirectionBit = value
    
End Function

Public Property Let AnalogState(state As Eth32AnalogState)
    Dim result As Long
    
    precheck
    
    result = eth32_set_analog_state(handle, state)
    
    If (result) Then
        handle_error result
        Exit Property
    End If

End Property

Public Property Get AnalogState() As Eth32AnalogState
    Dim result As Long
    Dim value As Long
    
    precheck
    
    result = eth32_get_analog_state(handle, value)
    
    If (result) Then
        handle_error result
        Exit Property
    End If
    
    AnalogState = value
    
End Property

Public Property Let AnalogReference(ref As Eth32AnalogReference)
    Dim result As Long
    
    precheck
    
    result = eth32_set_analog_reference(handle, ref)
    
    If (result) Then
        handle_error result
        Exit Property
    End If

End Property

Public Property Get AnalogReference() As Eth32AnalogReference
    Dim result As Long
    Dim value As Long
    
    precheck
    
    result = eth32_get_analog_reference(handle, value)
    
    If (result) Then
        handle_error result
        Exit Property
    End If
    
    AnalogReference = value
    
End Property

Public Function InputAnalog(ByVal channel As Long) As Long
    Dim result As Long
    Dim value As Long
    
    precheck
    
    result = eth32_input_analog(handle, channel, value)
    
    If (result) Then
        handle_error result
        Exit Function
    End If
    
    InputAnalog = value
End Function

Public Sub SetAnalogEventDef(ByVal bank As Long, ByVal channel As Long, ByVal lomark As Long, ByVal himark As Long, ByVal defaultval As Eth32AnalogEvtDef)
    Dim result As Long
    
    precheck
    
    result = eth32_set_analog_eventdef(handle, bank, channel, lomark, himark, defaultval)

    If (result) Then
        handle_error result
        Exit Sub
    End If
End Sub

Public Sub GetAnalogEventDef(ByVal bank As Long, ByVal channel As Long, ByRef lomark As Long, ByRef himark As Long)
    Dim result As Long
    
    precheck
    
    result = eth32_get_analog_eventdef(handle, bank, channel, lomark, himark)

    If (result) Then
        handle_error result
        Exit Sub
    End If
End Sub

Public Property Let AnalogAssignment(ByVal channel As Long, ByVal source As Eth32AnalogChannel)
    Dim result As Long
    
    precheck
    
    result = eth32_set_analog_assignment(handle, channel, source)

    If (result) Then
        handle_error result
        Exit Property
    End If

End Property

Public Property Get AnalogAssignment(ByVal channel As Long) As Eth32AnalogChannel
    Dim result As Long
    Dim value As Long
    
    precheck
    
    result = eth32_get_analog_assignment(handle, channel, value)

    If (result) Then
        handle_error result
        Exit Property
    End If
    
    AnalogAssignment = value
    
End Property

Public Sub ResetDevice()
    Dim result As Long
    
    precheck
    
    result = eth32_reset(handle)

    If (result) Then
        handle_error result
        Exit Sub
    End If
End Sub

Public Property Get SerialNum() As String
    Dim result As Long
    Dim sernum As String
    
    precheck
    
    sernum = String(100, " ")
    
    result = eth32_get_serialnum_string(handle, sernum, Len(sernum))

    If (result) Then
        handle_error result
        Exit Property
    End If
    
    
    SerialNum = Trim(sernum)
    
End Property

Public Property Get ProductID() As Long
    Dim result As Long
    Dim value As Long
    
    precheck
    
    result = eth32_get_product_id(handle, value)

    If (result) Then
        handle_error result
        Exit Property
    End If
    
    ProductID = value
    
End Property

Private Sub get_serial()
    Dim result As Long
    Dim value As Long
    Dim temp_batch As Long
    Dim temp_unit As Long
        
    precheck
    
    result = eth32_get_serialnum(handle, temp_batch, temp_unit)
    
    If (result) Then
        handle_error result
        Exit Sub
    End If
    
    serial_batch = temp_batch
    serial_unit = temp_unit
    serial_have = True
    

End Sub

Public Property Get SerialBatch() As Long

    If serial_have = False Then get_serial
    
    SerialBatch = serial_batch
End Property

Public Property Get SerialUnit() As Long
    
    If serial_have = False Then get_serial
    
    SerialUnit = serial_unit
End Property

Public Property Get FirmwareMajor() As Long
    Dim result As Long
    Dim value As Long
    Dim junk As Long
    
    precheck
   
    result = eth32_get_firmware_release(handle, value, junk)

    If (result) Then
        handle_error result
        Exit Property
    End If
    
    FirmwareMajor = value
    
End Property

Public Property Get FirmwareMinor() As Long
    Dim result As Long
    Dim value As Long
    Dim junk As Long
    
    precheck
    
    result = eth32_get_firmware_release(handle, junk, value)

    If (result) Then
        handle_error result
        Exit Property
    End If
    
    FirmwareMinor = value
    
End Property

Public Function ConnectionFlags(ByVal reset As Long) As Eth32ConnectionFlag
    Dim result As Long
    Dim value As Long
    
    precheck
    
    result = eth32_connection_flags(handle, reset, value)

    If (result) Then
        handle_error result
        Exit Function
    End If
    
    ConnectionFlags = value

End Function

Public Property Let EventQueueLimit(ByVal limit As Long)
    Dim result As Long
    Dim fullqueue As Long
    Dim junk As Long
    Dim junk2 As Long
    
    precheck

    result = eth32_get_event_queue_status(handle, junk, fullqueue, junk2)
    If (result) Then
        handle_error result
        Exit Property
    End If
    
    result = eth32_set_event_queue_config(handle, limit, fullqueue)
    If (result) Then
        handle_error result
        Exit Property
    End If
    
End Property

Public Property Get EventQueueLimit() As Long
    Dim result As Long
    Dim value As Long
    Dim junk As Long
    Dim junk2 As Long

    precheck

    result = eth32_get_event_queue_status(handle, value, junk, junk2)
    If (result) Then
        handle_error result
        Exit Property
    End If
    
    EventQueueLimit = value

End Property

Public Property Let EventQueueMode(ByVal mode As Eth32QueueMode)
    Dim result As Long
    Dim limit As Long
    Dim junk As Long
    Dim junk2 As Long
    
    precheck

    result = eth32_get_event_queue_status(handle, limit, junk, junk2)
    If (result) Then
        handle_error result
        Exit Property
    End If
    
    result = eth32_set_event_queue_config(handle, limit, mode)
    If (result) Then
        handle_error result
        Exit Property
    End If
    
End Property

Public Property Get EventQueueMode() As Eth32QueueMode
    Dim result As Long
    Dim value As Long
    Dim junk As Long
    Dim junk2 As Long

    precheck

    result = eth32_get_event_queue_status(handle, junk, value, junk2)
    If (result) Then
        handle_error result
        Exit Property
    End If
    
    EventQueueMode = value

End Property

Public Property Get EventQueueCurrentSize() As Long
    ' Indicates number of event messages currently in the event queue
    Dim result As Long
    Dim value As Long
    Dim junk As Long
    Dim junk2 As Long

    precheck

    result = eth32_get_event_queue_status(handle, junk, junk2, value)
    If (result) Then
        handle_error result
        Exit Property
    End If
    
    EventQueueCurrentSize = value
    
End Property

Public Sub EmptyEventQueue()
    Dim result As Long

    precheck

    result = eth32_empty_event_queue(handle)
    If (result) Then
        handle_error result
        Exit Sub
    End If

End Sub

Public Sub EnableEvent(ByVal eventtype As Eth32EventType, ByVal port As Long, ByVal bit As Long, ByVal id As Long)
    Dim result As Long

    precheck

    result = eth32_enable_event(handle, eventtype, port, bit, id)
    If (result) Then
        handle_error result
        Exit Sub
    End If
End Sub

Public Sub DisableEvent(ByVal eventtype As Eth32EventType, ByVal port As Long, ByVal bit As Long)
    Dim result As Long

    precheck

    result = eth32_disable_event(handle, eventtype, port, bit)
    If (result) Then
        handle_error result
        Exit Sub
    End If
End Sub

Public Property Let CounterState(ByVal counter As Long, ByVal state As Eth32CounterState)
    Dim result As Long

    precheck

    result = eth32_set_counter_state(handle, counter, state)
    If (result) Then
        handle_error result
        Exit Property
    End If

End Property

Public Property Get CounterState(ByVal counter As Long) As Eth32CounterState
    Dim result As Long
    Dim value As Long

    precheck

    result = eth32_get_counter_state(handle, counter, value)
    If (result) Then
        handle_error result
        Exit Property
    End If
    
    CounterState = value
End Property

Public Property Let CounterValue(ByVal counter As Long, ByVal value As Long)
    Dim result As Long

    precheck

    result = eth32_set_counter_value(handle, counter, value)
    If (result) Then
        handle_error result
        Exit Property
    End If

End Property

Public Property Get CounterValue(ByVal counter As Long) As Long
    Dim result As Long
    Dim value As Long

    precheck

    result = eth32_get_counter_value(handle, counter, value)
    If (result) Then
        handle_error result
        Exit Property
    End If

    CounterValue = value
End Property

Public Property Let CounterRollover(ByVal counter As Long, ByVal value As Long)
    Dim result As Long

    precheck

    result = eth32_set_counter_rollover(handle, counter, value)
    If (result) Then
        handle_error result
        Exit Property
    End If

End Property

Public Property Get CounterRollover(ByVal counter As Long) As Long
    Dim result As Long
    Dim value As Long

    precheck

    result = eth32_get_counter_rollover(handle, counter, value)
    If (result) Then
        handle_error result
        Exit Property
    End If

    CounterRollover = value
End Property


Public Property Let CounterThreshold(ByVal counter As Long, ByVal value As Long)
    Dim result As Long

    precheck

    result = eth32_set_counter_threshold(handle, counter, value)
    If (result) Then
        handle_error result
        Exit Property
    End If

End Property


Public Property Get CounterThreshold(ByVal counter As Long) As Long
    Dim result As Long
    Dim value As Long

    precheck

    result = eth32_get_counter_threshold(handle, counter, value)
    If (result) Then
        handle_error result
        Exit Property
    End If

    CounterThreshold = value
End Property


Public Property Let PwmClockState(ByVal state As Eth32PwmClock)
    Dim result As Long

    precheck

    result = eth32_set_pwm_clock_state(handle, state)
    If (result) Then
        handle_error result
        Exit Property
    End If

End Property

Public Property Get PwmClockState() As Eth32PwmClock
    Dim result As Long
    Dim value As Long

    precheck

    result = eth32_get_pwm_clock_state(handle, value)
    If (result) Then
        handle_error result
        Exit Property
    End If

    PwmClockState = value
End Property


Public Property Let PwmBasePeriod(ByVal period As Long)
    Dim result As Long

    precheck

    result = eth32_set_pwm_base_period(handle, period)
    If (result) Then
        handle_error result
        Exit Property
    End If

End Property

Public Property Get PwmBasePeriod() As Long
    Dim result As Long
    Dim value As Long

    precheck

    result = eth32_get_pwm_base_period(handle, value)
    If (result) Then
        handle_error result
        Exit Property
    End If

    PwmBasePeriod = value
End Property

Public Property Let PwmChannel(ByVal channel As Long, ByVal state As Eth32PwmChannel)
    Dim result As Long

    precheck

    result = eth32_set_pwm_channel(handle, channel, state)
    If (result) Then
        handle_error result
        Exit Property
    End If

End Property

Public Property Get PwmChannel(ByVal channel As Long) As Eth32PwmChannel
    Dim result As Long
    Dim value As Long

    precheck

    result = eth32_get_pwm_channel(handle, channel, value)
    If (result) Then
        handle_error result
        Exit Property
    End If

    PwmChannel = value
End Property

Public Property Let PwmDutyPeriod(ByVal channel As Long, ByVal period As Long)
    Dim result As Long

    precheck

    result = eth32_set_pwm_duty_period(handle, channel, period)
    If (result) Then
        handle_error result
        Exit Property
    End If

End Property

Public Property Get PwmDutyPeriod(ByVal channel As Long) As Long
    Dim result As Long
    Dim value As Long

    precheck

    result = eth32_get_pwm_duty_period(handle, channel, value)
    If (result) Then
        handle_error result
        Exit Property
    End If

    PwmDutyPeriod = value
End Property

Public Sub SetPwmParameters(ByVal channel As Long, ByVal state As Eth32PwmChannel, ByVal freq As Single, ByVal duty As Single)
    Dim result As Long

    precheck

    result = eth32_set_pwm_parameters(handle, channel, state, freq, duty)
    If (result) Then
        handle_error result
        Exit Sub
    End If
End Sub

Public Sub GetPwmParameters(ByVal channel As Long, ByRef state As Eth32PwmChannel, ByRef freq As Single, ByRef duty As Single)
    Dim result As Long
    Dim value As Long

    precheck

    result = eth32_get_pwm_parameters(handle, channel, value, freq, duty)
    If (result) Then
        handle_error result
        Exit Sub
    End If
    
    state = value
End Sub

Public Function GetEeprom(address As Long, length As Long) As Byte()
    Dim result As Long
    Dim results() As Byte
    
    
    precheck
    
    If firmware_major < 3 Then handle_error ETH_NOT_SUPPORTED
    
    ' Specific check to ETH32 -- but keeps us from inadvertently trying
    ' to allocate a huge amount of memory:
    If length > 256 Then handle_error ETH_INVALID_OTHER
    
    ReDim results(length - 1) ' specify the upper bound index
    
    result = eth32_get_eeprom(handle, address, length, results(0))
    If result <> 0 Then handle_error result
     
    GetEeprom = results
End Function

Public Sub SetEeprom(address As Long, length As Long, buffer() As Byte)
    Dim result As Long

    precheck
    
    If firmware_major < 3 Then handle_error ETH_NOT_SUPPORTED
    
    If UBound(buffer) < (length - 1) Then handle_error ETH_BUFSIZE
    
    result = eth32_set_eeprom(handle, address, length, buffer(0))
    
    If result <> 0 Then handle_error result
    
    
End Sub



Public Sub Disconnect()
    ' This function is named disconnect instead of close because close is reserved in VB
    Dim result As Long
    
    
    ' Close the board if it is open
    If (handle) Then
        result = eth32_close(handle)
        If (result) Then
            handle_error result
            Exit Sub
        End If
    
        ' Now destroy the window for handling events
        Unload event_window
        Set event_window = Nothing
        
        ' Indicate that there is no active connection
        handle = 0
    End If
    
End Sub




Public Function ErrorString(errorcode As EthError) As String
    ' Translate an error code given by the Err object into string
    
    Dim str_ptr As Long         'pointer to error string
    Dim err_str_len             'length of error string
    Dim str As String           'holds error string
    

    If (errorcode < EthErrorOffset + EthErrorVBClass) Then
        ' This is an API Error - get the error string from the API
        'Get pointer to error string
        str_ptr = eth32_error_string(-(errorcode - EthErrorOffset))
        'Get string length
        err_str_len = mod_eth32_lstrlen(str_ptr)
        'Create buffer to hold error string
        str = Space(err_str_len + 1)
        
        'Copy error string into buffer
        mod_eth32_lstrcpy str, str_ptr
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
    
    ErrorString = str
End Function

Private Sub Class_Initialize()
    handle = 0

End Sub

Private Sub Class_Terminate()
    ' When the class is being unloaded, if the connection is still open, close it
    
    
    If Connected() Then
        Me.Disconnect
    End If
    
End Sub
