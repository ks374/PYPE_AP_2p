VERSION 1.0 CLASS
BEGIN
  MultiUse = -1  'True
  Persistable = 0  'NotPersistable
  DataBindingBehavior = 0  'vbNone
  DataSourceBehavior  = 0  'vbNone
  MTSTransactionMode  = 0  'NotAnMTSObject
END
Attribute VB_Name = "Eth32ConfigPlugin"
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
' The Eth32ConfigPlugin class provides an interface to loading "plugins"
' into the ETH32 Configuration API, as well as a method for detecting the
' network interfaces that are present on the PC.

Option Explicit


Private handle As Long         'handle to results
Private numresults_internal As Long


Private Function handle_error(ByVal ecode As Long, Optional ByVal apierror As Boolean = True) As EthError
    ' If API error is true, then this error code came directly from an API
    ' function and needs to be converted into the VB class error code.
    ' Otherwise, it is already a VB class error code
    
    If apierror Then
        ecode = EthErrorOffset + (-ecode)
    End If
    
    err.Raise ecode, "Eth32ConfigPlugin", Eth32ErrorString(ecode)

    handle_error = ecode
End Function

Private Sub Class_Initialize()
    handle = 0
    numresults_internal = 0
End Sub

Private Sub Class_Terminate()
    Call Free
End Sub

Public Sub Free()
    If (handle <> 0) Then
        eth32cfg_plugin_interface_list_free handle
    End If
    handle = 0
    numresults_internal = 0
End Sub

Public Sub Load(ByVal plugin_type As Eth32ConfigPluginType)
    Dim ecode As Long
    
    ecode = eth32cfg_plugin_load(plugin_type)
    
    If ecode <> 0 Then handle_error ecode
    
End Sub

Public Property Get NumInterfaces() As Long
    NumInterfaces = numresults_internal
End Property

Public Function GetInterfaces() As Long
    Dim ecode As Long
    Dim number As Long
    Dim temphandle As Long
    

    temphandle = eth32cfg_plugin_interface_list(number, ecode)
    If ecode <> 0 Then handle_error ecode
    
    ' Just in case we have previous results to free:
    Free
    
    handle = temphandle
    numresults_internal = number
    
    GetInterfaces = numresults_internal
End Function

Public Sub ChooseInterface(ByVal index As Long)
    Dim ecode As Long
            
    If index < 0 Or index >= numresults_internal Then handle_error ETH_INVALID_INDEX

    ecode = eth32cfg_plugin_choose_interface(handle, index)
    
    If ecode <> 0 Then handle_error (ecode)
    
End Sub

Private Function get_name(ByVal index As Long, ByVal name_type As Eth32ConfigInterfaceName) As String
    Dim ecode As Long
    Dim length As Long
    Dim namestring As String
    
    length = 0
    ecode = eth32cfg_plugin_interface_name(handle, index, name_type, namestring, length)
    If ecode = ETH_BUFSIZE Then
        namestring = Space(length)
        ecode = eth32cfg_plugin_interface_name(handle, index, name_type, namestring, length)
        If ecode = 0 Then
            get_name = Left(namestring, length - 1)
            Exit Function
        ElseIf ecode = ETH_NOT_SUPPORTED Then
            get_name = ""
            Exit Function
        Else
            handle_error ecode
        End If
    ElseIf ecode = ETH_NOT_SUPPORTED Then
        get_name = ""
        Exit Function
    Else
        handle_error ecode
    End If
End Function

Friend Property Get NetworkInterface(ByVal index As Long) As Eth32ConfigPluginInterface
    Dim iface As Eth32ConfigPluginInterface
    Dim ecode As Long
    Dim ip As eth32cfg_ip
    Dim netmask As eth32cfg_ip
    Dim iftype As Long
    
    If index < 0 Or index >= numresults_internal Then handle_error ETH_INVALID_INDEX
    

    ecode = eth32cfg_plugin_interface_address(handle, index, ip, netmask)
    If ecode = 0 Then
        iface.ip = ip
        iface.netmask = netmask
    ElseIf ecode = ETH_NOT_SUPPORTED Then
        ' Leave the ip/netmask zeroed out in the structure
    Else
        handle_error ecode
    End If
    
    iface.StandardName = get_name(index, ETH32CFG_IFACENAME_STANDARD)
    iface.FriendlyName = get_name(index, ETH32CFG_IFACENAME_FRIENDLY)
    iface.Description = get_name(index, ETH32CFG_IFACENAME_DESCRIPTION)
    
    ecode = eth32cfg_plugin_interface_type(handle, index, iftype)
    If ecode = 0 Then
        iface.InterfaceType = iftype
    ElseIf ecode = ETH_NOT_SUPPORTED Then
        iface.InterfaceType = ETH32CFG_IFTYPE_NONE
    Else
        handle_error ecode
    End If
        
    NetworkInterface = iface
End Property
