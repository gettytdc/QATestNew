Imports System.Runtime.InteropServices
Imports System.Text
Imports BluePrism.Common.Security

Namespace WebApis.RequestHandling

    ''' <summary>
    ''' Helper functionality for basic authentication
    ''' </summary>
    Public Class BasicAuthenticationHelper

        ''' <summary>
        ''' Get the Authorization header value that can be used when using
        ''' Basic Authentication and pre-emptively sending the header.
        ''' </summary>
        ''' <param name="userName">The Basic Authentication username</param>
        ''' <param name="password">The Basic Authentication password</param>
        ''' <param name="encoding">The encoding of the HTTP Header Value</param>
        ''' <returns>A string in the form "Basic Base64(username:password) </returns>
        ''' <remarks>This function will expose the base64 encoded password
        ''' in memory. This is necessary as can only add a Http Header Value 
        ''' to our request as a string rather than as a secure string. However,
        ''' this function prevents the un-encoded password from being exposed in
        ''' memory, which is the limit of what we can achieve.</remarks>
        Public Shared Function GetAuthorizationHeaderValue(userName As String,
                                                           password As SafeString,
                                                           encoding As Encoding) As String

            ' Use the pinned secure string mechanism to access the password's
            ' underlying bytes without exposing the password in memory
            Using p As New PinnedSecureString(password)

                Dim encodedUserName = encoding.GetBytes($"{userName}:")
                Dim encodedPassword = p.GetBytes(encoding)

                ' Create a pinned byte array that will be used to store the username and 
                ' password before it is base 64 encoded
                Dim userNameAndPassword(encodedUserName.Length + encodedPassword.Length - 1) As Byte
                Dim userNameAndPasswordPin = GCHandle.Alloc(userNameAndPassword, GCHandleType.Pinned)

                Array.Copy(encodedUserName, 0, userNameAndPassword, 0, encodedUserName.Length)
                Array.Copy(encodedPassword, 0, userNameAndPassword, encodedUserName.Length, encodedPassword.Length)

                Dim base64UserNameAndPassword = $"{Convert.ToBase64String(userNameAndPassword)}"

                ' Zero the bytes and free the pinned byte array to clear the exposed
                ' username and password from memory.
                For i As Integer = 0 To userNameAndPassword.Length - 1
                    userNameAndPassword(i) = 0
                Next
                If userNameAndPasswordPin.IsAllocated Then userNameAndPasswordPin.Free()

                Return $"Basic {base64UserNameAndPassword}"

            End Using


        End Function
    End Class
End NameSpace