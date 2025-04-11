VERSION 5.00
Begin VB.Form frmButtons 
   BorderStyle     =   1  'Fixed Single
   Caption         =   "ETH32 Buttons Example"
   ClientHeight    =   1800
   ClientLeft      =   45
   ClientTop       =   435
   ClientWidth     =   4440
   LinkTopic       =   "Form1"
   MaxButton       =   0   'False
   ScaleHeight     =   1800
   ScaleWidth      =   4440
   StartUpPosition =   3  'Windows Default
   Begin VB.CommandButton cmdConnect 
      Caption         =   "Connect"
      Height          =   375
      Left            =   3360
      TabIndex        =   1
      Top             =   240
      Width           =   975
   End
   Begin VB.TextBox txtAddress 
      Height          =   285
      Left            =   1680
      TabIndex        =   0
      Text            =   "192.168.1.100"
      Top             =   240
      Width           =   1455
   End
   Begin VB.Label Label4 
      Caption         =   "Once connected, press external pushbuttons to update screen status and on-board LEDs."
      Height          =   855
      Left            =   240
      TabIndex        =   5
      Top             =   720
      Width           =   1695
   End
   Begin VB.Label Label3 
      Caption         =   "Button 1 Status"
      Height          =   255
      Left            =   2520
      TabIndex        =   4
      Top             =   1320
      Width           =   1455
   End
   Begin VB.Label Label2 
      Caption         =   "Button 0 Status"
      Height          =   255
      Left            =   2520
      TabIndex        =   3
      Top             =   840
      Width           =   1455
   End
   Begin VB.Shape button1 
      FillColor       =   &H000000FF&
      Height          =   375
      Left            =   2040
      Shape           =   1  'Square
      Top             =   1200
      Width           =   375
   End
   Begin VB.Shape button0 
      FillColor       =   &H000000FF&
      Height          =   375
      Left            =   2040
      Shape           =   1  'Square
      Top             =   720
      Width           =   375
   End
   Begin VB.Label Label1 
      Caption         =   "ETH32 Address:"
      Height          =   255
      Left            =   360
      TabIndex        =   2
      Top             =   240
      Width           =   1215
   End
End
Attribute VB_Name = "frmButtons"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
Option Explicit

Dim WithEvents dev As Eth32
Attribute dev.VB_VarHelpID = -1

Private Const ID_BUTTON0 As Long = 100
Private Const ID_BUTTON1 As Long = 101


Private Sub cmdConnect_Click()
    ' If we're already connected, disconnect
    If Not (dev Is Nothing) Then
        ' dev is at least instantiated
        If dev.Connected Then
            dev.Disconnect
        End If
    End If
    
    ' If dev is not instantiated, instantiate it
    If dev Is Nothing Then
        Set dev = New Eth32
    End If
    
    On Error GoTo err_handler
    
    ' Connect, use a 5 second timeout
    dev.Connect txtAddress.Text, ETH32_PORT, 5000
    
    ' Enable pullup resistors on pushbutton lines
    dev.OutputBit 0, 0, 1  ' Port 0, bit 0
    dev.OutputBit 0, 1, 1  ' Port 0, bit 1
    
    ' Enable events on both pushbutton lines
    dev.EnableEvent EVENT_DIGITAL, 0, 0, ID_BUTTON0
    dev.EnableEvent EVENT_DIGITAL, 0, 1, ID_BUTTON1
    
    
    MsgBox "Successfully connected to ETH32 device.", vbInformation
    
    
    Exit Sub
err_handler:
    MsgBox "Error connecting to the ETH32 device: " & dev.ErrorString(Err.Number)
    
End Sub


Private Sub dev_EventFired(ByVal id As Long, ByVal eventtype As Long, ByVal port As Long, ByVal bit As Long, ByVal prev_value As Long, ByVal value As Long, ByVal direction As Long)
    ' This is the event that is executed each time the ETH32 fires an
    ' event.  In this case, this means it is executed each time either
    ' pushbutton is pressed or released
    
    Select Case id
        Case ID_BUTTON0
            If value Then
                ' Button has just been released
                button0.FillStyle = 1 ' Make status square transparent
                dev.Led(0) = False ' Turn off LED on ETH32
            Else
                ' Button has just been pressed
                button0.FillStyle = 0 ' Make status square filled
                dev.Led(0) = True ' Turn on LED on ETH32
            End If
        Case ID_BUTTON1
            If value Then
                ' Button has just been released
                button1.FillStyle = 1 ' Make status square transparent
                dev.Led(1) = False ' Turn off LED on ETH32
            Else
                ' Button has just been pressed
                button1.FillStyle = 0 ' Make status square filled
                dev.Led(1) = True ' Turn on LED on ETH32
            End If
    
    End Select
    
End Sub

Private Sub Form_Unload(Cancel As Integer)
    ' When this form unloads, make sure the connection is closed, otherwise
    ' it will keep the application running.
    If Not (dev Is Nothing) Then
        ' dev is at least instantiated
        If dev.Connected Then
            dev.Disconnect
        End If
    End If
    
End Sub

