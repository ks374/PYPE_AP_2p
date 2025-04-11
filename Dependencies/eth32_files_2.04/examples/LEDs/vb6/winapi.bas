Attribute VB_Name = "winapi"
Option Explicit

Public Declare Function SendMessage Lib "user32" Alias "SendMessageA" ( _
                ByVal hwnd As Long, _
                ByVal wMsg As Long, _
                ByVal wParam As Long, _
                lParam As Any) As Long
Public Const CB_SHOWDROPDOWN = &H14F




