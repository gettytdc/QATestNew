Public Enum BusinessObjectEventCode
    <EventCode("B001","EventCodeAttribute_TheUser0CreatedTheBusinessObject1")>
    CreateBusinessObject
    <EventCode("B002","EventCodeAttribute_TheUser0DeletedTheBusinessObject1")>
    DeleteBusinessObject
    <EventCode("B003","EventCodeAttribute_TheUser0ClonedTheBusinessObject1")>
    CloneBusinessObject
    <EventCode("B004","EventCodeAttribute_TheUser0ModifiedTheBusinessObject1")>
    ModifyBusinessObject
    <EventCode("B005","EventCodeAttribute_TheUser0UnlockedTheBusinessObject1")>
    UnlockBusinessObject
    <EventCode("B006","EventCodeAttribute_TheUser0ImportedTheBusinessObject1")>
    ImportBusinessObject
    <EventCode("B007","EventCodeAttribute_TheUser0ExportedTheBusinessObject1")>
    ExportBusinessObject
    <EventCode("B008")>
    Unused
    <EventCode("B009","EventCodeAttribute_TheUser0ChangedTheAttributesOfTheBusinessObject1")>
    ChangedAttributes
    <EventCode("B010","EventCodeAttribute_TheUser0RefreshedTheDependenciesOfTheBusinessObject1")>
    RefreshDependencies
End Enum
