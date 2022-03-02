Public Enum ProcessEventCode
    <EventCode("P001","EventCodeAttribute_TheUser0CreatedTheProcess1")>
    CreateProcess
    <EventCode("P002","EventCodeAttribute_TheUser0DeletedTheProcess1")>
    DeleteProcess
    <EventCode("P003","EventCodeAttribute_TheUser0ClonedTheProcess1")>
    CloneProcess
    <EventCode("P004","EventCodeAttribute_TheUser0ModifiedTheProcess1")>
    ModifyProcess
    <EventCode("P005","EventCodeAttribute_TheUser0UnlockedTheProcess1")>
    UnlockProcess
    <EventCode("P006","EventCodeAttribute_TheUser0ImportedTheProcess1")>
    ImportProcess
    <EventCode("P007","EventCodeAttribute_TheUser0ExportedTheProcess1")>
    ExportProcess
    <EventCode("P008")>
    Unused
    <EventCode("P009","EventCodeAttribute_TheUser0ChangedTheAttributesOfTheProcess1")>
    ChangedAttributes
    <EventCode("P010","EventCodeAttribute_TheUser0RefreshedTheDependenciesOfTheProcess1")>
    RefreshDependencies
End Enum
