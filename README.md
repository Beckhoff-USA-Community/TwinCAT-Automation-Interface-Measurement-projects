# TC_AI_MeasurementProject

Automates creation and recording of TwinCAT Measurement & Scope projects from a JSON configuration.  

## Features

- Launches Visual Studio (TwinCAT XAE shell) via COM automation  
- Creates and saves a new TwinCAT Measurement Project from JSON  
- Populates DataPool ADS acquisitions with name, data type, symbol, target port & system  
- Builds YT/XY charts with axis groups and channels  
- Starts/stops scope recording and exports CSV/SVD files  
- Includes a sample PLC project to generate signals for testing  

## Prerequisites

- **Windows 10/11** with TwinCAT 3 Package manager installed
- **Required TwinCAT Modules**:
  - TwinCAT.Standard.XAE 4026.17.0+
  - TE1300.ScopeViewProfessional.XAE 34.49.7+
- **Development Environment**:
  - Visual Studio 2022 or later
  - .NET 8.0 SDK
- **NuGet Dependencies**:
  - [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json/)
  - [EnvDTE](https://www.nuget.org/packages/envdte)

### TwinCAT COM Assemblies

Ensure these are in your PATH (typically at `C:\Program Files (x86)\Beckhoff\TwinCAT\Functions\TE130x-Scope-View`):
- `TwinCAT.Measurement.AutomationInterface.dll`
- `TwinCAT.Scope2.Communications.dll`

## Sample TwinCAT PLC Project

A complementary TwinCAT solution **TwinCAT Project Signal Gen** is provided alongside this C# code. It generates simulated ADS signals.

## Getting Started

1. **Clone the repository**  
   ```bash
   git clone https://github.com/your-org/TC_AI_MeasurementProject.git
   cd TC_AI_MeasurementProject
   ```

2. **Build**  
   ```bash
   dotnet build
   ```

3. **Configure paths**  
   In `Automation.cs`, update the VS shell ProgID, `baseFolder`, and `TWINCAT_DIR` constants.

4. **JSON configuration**  
   Edit `ConfigFiles/MeasurementConfig.json` to define your acquisitions and charts.

5. **Run**  
   ```bash
   dotnet run --project TC_AI_MeasurementProject.csproj
   ```

## Configuration File

A minimal example `ConfigFiles/MeasurementConfig.json`:

```json
{
  "measurementProject": {
    "scopeProject": {
      "dataPool": {
        "adsAcquisitions": [
          {
            "name": "Acquisition1",
            "dataType": "Double",
            "symbolName": "MAIN.tDoubleVar",
            "targetPort": 851,
            "targetSystem": "127.0.0.1"
          }
        ]
      },
      "charts": [
        {
          "type": "YT",
          "axisGroups": [
            {
              "name": "AxisGroup1",
              "channels": [
                {
                  "acquisition": "Acquisition1",
                  "name": "Channel1"
                }
              ]
            }
          ]
        }
      ]
    }
  }
}
```

- **adsAcquisitions**: list of ADS symbols under DataPool  
- **type**: chart type, e.g., `"YT"` or `"XY"`  
- **axisGroups**: grouping of channels mapping to acquisitions  

## Project Structure

- `TC_AI_MeasurementProject/` — C# source code and config  
- `TwinCAT Project Signal Gen/` — sample PLC project generating test signals  

## License

All sample code provided by [Beckhoff Automation LLC](https://www.beckhoff.com/en-us/) are for illustrative purposes only and are provided “as is” and without any warranties, express or implied. Actual implementations in applications will vary significantly. Beckhoff Automation LLC shall have no liability for, and does not waive any rights in relation to, any code samples that it provides or the use of such code samples for any purpose.

## Support

Should you have any questions regarding the provided sample code, please contact your local Beckhoff support team. Contact information can be found on the official Beckhoff website at https://www.beckhoff.com/en-us/support/.

## Further Information

See Beckhoff Infosys for [Creating and handling TwinCAT Measurement projects](https://infosys.beckhoff.com/content/1033/tc3_automationinterface/498477707.html?id=2276188083030678414).
