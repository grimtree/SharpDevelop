' Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
' This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

Imports System.CodeDom
Imports System.ComponentModel
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Threading.Tasks
Imports System.Threading

Imports ICSharpCode.Core
Imports ICSharpCode.SharpDevelop
Imports ICSharpCode.SharpDevelop.Project

Public Class VBNetProject
	Inherits CompilableProject
	
	Protected Overrides Sub OnPropertyChanged(e As ProjectPropertyChangedEventArgs)
		MyBase.OnPropertyChanged(e)
		If e.PropertyName = "OutputType" Then
			Select Case Me.OutputType
				Case OutputType.WinExe
					SetProperty(e.Configuration, e.Platform, "MyType", "WindowsForms", e.NewLocation, True)
					Exit Select
				Case OutputType.Exe
					SetProperty(e.Configuration, e.Platform, "MyType", "Console", e.NewLocation, True)
					Exit Select
				Case Else
					SetProperty(e.Configuration, e.Platform, "MyType", "Windows", e.NewLocation, True)
					Exit Select
			End Select
		End If
	End Sub

	Public Sub New(info As ProjectLoadInformation)
		MyBase.New(info)
		InitVB()
	End Sub

	Public Const DefaultTargetsFile As String = "$(MSBuildToolsPath)\Microsoft.VisualBasic.targets"

	Public Sub New(info As ProjectCreateInformation)
		MyBase.New(info)
		InitVB()

		Me.AddImport(DefaultTargetsFile, Nothing)

		SetProperty("Debug", Nothing, "DefineConstants", "DEBUG=1,TRACE=1", PropertyStorageLocations.ConfigurationSpecific, True)
		SetProperty("Release", Nothing, "DefineConstants", "TRACE=1", PropertyStorageLocations.ConfigurationSpecific, True)
	End Sub

	Public Overrides Function ResolveAssemblyReferences(cancellationToken As CancellationToken) As IEnumerable(Of ReferenceProjectItem)
		Dim additionalItems As ReferenceProjectItem() = {New ReferenceProjectItem(Me, "mscorlib"), New ReferenceProjectItem(Me, "Microsoft.VisualBasic")}
		Return SD.MSBuildEngine.ResolveAssemblyReferences(Me, additionalItems)
	End Function

	Private Sub InitVB()
		reparseReferencesSensitiveProperties.Add("TargetFrameworkVersion")
		reparseCodeSensitiveProperties.Add("DefineConstants")
	End Sub

	Public Overrides ReadOnly Property Language() As String
		Get
			Return VBNetProjectBinding.LanguageName
		End Get
	End Property
	
	Public Overrides Function BuildAsync(options As ProjectBuildOptions, feedbackSink As IBuildFeedbackSink, progressMonitor As IProgressMonitor) As Task(Of Boolean)
		If Me.MinimumSolutionVersion = SolutionFormatVersion.VS2005 Then
			Return SD.MSBuildEngine.BuildAsync(Me, options, feedbackSink, progressMonitor.CancellationToken, { Path.Combine(FileUtility.ApplicationRootPath, "bin\SharpDevelop.CheckMSBuild35Features.targets") })
		End If
		
		Return MyBase.BuildAsync(options, feedbackSink, progressMonitor)
	End Function

	Public ReadOnly Property OptionInfer() As Nullable(Of Boolean)
		Get
			Return GetValue("OptionInfer", False)
		End Get
	End Property

	Public ReadOnly Property OptionStrict() As Nullable(Of Boolean)
		Get
			Return GetValue("OptionStrict", False)
		End Get
	End Property

	Public ReadOnly Property OptionExplicit() As Nullable(Of Boolean)
		Get
			Return GetValue("OptionExplicit", True)
		End Get
	End Property

	Public ReadOnly Property OptionCompare() As Nullable(Of CompareKind)
		Get
			Dim val As String = GetEvaluatedProperty("OptionCompare")

			If "Text".Equals(val, StringComparison.OrdinalIgnoreCase) Then
				Return CompareKind.Text
			End If

			Return CompareKind.Binary
		End Get
	End Property

	Private Function GetValue(name As String, defaultVal As Boolean) As System.Nullable(Of Boolean)
		Dim val As String
		Try
			val = GetEvaluatedProperty(name)
		Catch generatedExceptionName As ObjectDisposedException
			' This can happen when the project is disposed but the resolver still tries
			' to access Option Infer (or similar).
			val = Nothing
		End Try

		If val Is Nothing Then
			Return defaultVal
		End If

		Return "On".Equals(val, StringComparison.OrdinalIgnoreCase)
	End Function

	Public Overrides Function GetDefaultNamespace(fileName As String) As String
		' use root namespace everywhere, ignore the folder name
		Return Me.RootNamespace
	End Function

	Public Overrides Function CreateCodeDomProvider() As System.CodeDom.Compiler.CodeDomProvider
		Return New Microsoft.VisualBasic.VBCodeProvider()
	End Function

	Protected Overrides Function CreateDefaultBehavior() As ProjectBehavior
		Return New VBProjectBehavior(Me, MyBase.CreateDefaultBehavior())
	End Function
End Class

Public Class VBProjectBehavior
	Inherits ProjectBehavior
	Public Sub New(project As VBNetProject, Optional [next] As ProjectBehavior = Nothing)

		MyBase.New(project, [next])
	End Sub

	Public Overrides Function GetDefaultItemType(fileName As String) As ItemType
		If String.Equals(Path.GetExtension(fileName), ".vb", StringComparison.OrdinalIgnoreCase) Then
			Return ItemType.Compile
		Else
			Return MyBase.GetDefaultItemType(fileName)
		End If
	End Function
End Class
