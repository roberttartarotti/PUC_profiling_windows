# .NET 10.0 Upgrade Plan

## Execution Steps

Execute steps below sequentially one by one in the order they are listed.

1. Validate that a .NET 10.0 SDK required for this upgrade is installed on the machine and if not, help to get it installed
2. Ensure that the SDK version specified in global.json files is compatible with the .NET 10.0 upgrade
3. Upgrade Aula3\Aula3.csproj to .NET 10.0

## Settings

This section contains settings and data used by execution steps.

### Excluded projects

None.

### Aggregate NuGet packages modifications across all projects

No NuGet package modifications are required for this upgrade.

### Project upgrade details

This section contains details about each project upgrade and modifications that need to be done in the project.

#### Aula3\Aula3.csproj modifications

Project properties changes:
  - Target framework should be changed from `net8.0` to `net10.0`

Other changes:
  - No other changes required for this upgrade