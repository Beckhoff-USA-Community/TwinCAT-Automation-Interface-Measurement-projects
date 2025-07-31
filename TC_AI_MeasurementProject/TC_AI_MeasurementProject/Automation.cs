using System;
using System.IO;
using Microsoft.Win32;
using EnvDTE;
using EnvDTE90;
using Microsoft.VisualStudio.Shell.Interop;
using TC_AI_MeasurementProject;
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

        // solution names and folders
        private readonly string baseFolder = @"C:\temp\Training\AI_Training\";
        private readonly string solutionFolder = "Tutorials";
        private readonly string solutionName = @"\Training.sln";
        private readonly string MeasurmentProjectName = "MeasurmentProject";
        private readonly string ScopeProjectName = "ScopeProject";

        // dynamic TwinCAT settings
        private readonly string _twinCATDir;
        private readonly string _progId;
        private readonly string _templateEmptyMeasurementProject;
        private readonly string _templateEmptyScopeProject;

        public const string vsProjectKindMisc = "{66A2671D-8FB5-11D2-AA7E-00C04F688DDE}";
        static readonly string[] ProgIdsToTry = { "TcXaeShell.DTE.17.0", "TcXaeShell.DTE.15.0" };

        public Automation()
        {
            // register COM message filter
            MessageFilter.Register();

   
            // 1) Read the TwinCAT3DIR env-var (e.g. "C:\TwinCAT\3.1")
            var installPath = Environment.GetEnvironmentVariable("TwinCAT3DIR")
                ?.TrimEnd('\\')
                ?? throw new InvalidOperationException("Environment variable 'TwinCAT3DIR' is not set.");


            // 2) Go up one directory (drops the "3.1") to get the Functions root
            var rootPath = Path.GetDirectoryName(installPath)
                ?? throw new InvalidOperationException($"Cannot determine parent directory of '{installPath}'.");


            _twinCATDir = rootPath;


            // 3) Build full paths to the measurement/scope templates
            _templateEmptyMeasurementProject = Path.Combine(
                _twinCATDir,
                "Functions", "TwinCAT Measurement",
                "Templates", "Projects",
                "Empty Measurement Project.tcmproj");

            _templateEmptyScopeProject = Path.Combine(
                _twinCATDir,
                "Functions", "TwinCAT Measurement",
                "Templates", "ProjectItems",
                "Empty Scope Project.tcscopex");


            // 4) Detect which ProgID is registered
            _progId = DetectProgId();
        }

        ~Automation()
        {
            // revoke COM message filter
            MessageFilter.Revoke();
        }

        private static string DetectProgId()
        {
            using var classesRoot = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Default);
            foreach (var id in ProgIdsToTry)
            {
                using var key = classesRoot.OpenSubKey(id);
                if (key != null)
                    return id;
            }
            throw new InvalidOperationException(
                "Neither TcXaeShell.DTE.17.0 nor TcXaeShell.DTE.15.0 was found in the registry.");
        }

        #region VS Handling

        public void CreateVsInstance()
        {
            // instantiate the correct XAE Shell based on detected ProgID
            Type t = Type.GetTypeFromProgID(_progId)
                     ?? throw new InvalidOperationException($"Could not load ProgID {_progId}.");
            _dte = (DTE)Activator.CreateInstance(t);
            _dte.SuppressUI = false;
            _dte.MainWindow.Visible = true;

            // prepare folders
            DirectoryHelper.DeleteDirectory(baseFolder);
            Directory.CreateDirectory(Path.Combine(baseFolder, solutionFolder));

            // create and save the solution
            _solution = _dte.Solution;
            _solution.Create(baseFolder, solutionFolder);
            var solutionPath = Path.Combine(baseFolder, solutionFolder) + solutionName;
            _solution.SaveAs(solutionPath);
        }

        public void ExitVs()
        {
            _dte?.Quit();
        }

        #endregion

        #region Project and Solution Handling

        public void SaveAll()
        {
            if (_dte?.Solution?.Projects == null) return;

            for (var i = 1; i <= _dte.Solution.Projects.Count; i++)
            {
                if (_dte.Solution.Projects.Item(i).Kind == vsProjectKindMisc)
                    continue;
                _dte.Solution.Projects.Item(i).Save();
            }
            _dte.Solution.SaveAs(_dte.Solution.FullName);
        }

        public void CloseSolution()
        {
            if (_solution != null)
                _solution.Close(false);
            _dte = null;
        }

        #endregion

        #region Measurement Project Handling

        public void CreateMeasurementProjectFromJsonFile(string jsonFilePath)
        {
            if (string.IsNullOrWhiteSpace(jsonFilePath))
                throw new ArgumentException("JSON file path must be provided.", nameof(jsonFilePath));
            if (!File.Exists(jsonFilePath))
                throw new FileNotFoundException($"Measurement project JSON file not found: {jsonFilePath}", jsonFilePath);

            string jsonConfig = File.ReadAllText(jsonFilePath);
            CreateMeasurementProjectFromJson(jsonConfig);
        }

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
                _templateEmptyMeasurementProject,
                _templateEmptyScopeProject);

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
            _scopeRoot.SaveSVD(Path.Combine(baseFolder, solutionFolder, "MeasurementProject.svd"));
        }

        public void ExportMeasurementProjectRecird()
        {
            if (_scopeRoot == null) throw new InvalidOperationException("Build the project first.");
            _scopeRoot.ExportCSV(Path.Combine(baseFolder, solutionFolder, "MeasurementProject.csv"));
        }

        #endregion
    }
}
