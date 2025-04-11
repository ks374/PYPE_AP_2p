VERSION 1.0 CLASS
BEGIN
  MultiUse = -1  'True
  Persistable = 0  'NotPersistable
  DataBindingBehavior = 0  'vbNone
  DataSourceBehavior  = 0  'vbNone
  MTSTransactionMode  = 0  'NotAnMTSObject
END
Attribute VB_Name = "Eth32Config"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = True
Attribute VB_PredeclaredId = False
Attribute VB_Exposed = False
' Winford Engineering ETH32 Configuration VB5/6 Class Library
' Copyright 2010
' www.winford.com
'
' This module is intended for use with Visual Basic 5/6 to provide
' an easy way for Visual Basic 5/6 developers to utilize the
' ETH32 Configuration and Discovery features of the ETH32 API (eth32api.dll)

Option Explicit



Private handle As Long         'handle to results
Private numresults_internal As Long
Private bcastaddr_internal As eth32cfg_ip

Private Function handle_error(ByVal ecode As Long, Optional ByVal apierror As Boolean = True) As EthError
    ' If API error is true, then this error code came directly from an API
    ' function and needs to be converted into the VB class error code.
    ' Otherwise, it is already a VB class error code
    
    If apierror Then
        ecode = EthErrorOffset + (-ecode)
    End If
    
    err.Raise ecode, "Eth32Config", Eth32ErrorString(ecode)

    handle_error = ecode
End Function

Private Sub Class_Initialize()
    handle = 0
    numresults_internal = 0
    bcastaddr_internal.buf(0) = 255 ' Default broadcast address
    bcastaddr_internal.buf(1) = 255
    bcastaddr_internal.buf(2) = 255
    bcastaddr_internal.buf(3) = 255
End Sub

Private Sub Class_Terminate()
    Call Free
End Sub

Public Sub Free()
    If (handle <> 0) Then
        eth32cfg_free handle
    End If
    handle = 0
    numresults_internal = 0
End Sub

Public Property Get NumResults() As Long
    NumResults = numresults_internal
End Property

Friend Function IpConvertToString(ipbinary As eth32cfg_ip) As String
    Dim ecode As Long
    Dim pos As Variant
    Dim Result As String
    
    Result = Space(20)
    ecode = eth32cfg_ip_to_string(ipbinary, Result)
    
    If ecode Then handle_error ecode
    
    pos = InStr(1, Result, Chr(0), vbBinaryCompare)
    
   
    If IsNull(pos) Then
        handle_error (ETH_INVALID_IP)
    End If
    
    If pos = 0 Then
        handle_error (ETH_INVALID_IP)
    End If

    IpConvertToString = Left(Result, pos - 1)
End Function

Friend Function IpConvert(ByVal ipstring As String) As eth32cfg_ip
    Dim Result As eth32cfg_ip
    Dim ecode As Long
    
    ecode = eth32cfg_string_to_ip(ipstring, Result)
    
    If ecode Then handle_error ecode

    IpConvert = Result
End Function

Friend Function MacConvertToString(macbinary As eth32cfg_mac) As String
    Dim Result As String
    Dim temp As String
    Dim i As Long
    
    For i = 0 To 5
        temp = Hex(macbinary.buf(i))
        If Len(temp) = 1 Then temp = "0" & temp
        
        Result = Result & temp
        If i < 5 Then
            Result = Result & ":"
        End If
    Next
    
    MacConvertToString = Result
End Function

Friend Function MacConvert(ByVal macstring As String) As eth32cfg_mac
    Dim Result As eth32cfg_mac
    Dim i As Long
    Dim pos As Variant
    
    On Error GoTo macconvert_err
    
    ' Get the first five octets
    For i = 0 To 4
        pos = InStr(1, macstring, ":")
        If IsNull(pos) Then GoTo macconvert_err
        If pos < 2 Or pos > 3 Then GoTo macconvert_err
        Result.buf(i) = CByte("&H" & Left(macstring, pos - 1))
    
        macstring = Mid(macstring, pos + 1)
    Next i
    
    Result.buf(5) = CByte("&H" & macstring)
    
    
    MacConvert = Result
    Exit Function
macconvert_err:
    handle_error (ETH_INVALID_OTHER)
    
    
End Function

Friend Property Get BroadcastAddress() As eth32cfg_ip
    BroadcastAddress = bcastaddr_internal
End Property

Friend Property Let BroadcastAddress(ByRef bcast As eth32cfg_ip)
    bcastaddr_internal = bcast
End Property

Public Property Get BroadcastAddressString() As String
    BroadcastAddressString = IpConvertToString(bcastaddr_internal)
End Property

Public Property Let BroadcastAddressString(bcast As String)
    ' This will raise an error if an invalid string is provided
    bcastaddr_internal = IpConvert(bcast)
End Property

Public Function SerialNumString(ByVal product_id As Byte, ByVal serialnum_batch As Integer, ByVal serialnum_unit As Integer) As String
    Dim Result As String
    Dim ecode As Long
    Dim pos As Long
    
    Result = Space(100)
    
    ecode = eth32cfg_serialnum_string(product_id, serialnum_batch, serialnum_unit, Result, Len(Result))
    If ecode Then handle_error (ecode)
    
    pos = InStr(1, Result, Chr(0), vbBinaryCompare)
    
   
    If IsNull(pos) Then
        handle_error (ETH_INVALID_OTHER)
    End If
    
    If pos = 0 Then
        handle_error (ETH_INVALID_OTHER)
    End If

    SerialNumString = Left(Result, pos - 1)
End Function

Public Function Query() As Long
    Dim number As Long
    Dim temp As Long
    Dim ecode As Long
    
    temp = eth32cfg_query(bcastaddr_internal, number, ecode)
    
    If temp = 0 Then handle_error (ecode)
    
    ' If successful, go ahead and free the previous results before we overwrite the pointer (if applicable)
    If handle <> 0 Then Call Free

    handle = temp
    numresults_internal = number
    
    Query = numresults_internal
End Function

Friend Function DiscoverIp(ByVal filter As Eth32ConfigFilter, mac As eth32cfg_mac, ByVal product_id As Byte, ByVal serialnum_batch As Integer, ByVal serialnum_unit As Integer) As Long
    Dim number As Long
    Dim temp As Long
    Dim ecode As Long

    temp = eth32cfg_discover_ip(bcastaddr_internal, filter, mac, product_id, serialnum_batch, serialnum_unit, number, ecode)

    If temp = 0 Then handle_error (ecode)
    
    ' If successful, go ahead and free the previous results before we overwrite the pointer (if applicable)
    If handle <> 0 Then Call Free

    handle = temp
    numresults_internal = number
    
    DiscoverIp = numresults_internal
End Function

Friend Sub SetConfig(config_data As eth32cfg_data)
    Dim ecode As Long
    
    ecode = eth32cfg_set_config(bcastaddr_internal, config_data)
    
    If ecode Then handle_error (ecode)

End Sub

Friend Property Get Result(ByVal index As Long) As eth32cfg_data
    Dim resultdata As eth32cfg_data
    Dim ecode As Long
    
    If index < 0 Or index >= numresults_internal Then handle_error (ETH_INVALID_INDEX)

    ecode = eth32cfg_get_config(handle, index, resultdata)
    If ecode <> 0 Then handle_error (ecode)
    
    Result = resultdata
End Property


