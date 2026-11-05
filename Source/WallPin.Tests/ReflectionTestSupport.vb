Option Explicit On
Option Strict On
Option Infer Off

Imports System
Imports System.Reflection

''' <summary>
''' Provides reflection helpers for inspecting WallPin's non-public implementation
''' without changing the production assembly's visibility.
''' </summary>
Friend Module ReflectionTestSupport

    Private Const ProductionAssemblyName As String = "WallPin"

    ''' <summary>
    ''' Gets a type from the production WallPin assembly.
    ''' </summary>
    ''' <param name="typeName">The fully qualified type name.</param>
    ''' <returns>The requested production type.</returns>
    Friend Function GetProductionType(typeName As String) As Type
        Dim productionAssembly As Assembly = Assembly.Load(ProductionAssemblyName)
        Dim productionType As Type = productionAssembly.GetType(typeName, throwOnError:=True, ignoreCase:=False)

        Return productionType
    End Function

    ''' <summary>
    ''' Gets a static field value from a production type.
    ''' </summary>
    ''' <param name="typeName">The fully qualified declaring type name.</param>
    ''' <param name="fieldName">The field name.</param>
    ''' <returns>The field value.</returns>
    Friend Function GetStaticFieldValue(typeName As String, fieldName As String) As Object
        Dim productionType As Type = GetProductionType(typeName)
        Dim flags As BindingFlags = BindingFlags.Static Or BindingFlags.Public Or BindingFlags.NonPublic
        Dim field As FieldInfo = productionType.GetField(fieldName, flags)

        If field Is Nothing Then
            Throw New MissingFieldException(typeName, fieldName)
        End If

        Return field.GetValue(Nothing)
    End Function

End Module
