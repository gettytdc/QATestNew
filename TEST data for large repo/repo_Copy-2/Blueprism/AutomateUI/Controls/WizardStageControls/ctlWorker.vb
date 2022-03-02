''' <summary>
''' Control which details the work being done within a wizard stage.
''' </summary>
Public Class ctlWorker : Inherits ctlWizardStageControl

#Region " Worker Control Properties "

    ''' <summary>
    ''' The text in the control - ie. that in bold above the progress bar.
    ''' </summary>
    Public Overrides Property Text() As String
        Get
            Return lblWorking.Text
        End Get
        Set(ByVal value As String)
            lblWorking.Text = value
        End Set
    End Property

    ''' <summary>
    ''' The current state label in the control
    ''' </summary>
    Public Property StateLabel() As String
        Get
            Return lblState.Text
        End Get
        Set(ByVal value As String)
            lblState.Text = value
        End Set
    End Property

#End Region

#Region " Progress Bar properties "

    ''' <summary>
    ''' Gets the actual progress bar.
    ''' </summary>
    Public ReadOnly Property Progress() As ProgressBar
        Get
            Return mProgress
        End Get
    End Property

    ''' <summary>
    ''' Sets the minimum value of the progress bar.
    ''' </summary>
    Public Property Minimum() As Integer
        Get
            Return mProgress.Minimum
        End Get
        Set(ByVal value As Integer)
            mProgress.Minimum = value
        End Set
    End Property

    ''' <summary>
    ''' Sets the maximum value of the progress bar
    ''' </summary>
    Public Property Maximum() As Integer
        Get
            Return mProgress.Maximum
        End Get
        Set(ByVal value As Integer)
            mProgress.Maximum = value
        End Set
    End Property

    ''' <summary>
    ''' Sets the current value of the progress bar.
    ''' </summary>
    Public Property Value() As Integer
        Get
            Return mProgress.Value
        End Get
        Set(ByVal value As Integer)
            mProgress.Value = value
        End Set
    End Property

    ''' <summary>
    ''' Gets the current step value of the progress bar ie. the amount by which
    ''' a call to <see cref="PerformStep"/> will bump the value
    ''' </summary>
    Public Property [Step]() As Integer
        Get
            Return mProgress.Step
        End Get
        Set(ByVal value As Integer)
            mProgress.Step = value
        End Set
    End Property

    ''' <summary>
    ''' The style of progress bar
    ''' </summary>
    Public Property Style() As ProgressBarStyle
        Get
            Return mProgress.Style
        End Get
        Set(ByVal value As ProgressBarStyle)
            mProgress.Style = value
        End Set
    End Property

    ''' <summary>
    ''' The animation speed of the marquee style of the progress bar.
    ''' </summary>
    Public Property MarqueeAnimationSpeed() As Integer
        Get
            Return mProgress.MarqueeAnimationSpeed
        End Get
        Set(ByVal value As Integer)
            mProgress.MarqueeAnimationSpeed = value
        End Set
    End Property

#End Region

#Region " Progress Bar Methods "

    ''' <summary>
    ''' Bumps the value of the progress bar by one step.
    ''' <seealso cref="[Step]"/>
    ''' </summary>
    Public Sub PerformStep()
        mProgress.PerformStep()
    End Sub

#End Region

End Class
