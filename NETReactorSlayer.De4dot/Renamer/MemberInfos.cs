/*
    Copyright (C) 2021 CodeStrikers.org
    This file is part of NETReactorSlayer.
    NETReactorSlayer is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    NETReactorSlayer is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
    You should have received a copy of the GNU General Public License
    along with NETReactorSlayer.  If not, see <http://www.gnu.org/licenses/>.
*/

using System.Collections.Generic;
using NETReactorSlayer.De4dot.Renamer.AsmModules;

namespace NETReactorSlayer.De4dot.Renamer
{
    public class MemberInfos
    {
        public MemberInfos() => _checkWinFormsClass = new DerivedFrom(WinformsClasses);

        public bool IsWinFormsClass(MTypeDef type) => _checkWinFormsClass.Check(type);

        public TypeInfo Type(MTypeDef t) => _allTypeInfos[t];

        public bool TryGetType(MTypeDef t, out TypeInfo info) => _allTypeInfos.TryGetValue(t, out info);

        public bool TryGetEvent(MEventDef e, out EventInfo info) => _allEventInfos.TryGetValue(e, out info);

        public bool TryGetProperty(MPropertyDef p, out PropertyInfo info) => _allPropertyInfos.TryGetValue(p, out info);

        public PropertyInfo Property(MPropertyDef prop) => _allPropertyInfos[prop];

        public EventInfo Event(MEventDef evt) => _allEventInfos[evt];

        public FieldInfo Field(MFieldDef field) => _allFieldInfos[field];

        public MethodInfo Method(MMethodDef method) => _allMethodInfos[method];

        public GenericParamInfo GenericParam(MGenericParamDef gparam) => _allGenericParamInfos[gparam];

        public ParamInfo Param(MParamDef param) => _allParamInfos[param];

        public void Add(MPropertyDef prop) => _allPropertyInfos[prop] = new PropertyInfo(prop);

        public void Add(MEventDef evt) => _allEventInfos[evt] = new EventInfo(evt);

        public void Initialize(Modules modules)
        {
            foreach (var type in modules.AllTypes)
            {
                _allTypeInfos[type] = new TypeInfo(type, this);

                foreach (var gp in type.GenericParams)
                    _allGenericParamInfos[gp] = new GenericParamInfo(gp);

                foreach (var field in type.AllFields)
                    _allFieldInfos[field] = new FieldInfo(field);

                foreach (var evt in type.AllEvents)
                    Add(evt);

                foreach (var prop in type.AllProperties)
                    Add(prop);

                foreach (var method in type.AllMethods)
                {
                    _allMethodInfos[method] = new MethodInfo(method);
                    foreach (var gp in method.GenericParams)
                        _allGenericParamInfos[gp] = new GenericParamInfo(gp);
                    foreach (var param in method.AllParamDefs)
                        _allParamInfos[param] = new ParamInfo(param);
                }
            }
        }

        private readonly Dictionary<MEventDef, EventInfo> _allEventInfos = new();
        private readonly Dictionary<MFieldDef, FieldInfo> _allFieldInfos = new();

        private readonly Dictionary<MGenericParamDef, GenericParamInfo> _allGenericParamInfos = new();

        private readonly Dictionary<MMethodDef, MethodInfo> _allMethodInfos = new();
        private readonly Dictionary<MParamDef, ParamInfo> _allParamInfos = new();

        private readonly Dictionary<MPropertyDef, PropertyInfo> _allPropertyInfos = new();

        private readonly Dictionary<MTypeDef, TypeInfo> _allTypeInfos = new();
        private readonly DerivedFrom _checkWinFormsClass;

        private static readonly string[] WinformsClasses =
        {
            "System.Windows.Forms.Control",
            "System.Windows.Forms.AxHost",
            "System.Windows.Forms.ButtonBase",
            "System.Windows.Forms.Button",
            "System.Windows.Forms.CheckBox",
            "System.Windows.Forms.RadioButton",
            "System.Windows.Forms.DataGrid",
            "System.Windows.Forms.DataGridView",
            "System.Windows.Forms.DataVisualization.Charting.Chart",
            "System.Windows.Forms.DateTimePicker",
            "System.Windows.Forms.GroupBox",
            "System.Windows.Forms.Integration.ElementHost",
            "System.Windows.Forms.Label",
            "System.Windows.Forms.LinkLabel",
            "System.Windows.Forms.ListControl",
            "System.Windows.Forms.ComboBox",
            "Microsoft.VisualBasic.Compatibility.VB6.DriveListBox",
            "System.Windows.Forms.DataGridViewComboBoxEditingControl",
            "System.Windows.Forms.ListBox",
            "Microsoft.VisualBasic.Compatibility.VB6.DirListBox",
            "Microsoft.VisualBasic.Compatibility.VB6.FileListBox",
            "System.Windows.Forms.CheckedListBox",
            "System.Windows.Forms.ListView",
            "System.Windows.Forms.MdiClient",
            "System.Windows.Forms.MonthCalendar",
            "System.Windows.Forms.PictureBox",
            "System.Windows.Forms.PrintPreviewControl",
            "System.Windows.Forms.ProgressBar",
            "System.Windows.Forms.ScrollableControl",
            "System.Windows.Forms.ContainerControl",
            "System.Windows.Forms.Form",
            "System.ComponentModel.Design.CollectionEditor.CollectionForm",
            "System.Messaging.Design.QueuePathDialog",
            "System.ServiceProcess.Design.ServiceInstallerDialog",
            "System.Web.UI.Design.WebControls.CalendarAutoFormatDialog",
            "System.Web.UI.Design.WebControls.RegexEditorDialog",
            "System.Windows.Forms.Design.ComponentEditorForm",
            "System.Windows.Forms.PrintPreviewDialog",
            "System.Windows.Forms.ThreadExceptionDialog",
            "System.Workflow.Activities.Rules.Design.RuleConditionDialog",
            "System.Workflow.Activities.Rules.Design.RuleSetDialog",
            "System.Workflow.ComponentModel.Design.ThemeConfigurationDialog",
            "System.Workflow.ComponentModel.Design.TypeBrowserDialog",
            "System.Workflow.ComponentModel.Design.WorkflowPageSetupDialog",
            "System.Windows.Forms.PropertyGrid",
            "System.Windows.Forms.SplitContainer",
            "System.Windows.Forms.ToolStripContainer",
            "System.Windows.Forms.ToolStripPanel",
            "System.Windows.Forms.UpDownBase",
            "System.Windows.Forms.DomainUpDown",
            "System.Windows.Forms.NumericUpDown",
            "System.Windows.Forms.UserControl",
            "Microsoft.VisualBasic.Compatibility.VB6.ADODC",
            "System.Web.UI.Design.WebControls.ParameterEditorUserControl",
            "System.Workflow.ComponentModel.Design.WorkflowOutline",
            "System.Workflow.ComponentModel.Design.WorkflowView",
            "System.Windows.Forms.Design.ComponentTray",
            "System.Windows.Forms.Panel",
            "System.Windows.Forms.Design.ComponentEditorPage",
            "System.Windows.Forms.FlowLayoutPanel",
            "System.Windows.Forms.SplitterPanel",
            "System.Windows.Forms.TableLayoutPanel",
            "System.ComponentModel.Design.ByteViewer",
            "System.Windows.Forms.TabPage",
            "System.Windows.Forms.ToolStripContentPanel",
            "System.Windows.Forms.ToolStrip",
            "System.Windows.Forms.BindingNavigator",
            "System.Windows.Forms.MenuStrip",
            "System.Windows.Forms.StatusStrip",
            "System.Windows.Forms.ToolStripDropDown",
            "System.Windows.Forms.ToolStripDropDownMenu",
            "System.Windows.Forms.ContextMenuStrip",
            "System.Windows.Forms.ToolStripOverflow",
            "System.Windows.Forms.ScrollBar",
            "System.Windows.Forms.HScrollBar",
            "System.Windows.Forms.VScrollBar",
            "System.Windows.Forms.Splitter",
            "System.Windows.Forms.StatusBar",
            "System.Windows.Forms.TabControl",
            "System.Windows.Forms.TextBoxBase",
            "System.Windows.Forms.MaskedTextBox",
            "System.Windows.Forms.RichTextBox",
            "System.Windows.Forms.TextBox",
            "System.Windows.Forms.DataGridTextBox",
            "System.Windows.Forms.DataGridViewTextBoxEditingControl",
            "System.Windows.Forms.ToolBar",
            "System.Windows.Forms.TrackBar",
            "System.Windows.Forms.TreeView",
            "System.ComponentModel.Design.ObjectSelectorEditor.Selector",
            "System.Windows.Forms.WebBrowserBase",
            "System.Windows.Forms.WebBrowser"
        };
    }
}