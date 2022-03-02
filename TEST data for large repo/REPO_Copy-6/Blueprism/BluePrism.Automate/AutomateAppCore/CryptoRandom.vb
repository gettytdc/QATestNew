Imports System.Security.Cryptography

''' <summary>
''' Class representing a Cryptographically strong random number generator
''' </summary>
Public Class CryptoRandom
    Inherits Random
    Implements IDisposable

    Private Const BufferSize As Integer = 1024
    ' must be a multiple of 4
    Private ReadOnly _randomBuffer As Byte()
    Private _bufferOffset As Integer
    Private ReadOnly rng As RNGCryptoServiceProvider

    ''' <summary>
    ''' Constructor, init the random generator.
    ''' </summary>
    Public Sub New()
        _randomBuffer = New Byte(BufferSize - 1) {}
        rng = New RNGCryptoServiceProvider()
        _bufferOffset = _randomBuffer.Length
    End Sub

    Private Sub FillBuffer()
        rng.GetBytes(_randomBuffer)
        _bufferOffset = 0
    End Sub

    ''' <summary>
    ''' Get the next random integer
    ''' </summary>
    Public Overrides Function [Next]() As Integer
        If _bufferOffset >= _randomBuffer.Length Then
            FillBuffer()
        End If

        Dim val = BitConverter.ToInt32(_randomBuffer, _bufferOffset) And &H7FFFFFFF
        _bufferOffset += 4

        Return val
    End Function

    ''' <summary>
    ''' Get the next random integer between 0 and the supplied maximum
    ''' </summary>
    Public Overrides Function [Next](maxValue As Integer) As Integer
        Return [Next]() Mod maxValue
    End Function

    ''' <summary>
    ''' Get the next random integer betwen two bounds
    ''' </summary>
    Public Overrides Function [Next](minValue As Integer, maxValue As Integer) As Integer
        If maxValue < minValue Then
            Throw New ArgumentOutOfRangeException(NameOf(maxValue), "maxValue must be greater than or equal to minValue")
        End If
        Dim range = maxValue - minValue
        Return minValue + [Next](range)
    End Function

    ''' <summary>
    ''' Constructor, init the random generator.
    ''' </summary>
    Public Overrides Function NextDouble() As Double
        Dim val = [Next]()
        Return CDbl(val) / Integer.MaxValue
    End Function

    ''' <summary>
    ''' Populate a byte array with random data
    ''' </summary>
    Public Sub GetBytes(buff As Byte())
        rng.GetBytes(buff)
    End Sub

    ''' <summary>
    ''' Tidy up
    ''' </summary>
    Public Sub Dispose() Implements IDisposable.Dispose
        If rng IsNot Nothing Then rng.Dispose()
    End Sub
End Class
