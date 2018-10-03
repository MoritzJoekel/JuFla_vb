﻿Imports System.ComponentModel
Imports DGVPrinterHelper
Imports System.Data.OleDb
Imports System.IO

Public Class FrmMain
    Public HomeStream As String = Application.UserAppDataPath
    Public DataStream As String = HomeStream + "\JuFla_Data.xml"
    Public TblImport As DataTable
    Private Sub VeranstaltungsdatenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles VeranstaltungsdatenToolStripMenuItem.Click
        FrmVeranstaltung.Show()
    End Sub
    ''' <summary>
    ''' Wird beim Laden der Anwendung aufgerufen
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Public Sub FrmMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Init(True)
        TiMain.Start()
    End Sub

    Public Sub Init(LoadDb As Boolean)
        If LoadDb = False Then
            Me.Text = "Jugendflamme - Landkreis " & My.Settings.RsLandkreis & " in " & My.Settings.RsOrt
        ElseIf LoadDb = True Then
            Me.Text = "Jugendflamme - Landkreis " & My.Settings.RsLandkreis & " in " & My.Settings.RsOrt
            Try
                DtsJuFla.ReadXml(DataStream)
            Catch ex As Exception
                MsgBox(ex.Message, MsgBoxStyle.Exclamation)
            End Try
        End If
    End Sub

    Private Sub FrmMain_Closing(sender As Object, e As CancelEventArgs) Handles MyBase.Closing
        Try
            DtsJuFla.WriteXml(DataStream)
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
        TiMain.Stop()
    End Sub

    Private Sub MannschaftStufe2HinzufügenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles MannschaftStufe2HinzufügenToolStripMenuItem.Click
        Dim Startnummer As Integer = InputBox("Startnummer: (0 = Nummer wird automatisch generiert)", , 0)
        Dim Ort As String = InputBox("Ort: (Ort-Ortsteil)")

        If Startnummer = 0 Then
            If DtsJuFla.TblJuFla2Mannschaften.Compute("Max(Startnummer)", Nothing) Is DBNull.Value Then
                Startnummer = 1
            Else
                Startnummer = DtsJuFla.TblJuFla2Mannschaften.Compute("Max(Startnummer)", Nothing) + 1
            End If
        End If
        Try
            DtsJuFla.TblJuFla2Mannschaften.Rows.Add(Nothing, Startnummer, Ort)
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Sub BtJuFla2Mannschaftloeschen_Click(sender As Object, e As EventArgs)
        Try
            For Each row As DataGridViewRow In DgvJuFla2Mannschaften.SelectedCells
                DgvJuFla2Mannschaften.Rows.Remove(row)
            Next
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Sub ÜbersichtMannschaftenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ÜbersichtMannschaftenToolStripMenuItem.Click
        FrmUebersicht_Mannschaften.BsJuFla2Mannschaften.DataSource = Me.DtsJuFla.TblJuFla2Mannschaften
        FrmUebersicht_Mannschaften.BsJuFla3Mannschaften.DataSource = Me.DtsJuFla.TblJuFla3Mannschaften
        FrmUebersicht_Mannschaften.ShowDialog(Me)
    End Sub

    Private Sub MannschaftStufe3HinzufügenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles MannschaftStufe3HinzufügenToolStripMenuItem.Click
        Dim Startnummer As Integer = InputBox("Startnummer: (0 = Nummer wird automatisch generiert)", , 0)
        Dim Ort As String = InputBox("Ort: (Ort-Ortsteil)")

        If Startnummer = 0 Then
            If DtsJuFla.TblJuFla3Mannschaften.Compute("Max(Startnummer)", Nothing) IsNot DBNull.Value Then
                Startnummer = DtsJuFla.TblJuFla3Mannschaften.Compute("Max(Startnummer)", Nothing) + 1
            Else
                Startnummer = 1
            End If
        End If
        Try
            DtsJuFla.TblJuFla3Mannschaften.Rows.Add(Nothing, Startnummer, Ort)
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Sub TiMain_Tick(sender As Object, e As EventArgs) Handles TiMain.Tick
        TbJuFla2AnzMember.Text = DgvJuFla2Member.Rows.Count
        TbJuFla3AnzBewerber.Text = DgvJuFla3Member.Rows.Count
        DgvJuFla2Member.Refresh()
        DgvJuFla3Member.Refresh()
    End Sub

    Private Sub WettbewerbseingabeToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles WettbewerbseingabeToolStripMenuItem.Click
        FrmWettbewerbseingabe.BsJuFla2Mannschaften.DataSource = Me.DtsJuFla.TblJuFla2Mannschaften
        FrmWettbewerbseingabe.BsJuFla3Mannschaften.DataSource = Me.DtsJuFla.TblJuFla3Mannschaften
        FrmWettbewerbseingabe.ShowDialog(Me)
    End Sub

    Private Sub DatenbankLeerenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles DatenbankLeerenToolStripMenuItem.Click
        Dim Result As MsgBoxResult = MsgBox("Wollen sie die Datenbank wirklich leeren?? (Backup wird erstellt)", MsgBoxStyle.YesNo)
        If Result = MsgBoxResult.Yes Then
            My.Computer.FileSystem.CopyFile(DataStream, HomeStream + "\JuFla_Data_Backup.xml", True)
            DtsJuFla.Clear()
            My.Computer.FileSystem.DeleteFile(DataStream)
        End If
    End Sub

    ''' <summary>
    ''' Manuelles Hinzufügen von Mitgliedern in die ausgewählte Mannschaft
    ''' </summary>
    ''' <param name="Stufe">Unterscheidung ob JuFla2 oder JuFla3 (2//3)</param>
    Private Sub AddMember(Stufe As Integer)
        If Stufe = 2 Then
            Try
                Dim sn As Integer = TbRoJuFla2Startnummer.Text
                Dim Name As String = InputBox("Name des Bewerbers", , "undefined")
                Dim Vorname As String = InputBox("Vorname des Bewerbers",, "undefinded")
                Dim Geschlecht As String = InputBox("Geschlecht des Bewerbers (m = männlich / w = weiblich",, "undefined")
                Dim Geburtsdatum As Date = InputBox("Geburtsdatum des Bewerbers (dd-mm-YYYY)",, "01.01.1900")
                Dim Ausweisnummer As Integer = InputBox("Ausweisnummer des Bewerbers",, "0")

                DtsJuFla.TblJuFla2Member.Rows.Add(Nothing, sn, Name, Vorname, Geschlecht, Geburtsdatum, Ausweisnummer, 0, 0, False, False, Name + ", " + Vorname)
            Catch ex As Exception
                MsgBox(ex.Message)
            End Try

        ElseIf Stufe = 3 Then
            Try
                Dim sn As Integer = TbJuFla3Startnummer.Text
                Dim Name As String = InputBox("Name des Bewerbers", , "undefined")
                Dim Vorname As String = InputBox("Vorname des Bewerbers",, "undefinded")
                Dim Geschlecht As String = InputBox("Geschlecht des Bewerbers (m = männlich / w = weiblich",, "undefined")
                Dim Geburtsdatum As Date = InputBox("Geburtsdatum des Bewerbers (dd-mm-YYYY)",, "01.01.1900")
                Dim Ausweisnummer As Integer = InputBox("Ausweisnummer des Bewerbers",, "0")

                DtsJuFla.TblJuFla3Member.Rows.Add(Nothing, sn, Name, Vorname, Geschlecht, Geburtsdatum, Ausweisnummer, 0, 0, 0, False, Name + ", " + Vorname, False)
            Catch ex As Exception
                MsgBox(ex.Message)
            End Try
        End If
    End Sub
    ''' <summary>
    ''' Druckt Listen der Member der ausgewählten Mannschaft
    ''' </summary>
    ''' <param name="Stufe">Unterscheidung ob JuFla2 oder JuFla3 (2//3)</param>
    Private Sub PrintMember(Stufe As Integer)
        If Stufe = 2 Then

            Dim printer As DGVPrinter = New DGVPrinter With {
                .Title = "Jugendflamme Stufe 2",
                .SubTitle = "Teilnehmer der Mannschaft: " & CbJuFla2Ort.Text & " (" & TbRoJuFla2Startnummer.Text & ")",
                .PorportionalColumns = True,
                .Footer = System.DateTime.Now.ToString,
                .PageText = "Anzahl Bewerber: " & TbJuFla2AnzMember.Text,
                .PageNumbers = False
            }
            printer.PrintDataGridView(DgvJuFla2Member)

        ElseIf Stufe = 3 Then

            Dim printer As DGVPrinter = New DGVPrinter With {
                .Title = "Jugendflamme Stufe 3",
                .SubTitle = "Teilnehmer der Mannschaft: " & CbJuFla3Mannschaft.Text & " (" & TbJuFla3Startnummer.Text & ")",
                .PorportionalColumns = True,
                .Footer = System.DateTime.Now.ToString,
                .PageText = "Anzahl Bewerber: " & TbJuFla3AnzBewerber.Text,
                .PageNumbers = False
            }
            printer.PrintDataGridView(DgvJuFla3Member)
        End If
    End Sub

    ''' <summary>
    ''' Behandelt alle Click-Ereignisse auf FrmMain
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub AnyButton_Click(sender As Object, e As EventArgs) Handles BtJuFla2Import.Click, BtJuFla3Import.Click,
            BtJuFla2PrintMember.Click, BtJuFla3PrintMember.Click, BtJuFla2AddMember.Click, BtJuFla3AddMember.Click, CmsJuFla2RemoveMember.Click, CmsJuFla3RemoveMember.Click
        Select Case True
            Case sender Is BtJuFla2Import : Import(2)
            Case sender Is BtJuFla3Import : Import(3)
            Case sender Is BtJuFla2PrintMember : PrintMember(2)
            Case sender Is BtJuFla3PrintMember : PrintMember(3)
            Case sender Is BtJuFla2AddMember : AddMember(2)
            Case sender Is BtJuFla3AddMember : AddMember(3)
            Case sender Is CmsJuFla2RemoveMember : RemoveMember(2)
            Case sender Is CmsJuFla3RemoveMember : RemoveMember(3)
        End Select
    End Sub

    ''' <summary>
    ''' Ruft einen FileDialog zum Import auf und importiert Daten aus Excel in die entsprechende Member-Datentabelle
    ''' </summary>
    ''' <param name="Stufe">Unterscheidung ob JuFla2 oder JuFla3 (2//3)</param>
    Public Sub Import(Stufe As Integer)

        ' Dialog zum Auswählen der Excel-Datei aufrufen
        Dim OfdImport As OpenFileDialog = New OpenFileDialog With {
            .Filter = "Excel-Arbeitsmappen|*.xls; *.xlsx"
        }
        OfdImport.ShowDialog()

        Dim sourceFile = OfdImport.FileName
        If sourceFile = "" Then
            Exit Sub
        End If

        ' Konvertierung von Excel zu .CSV
        Dim worksheetName = "Jugendflamme"
        Dim targetFile = HomeStream + "\Import.csv"

        Dim strConn As String = "Provider=Microsoft.ACE.OLEDB.12.0; Data Source=" & sourceFile & "; Extended Properties=Excel 12.0"
        Dim conn As OleDbConnection
        Dim wrtr As StreamWriter
        Dim cmd As OleDbCommand
        Dim da As OleDbDataAdapter

        ' Neue Verbindung mittels OleDb, CSV schreiben
        Try
            conn = New OleDbConnection(strConn)
            conn.Open()
            cmd = New OleDbCommand("SELECT * FROM [" & worksheetName & "$]", conn) With {
                .CommandType = CommandType.Text
            }
            wrtr = New StreamWriter(targetFile)
            da = New OleDbDataAdapter(cmd)

            TblImport = New DataTable()
            da.Fill(TblImport)

            For x As Integer = 0 To TblImport.Rows.Count - 1
                Dim rowString As String = Nothing
                For y As Integer = 0 To TblImport.Columns.Count - 1
                    rowString &= TblImport.Rows(x)(y).ToString() & ";"
                Next y
                wrtr.WriteLine(rowString)
            Next x


        Catch exc As Exception
            MessageBox.Show(exc.Message)

        Finally
            If conn.State = ConnectionState.Open Then
                conn.Close()
            End If
            If wrtr.BaseStream.ToString <> "" Then
                wrtr.Close()
            End If
        End Try

        ' Importierte Tabelle manipulieren, auf gewünschte Informationen zuschneiden
        For rowIndex As Integer = 0 To 7
            TblImport.Rows(rowIndex).Delete()
        Next
        Dim TotalRows As Integer = TblImport.Rows.Count

        For rowIndex As Integer = TotalRows - 4 To TotalRows - 1
            TblImport.Rows(rowIndex).Delete()
        Next

        Dim columns As DataColumnCollection = TblImport.Columns
        Dim Totalcolums As Integer = TblImport.Columns.Count
        Dim Data = TblImport

        '9 - Stufe 2 // 10 - Stufe 3
        columns.Remove("F1") ' Entferne (Spalte) laufende Nummer
        columns.Remove("F6") ' Entferne (Spalte) Abnahmedatum

        ' Entferne alle restlichen Spalten
        For columnIndex As Integer = 8 To 18
            columns.Remove("F" & columnIndex)
        Next

        TblImport.AcceptChanges()

        ' Unterscheidung ob für Stufe 2 oder Stufe 3 importiert werden soll
        If Stufe = 2 Then

            ' Durchlaufe alle Zeilen der Tabelle und sortiere Informationen ein
            For Each row As DataRow In TblImport.Rows

                Dim Name As String = row(0)
                Dim Vorname As String = row(1)
                Dim Geschlecht As String = row(2)
                Dim Geburtsdatum As Date = row(3).ToString
                Dim Ausweisnummer As Integer = row(4)

                ' Erstellt eine neue Row in JuFla2Member in der Datenbank
                DtsJuFla.TblJuFla2Member.Rows.Add(Nothing, TbRoJuFla2Startnummer.Text, Name, Vorname, Geschlecht, Geburtsdatum, Ausweisnummer, 0, 0, False, False, Name & ", " & Vorname)
            Next

        ElseIf Stufe = 3 Then

            ' Durchlaufe alle Zeilen der Tabelle und sortiere Informationen ein
            For Each row As DataRow In TblImport.Rows

                Dim Name As String = row(0)
                Dim Vorname As String = row(1)
                Dim Geschlecht As String = row(2)
                Dim Geburtsdatum As Date = row(3).ToString
                Dim Ausweisnummer As Integer = row(4)

                ' Erstellt eine neu eRow in JuFla2Member in der Datenbank
                DtsJuFla.TblJuFla3Member.Rows.Add(Nothing, TbJuFla3Startnummer.Text, Name, Vorname, Geschlecht, Geburtsdatum, Ausweisnummer, 0, 0, 0, False, Name & ", " & Vorname, False)
            Next
        End If
    End Sub

    ''' <summary>
    ''' Behandelt Context-Menustrip-Ereignis zum Entfernen des ausgewählten Members
    ''' </summary>
    ''' <param name="Stufe">Unterscheidung ob JuFla2 oder JuFla3 (2//3)</param>
    Private Sub RemoveMember(Stufe As Integer)
        If Stufe = 2 Then
            Try
                Dim index As Integer = DgvJuFla2Member.CurrentCell.RowIndex
                Dim result As MsgBoxResult = MsgBox("Bewerber " & DgvJuFla2Member.Rows(index).Cells(2).Value.ToString & " wirklich entfernen?", MsgBoxStyle.YesNo)
                If result = MsgBoxResult.Yes Then
                    DgvJuFla2Member.Rows.RemoveAt(index)
                End If
            Catch ex As Exception
                MsgBox(ex.Message)
            End Try

        ElseIf Stufe = 3 Then

            Try
                Dim index As Integer = DgvJuFla3Member.CurrentCell.RowIndex
                Dim result As MsgBoxResult = MsgBox("Bewerber " & DgvJuFla3Member.Rows(index).Cells(2).Value.ToString & " wirklich entfernen?", MsgBoxStyle.YesNo)
                If result = MsgBoxResult.Yes Then
                    DgvJuFla3Member.Rows.RemoveAt(index)
                End If
            Catch ex As Exception
                MsgBox(ex.Message)
            End Try
        End If
    End Sub
End Class
