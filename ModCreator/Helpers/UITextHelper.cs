using System;

namespace ModCreator.Helpers
{
    /// <summary>
    /// Main window UI texts helper for XAML bindings
    /// </summary>
    public static class MainWindowText
    {
        public static string AppTitle => MessageHelper.Get("Windows.MainWindow.AppTitle");
        public static string AppName => MessageHelper.Get("Windows.MainWindow.AppName");
        public static string AppSubtitle => MessageHelper.Get("Windows.MainWindow.AppSubtitle");
        public static string Help => MessageHelper.Get("Windows.MainWindow.Help");
        public static string About => MessageHelper.Get("Windows.MainWindow.About");
        public static string Workplace => MessageHelper.Get("Windows.MainWindow.Workplace");
        public static string Browse => MessageHelper.Get("Windows.MainWindow.Browse");
        public static string ProjectList => MessageHelper.Get("Windows.MainWindow.ProjectList");
        public static string CreateNewProject => MessageHelper.Get("Windows.MainWindow.CreateNewProject");
        public static string RefreshList => MessageHelper.Get("Windows.MainWindow.RefreshList");
        public static string SearchPlaceholder => MessageHelper.Get("Windows.MainWindow.SearchPlaceholder");
        public static string HeaderProjectName => MessageHelper.Get("Windows.MainWindow.HeaderProjectName");
        public static string HeaderDescription => MessageHelper.Get("Windows.MainWindow.HeaderDescription");
        public static string HeaderId => MessageHelper.Get("Windows.MainWindow.HeaderId");
        public static string HeaderState => MessageHelper.Get("Windows.MainWindow.HeaderState");
        public static string HeaderCreated => MessageHelper.Get("Windows.MainWindow.HeaderCreated");
        public static string HeaderModified => MessageHelper.Get("Windows.MainWindow.HeaderModified");
        public static string HeaderActions => MessageHelper.Get("Windows.MainWindow.HeaderActions");
        public static string GridOpenFolder => MessageHelper.Get("Windows.MainWindow.GridOpenFolder");
        public static string TooltipOpenFolder => MessageHelper.Get("Windows.MainWindow.TooltipOpenFolder");
        public static string GridEditInfo => MessageHelper.Get("Windows.MainWindow.GridEditInfo");
        public static string TooltipEditInfo => MessageHelper.Get("Windows.MainWindow.TooltipEditInfo");
        public static string GridDelete => MessageHelper.Get("Windows.MainWindow.GridDelete");
        public static string TooltipDelete => MessageHelper.Get("Windows.MainWindow.TooltipDelete");
        public static string ProjectDetails => MessageHelper.Get("Windows.MainWindow.ProjectDetails");
        public static string NoProjectSelected => MessageHelper.Get("Windows.MainWindow.NoProjectSelected");
        public static string ProjectDetailsName => MessageHelper.Get("Windows.MainWindow.ProjectDetailsName");
        public static string ProjectDetailsId => MessageHelper.Get("Windows.MainWindow.ProjectDetailsId");
        public static string ProjectDetailsPath => MessageHelper.Get("Windows.MainWindow.ProjectDetailsPath");
        public static string ProjectDetailsDescription => MessageHelper.Get("Windows.MainWindow.ProjectDetailsDescription");
        public static string ProjectDetailsAuthor => MessageHelper.Get("Windows.MainWindow.ProjectDetailsAuthor");
        public static string Actions => MessageHelper.Get("Windows.MainWindow.Actions");
        public static string OpenFolder => MessageHelper.Get("Windows.MainWindow.OpenFolder");
        public static string EditInfo => MessageHelper.Get("Windows.MainWindow.EditInfo");
        public static string RemoveProject => MessageHelper.Get("Windows.MainWindow.RemoveProject");
        public static string Info => MessageHelper.Get("Windows.MainWindow.Info");
        public static string TotalProjects => MessageHelper.Get("Windows.MainWindow.TotalProjects");
        public static string Template => MessageHelper.Get("Windows.MainWindow.Template");
    }

    /// <summary>
    /// About window UI texts helper for XAML bindings
    /// </summary>
    public static class AboutWindowText
    {
        public static string About => MessageHelper.Get("Windows.AboutWindow.About");
        public static string Version => MessageHelper.Get("Windows.AboutWindow.Version");
        public static string Author => MessageHelper.Get("Windows.AboutWindow.Author");
        public static string Repository => MessageHelper.Get("Windows.AboutWindow.Repository");
        public static string License => MessageHelper.Get("Windows.AboutWindow.License");
        public static string Features => MessageHelper.Get("Windows.AboutWindow.Features");
        public static string FeaturesList => MessageHelper.Get("Windows.AboutWindow.FeaturesList");
        public static string Close => MessageHelper.Get("Windows.AboutWindow.Close");
    }

    /// <summary>
    /// Help window UI texts helper for XAML bindings
    /// </summary>
    public static class HelpWindowText
    {
        public static string Help => MessageHelper.Get("Windows.HelpWindow.Help");
        public static string HelpTitle => MessageHelper.Get("Windows.HelpWindow.HelpTitle");
        public static string HelpSubtitle => MessageHelper.Get("Windows.HelpWindow.HelpSubtitle");
        public static string Topics => MessageHelper.Get("Windows.HelpWindow.Topics");
    }

    /// <summary>
    /// New project window UI texts helper for XAML bindings
    /// </summary>
    public static class NewProjectWindowText
    {
        public static string NewProjectTitle => MessageHelper.Get("Windows.NewProjectWindow.NewProjectTitle");
        public static string ProjectFieldName => MessageHelper.Get("Windows.NewProjectWindow.ProjectFieldName");
        public static string ProjectFieldDescription => MessageHelper.Get("Windows.NewProjectWindow.ProjectFieldDescription");
        public static string ProjectFieldAuthor => MessageHelper.Get("Windows.NewProjectWindow.ProjectFieldAuthor");
        public static string Create => MessageHelper.Get("Windows.NewProjectWindow.Create");
        public static string Cancel => MessageHelper.Get("Windows.NewProjectWindow.Cancel");
    }

    /// <summary>
    /// Input window UI texts helper for XAML bindings
    /// </summary>
    public static class InputWindowText
    {
        public static string DefaultTitle => MessageHelper.Get("Windows.InputWindow.DefaultTitle");
        public static string DefaultLabel => MessageHelper.Get("Windows.InputWindow.DefaultLabel");
        public static string OK => MessageHelper.Get("Windows.InputWindow.OK");
        public static string Cancel => MessageHelper.Get("Windows.InputWindow.Cancel");
    }

    /// <summary>
    /// ModEvent item select window UI texts helper for XAML bindings
    /// </summary>
    public static class ModEventItemSelectWindowText
    {
        public static string DefaultTitle => MessageHelper.Get("Windows.ModEventItemSelectWindow.DefaultTitle");
        public static string Category => MessageHelper.Get("Windows.ModEventItemSelectWindow.Category");
        public static string SearchPlaceholder => MessageHelper.Get("Windows.ModEventItemSelectWindow.SearchPlaceholder");
        public static string SelectItem => MessageHelper.Get("Windows.ModEventItemSelectWindow.SelectItem");
        public static string Description => MessageHelper.Get("Windows.ModEventItemSelectWindow.Description");
        public static string Code => MessageHelper.Get("Windows.ModEventItemSelectWindow.Code");
        public static string OK => MessageHelper.Get("Windows.ModEventItemSelectWindow.OK");
        public static string Cancel => MessageHelper.Get("Windows.ModEventItemSelectWindow.Cancel");
    }

    /// <summary>
    /// Project editor window UI texts helper for XAML bindings
    /// </summary>
    public static class ProjectEditorWindowText
    {
        public static string ProjectEditorTitle => MessageHelper.Get("Windows.ProjectEditorWindow.ProjectEditorTitle");
        public static string ProjectIdLabel => MessageHelper.Get("Windows.ProjectEditorWindow.ProjectIdLabel");
        public static string Translate => MessageHelper.Get("Windows.ProjectEditorWindow.Translate");
        public static string Help => MessageHelper.Get("Windows.ProjectEditorWindow.Help");
        public static string Save => MessageHelper.Get("Windows.ProjectEditorWindow.Save");
        public static string Cancel => MessageHelper.Get("Windows.ProjectEditorWindow.Cancel");
        public static string TabProjectInfo => MessageHelper.Get("Windows.ProjectEditorWindow.TabProjectInfo");
        public static string ProjectDetailsId => MessageHelper.Get("Windows.ProjectEditorWindow.ProjectDetailsId");
        public static string ProjectDetailsPath => MessageHelper.Get("Windows.ProjectEditorWindow.ProjectDetailsPath");
        public static string ProjectDetailsDescription => MessageHelper.Get("Windows.ProjectEditorWindow.ProjectDetailsDescription");
        public static string ProjectDetailsAuthor => MessageHelper.Get("Windows.ProjectEditorWindow.ProjectDetailsAuthor");
        public static string ProjectDetailsTitleImg => MessageHelper.Get("Windows.ProjectEditorWindow.ProjectDetailsTitleImg");
        public static string HeaderCreated => MessageHelper.Get("Windows.ProjectEditorWindow.HeaderCreated");
        public static string HeaderModified => MessageHelper.Get("Windows.ProjectEditorWindow.HeaderModified");
        public static string HeaderState => MessageHelper.Get("Windows.ProjectEditorWindow.HeaderState");
        public static string HeaderTitleImg => MessageHelper.Get("Windows.ProjectEditorWindow.HeaderTitleImg");
        public static string TabModConf => MessageHelper.Get("Windows.ProjectEditorWindow.TabModConf");
        public static string ConfFiles => MessageHelper.Get("Windows.ProjectEditorWindow.ConfFiles");
        public static string TooltipCreateFolder => MessageHelper.Get("Windows.ProjectEditorWindow.TooltipCreateFolder");
        public static string TooltipDeleteFolder => MessageHelper.Get("Windows.ProjectEditorWindow.TooltipDeleteFolder");
        public static string TooltipAddConf => MessageHelper.Get("Windows.ProjectEditorWindow.TooltipAddConf");
        public static string TooltipCloneConf => MessageHelper.Get("Windows.ProjectEditorWindow.TooltipCloneConf");
        public static string TooltipRenameConf => MessageHelper.Get("Windows.ProjectEditorWindow.TooltipRenameConf");
        public static string TooltipRemoveConf => MessageHelper.Get("Windows.ProjectEditorWindow.TooltipRemoveConf");
        public static string JsonEditor => MessageHelper.Get("Windows.ProjectEditorWindow.JsonEditor");
        public static string SearchAndReplace => MessageHelper.Get("Windows.ProjectEditorWindow.SearchAndReplace");
        public static string OpenInNotepad => MessageHelper.Get("Windows.ProjectEditorWindow.OpenInNotepad");
        public static string Find => MessageHelper.Get("Windows.ProjectEditorWindow.Find");
        public static string FindNext => MessageHelper.Get("Windows.ProjectEditorWindow.FindNext");
        public static string Close => MessageHelper.Get("Windows.ProjectEditorWindow.Close");
        public static string Replace => MessageHelper.Get("Windows.ProjectEditorWindow.Replace");
        public static string ReplaceButton => MessageHelper.Get("Windows.ProjectEditorWindow.ReplaceButton");
        public static string ReplaceAll => MessageHelper.Get("Windows.ProjectEditorWindow.ReplaceAll");
        public static string TabModImg => MessageHelper.Get("Windows.ProjectEditorWindow.TabModImg");
        public static string ImageFiles => MessageHelper.Get("Windows.ProjectEditorWindow.ImageFiles");
        public static string TooltipCreateImgFolder => MessageHelper.Get("Windows.ProjectEditorWindow.TooltipCreateImgFolder");
        public static string TooltipDeleteImgFolder => MessageHelper.Get("Windows.ProjectEditorWindow.TooltipDeleteImgFolder");
        public static string TooltipImportImage => MessageHelper.Get("Windows.ProjectEditorWindow.TooltipImportImage");
        public static string TooltipExportImage => MessageHelper.Get("Windows.ProjectEditorWindow.TooltipExportImage");
        public static string TooltipRemoveImage => MessageHelper.Get("Windows.ProjectEditorWindow.TooltipRemoveImage");
        public static string ImagePreview => MessageHelper.Get("Windows.ProjectEditorWindow.ImagePreview");
        public static string TabGlobalVariables => MessageHelper.Get("Windows.ProjectEditorWindow.TabGlobalVariables");
        public static string GridView => MessageHelper.Get("Windows.ProjectEditorWindow.GridView");
        public static string SourceView => MessageHelper.Get("Windows.ProjectEditorWindow.SourceView");
        public static string AddVariable => MessageHelper.Get("Windows.ProjectEditorWindow.AddVariable");
        public static string VariableName => MessageHelper.Get("Windows.ProjectEditorWindow.VariableName");
        public static string VariableType => MessageHelper.Get("Windows.ProjectEditorWindow.VariableType");
        public static string VariableTypeTooltip => MessageHelper.Get("Windows.ProjectEditorWindow.VariableTypeTooltip");
        public static string VariableValue => MessageHelper.Get("Windows.ProjectEditorWindow.VariableValue");
        public static string VariableDescription => MessageHelper.Get("Windows.ProjectEditorWindow.VariableDescription");
        public static string HeaderActions => MessageHelper.Get("Windows.ProjectEditorWindow.HeaderActions");
        public static string TooltipCloneVariable => MessageHelper.Get("Windows.ProjectEditorWindow.TooltipCloneVariable");
        public static string TooltipRemoveVariable => MessageHelper.Get("Windows.ProjectEditorWindow.TooltipRemoveVariable");
        public static string GenerateCode => MessageHelper.Get("Windows.ProjectEditorWindow.GenerateCode");
        public static string GenerateCodeNote => MessageHelper.Get("Windows.ProjectEditorWindow.GenerateCodeNote");
        public static string TabModEvent => MessageHelper.Get("Windows.ProjectEditorWindow.TabModEvent");
        public static string ModEventFiles => MessageHelper.Get("Windows.ProjectEditorWindow.ModEventFiles");
        public static string TooltipCreateModEvent => MessageHelper.Get("Windows.ProjectEditorWindow.TooltipCreateModEvent");
        public static string TooltipCloneModEvent => MessageHelper.Get("Windows.ProjectEditorWindow.TooltipCloneModEvent");
        public static string TooltipRenameModEvent => MessageHelper.Get("Windows.ProjectEditorWindow.TooltipRenameModEvent");
        public static string TooltipDeleteModEvent => MessageHelper.Get("Windows.ProjectEditorWindow.TooltipDeleteModEvent");
        public static string ModEventEditor => MessageHelper.Get("Windows.ProjectEditorWindow.ModEventEditor");
        public static string GuiMode => MessageHelper.Get("Windows.ProjectEditorWindow.GuiMode");
        public static string CodeMode => MessageHelper.Get("Windows.ProjectEditorWindow.CodeMode");
        public static string SaveModEvent => MessageHelper.Get("Windows.ProjectEditorWindow.SaveModEvent");
        public static string BasicSettings => MessageHelper.Get("Windows.ProjectEditorWindow.BasicSettings");
        public static string Mode => MessageHelper.Get("Windows.ProjectEditorWindow.Mode");
        public static string OrderIndex => MessageHelper.Get("Windows.ProjectEditorWindow.OrderIndex");
        public static string CacheType => MessageHelper.Get("Windows.ProjectEditorWindow.CacheType");
        public static string WorkOn => MessageHelper.Get("Windows.ProjectEditorWindow.WorkOn");
        public static string EventMethodName => MessageHelper.Get("Windows.ProjectEditorWindow.EventMethodName");
        public static string EventMethodNameTooltip => MessageHelper.Get("Windows.ProjectEditorWindow.EventMethodNameTooltip");
        public static string Event => MessageHelper.Get("Windows.ProjectEditorWindow.Event");
        public static string AddEvent => MessageHelper.Get("Windows.ProjectEditorWindow.AddEvent");
        public static string Conditions => MessageHelper.Get("Windows.ProjectEditorWindow.Conditions");
        public static string AddCondition => MessageHelper.Get("Windows.ProjectEditorWindow.AddCondition");
        public static string Actions => MessageHelper.Get("Windows.ProjectEditorWindow.Actions");
        public static string AddAction => MessageHelper.Get("Windows.ProjectEditorWindow.AddAction");
        public static string OpenInExplorer => MessageHelper.Get("Windows.ProjectEditorWindow.OpenInExplorer");
        public static string Refresh => MessageHelper.Get("Windows.ProjectEditorWindow.Refresh");
        public static string NoteDirectFileManagement => MessageHelper.Get("Windows.ProjectEditorWindow.NoteDirectFileManagement");
        public static string IconFolder => MessageHelper.Get("Windows.ProjectEditorWindow.IconFolder");
        public static string IconDelete => MessageHelper.Get("Windows.ProjectEditorWindow.IconDelete");
        public static string IconAdd => MessageHelper.Get("Windows.ProjectEditorWindow.IconAdd");
        public static string IconClone => MessageHelper.Get("Windows.ProjectEditorWindow.IconClone");
        public static string IconEdit => MessageHelper.Get("Windows.ProjectEditorWindow.IconEdit");
        public static string IconRemove => MessageHelper.Get("Windows.ProjectEditorWindow.IconRemove");
        public static string IconImport => MessageHelper.Get("Windows.ProjectEditorWindow.IconImport");
        public static string IconExport => MessageHelper.Get("Windows.ProjectEditorWindow.IconExport");
        public static string IconFilter => MessageHelper.Get("Windows.ProjectEditorWindow.IconFilter");
        public static string IconPattern => MessageHelper.Get("Windows.ProjectEditorWindow.IconPattern");
        public static string TooltipCreateEventFolder => MessageHelper.Get("Windows.ProjectEditorWindow.TooltipCreateEventFolder");
        public static string TooltipDeleteEventFolder => MessageHelper.Get("Windows.ProjectEditorWindow.TooltipDeleteEventFolder");
        public static string TooltipToggleFilterLocalText => MessageHelper.Get("Windows.ProjectEditorWindow.TooltipToggleFilterLocalText");
        public static string TooltipCreateFromPattern => MessageHelper.Get("Windows.ProjectEditorWindow.TooltipCreateFromPattern");
    }

    /// <summary>
    /// Add configuration window UI texts helper for XAML bindings
    /// </summary>
    public static class AddConfWindowText
    {
        public static string AddConfTitle => MessageHelper.Get("Windows.AddConfWindow.AddConfTitle");
        public static string SelectConf => MessageHelper.Get("Windows.AddConfWindow.SelectConf");
        public static string Prefix => MessageHelper.Get("Windows.AddConfWindow.Prefix");
        public static string Description => MessageHelper.Get("Windows.AddConfWindow.Description");
        public static string Add => MessageHelper.Get("Windows.AddConfWindow.Add");
        public static string Cancel => MessageHelper.Get("Windows.AddConfWindow.Cancel");
        public static string SearchPlaceholder => MessageHelper.Get("Windows.AddConfWindow.SearchPlaceholder");
        public static string Selected => MessageHelper.Get("Windows.AddConfWindow.Selected");
    }

    /// <summary>
    /// Pattern selector window UI texts helper for XAML bindings
    /// </summary>
    public static class PatternSelectorWindowText
    {
        public static string WindowTitle => MessageHelper.Get("Windows.PatternSelectorWindow.WindowTitle");
        public static string SearchPlaceholder => MessageHelper.Get("Windows.PatternSelectorWindow.SearchPlaceholder");
        public static string SelectPattern => MessageHelper.Get("Windows.PatternSelectorWindow.SelectPattern");
        public static string PatternDetails => MessageHelper.Get("Windows.PatternSelectorWindow.PatternDetails");
        public static string Create => MessageHelper.Get("Windows.PatternSelectorWindow.Create");
        public static string Cancel => MessageHelper.Get("Windows.PatternSelectorWindow.Cancel");
        public static string AddRow => MessageHelper.Get("Windows.PatternSelectorWindow.AddRow");
        public static string RemoveRow => MessageHelper.Get("Windows.PatternSelectorWindow.RemoveRow");
    }
}
