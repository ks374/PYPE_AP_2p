VERSION 5.00
Begin VB.Form eth32_form 
   Caption         =   "eth32 event form"
   ClientHeight    =   2445
   ClientLeft      =   60
   ClientTop       =   450
   ClientWidth     =   4095
   LinkTopic       =   "Form1"
   ScaleHeight     =   2445
   ScaleWidth      =   4095
   StartUpPosition =   3  'Windows Default
   Visible         =   0   'False
   Begin VB.CommandButton event_notify 
      Caption         =   "event_notify"
      Height          =   375
      Left            =   1320
      TabIndex        =   0
      Top             =   1800
      Width           =   1215
   End
End
Attribute VB_Name = "eth32_form"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
Option Explicit
' This is a helper form for the eth32 class.  It assists the class
' in handling events produced by the device in a VB-safe way.


' Store a reference to the eth32 class instance that this form instance is for
' (Note that a new form instance is created for each eth32 class instance)
' We store a reference to our parent eth32 object as a long variable so that
' we don't create a circular reference which prevents the eth32 object from
' unloading when it goes out of scope.  Some people call this a "weak reference"
Public eth32_object As Long

Private Sub event_notify_Click()
    Dim ethobj As eth32
    Dim n As Long
    
    ' Create an actual reference variable from our weak reference
    mod_eth32_copymemory ethobj, eth32_object, 4
        
    ' Simply instruct the object to check the event queue in the API and process
    ' any events it finds
    ethobj.CheckEvents
    
    ' This is important: Now we need to zero out our reference so that when
    ' it goes out of scope, VB won't automatically call Release and unload the
    ' eth32 object
    n = 0
    mod_eth32_copymemory ethobj, n, 4
    
    
End Sub
