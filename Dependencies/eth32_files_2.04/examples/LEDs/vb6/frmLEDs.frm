VERSION 5.00
Begin VB.Form frmLEDs 
   BorderStyle     =   1  'Fixed Single
   Caption         =   "ETH32 LEDs Example"
   ClientHeight    =   1800
   ClientLeft      =   45
   ClientTop       =   435
   ClientWidth     =   4440
   LinkTopic       =   "Form1"
   MaxButton       =   0   'False
   ScaleHeight     =   1800
   ScaleWidth      =   4440
   StartUpPosition =   3  'Windows Default
   Begin VB.CommandButton cmdDetect 
      Caption         =   "Detect"
      Height          =   375
      Left            =   3360
      TabIndex        =   7
      Top             =   120
      Width           =   855
   End
   Begin VB.ComboBox cmbETH32 
      Height          =   315
      Left            =   1440
      TabIndex        =   6
      Top             =   90
      Width           =   1815
   End
   Begin VB.CommandButton cmdRefresh 
      Caption         =   "Refresh"
      Height          =   375
      Left            =   3360
      TabIndex        =   4
      Top             =   1080
      Width           =   855
   End
   Begin VB.CheckBox led1 
      Caption         =   "LED1"
      Height          =   375
      Left            =   1920
      TabIndex        =   2
      Top             =   1320
      Width           =   1215
   End
   Begin VB.CheckBox led0 
      Caption         =   "LED0"
      Height          =   255
      Left            =   1920
      TabIndex        =   1
      Top             =   960
      Width           =   1215
   End
   Begin VB.CommandButton cmdConnect 
      Caption         =   "Connect"
      Height          =   375
      Left            =   3360
      TabIndex        =   0
      Top             =   600
      Width           =   855
   End
   Begin VB.Label Label2 
      Caption         =   "Check / Uncheck boxes to turn LEDs on or off."
      Height          =   615
      Left            =   120
      TabIndex        =   5
      Top             =   960
      Width           =   1695
   End
   Begin VB.Label Label1 
      Caption         =   "ETH32 Address:"
      Height          =   255
      Left            =   120
      TabIndex        =   3
      Top             =   120
      Width           =   1215
   End
End
Attribute VB_Name = "frmLEDs"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
Option Explicit

Dim WithEvents dev As Eth32
Attribute dev.VB_VarHelpID = -1



Private Sub cmdConnect_Click()
    ' Make sure that at least something is entered as the IP address or hostname
    If Len(cmbETH32.Text) = 0 Then
        MsgBox "Please select (or manually type) an IP address or host name of the ETH32 to connect to."
        Exit Sub
    End If
    
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
    dev.Connect cmbETH32.Text, ETH32_PORT, 5000
    
    ' After connecting, call the procedure for the Refresh button
    ' so that the initial state of the check boxes matches the
    ' state of the ETH32 LED's
    cmdRefresh_Click
    
    MsgBox "Successfully connected to ETH32 device.", vbInformation
    
    
    Exit Sub
err_handler:
    MsgBox "Error connecting to the ETH32 device: " & dev.ErrorString(Err.number)
    
End Sub

Private Sub cmdDetect_Click()
    Dim ethdetect As New Eth32Config
    Dim ip As eth32cfg_ip
    Dim i As Long
    
    ' Start with an empty combo box
    cmbETH32.Clear
    
    ' Detect any ETH32 devices on the local network
    ' Make the mouse pointer an hour-glass during detection (which takes a couple seconds)
    ' so the user knows it's in progress.
    Screen.MousePointer = vbHourglass
    ethdetect.Query
    Screen.MousePointer = vbDefault
    
    If ethdetect.NumResults Then ' If we found at least one device
        ' loop through them and add the Active IP of each to the combo box list
        For i = 0 To (ethdetect.NumResults - 1)
            ' Retrieve the active IP of this particular device
            ip = ethdetect.Result(i).active_ip
            ' Convert the IP to a string and add it to the combo box
            cmbETH32.AddItem ethdetect.IpConvertToString(ip)
        Next
    
        ' Make the combo box drop down so the user can see the available options
        Call SendMessage(cmbETH32.hwnd, CB_SHOWDROPDOWN, 1, 0)
    
    Else
        ' Otherwise, we didn't find any devices.  Let the user know
        MsgBox "No devices were found.  Detection will only find ETH32 devices on the local network segment.  For devices that are outside the local segment, please enter the IP address or host name into the combo, and click Connect."
    End If
  
End Sub

Private Sub cmdRefresh_Click()
    If dev Is Nothing Then
        MsgBox "Please connect first."
        Exit Sub
    End If
    If dev.Connected = False Then
        MsgBox "Please connect first."
        Exit Sub
    End If
        
    
    If dev.Led(0) Then
        led0.value = 1
    Else
        led0.value = 0
    End If
    
    If dev.Led(1) Then
        led1.value = 1
    Else
        led1.value = 0
    End If
    
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

Private Sub led0_Click()
    If dev Is Nothing Then
        MsgBox "Please connect first."
        Exit Sub
    End If
    If dev.Connected = False Then
        MsgBox "Please connect first."
        Exit Sub
    End If

    ' Update the ETH32 LED to match the state of the checkbox
    dev.Led(0) = led0.value
    
End Sub

Private Sub led1_Click()
    If dev Is Nothing Then
        MsgBox "Please connect first."
        Exit Sub
    End If
    If dev.Connected = False Then
        MsgBox "Please connect first."
        Exit Sub
    End If

    ' Update the ETH32 LED to match the state of the checkbox
    dev.Led(1) = led1.value

End Sub
