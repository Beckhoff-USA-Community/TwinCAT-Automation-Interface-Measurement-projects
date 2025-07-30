using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using EnvDTE;
using EnvDTE90;
using Microsoft.VisualStudio.Shell.Interop;
using TC_AI_MeasurementProject;
using TCatSysManagerLib;
using TwinCAT.Measurement.AutomationInterface;
using TwinCAT.Scope2.Communications;

namespace Training_Tutorials
{
    public class Automation
    {
    
        // member variables
        private DTE? _dte;
        private Solution? _solution;
        private IMeasurementScope? _scopeRoot;


        // names
        string solutionFolder = "Tutorials";
        string solutionName = @"\Training.sln";
        string MeasurmentProjectName = "MeasurmentProject";
        string ScopeProjectName = "ScopeProject";

        // TODO: Change with your local paths
        string baseFolder = @"C:\temp\Training\AI_Training\";

        //VS version
        const string PROGID = "TcXaeShell.DTE.17.0";

        // fixed template path  
        // Base TwinCAT install directory
        const string TWINCAT_DIR = @"C:\Program Files (x86)\Beckhoff\TwinCAT";

        // Measurement‐project templates
        static readonly string TEMPLATE_EMPTY_MEASUREMENT_PROJECT = Path.Combine(TWINCAT_DIR, "Functions", "TwinCAT Measurement", "Templates", "Projects", "Empty Measurement Project.tcmproj");
        static readonly string TEMPLATE_EMPTY_SCOPE_PROJECT  = Path.Combine(TWINCAT_DIR, "Functions", "TwinCAT Measurement", "Templates", "ProjectItems", "Empty Scope Project.tcscopex");

        public const string vsProjectKindMisc = "{66A2671D-8FB5-11D2-AA7E-00C04F688DDE}";

        public Automation()
        {
            // register COM message filter
            MessageFilter.Register();
        }

        ~Automation()
        {
            // revoke COM message filter
            MessageFilter.Revoke();
        }

        #region VS Handling

        public void CreateVsInstance()
        {
            /* ------------------------------------------------------
            * Create Visual Studio instance and make VS window visible
            * ------------------------------------------------------ */
            Type t = System.Type.GetTypeFromProgID(PROGID);
            _dte = (DTE)Activator.CreateInstance(t);
            _dte.SuppressUI = false;
            _dte.MainWindow.Visible = true;

            /* ------------------------------------------------------
            * Activate silent mode of VS optional (not required in this training)
            * Required additional to suppressUI in versions > 4020.0
            * ------------------------------------------------------ */
            ITcAutomationSettings settings = _dte.GetObject("TcAutomationSettings") as ITcAutomationSettings;
            settings.SilentMode = true;

            /* ------------------------------------------------------
             * Access remote manager (not required in this training)
             * ------------------------------------------------------ */
            /*ITcRemoteManager remoteManager = dte.GetObject("TcRemoteManager");
            remoteManager.Version = "3.1.4022.30";*/


            /* ------------------------------------------------------
             * Create directories for new Visual Studio solution
             * ------------------------------------------------------ */
            DirectoryHelper.DeleteDirectory(baseFolder);
            Directory.CreateDirectory(baseFolder + solutionFolder);

            /* ------------------------------------------------------
            * Create and save new solution
            * ------------------------------------------------------ */
            _solution = _dte.Solution;
            _solution.Create(baseFolder, solutionFolder);
            _solution.SaveAs(baseFolder + solutionFolder + solutionName);
        }


        public void ExitVs()
        {
            if (_dte != null)
            {
                _dte.Quit();
            }
        }

        #endregion

        #region Project and Solution Handling

        public void SaveAll()
        {
            if (_dte == null)
                return;

            if (_dte.Solution == null)
                return;

            if (_dte.Solution.Projects != null)
            {
                for (var i = 1; i <= _dte.Solution.Projects.Count; i++)
                {
                    // skip virtual projects and folders
                    if (_dte.Solution.Projects.Item(i).Kind == vsProjectKindMisc)
                        continue;

                    _dte.Solution.Projects.Item(i).Save();
                }
            }

            _dte.Solution.SaveAs(_dte.Solution.FullName);
        }

        public void CloseSolution()
        {
            /* ------------------------------------------------------
            * Close the solution if it was open
            * ------------------------------------------------------ */
            if (_solution != null)
                _solution.Close(false);

            _dte = null;
        }



        #endregion

        #region Measurement Project Handling

        /// <summary>
        /// Creates a measurement project based on the provided JSON file path.
        /// </summary>
        /// <param name="jsonFilePath">Path to the JSON file matching MeasurementProjectConfig schema.</param>
        public void CreateMeasurementProjectFromJsonFile(string jsonFilePath)
        {
            if (string.IsNullOrWhiteSpace(jsonFilePath))
                throw new ArgumentException("JSON file path must be provided.", nameof(jsonFilePath));

            if (!File.Exists(jsonFilePath))
                throw new FileNotFoundException($"Measurement project JSON file not found: {jsonFilePath}", jsonFilePath);

            string jsonConfig = File.ReadAllText(jsonFilePath);
            CreateMeasurementProjectFromJson(jsonConfig);
        }

        /// <summary>
        /// Creates a measurement project based on the provided JSON string.
        /// </summary>
        /// <param name="jsonConfig">JSON string matching MeasurementProjectConfig schema.</param>
        public void CreateMeasurementProjectFromJson(string jsonConfig)
        {
            if (string.IsNullOrWhiteSpace(jsonConfig))
                throw new ArgumentException("JSON content must be provided.", nameof(jsonConfig));

            var builder = new MeasurementProjectBuilder(
                _solution!,
                baseFolder,
                solutionFolder,
                MeasurmentProjectName,
                ScopeProjectName,
                TEMPLATE_EMPTY_MEASUREMENT_PROJECT,
                TEMPLATE_EMPTY_SCOPE_PROJECT
            );

            _scopeRoot = builder.BuildFromJson(jsonConfig);
        }

        public void StartRecording()
        {
            if (_scopeRoot == null) throw new InvalidOperationException("Build the project first.");
            _scopeRoot.StartRecord();
        }
        public void StopRecording()
        {
            if (_scopeRoot == null) throw new InvalidOperationException("Build the project first.");
            _scopeRoot.StopRecord();
        }
        public void SaveMeasurementProjectRecord()
        {

            if (_scopeRoot == null) throw new InvalidOperationException("Build the project first.");
            _scopeRoot.SaveSVD(baseFolder + solutionFolder + @"\MeasurementProject.svd");
        }
        public void ExportMeasurementProjectRecird()
        {
            if (_scopeRoot == null) throw new InvalidOperationException("Build the project first.");
            _scopeRoot.ExportCSV(baseFolder + solutionFolder + @"\MeasurementProject.csv");
        }

        #endregion


    }

}
