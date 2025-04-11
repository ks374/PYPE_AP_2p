VERSION 5.00
Begin VB.Form frmAnalog 
   BorderStyle     =   1  'Fixed Single
   Caption         =   "ETH32 Analog Example"
   ClientHeight    =   3990
   ClientLeft      =   45
   ClientTop       =   435
   ClientWidth     =   6030
   LinkTopic       =   "Form1"
   MaxButton       =   0   'False
   ScaleHeight     =   3990
   ScaleWidth      =   6030
   StartUpPosition =   3  'Windows Default
   Begin VB.Timer tmrPoll 
      Interval        =   100
      Left            =   4920
      Top             =   3360
   End
   Begin VB.CheckBox chkPoll 
      Caption         =   "Monitor (poll) analog reading"
      Height          =   255
      Left            =   1800
      TabIndex        =   9
      Top             =   2280
      Value           =   1  'Checked
      Width           =   3015
   End
   Begin VB.HScrollBar scrollLoMark 
      Height          =   255
      Left            =   1440
      Max             =   255
      TabIndex        =   7
      Top             =   1440
      Value           =   80
      Width           =   4335
   End
   Begin VB.HScrollBar scrollHiMark 
      Height          =   255
      Left            =   1440
      Max             =   255
      TabIndex        =   6
      Top             =   1080
      Value           =   130
      Width           =   4335
   End
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
   Begin VB.Label Label9 
      Alignment       =   1  'Right Justify
      Caption         =   "Low"
      Height          =   255
      Left            =   1920
      TabIndex        =   15
      Top             =   3240
      Width           =   615
   End
   Begin VB.Label Label8 
      Alignment       =   1  'Right Justify
      Caption         =   "High"
      Height          =   255
      Left            =   1920
      TabIndex        =   14
      Top             =   2760
      Width           =   615
   End
   Begin VB.Label lblEventLow 
      BorderStyle     =   1  'Fixed Single
      Caption         =   "0"
      Height          =   375
      Left            =   2760
      TabIndex        =   13
      Top             =   3240
      Width           =   855
   End
   Begin VB.Label lblEventHigh 
      BorderStyle     =   1  'Fixed Single
      Caption         =   "0"
      Height          =   375
      Left            =   2760
      TabIndex        =   12
      Top             =   2760
      Width           =   855
   End
   Begin VB.Label Label7 
      Caption         =   "(box highlights and number increases when event is received)"
      Height          =   855
      Left            =   240
      TabIndex        =   11
      Top             =   3000
      Width           =   1455
   End
   Begin VB.Label Label6 
      Alignment       =   1  'Right Justify
      Caption         =   "Analog Events"
      BeginProperty Font 
         Name            =   "MS Sans Serif"
         Size            =   8.25
         Charset         =   0
         Weight          =   700
         Underline       =   0   'False
         Italic          =   0   'False
         Strikethrough   =   0   'False
      EndProperty
      Height          =   255
      Left            =   240
      TabIndex        =   10
      Top             =   2640
      Width           =   1455
   End
   Begin VB.Shape rectGraph 
      BackColor       =   &H000000FF&
      BackStyle       =   1  'Opaque
      Height          =   255
      Left            =   1680
      Top             =   1920
      Width           =   15
   End
   Begin VB.Shape rectOutline 
      Height          =   255
      Left            =   1680
      Top             =   1920
      Width           =   3855
   End
   Begin VB.Label Label5 
      Alignment       =   1  'Right Justify
      Caption         =   "Analog Reading:"
      Height          =   255
      Left            =   0
      TabIndex        =   8
      Top             =   1920
      Width           =   1335
   End
   Begin VB.Label Label4 
      Alignment       =   1  'Right Justify
      Caption         =   "Lo-Mark:"
      Height          =   255
      Left            =   600
      TabIndex        =   5
      Top             =   1440
      Width           =   735
   End
   Begin VB.Label Label3 
      Alignment       =   1  'Right Justify
      Caption         =   "Hi-Mark:"
      Height          =   255
      Left            =   600
      TabIndex        =   4
      Top             =   1080
      Width           =   735
   End
   Begin VB.Label Label2 
      Caption         =   "Analog Event Thresholds"
      Height          =   255
      Left            =   240
      TabIndex        =   3
      Top             =   720
      Width           =   1935
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
Attribute VB_Name = "frmAnalog"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
Option Explicit

Dim WithEvents dev As Eth32
Attribute dev.VB_VarHelpID = -1

Dim hicount As Long
Dim locount As Long


Private Sub chkPoll_Click()
    ' The checkbox has been checked or unchecked - update the timer
    ' accordingly
    If chkPoll.value Then
        tmrPoll.Enabled = True
    Else
        tmrPoll.Enabled = False
    End If
    
End Sub

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
    
    ' Use internal 5V voltage reference
    dev.AnalogReference = REF_INTERNAL
    
    ' Enable Analog to Digital Convertor
    dev.AnalogState = ADC_ENABLED

    ' Create the initial analog event definition based on the slide bars
    updateEventDefinition

    ' Enable analog event
    dev.EnableEvent EVENT_ANALOG, 0, 0, 100  ' Bank 0, Logical Channel 0, ID 100
    
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
        Case 100 ' We used an ID of 100 when enabling the analog event
            If direction > 0 Then
                ' The signal exceeded the hi-mark
                hicount = hicount + 1
                lblEventHigh.Caption = CStr(hicount)
                lblEventHigh.BackColor = vbYellow
                lblEventLow.BackColor = vbButtonFace
            Else
                ' The signal went below the lo-mark
                locount = locount + 1
                lblEventLow.Caption = CStr(locount)
                lblEventLow.BackColor = vbYellow
                lblEventHigh.BackColor = vbButtonFace
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

Private Sub updateEventDefinition()
    ' This function is called when either of the analog event definition
    ' scrollbars on the form are updated.

    On Error GoTo err_handler

    ' First make sure the Hi-mark is higher than the lo-mark.
    If (scrollHiMark.value <= scrollLoMark.value) Then
        ' HiMark isn't higher - we need to fix it.
        ' See whether LoMark is maxed out
        If (scrollLoMark.value >= scrollLoMark.max) Then
            ' It's maxed out, so set HiMark to max and LoMark to one under
            scrollHiMark.value = scrollHiMark.max
            scrollLoMark.value = scrollHiMark.value - 1
        Else
            ' It's not maxed out, so set HiMark to one over it.
            scrollHiMark.value = scrollLoMark.value + 1
        End If
    End If
    
    ' Clear the backgrounds on both event counter labels to show that
    ' a new event definition has been set.
    lblEventHigh.BackColor = vbButtonFace
    lblEventLow.BackColor = vbButtonFace

    ' Go no further if we're not instantiated or connected
    
    If dev Is Nothing Then Exit Sub
    If dev.Connected = False Then Exit Sub
    
    ' Now update the event definition.
    ' The scrollbars range in value from 0 to 255, so we can pass their
    ' value directly into the method that defines the analog event.
    ' (Remember that the analog events use the eight most significant
    '  bits of the analog reading (0-255), not all 10 bits (0-1023)).
    dev.SetAnalogEventDef 0, 0, scrollLoMark.value, scrollHiMark.value, ANEVT_DEFAULT_LOW


    Exit Sub
err_handler:
    MsgBox "Error connecting to the ETH32 device: " & dev.ErrorString(Err.Number)

End Sub


Private Sub scrollHiMark_Change()
    '  Call a helper function to handle the new event definition
    updateEventDefinition
End Sub

Private Sub scrollLoMark_Change()
    '  Call a helper function to handle the new event definition
    updateEventDefinition
End Sub

Private Sub tmrPoll_Timer()
    ' This event fires periodically when the timer is enabled
    
    ' Don't do anything if the object is not instantiated or not connected
    If dev Is Nothing Then Exit Sub
    If dev.Connected = False Then Exit Sub
    
    ' We use a rectangle with a red background as a very
    ' primitive way of creating a bar graph.
    ' Alter the width of the rectangle based on the analog reading.
    ' Calculate the width based on a maximum width, which is the
    ' width of the rectangle with the black outline.
    rectGraph.Width = (dev.InputAnalog(0) / 1023#) * rectOutline.Width
    
    Exit Sub
err_handler:
    MsgBox "Error reading an analog value from the ETH32: " & dev.ErrorString(Err.Number)
    
End Sub
