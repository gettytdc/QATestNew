Public Class ctlFileOrDatabaseChooser : Inherits ctlWizardStageControl

    Friend Property ProcessLocation As FileOrDatabaseStage.ProcessLocationType
        Get
            If rdoDatabase.Checked Then
                Return FileOrDatabaseStage.ProcessLocationType.Database
            ElseIf rdoFile.Checked Then
                Return FileOrDatabaseStage.ProcessLocationType.File
            End If
        End Get
        Set(value As FileOrDatabaseStage.ProcessLocationType)
            If value = FileOrDatabaseStage.ProcessLocationType.Database Then
                rdoDatabase.Checked = True
                rdoFile.Checked = False
            ElseIf value = FileOrDatabaseStage.ProcessLocationType.File Then
                rdoFile.Checked = True
                rdoDatabase.Checked = False
            End If
        End Set
    End Property

End Class
