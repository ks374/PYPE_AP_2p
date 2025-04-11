Imports WinfordEthIO

Public Class frmButtons
    Inherits System.Windows.Forms.Form

    Dim dev As New Eth32()
    Private Const ID_BUTTON0 As Integer = 100
    Private Const ID_BUTTON1 As Integer = 101


#Region " Windows Form Designer generated code "

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call

    End Sub

    'Form overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents txtAddress As System.Windows.Forms.TextBox
    Friend WithEvents cmdConnect As System.Windows.Forms.Button
    Friend WithEvents button0 As System.Windows.Forms.Label
    Friend WithEvents button1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.txtAddress = New System.Windows.Forms.TextBox()
        Me.cmdConnect = New System.Windows.Forms.Button()
        Me.button0 = New System.Windows.Forms.Label()
        Me.button1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.Location = New System.Drawing.Point(8, 16)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(88, 16)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "ETH32 Address:"
        '
        'txtAddress
        '
        Me.txtAddress.Location = New System.Drawing.Point(104, 16)
        Me.txtAddress.Name = "txtAddress"
        Me.txtAddress.Size = New System.Drawing.Size(96, 20)
        Me.txtAddress.TabIndex = 1
        Me.txtAddress.Text = "192.168.1.100"
        '
        'cmdConnect
        '
        Me.cmdConnect.Location = New System.Drawing.Point(208, 16)
        Me.cmdConnect.Name = "cmdConnect"
        Me.cmdConnect.Size = New System.Drawing.Size(72, 24)
        Me.cmdConnect.TabIndex = 2
        Me.cmdConnect.Text = "Connect"
        '
        'button0
        '
        Me.button0.BackColor = System.Drawing.SystemColors.Control
        Me.button0.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.button0.Location = New System.Drawing.Point(144, 56)
        Me.button0.Name = "button0"
        Me.button0.Size = New System.Drawing.Size(24, 24)
        Me.button0.TabIndex = 3
        '
        'button1
        '
        Me.button1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.button1.Location = New System.Drawing.Point(144, 88)
        Me.button1.Name = "button1"
        Me.button1.Size = New System.Drawing.Size(24, 24)
        Me.button1.TabIndex = 4
        '
        'Label2
        '
        Me.Label2.Location = New System.Drawing.Point(176, 64)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(104, 16)
        Me.Label2.TabIndex = 5
        Me.Label2.Text = "Button 0 Status"
        '
        'Label3
        '
        Me.Label3.Location = New System.Drawing.Point(176, 96)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(104, 16)
        Me.Label3.TabIndex = 6
        Me.Label3.Text = "Button 1 Status"
        '
        'Label4
        '
        Me.Label4.Location = New System.Drawing.Point(8, 56)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(128, 56)
        Me.Label4.TabIndex = 7
        Me.Label4.Text = "Once connected, press external pushbuttons to update screen status and on-board L" & _
        "EDs."
        '
        'frmButtons
        '
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
        Me.ClientSize = New System.Drawing.Size(294, 128)
        Me.Controls.AddRange(New System.Windows.Forms.Control() {Me.Label4, Me.Label3, Me.Label2, Me.button1, Me.button0, Me.cmdConnect, Me.txtAddress, Me.Label1})
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.MaximumSize = New System.Drawing.Size(300, 160)
        Me.MinimumSize = New System.Drawing.Size(300, 160)
        Me.Name = "frmButtons"
        Me.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide
        Me.Text = "ETH32 Buttons Example"
        Me.ResumeLayout(False)

    End Sub

#End Region

    Private Sub Eth32EventHandler(ByVal s As System.Object, ByVal e As System.EventArgs)
        ' If this function was called by Eth32 event processing, then
        ' s (sender) is actually the Eth32 object that received the event
        ' and e is actually an Eth32EventArgs object. But to be safe,
        ' we'll make sure by using this code:
        Dim args As Eth32EventArgs
        Dim sender As Eth32

        If TypeOf s Is Eth32 Then
            sender = CType(s, Eth32)
        Else
            ' s wasn’t really an Eth32 object - quit
            Exit Sub
        End If

        ' Since we're going to be working with Forms controls, test one to see if 
        ' we need to use Invoke (because we're running on a different thread)
        ' and if so, call ourselves (this function) using Invoke.  At that point,
        ' this same test will indicate that we don't need to use Invoke and it will
        ' actually do what we wanted.
        If Me.InvokeRequired Then
            Dim d As New EventHandler(AddressOf Eth32EventHandler)
            Me.Invoke(d, New Object() {s, e})
            Exit Sub
        End If

        If TypeOf e Is Eth32EventArgs Then
            args = CType(e, Eth32EventArgs)
        Else
            ' The arguments weren’t really Eth32EventArgs - quit
            Exit Sub
        End If

        ' You may now easily access the Eth32 object by using sender and
        ' the event information by using args.event_info
        Select Case args.event_info.id
            Case ID_BUTTON0
                If args.event_info.val Then
                    ' Button has just been released
                    button0.BackColor = System.Drawing.SystemColors.Control ' Set status square color to same as form 
                    dev.Led(0) = False ' Turn off LED on ETH32
                Else
                    ' Button has just been pressed
                    button0.BackColor = System.Drawing.Color.Red  ' Set status square color to Red
                    dev.Led(0) = True  ' Turn on LED on ETH32
                End If

            Case ID_BUTTON1
                If args.event_info.val Then
                    ' Button has just been released
                    button1.BackColor = System.Drawing.SystemColors.Control ' Set status square color to same as form 
                    dev.Led(1) = False ' Turn off LED on ETH32
                Else
                    ' Button has just been pressed
                    button1.BackColor = System.Drawing.Color.Red  ' Set status square color to Red
                    dev.Led(1) = True  ' Turn on LED on ETH32
                End If
        End Select



    End Sub

    Private Sub cmdConnect_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdConnect.Click
        ' If we're already connected, disconnect and create a new object
        If dev.Connected Then
            dev.Disconnect()
            dev = New Eth32()
        End If

        Try
            ' Connect, use a 5 second timeout
            dev.Connect(txtAddress.Text, Eth32.DefaultPort, 5000)

            ' Enable pullup resistors on pushbutton lines
            dev.OutputBit(0, 0, 1) ' Port 0, bit 0
            dev.OutputBit(0, 1, 1) ' Port 0, bit 1

            ' Enable events on both pushbutton lines
            dev.EnableEvent(Eth32EventType.Digital, 0, 0, ID_BUTTON0)
            dev.EnableEvent(Eth32EventType.Digital, 0, 1, ID_BUTTON1)

            AddHandler dev.HardwareEvent, AddressOf Eth32EventHandler

            MsgBox("Successfully connected to ETH32 device.", MsgBoxStyle.Information)

        Catch my_error As Eth32Exception
            MsgBox("Error connecting or configuring the ETH32: " & Eth32.ErrorString(my_error.ErrorCode), MsgBoxStyle.Exclamation)
        End Try

    End Sub

    Private Sub frmButtons_Closing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles MyBase.Closing
        ' When this form closes, close the connection to the ETH32 as well
        If Not (dev Is Nothing) Then
            ' dev is at least instantiated
            If dev.Connected Then
                dev.Disconnect()
            End If
        End If

    End Sub
End Class
