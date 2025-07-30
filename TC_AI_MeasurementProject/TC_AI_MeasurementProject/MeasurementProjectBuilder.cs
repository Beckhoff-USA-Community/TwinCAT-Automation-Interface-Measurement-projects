using EnvDTE;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using TwinCAT.Measurement.AutomationInterface;
using TwinCAT.Scope2.Communications;

namespace TC_AI_MeasurementProject
{
    public class MeasurementProjectBuilder
    {
        private readonly Solution _solution;
        private readonly string _baseFolder;
        private readonly string _solutionFolder;
        private readonly string _measurementProjectName;
        private readonly string _scopeProjectName;
        private readonly string _measurementTemplateId;
        private readonly string _scopeTemplateId;

        public MeasurementProjectBuilder(
            Solution solution,
            string baseFolder,
            string solutionFolder,
            string measurementProjectName,
            string scopeProjectName,
            string measurementTemplateId,
            string scopeTemplateId)
        {
            _solution = solution;
            _baseFolder = baseFolder;
            _solutionFolder = solutionFolder;
            _measurementProjectName = measurementProjectName;
            _scopeProjectName = scopeProjectName;
            _measurementTemplateId = measurementTemplateId;
            _scopeTemplateId = scopeTemplateId;
        }

        public IMeasurementScope BuildFromJson(string jsonConfig)
        {
            var config = JsonConvert.DeserializeObject<MeasurementProjectConfig>(jsonConfig);
            var scopeProjectConfig = config.MeasurementProject.ScopeProject;

            // 1) Create & show the root measurement scope control
            var project = _solution.AddFromTemplate(
                _measurementTemplateId,
                $"{_baseFolder}{_solutionFolder}\\{_measurementProjectName}\\",
                _measurementProjectName);

            var scopeItem = project.ProjectItems.AddFromTemplate(
                _scopeTemplateId,
                $"{_scopeProjectName}.tcscopex");

            var scopeRoot = (IMeasurementScope)scopeItem.Object;
            SafeShowControl(scopeRoot);

            // 2) Populate DataPool entries
            scopeRoot.LookUpChild("DataPool", out object dpObj);
            var dpItem = (ProjectItem)dpObj;
            var dpScope = (IMeasurementScope)dpItem.Object;
            var dpScopes = new Dictionary<string, IMeasurementScope>();

            foreach (var acq in scopeProjectConfig.DataPool.AdsAcquisitions)
            {
                var dpChildItem = SafeCreateChild(dpScope);
                var dpChildScope = (IMeasurementScope)dpChildItem.Object;

                foreach (Property prop in dpChildItem.Properties)
                {
                    switch (prop.Name)
                    {
                        case "Common.Name":
                            prop.Value = acq.Name;
                            break;
                        case "Symbol ADS.Symbol Name":
                            prop.Value = acq.SymbolName;
                            break;
                        case "Symbol.DataType":
                            prop.Value = Enum.Parse(typeof(Scope2DataType), acq.DataType, true);
                            break;
                        case "Symbol ADS.Target Port":
                            prop.Value = acq.TargetPort;
                            break;
                        case "Symbol ADS.Target System":
                            prop.Value = acq.TargetSystem;
                            break;
                    }
                }

                dpScopes[acq.Name] = dpChildScope;
            }

            // 3) Create charts, axis groups & channels
            foreach (var chart in scopeProjectConfig.Charts)
            {
                var chartItem = SafeCreateChild(scopeRoot);
                var chartScope = (IMeasurementScope)chartItem.Object;
                chartScope.ChangeName($"{chart.Type} Chart");

                foreach (Property p in chartItem.Properties)
                    if (p.Name == "Settings.Stacked Axes")
                        p.Value = true;

                foreach (var axisGroup in chart.AxisGroups)
                {
                    var axisItem = SafeCreateChild(chartScope);
                    var axisScope = (IMeasurementScope)axisItem.Object;
                    axisScope.ChangeName(axisGroup.Name);

                    for (int i = 0; i < axisGroup.Channels.Count; i++)
                    {
                        var chanConfig = axisGroup.Channels[i];
                        var chanItem = SafeCreateChild(axisScope);
                        var chanScope = (IMeasurementScope)chanItem.Object;
                        chanScope.ChangeName(chanConfig.Name);

                        foreach (Property prop in chanItem.Properties)
                        {
                            if (prop.Name == "Common.Name")
                                prop.Value = chanConfig.Name;
                            else if (chart.Type.Equals("YT", StringComparison.OrdinalIgnoreCase)
                                     && prop.Name == "Y-Data.Acquisition")
                                prop.Value = dpScopes[chanConfig.Acquisition];
                            else if (chart.Type.Equals("XY", StringComparison.OrdinalIgnoreCase))
                            {
                                if (i == 0 && prop.Name == "X-Data.Acquisition")
                                    prop.Value = dpScopes[chanConfig.Acquisition];
                                else if (i == 1 && prop.Name == "Y-Data.Acquisition")
                                    prop.Value = dpScopes[chanConfig.Acquisition];
                            }
                        }
                    }
                }
            }

            return scopeRoot;
        }

        public void SafeShowControl(IMeasurementScope scopeRoot)
        {
            const int maxRetries = 5;
            const int baseDelayMs = 1000;

            for (int attempt = 1; attempt <= maxRetries; ++attempt)
            {
                try
                {
                    scopeRoot.ShowControl();
                    return;
                }
                catch (COMException ex) when (((uint)ex.ErrorCode == 0x800706BE) ||
                                              ((uint)ex.ErrorCode == 0x800706BA))
                {
                    System.Threading.Thread.Sleep(baseDelayMs * attempt);
                }
            }

            throw new InvalidOperationException(
                $"Unable to show scope control after {maxRetries} attempts.");
        }

        public ProjectItem SafeCreateChild(IMeasurementScope parentScope)
        {
            const int maxRetries = 5;
            const int baseDelayMs = 1000;
            uint lastHresult = 0;

            for (int attempt = 1; attempt <= maxRetries; ++attempt)
            {
                try
                {
                    parentScope.CreateChild(out object childObj);
                    return (ProjectItem)childObj;
                }
                catch (COMException ex) when (((uint)ex.ErrorCode == 0x800706BE) ||
                                              ((uint)ex.ErrorCode == 0x800706BA))
                {
                    lastHresult = (uint)ex.ErrorCode;
                    System.Threading.Thread.Sleep(baseDelayMs * attempt);
                }
            }

            throw new InvalidOperationException(
                $"Unable to create child after {maxRetries} attempts. Last HRESULT=0x{lastHresult:X8}");
        }
    }
}
