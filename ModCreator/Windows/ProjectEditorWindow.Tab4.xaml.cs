using ModCreator.Helpers;
using ModCreator.Models;
using ModCreator.WindowData;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MessageBox = System.Windows.MessageBox;

namespace ModCreator.Windows
{
    public partial class ProjectEditorWindow : CWindow<ProjectEditorWindowData>
    {
        [SupportedOSPlatform("windows6.1")]
        private void SetupVariablesSourceBinding()
        {
            var editor = this.FindName("txtVariablesSource") as ICSharpCode.AvalonEdit.TextEditor;
            if (editor == null) return;

            // Load C# syntax highlighting
            AvalonHelper.LoadCSharpSyntaxHighlighting(editor);
            
            // Load content from ModCreatorChildVars.cs if exists
            LoadVariablesSourceFile();
        }

        [SupportedOSPlatform("windows6.1")]
        private void LoadVariablesSourceFile()
        {
            if (WindowData?.Project == null) return;
            
            var editor = this.FindName("txtVariablesSource") as ICSharpCode.AvalonEdit.TextEditor;
            if (editor == null) return;

            var filePath = Path.Combine(WindowData.Project.ProjectPath, "ModProject", "ModCode", "ModMain", "Const", "ModCreatorChildVars.cs");
            
            editor.Text = File.Exists(filePath)
                ? File.ReadAllText(filePath)
                : "// File not found. Generate code first using 'Generate Code' button.";
        }

        private void AddVariable_Click(object sender, RoutedEventArgs e)
        {
            if (WindowData.GlobalVariables.Any(v => string.IsNullOrWhiteSpace(v.Name)))
            {
                MessageBox.Show(MessageHelper.Get("Messages.Error.CompleteExistingVariable"), MessageHelper.Get("Messages.Warning.Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            WindowData.GlobalVariables.Add(new GlobalVariable
            {
                Name = "",
                Type = "Object",
                Value = "",
                Description = ""
            });
            WindowData.StatusMessage = MessageHelper.Get("Messages.Success.AddedNewVariable");
        }

        private void DataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction != DataGridEditAction.Commit) return;

            var variable = e.Row.Item as GlobalVariable;
            if (variable == null) return;

            // Validate variable name
            if (string.IsNullOrWhiteSpace(variable.Name))
            {
                e.Cancel = true;
                MessageBox.Show(MessageHelper.Get("Messages.Error.EmptyVariableName"), MessageHelper.Get("Messages.Warning.Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                
                if (string.IsNullOrWhiteSpace(variable.Type) && string.IsNullOrWhiteSpace(variable.Value) && string.IsNullOrWhiteSpace(variable.Description))
                    WindowData.GlobalVariables.Remove(variable);
                return;
            }

            // Validate variable name format (must be valid C# identifier)
            if (!System.CodeDom.Compiler.CodeGenerator.IsValidLanguageIndependentIdentifier(variable.Name))
            {
                e.Cancel = true;
                MessageBox.Show(MessageHelper.GetFormat("Messages.Error.VariableNameInvalidIdentifier", variable.Name), MessageHelper.Get("Messages.Warning.Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Check for duplicate variable names
            var duplicates = WindowData.GlobalVariables.Where(v => v != variable && v.Name == variable.Name).ToList();
            if (duplicates.Any())
            {
                e.Cancel = true;
                MessageBox.Show(MessageHelper.GetFormat("Messages.Error.VariableNameDuplicate", variable.Name), MessageHelper.Get("Messages.Warning.Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validate type
            if (string.IsNullOrWhiteSpace(variable.Type))
            {
                e.Cancel = true;
                MessageBox.Show(MessageHelper.Get("Messages.Error.EmptyVariableType"), MessageHelper.Get("Messages.Warning.Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validate value against type
            if (!string.IsNullOrWhiteSpace(variable.Value))
            {
                if (!variable.ValidateValue(variable.Value, variable.Type))
                {
                    e.Cancel = true;
                    MessageBox.Show(MessageHelper.GetFormat("Messages.Error.InvalidValueForType", variable.Type, variable.ValidationError), MessageHelper.Get("Messages.Warning.Title"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
        }

        private void GenerateVariablesCode_Click(object sender, RoutedEventArgs e)
        {
            var outputPath = WindowData.SaveGlobalVariables();
            MessageBox.Show(MessageHelper.GetFormat("Messages.Success.VariablesCodeGenerated", outputPath), MessageHelper.Get("Messages.Info.Title"), MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CloneVariable_Click(object sender, RoutedEventArgs e)
        {
            var variable = (sender as Button)?.Tag as GlobalVariable;
            if (variable == null) return;

            var clonedVar = new GlobalVariable
            {
                Name = variable.Name + "_copy",
                Type = variable.Type,
                Value = variable.Value,
                Description = variable.Description
            };

            var index = WindowData.GlobalVariables.IndexOf(variable);
            if (index >= 0)
                WindowData.GlobalVariables.Insert(index + 1, clonedVar);
            else
                WindowData.GlobalVariables.Add(clonedVar);
            
            WindowData.StatusMessage = MessageHelper.GetFormat("Messages.Success.ClonedVariable", variable.Name);
        }

        private void RemoveVariable_Click(object sender, RoutedEventArgs e)
        {
            var variable = (sender as Button)?.Tag as GlobalVariable;
            if (variable == null) return;

            var result = MessageBox.Show(MessageHelper.GetFormat("Messages.Confirmation.RemoveVariable", variable.Name), MessageHelper.Get("Messages.Confirmation.Title"), MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                WindowData.GlobalVariables.Remove(variable);
                WindowData.StatusMessage = MessageHelper.GetFormat("Messages.Success.RemovedVariable", variable.Name);
            }
        }

        [SupportedOSPlatform("windows7.0")]
        private void ToggleGridView_Click(object sender, RoutedEventArgs e)
        {
            var dgVariables = this.FindName("dgGlobalVariables") as DataGrid;
            var txtSource = this.FindName("txtVariablesSource") as ICSharpCode.AvalonEdit.TextEditor;
            var btnGrid = this.FindName("btnGridView") as Button;
            var btnSource = this.FindName("btnSourceView") as Button;

            if (dgVariables == null || txtSource == null || btnGrid == null || btnSource == null) return;

            dgVariables.Visibility = Visibility.Visible;
            txtSource.Visibility = Visibility.Collapsed;
            
            btnGrid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2E5090"));
            btnGrid.Foreground = Brushes.White;
            btnSource.Background = Brushes.White;
            btnSource.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF666666"));
            WindowData.StatusMessage = MessageHelper.Get("Messages.Success.SwitchedToGridView");
        }

        [SupportedOSPlatform("windows7.0")]
        private void ToggleSourceView_Click(object sender, RoutedEventArgs e)
        {
            var dgVariables = this.FindName("dgGlobalVariables") as DataGrid;
            var txtSource = this.FindName("txtVariablesSource") as ICSharpCode.AvalonEdit.TextEditor;
            var btnGrid = this.FindName("btnGridView") as Button;
            var btnSource = this.FindName("btnSourceView") as Button;

            if (dgVariables == null || txtSource == null || btnGrid == null || btnSource == null) return;

            LoadVariablesSourceFile();

            dgVariables.Visibility = Visibility.Collapsed;
            txtSource.Visibility = Visibility.Visible;
            
            btnSource.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2E5090"));
            btnSource.Foreground = Brushes.White;
            btnGrid.Background = Brushes.White;
            btnGrid.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF666666"));
            WindowData.StatusMessage = MessageHelper.Get("Messages.Success.SwitchedToSourceView");
        }
    }
}