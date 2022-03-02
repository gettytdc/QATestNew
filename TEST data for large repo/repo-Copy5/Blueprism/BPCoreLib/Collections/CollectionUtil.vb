Namespace Collections

    ''' <summary>
    ''' Utility class providing some basic utility functions for collections
    ''' </summary>
    Public Class CollectionUtil

        ''' <summary>
        ''' Static class - can't be instantiated.
        ''' </summary>
        Private Sub New()
        End Sub

        ''' <summary>
        ''' Gets a string representation of the given collection.
        ''' </summary>
        ''' <param name="enu">The enumerable object for which a string representation
        ''' is required.</param>
        ''' <returns>The string representation of the given enumerable</returns>
        Public Overloads Shared Function ToString(ByVal enu As IEnumerable) As String
            If enu Is Nothing Then Return ""
            Dim sb As New StringBuilder()
            sb.Append("[")
            Dim joiner As String = ""
            For Each obj As Object In enu
                sb.Append(joiner).Append(obj)
                joiner = "; "
            Next
            Return sb.Append("]").ToString()
        End Function


        ''' <summary>
        ''' Gets a read-only collection based on the given elements.
        ''' </summary>
        ''' <typeparam name="T">The type of element to hold in the collection
        ''' </typeparam>
        ''' <param name="elements">The elements which should make up the collection
        ''' </param>
        ''' <returns>A readonly ICollection instance of the specified type, with the
        ''' given elements.</returns>
        Public Shared Function ToCollection(Of T)(ByVal ParamArray elements() As T) _
         As ICollection(Of T)
            Return GetReadOnly.ICollection(Of T)(elements)
        End Function

        ''' <summary>
        ''' Copies the given collection to the array of the required type (which must
        ''' match or be an ancestor class of the type of collection)
        ''' </summary>
        ''' <typeparam name="BaseT">The base type required in the array - must either
        ''' be the same type as the given collection or a base class of it.
        ''' </typeparam>
        ''' <typeparam name="T">The type of collection from which the array data is
        ''' gleaned.</typeparam>
        ''' <param name="coll">The collection with the data required in it</param>
        ''' <returns>An array of the specified base type containing the elements in
        ''' the collection.</returns>
        ''' <seealso cref="ToArray(Of T)"/>
        Public Shared Function ToArray(Of BaseT, T As {BaseT})( _
         ByVal coll As ICollection(Of T)) As BaseT()
            Dim arr(coll.Count - 1) As BaseT
            Dim i As Integer = 0
            For Each el As BaseT In coll
                arr(i) = el
                i += 1
            Next
            Return arr
        End Function

        ''' <summary>
        ''' Copies the given collection to the array of the required type. The array
        ''' type must match the collection type exactly.
        ''' </summary>
        ''' <typeparam name="T">The type of element held within the collection and
        ''' thus the generated array</typeparam>
        ''' <param name="coll">The collection to turn into an array</param>
        ''' <returns>The collection as an array of the specified type</returns>
        Public Shared Function ToArray(Of T)(ByVal coll As ICollection(Of T)) As T()
            Dim arr(coll.Count - 1) As T
            coll.CopyTo(arr, 0)
            Return arr
        End Function

        ''' <summary>
        ''' Joins the given enumerable into a single string, separated using the
        ''' given string separator.
        ''' </summary>
        ''' <param name="enu">The enumerable to join into a string</param>
        ''' <param name="separator">The separator to use in between the elements.
        ''' </param>
        ''' <returns>A string containing the elements from the given collection,
        ''' separated by the given string.</returns>
        Public Shared Function Join( _
         ByVal enu As IEnumerable, ByVal separator As String) As String
            If enu Is Nothing Then Return ""
            Return JoinInto(enu, separator, New StringBuilder()).ToString()
        End Function

        ''' <summary>
        ''' Joins the given enumerable into the given string builder, returning it
        ''' after the collection has been appended to it.
        ''' </summary>
        ''' <param name="enu">The enumerable to join into the string builder</param>
        ''' <param name="separator">The separator to use in between the elements.
        ''' </param>
        ''' <param name="sb">The string builder to join the collection into.</param>
        ''' <returns>A string containing the elements from the given collection,
        ''' separated by the given string.</returns>
        ''' <exception cref="ArgumentNullException">If the given string builder is
        ''' null</exception>
        Public Shared Function JoinInto(ByVal enu As IEnumerable, _
         ByVal separator As String, ByVal sb As StringBuilder) As StringBuilder
            If SB Is Nothing Then Throw New ArgumentNullException(NameOf(sb))
            If enu Is Nothing Then Return SB
            Dim sep As String = ""
            For Each obj As Object In enu
                SB.Append(sep).Append(obj)
                sep = separator
            Next
            Return SB
        End Function

        ''' <summary>
        ''' Merges the given collections into one read-only list.
        ''' </summary>
        ''' <typeparam name="T">The type of collections to merge</typeparam>
        ''' <param name="colls">The collections to merge</param>
        ''' <returns>The collections merged into a list - ie. in the order that the
        ''' collections returned the elements in the order of the parameters, and
        ''' with all instances of any duplicate elements.</returns>
        Public Shared Function MergeList(Of T)( _
         ByVal ParamArray colls() As IEnumerable(Of T)) As IList(Of T)
            Dim accum As New List(Of T)
            For Each c As ICollection(Of T) In colls
                accum.AddRange(c)
            Next
            Return GetReadOnly.IList(accum)
        End Function

        ''' <summary>
        ''' Merges the given collections into one read-only set.
        ''' </summary>
        ''' <typeparam name="T">The type of collections to merge</typeparam>
        ''' <param name="colls">The collections to merge</param>
        ''' <returns>The collections merged into a set - ie. in arbitrary order
        ''' and with only one instance of each duplicate element.</returns>
        Public Shared Function MergeSet(Of T)( _
         ByVal ParamArray colls() As IEnumerable(Of T)) As IBPSet(Of T)
            Dim accum As New clsSet(Of T)
            For Each c As ICollection(Of T) In colls
                accum.Union(c)
            Next
            Return GetReadOnly.IBPSet(accum)
        End Function

        ''' <summary>
        ''' Applies a conversion function to all elements in the given enumerable and
        ''' returns a collection which contains all the converted elements.
        ''' </summary>
        ''' <typeparam name="T">The type of both the input and output collection
        ''' elements.</typeparam>
        ''' <param name="els">The collection to be converted</param>
        ''' <param name="fn">The function to apply to each element in the collection.
        ''' </param>
        ''' <returns>The collection of converted elements</returns>
        ''' <remarks>See the overload with two generic types to convert a collection
        ''' of one type to a collection of another.</remarks>
        Public Shared Function Convert(Of T)( _
         ByVal els As IEnumerable(Of T), ByVal fn As Func(Of T, T)) _
         As ICollection(Of T)
            Return Convert(Of T, T)(els, fn)
        End Function

        ''' <summary>
        ''' Applies a conversion function to all elements in the given enumerable and
        ''' returns a collection which contains all the converted elements.
        ''' </summary>
        ''' <typeparam name="TIn">The type of element in the collection to be
        ''' converted</typeparam>
        ''' <typeparam name="TOut">The type of element in the collection to be output
        ''' </typeparam>
        ''' <param name="els">The collection to be converted</param>
        ''' <param name="fn">The function to apply to each element in the collection.
        ''' </param>
        ''' <returns>The collection of converted elements</returns>
        ''' <remarks>See the overload with two generic types to convert a collection
        ''' of one type to a collection of another.</remarks>
        Public Shared Function Convert(Of TIn, TOut)( _
         ByVal els As IEnumerable(Of TIn), ByVal fn As Func(Of TIn, TOut)) _
         As ICollection(Of TOut)
            If els Is Nothing Then Return Nothing
            Dim outList As New List(Of TOut)
            For Each el As TIn In els
                outList.Add(fn(el))
            Next
            Return outList
        End Function

        ''' <summary>
        ''' Filters the given enumerable returning a collection containing only those
        ''' elements which satisfy a particular predicate.
        ''' </summary>
        ''' <typeparam name="T">The type of collection to filter</typeparam>
        ''' <param name="els">The elements to be filtered</param>
        ''' <param name="p">The predicate which determines if the element should be
        ''' retained in the outputted collection or not.</param>
        ''' <returns>A collection of objects filtered from the given enumerable
        ''' which satisfied the specified predicate.</returns>
        Public Shared Function Filter(Of T)(ByVal els As IEnumerable(Of T), _
         ByVal p As Predicate(Of T)) As ICollection(Of T)
            If els Is Nothing Then Return Nothing
            Dim outList As New List(Of T)
            For Each el As T In els
                If p(el) Then outList.Add(el)
            Next
            Return outList
        End Function

        ''' <summary>
        ''' Filters the given dictionary returning a dictionary containing only those
        ''' entries whose keys satisfy a particular predicate
        ''' </summary>
        ''' <typeparam name="TKey">The type of key in the dictionary</typeparam>
        ''' <typeparam name="TValue">The type of value in the dictionary</typeparam>
        ''' <param name="dict">The dictionary to be filtered</param>
        ''' <param name="p">The predicate to be satisfied for the keys to determine
        ''' if the entry should be included in the outputted dictionary.</param>
        ''' <returns>A dictionary containing only those entries whose keys satisfied
        ''' the specified predicate.</returns>
        Public Shared Function FilterByKey(Of TKey, TValue)( _
         ByVal dict As IDictionary(Of TKey, TValue), ByVal p As Predicate(Of TKey)) _
         As IDictionary(Of TKey, TValue)
            If dict Is Nothing Then Return Nothing
            Dim outMap As New Dictionary(Of TKey, TValue)
            For Each entry As KeyValuePair(Of TKey, TValue) In dict
                If p(entry.Key) Then outMap(entry.Key) = entry.Value
            Next
            Return outMap
        End Function

        ''' <summary>
        ''' Filters the given dictionary returning a dictionary containing only those
        ''' entries whose values satisfy a particular predicate
        ''' </summary>
        ''' <typeparam name="TKey">The type of key in the dictionary</typeparam>
        ''' <typeparam name="TValue">The type of value in the dictionary</typeparam>
        ''' <param name="dict">The dictionary to be filtered</param>
        ''' <param name="p">The predicate to be satisfied for the values to determine
        ''' if the entry should be included in the outputted dictionary.</param>
        ''' <returns>A dictionary containing only those entries whose values
        ''' satisfied the specified predicate.</returns>
        Public Shared Function FilterByValue(Of TKey, TValue)( _
         ByVal dict As IDictionary(Of TKey, TValue), ByVal p As Predicate(Of TValue)) _
         As IDictionary(Of TKey, TValue)
            If dict Is Nothing Then Return Nothing
            Dim outMap As New Dictionary(Of TKey, TValue)
            For Each entry As KeyValuePair(Of TKey, TValue) In dict
                If p(entry.Value) Then outMap(entry.Key) = entry.Value
            Next
            Return outMap
        End Function

        ''' <summary>
        ''' Utility method to get the first (top) element in a collection.
        ''' </summary>
        ''' <typeparam name="T">The type of object held by the given collection,
        ''' and thus the type of object returned.</typeparam>
        ''' <param name="collection">The collection of which the first element is 
        ''' required.</param>
        ''' <returns>The first element of the collection or nothing if the
        ''' collection is empty.</returns>
        Public Shared Function First(Of T)(ByVal collection As IEnumerable(Of T)) As T
            If TypeOf collection Is IList(Of T) Then
                Dim lst = DirectCast(collection, IList(Of T))
                If lst.Count = 0 Then Return Nothing Else Return lst(0)
            End If

            If collection Is Nothing Then
                Return Nothing
            Else
                Return collection.First()
            End If
        End Function

        ''' <summary>
        ''' Utility method to get the last element in a collection.
        ''' </summary>
        ''' <typeparam name="T">The type of object held by the given collection,
        ''' and thus the type of object returned.</typeparam>
        ''' <param name="coll">The collection from which the last element should be
        ''' retrieved.</param>
        ''' <returns>The last element in the collection, or null if the collection
        ''' was empty.</returns>
        Public Shared Function Last(Of T)(ByVal coll As IEnumerable(Of T)) As T
            ' Bit of a shortcut if it's a list - no need to step through it, just
            ' get the last one using random access
            If TypeOf coll Is IList(Of T) Then
                Dim lst As IList(Of T) = DirectCast(coll, IList(Of T))
                If lst.Count = 0 Then Return Nothing Else Return lst(lst.Count - 1)
            End If

            Dim lastItem As T = Nothing
            For Each obj As T In coll
                lastItem = obj
            Next
            Return lastItem
        End Function

        ''' <summary>
        ''' Checks if the given collection is either null or empty.
        ''' </summary>
        ''' <param name="c">The collection to check</param>
        ''' <returns>True if the given collection is null or empty.</returns>
        Public Shared Function IsNullOrEmpty(ByVal c As ICollection) As Boolean
            Return (c Is Nothing OrElse c.Count = 0)
        End Function

        ''' <summary>
        ''' Checks if the given collection is either null or empty.
        ''' </summary>
        ''' <typeparam name="T">The type of collection - immaterial for this function
        ''' really.</typeparam>
        ''' <param name="c">The collection to check</param>
        ''' <returns>True if the given collection is null or empty.</returns>
        Public Shared Function IsNullOrEmpty(Of T)(ByVal c As ICollection(Of T)) _
         As Boolean
            Return (c Is Nothing OrElse c.Count = 0)
        End Function

        ''' <summary>
        ''' Gets the hash code of the given collection. This uses approximately a
        ''' third of the items in the collection to generate the hash code
        ''' </summary>
        ''' <param name="coll">The collection for which a hash is required.</param>
        ''' <returns>A hash representing the given collection, using approximately
        ''' a third of the collection's items to generate the hash.</returns>
        ''' <remarks>In terms of equality vs. hashcode, this relies on the elements
        ''' in the collection to correctly return the same hashcode for equal objects
        ''' </remarks>
        Public Overloads Shared Function GetHashCode(Of T)( _
         ByVal coll As ICollection(Of T)) As Integer
            Return GetHashCode(coll, 1.0 / 3.0)
        End Function

        ''' <summary>
        ''' Gets the hash code of the given collection using approximately the
        ''' specified number of elements.
        ''' </summary>
        ''' <param name="coll">The collection for which a hash is required.</param>
        ''' <param name="factor">The factor detailing the number of elements within
        ''' the collection to use for the generation of the hashcode where 0.5
        ''' indicates that roughly half the elements should be used. A value of 0.0
        ''' or less indicates that just the length of the collection is used, a value
        ''' of 1.0 or more indicates that every element should be used.
        ''' </param>
        ''' <returns>A hash representing the given collection.</returns>
        ''' <remarks>In terms of equality vs. hashcode, this relies on the elements
        ''' in the collection to correctly return the same hashcode for equal objects
        ''' </remarks>
        Public Overloads Shared Function GetHashCode(Of T)( _
         ByVal coll As ICollection(Of T), ByVal factor As Double) As Integer
            ' No collection - no hash
            If coll Is Nothing OrElse coll.Count = 0 Then Return 0

            ' Create a base hash to build on
            Dim hash As Integer = 28761 Xor coll.Count

            ' If factor indicates don't check elements, just hash with the count
            If factor <= 0.0 Then Return hash
            If factor >= 1.0 Then factor = 1.0

            ' Create an increment for the loop - this is how far on the loop counter
            ' will go before the next use of an element
            Dim incr As Integer = CInt(1.0 / factor)

            ' We can do things a lot faster with a list
            Dim lst As IList(Of T) = TryCast(coll, IList(Of T))
            If lst IsNot Nothing Then
                Dim ind As Integer = 0
                While ind < lst.Count
                    ' Cast the element into an object and get its hashcode
                    ' We treat a null object as a zero hashcode (so we don't need to
                    ' XOR it if it's null)
                    Dim elem As Object = CObj(lst(ind))
                    If elem IsNot Nothing Then hash = hash Xor elem.GetHashCode()
                    ind += incr
                End While
                Return hash
            End If

            ' Else it's not a list, we have to do this the hard way
            ' Start a counter at 1 (we always want to hash the first element)
            ' Count down to zero - each time in the loop, if the counter has reached
            ' zero, hash the current element, then reset the counter to the 'inc'
            ' value. It will then count down to zero
            Dim counter As Integer = 1
            For Each val As T In coll
                counter -= 1
                If counter <= 0 Then
                    Dim elem As Object = CObj(val)
                    If elem IsNot Nothing Then hash = hash Xor elem.GetHashCode()
                    counter = incr
                End If
            Next
            Return hash

        End Function

        ''' <summary>
        ''' Checks if the two collections are equal - ie. they each contain the same
        ''' elements, as decided by the <see cref="m:Object.Equals"/> method in the
        ''' same order. Note that, for the purposes of this method, a null enumerable
        ''' is 'equal' to an enumerable with no elements.
        ''' </summary>
        ''' <param name="one">The first collection to check</param>
        ''' <param name="two">The second collection to check</param>
        ''' <returns>True if both collections are null or empty, or both contain the
        ''' same elements in the same order; False otherwise</returns>
        Public Shared Function AreEqual(Of T)( _
         ByVal one As ICollection(Of T), ByVal two As ICollection(Of T)) As Boolean

            ' If they are both null, I guess they are both equal
            If one Is Nothing AndAlso two Is Nothing Then Return True

            ' If either is null and the other is empty, they are still equal
            If one Is Nothing AndAlso two.Count = 0 Then Return True
            If two Is Nothing AndAlso one.Count = 0 Then Return True

            ' If either is null now, we know they are not equal
            If one Is Nothing OrElse two Is Nothing Then Return False

            ' Quickly check the length if we have access to it
            If one.Count <> two.Count Then Return False

            ' The hashcode is also a shortcut way of testing a collection
            If GetHashCode(one) <> GetHashCode(two) Then Return False

            ' So we now know that both collections have some values to test, and
            ' their hashes match, so we have to get down and dirty
            Dim oneEnum As IEnumerator = one.GetEnumerator()
            Dim twoEnum As IEnumerator = two.GetEnumerator()

            While True
                ' Get whether there is an element left in each enumerator
                Dim foundOne As Boolean = oneEnum.MoveNext()
                Dim foundTwo As Boolean = twoEnum.MoveNext()

                ' If neither have any elements left, then we're done and we got to
                ' the end without finding a difference - ergo... equal.
                If Not foundOne AndAlso Not foundTwo Then Return True

                ' Compare whether they both have an element (at least one of them
                ' has) and, if so, that their element matches in value.
                If foundOne <> foundTwo OrElse Not _
                 Object.Equals(oneEnum.Current, twoEnum.Current) Then Return False

            End While

            ' Can't actually get here - the while loop never breaks but to return
            ' Nevertheless, VB has that wonderful feature where a boolean function
            ' automagically returns false if there's no explicit return statement.
            ' Seems as good a place as any to utilise it.

        End Function

        ''' <summary>
        ''' Checks if the two enumerables are equivalent - ie. they each contain the
        ''' same elements, as decided by the <see cref="m:Object.Equals"/> method.
        ''' Note that the order of the elements is <em>not</em> tested in this
        ''' method, just their existence in each collection.
        ''' </summary>
        ''' <param name="one">The first enumerable to check</param>
        ''' <param name="two">The second enumerable to check</param>
        ''' <returns>True if both enumerables contain the same elements; False
        ''' otherwise </returns>
        Public Shared Function AreEquivalent( _
         ByVal one As IEnumerable, ByVal two As IEnumerable) As Boolean

            ' A null enumerable is equivalent to an empty enumerable...
            ' So check for both null first
            If one Is Nothing AndAlso two Is Nothing Then Return True

            ' Then replace any null enumerable with an empty one to ensure that we
            ' compare correctly (and without errors).
            If one Is Nothing Then one = GetEmpty.IEnumerable()
            If two Is Nothing Then two = GetEmpty.IEnumerable()

            Dim nowt As New Object() ' placeholder for 'Nothing' in the set.

            Dim matchedElements As New List(Of Object)

            ' Check that each element in 'one' has an equivalent in 'two'
            For Each el1 As Object In one
                Dim found As Boolean = False
                For Each el2 As Object In two
                    If Object.Equals(el1, el2) Then
                        found = True
                        matchedElements.Add(IIf(el2 Is Nothing, nowt, el2))
                        Exit For
                    End If
                Next
                If Not found Then Return False
            Next
            ' At this point all of one's elements were found to be in 'two'

            ' Now check that 'two' doesn't have any elements which haven't been
            ' matched, suggesting 'two' has more elements than 'one'
            For Each el As Object In two
                If el Is Nothing Then el = nowt
                If Not matchedElements.Remove(el) Then Return False
            Next

            ' Finally, ensure that matchedElements doesn't contain something which
            ' isn't in two; this would indicate that a count of elements is different
            ' ie. there were 2 occurrences of an element in one, but only 1 in two.
            If matchedElements.Count > 0 Then Return False

            ' They are equivalent
            Return True

        End Function

        ''' <summary>
        ''' Checks if one collection is a subset of another.
        ''' </summary>
        ''' <typeparam name="T">The type of the collections being checked</typeparam>
        ''' <param name="master">The master collection which is to be checked to see
        ''' if it contains all of the elements in the <paramref name="subset"/>.
        ''' This cannot be null.</param>
        ''' <param name="subset">The collection to check to see if all of its
        ''' elements exist in the <paramref name="master"/> collection. A null
        ''' collection is treated as an empty collection (automatically returning
        ''' true).</param>
        ''' <returns>True if all of the elements in <paramref name="subset"/> exist
        ''' in the <paramref name="master"/> collection, regardless of the order in
        ''' which the elements are placed in their collections.</returns>
        ''' <exception cref="ArgumentNullException">If <paramref name="master"/> is
        ''' null.</exception>
        Public Shared Function ContainsAll(Of T)( _
         ByVal master As ICollection(Of T), ByVal subset As IEnumerable(Of T)) _
         As Boolean
            If master Is Nothing Then Throw New ArgumentNullException(NameOf(master))
            If subset Is Nothing Then Return True
            For Each value As T In subset
                If Not master.Contains(value) Then Return False
            Next
            Return True
        End Function

        ''' <summary>
        ''' Checks if one collection contains any elements from another
        ''' </summary>
        ''' <typeparam name="T">The element type of the collections</typeparam>
        ''' <param name="master">The collection to check to see if any of the
        ''' <paramref name="subset"/> elements exist within it.</param>
        ''' <param name="subset">The subset of elements to check to see if they
        ''' appear in the <paramref name="master"/> collection.</param>
        ''' <returns>True if the <paramref name="master"/> collection is found to
        ''' contain any of the <paramref name="subset"/> elements. Note that if the
        ''' given subset is null or empty, this will return <c>False</c>.</returns>
        ''' <exception cref="ArgumentNullException">If <paramref name="master"/> is
        ''' null.</exception>
        Public Shared Function ContainsAny(Of T)( _
         ByVal master As ICollection(Of T), ByVal subset As IEnumerable(Of T)) _
         As Boolean
            If master Is Nothing Then Throw New ArgumentNullException(NameOf(master))
            If subset Is Nothing Then Return False
            For Each value As T In subset
                If master.Contains(value) Then Return True
            Next
            Return False
        End Function

        ''' <summary>
        ''' Makes the given <paramref name="toColl"/> collection a copy of the given
        ''' <paramref name="fromColl"/> collection. <i>ie.</i> it clears the 'to'
        ''' collection, and then adds a <see cref="ICloneable.Clone">clone</see> of
        ''' each of the elements in the 'from' collection to it.
        ''' </summary>
        ''' <typeparam name="T">The type of element that the collection holds - this
        ''' must be cloneable</typeparam>
        ''' <param name="fromColl">The collection to take the elements from</param>
        ''' <param name="toColl">The collection which should end up containing only
        ''' the cloned elements from the 'from' collection.</param>
        ''' <exception cref="ArgumentNullException">If either of the given
        ''' collections were null</exception>
        ''' <exception cref="ArgumentException">If the same collection was passed in
        ''' as the 'from' and the 'to' collection.</exception>
        ''' <exception cref="NotSupportedException">If the given 'to' collection is
        ''' read only.</exception>
        ''' <remarks>Note that, as it stands, this does not detect duplicate elements
        ''' in the collection - eg. CloneInto(New Object(){obj1, obj1}, outColl)
        ''' will clone 'obj1' into the output collection twice - so you end up with
        ''' two distinct objects, themselves distinct from obj1.
        ''' </remarks>
        Public Shared Sub CloneInto(Of T As {ICloneable})( _
         ByVal fromColl As ICollection(Of T), ByVal toColl As ICollection(Of T))

            If fromColl Is Nothing Then Throw New ArgumentNullException(
             NameOf(fromColl), My.Resources.CollectionUtil_YouCannotCloneFromANullCollection)
            If toColl Is Nothing Then Throw New ArgumentNullException(
             NameOf(toColl), My.Resources.CollectionUtil_YouCannotCloneIntoANullCollection)
            If fromColl Is toColl Then Throw New ArgumentException(
             My.Resources.CollectionUtil_YouCannotCloneACollectionToItself)

            toColl.Clear()
            For Each el As T In fromColl
                If el Is Nothing _
                 Then toColl.Add(Nothing) _
                 Else toColl.Add(DirectCast(el.Clone(), T))
            Next
        End Sub

        ''' <summary>
        ''' Flattens the given collections into a single collection, in the order
        ''' which the collections (and subsequently elements) were traversed.
        ''' </summary>
        ''' <typeparam name="T">The type of collection to return</typeparam>
        ''' <param name="colls">The collection of collections from which all elements
        ''' should be flattened into a single collection.</param>
        ''' <returns>A single collection containing all the elements in each of the
        ''' collections given.</returns>
        ''' <exception cref="ArgumentNullException">If <paramref name="colls"/> is
        ''' null.</exception>
        Public Shared Function Flatten(Of T)( _
         ByVal colls As ICollection(Of ICollection(Of T))) As ICollection(Of T)
            Return FlattenInto(colls, New List(Of T))
        End Function

        ''' <summary>
        ''' Flattens the given collections into the single given collection and
        ''' returns it. The elements are added in the order which the collections
        ''' (and subsequently elements) were traversed.
        ''' </summary>
        ''' <typeparam name="T">The type of collection to return</typeparam>
        ''' <param name="colls">The collection of collections from which all elements
        ''' should be flattened into a single collection.</param>
        ''' <param name="into">The collection into which all the elements should be
        ''' added.</param>
        ''' <returns>The specified collection after having all the elements in each
        ''' of the collections given added to it.</returns>
        ''' <remarks>Note that this does not modify the given collection at all, so
        ''' if it contained data before this method was called, the elements from the
        ''' specified collections would be added after that data.</remarks>
        ''' <exception cref="ArgumentNullException">If either
        ''' <paramref name="colls"/> or <paramref name="into"/> is null.</exception>
        ''' <exception cref="InvalidOperationException">If the
        ''' <paramref name="into"/> collection is read only.</exception>
        Public Shared Function FlattenInto(Of T)( _
         ByVal colls As ICollection(Of ICollection(Of T)), _
         ByVal into As ICollection(Of T)) As ICollection(Of T)
            If colls Is Nothing Then Throw New ArgumentNullException(NameOf(colls))
            If into Is Nothing Then Throw New ArgumentNullException(NameOf(into))

            For Each coll As ICollection(Of T) In colls
                For Each element As T In coll
                    into.Add(element)
                Next
            Next
            Return into
        End Function

    End Class

End Namespace
