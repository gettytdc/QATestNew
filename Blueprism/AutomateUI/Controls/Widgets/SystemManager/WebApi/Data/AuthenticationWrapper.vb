
Imports BluePrism.AutomateProcessCore.WebApis.Authentication

''' <summary>
''' Class to wrap a <see cref="IAuthentication"/> object. This is so the 
''' <see cref="WebApiManager"/> can keep a reference to this object, even if the 
''' underlying <see cref="IAuthentication"/> object is recreated through changes made 
''' in the details form.
''' </summary>
Friend Class AuthenticationWrapper

    Property Authentication As IAuthentication

    Public Sub New(auth As IAuthentication)
        Authentication = auth
    End Sub

End Class
