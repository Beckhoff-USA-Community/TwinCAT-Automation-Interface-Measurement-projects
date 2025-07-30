using Training_Tutorials;

namespace TC_AI_MeasurementProject
{
    internal class Program
    {

        static Automation automation = null;

        [STAThread]
        static void Main(string[] args)
        {
            automation = new Automation();

            automation.CreateVsInstance();

            automation.CreateMeasurementProjectFromJsonFile("ConfigFiles\\MeasurmentConfig.json");

            automation.StartRecording();

            Thread.Sleep(10000); // Wait for 10 seconds to simulate recording time

            automation.StopRecording();

            automation.SaveAll();
            automation.CloseSolution();
        }
    }
}
