Imports System.Runtime.CompilerServices
Imports System.Drawing
Imports System.Linq

Imports BluePrism.ApplicationManager.ApplicationManagerUtilities

Imports IdentifierTypes =
    BluePrism.ApplicationManager.ApplicationManagerUtilities.clsQuery.IdentifierTypes
Imports System.Collections.Generic

Namespace UIAutomation

    ''' <summary>
    ''' Extension methods for use in UIAutomation.
    ''' </summary>
    Public Module UIAutomationExtensions

#Region " Variables "

        ''' <summary>
        ''' The default identifier types used when getting a UIA element.
        ''' </summary>
        Private mDefaultIdentifierTypes As New HashSet(Of IdentifierTypes) From {
            IdentifierTypes.uClassName,
            IdentifierTypes.uAutomationId,
            IdentifierTypes.uControlType,
            IdentifierTypes.uName,
            IdentifierTypes.uItemType,
            IdentifierTypes.uTopLevelWindowId
        }

#End Region

#Region " IdentifierTypes Extensions "

        ''' <summary>
        ''' Checks if 'this' identifier type is a UIA parent attribute.
        ''' </summary>
        ''' <param name="this">The identifier type to check</param>
        ''' <returns>True if the given identifier represents a UIA parent; False
        ''' otherwise.</returns>
        <Extension>
        Friend Function IsUiaParent(this As IdentifierTypes) As Boolean
            Return {
                IdentifierTypes.puClassName,
                IdentifierTypes.puAutomationId,
                IdentifierTypes.puName,
                IdentifierTypes.puControlType,
                IdentifierTypes.puLocalizedControlType
            }.Contains(this)
        End Function

        ''' <summary>
        ''' Checks if 'this' identifier type is a UIA grandparent attribute.
        ''' </summary>
        ''' <param name="this">The identifier type to check</param>
        ''' <returns>True if the given identifier represents a UIA ancestor; False
        ''' otherwise.</returns>
        ''' <remarks>Currently, there are no UIA grandparent identifiers; so this is
        ''' a placeholder in case we decide to implement such a thing.</remarks>
        <Extension>
        Friend Function IsUiaGrandparent(this As IdentifierTypes) As Boolean
            Return False
        End Function

        ''' <summary>
        ''' Checks if 'this' identifier type is a UIA ancestor attribute.
        ''' </summary>
        ''' <param name="this">The identifier type to check</param>
        ''' <returns>True if the given identifier represents a UIA ancestor; False
        ''' otherwise.</returns>
        <Extension>
        Friend Function IsUiaAncestor(this As IdentifierTypes) As Boolean
            Return this.IsUiaParent() OrElse this.IsUiaGrandparent()
        End Function

        ''' <summary>
        ''' Checks if 'this' identifier type is a UIA bounds attribute.
        ''' </summary>
        ''' <param name="this">The identifier type to check</param>
        ''' <returns>True if the given identifier represents a UIA bounds attribute;
        ''' False otherwise.</returns>
        <Extension>
        Friend Function IsUiaBounds(this As IdentifierTypes) As Boolean
            Return {
                IdentifierTypes.uX,
                IdentifierTypes.uY,
                IdentifierTypes.uWidth,
                IdentifierTypes.uHeight
            }.Contains(this)
        End Function


#End Region

#Region " Windows.Rect Extensions "

        ''' <summary>
        ''' Converts the given <see cref="System.Windows.Rect"/> value to a
        ''' <see cref="System.Drawing.Rectangle"/>, as used in a lot of our existing
        ''' functions.
        ''' </summary>
        ''' <param name="r">The Rect to convert</param>
        ''' <returns>The Rectangle equivalent of <paramref name="r"/></returns>
        <Extension>
        Public Function ToRectangle(r As Windows.Rect) As Rectangle
            Return New Rectangle(CInt(r.X), CInt(r.Y), CInt(r.Width), CInt(r.Height))
        End Function

#End Region

#Region " StringBuilder Extensions "

        ''' <summary>
        ''' Appends an identifier and its value to a stringbuilder, checking if it is
        ''' a <see cref="mDefaultIdentifierTypes">default identifier</see> and
        ''' prefixing with a "+" if it is.
        ''' </summary>
        ''' <param name="this">The string builder to append to</param>
        ''' <param name="tp">The identifier type to append</param>
        ''' <param name="value">The value to append</param>
        ''' <returns>The stringbuilder with the given identifier and value appended
        ''' to it.</returns>
        <Extension>
        Friend Function AppendId(Of T)(this As StringBuilder,
         tp As IdentifierTypes, value As T) As StringBuilder

            If this.Length > 0 AndAlso this(this.Length - 1) <> " "c Then _
             this.Append(" ")
            If mDefaultIdentifierTypes.Contains(tp) Then this.Append("+"c)

            Return this.
                Append(tp.ToString()).
                Append("="c).
                Append(clsQuery.EncodeValue(value))

        End Function

#End Region

    End Module

End Namespace