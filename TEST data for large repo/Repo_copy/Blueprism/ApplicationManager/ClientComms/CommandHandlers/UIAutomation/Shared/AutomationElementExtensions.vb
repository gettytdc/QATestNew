Imports System.Linq
Imports System.Runtime.CompilerServices
Imports BluePrism.UIAutomation
Imports BluePrism.UIAutomation.Patterns

Namespace CommandHandlers.UIAutomation.Shared

    ''' <summary>
    ''' Extension methods for IAutomationPattern objects
    ''' </summary>
    Public Module AutomationElementExtensions

        ''' <summary>
        ''' Gets the specified type of pattern from an element. An exception is thrown
        ''' if the pattern cannot be found.
        ''' </summary>
        ''' <typeparam name="TPattern">The pattern type</typeparam>
        ''' <param name="element">The IAutomationElement instance</param>
        ''' <returns>The pattern instance</returns>
        ''' <exception cref="PatternNotFoundException">Thrown if pattern not found</exception>
        <Extension>
        Public Function EnsurePattern(Of TPattern As IAutomationPattern) _
            (element As IAutomationElement) As TPattern

            Dim pattern = element.GetCurrentPattern(Of TPattern)
            If pattern Is Nothing 
                Throw New PatternNotFoundException(Of TPattern)
            End If
            Return pattern

        End Function

        ''' <summary>
        ''' Gets the specified type of pattern from an element, executes a function
        ''' with the pattern and returns the result. An exception is thrown if the 
        ''' pattern cannot be found.
        ''' </summary>
        ''' <typeparam name="TPattern">The pattern type</typeparam>
        ''' <typeparam name="TResult">The result type</typeparam>
        ''' <param name="element">The IAutomationElement instance</param>
        ''' <param name="func">The function to execute using the pattern</param>
        ''' <returns>The return value of the function</returns>
        ''' <exception cref="PatternNotFoundException">Thrown if pattern not found</exception>
        <Extension>
        Public Function FromPattern(Of TPattern As IAutomationPattern, TResult) _
            (element As IAutomationElement, func As Func(Of TPattern, TResult)) As TResult

            Return FromPattern(element, func, True, Nothing)

        End Function

        ''' <summary>
        ''' Gets the specified type of pattern from an element, executes a function
        ''' with the pattern and returns the result. 
        ''' </summary>
        ''' <typeparam name="TPattern">The pattern type</typeparam>
        ''' <typeparam name="TResult">The result type</typeparam>
        ''' <param name="element">The IAutomationElement instance</param>
        ''' <param name="func">The function to execute using the pattern</param>
        ''' <returns>The return value of the function</returns>
        <Extension>
        Public Function FromPatternOrDefault(Of TPattern As IAutomationPattern, TResult) _
            (element As IAutomationElement,
             func As Func(Of TPattern, TResult),
             Optional defaultValue As TResult = Nothing) As TResult

            Return FromPattern(element, func, False, defaultValue)

        End Function

        Private Function FromPattern(Of TPattern As IAutomationPattern, TResult) _
            (element As IAutomationElement,
             func As Func(Of TPattern, TResult),
             throwIfMissing As Boolean,
             defaultValue As TResult) As TResult

            Dim pattern = element.GetCurrentPattern(Of TPattern)
            If pattern Is Nothing Then
                If throwIfMissing Then
                    Throw New PatternNotFoundException(Of TPattern)
                Else
                    Return defaultValue
                End If
            End If
            Return func(pattern)

        End Function

        ''' <summary>
        ''' Gets a pattern from the first subtree element that supports it
        ''' </summary>
        ''' <param name="patternType">The type of pattern</param>
        ''' <returns>The pattern from the first matching subtree element or null if 
        ''' the pattern is not supported</returns>
        <Extension>
        Public Function GetPatternFromSubtree(Of TPattern As IAutomationPattern) _
            (element As IAutomationElement, patternType As PatternType) _
            As TPattern

            Dim subtreeElement = element.FindAll(TreeScope.SubTree).
                    FirstOrDefault(Function(e)e.PatternIsSupported(patternType))
            If subtreeElement IsNot Nothing Then
                Return subtreeElement.GetCurrentPattern(Of TPattern)
            End If
            Return Nothing

        End Function

        ''' <summary>
        ''' Gets a pattern from the first subtree element that supports it, throwing
        ''' an exception if an element cannot be found
        ''' </summary>
        ''' <param name="patternType">The type of pattern</param>
        ''' <returns>The pattern from the first matching subtree element</returns>
        ''' <exception cref="PatternNotFoundException(Of TPattern)">Thrown if an
        ''' element cannot be found</exception>
        <Extension>
        Public Function EnsurePatternFromSubtree(Of TPattern As IAutomationPattern) _
            (element As IAutomationElement, patternType As PatternType) _
            As TPattern
            
            Dim pattern = element.GetPatternFromSubtree(Of TPattern)(patternType)
            If pattern Is Nothing
                Dim template = UIAutomationErrorResources.MissingPatternException_DefaultMessageTemplate
                Throw New PatternNotFoundException(Of TPattern)(template)
            End If
            Return pattern

        End Function

        ''' <summary>
        ''' Gets a pattern from the first ancestor element that supports it
        ''' </summary>
        ''' <param name="patternType">The type of pattern</param>
        ''' <returns>The pattern from the first matching subtree element or null if 
        ''' the pattern is not supported</returns>
        <Extension>
        Public Function GetPatternFromAncestors(Of TPattern As IAutomationPattern) _
            (element As IAutomationElement, 
             includeSelf As Boolean,
             patternType As PatternType) _
            As TPattern

            If includeSelf AndAlso element.PatternIsSupported(patternType)
                Return element.GetCurrentPattern(Of TPattern)
            End If

            Dim subtreeElement = element.FindAll(TreeScope.Ancestors).
                FirstOrDefault(Function(e)e.PatternIsSupported(patternType))
            If subtreeElement IsNot Nothing
                Return subtreeElement.GetCurrentPattern(Of TPattern)
            End If
            Return Nothing

        End Function

        ''' <summary>
        ''' Gets a pattern from the first ancestor element that supports it, throwing
        ''' an exception if an element cannot be found
        ''' </summary>
        ''' <param name="patternType">The type of pattern</param>
        ''' <returns>The pattern from the first matching subtree element</returns>
        ''' <exception cref="PatternNotFoundException(Of TPattern)">Thrown if an
        ''' element cannot be found</exception>
        <Extension>
        Public Function EnsurePatternFromAncestors(Of TPattern As IAutomationPattern) _
            (element As IAutomationElement, 
             includeSelf As Boolean,
             patternType As PatternType) _
            As TPattern
            
            Dim pattern = element.GetPatternFromAncestors(Of TPattern)(includeSelf, patternType)
            If pattern Is Nothing
                Dim template = If(includeSelf,
                    UIAutomationErrorResources.MissingPatternException_PatternNotInAncestorsOrSelfTemplate, 
                    UIAutomationErrorResources.MissingPatternException_PatternNotInAncestorsTemplate)
                Throw New PatternNotFoundException(Of TPattern)(template)
            End If
            Return pattern

        End Function


    End Module


End Namespace