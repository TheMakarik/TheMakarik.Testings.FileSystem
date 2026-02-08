<#
    Runs TheMakarik.Testing.FileSystem tests, return 0 if success else -1
    By: TheMakarik
#>

Set-Variable -Name "testProjects" -Value @("tests/TheMakarik.Testing.FileSystem.Tests/TheMakarik.Testing.FileSystem.Tests.csproj", "examples/Examples.NUnit3/Examples.NUnit3.csproj", "examples/Examples.xUnit/Examples.xUnit.csproj") -Option ReadOnly -Scope Local;

function Test-Dotnet {
    try {
        dotnet --list-sdks | Out-Null; 
        Write-Host "Found .NET..."
    } catch {
        Write-Host ".NET is not installed. Please/Add to GA .NET" -ForegroundColor Red;
        exit -1;
    }
}

function Invoke-Tests {
    $testProjects | ForEach-Object {
        dotnet test $_  | Out-Null; 
        if($LASTEXITCODE -ne 0) {
            Write-Host "One or more tests are fallen in project $_" -ForegroundColor Red
            exit -1;
        }
        Write-Host "All test are passed";
        exit 0;
    }
}

Test-Dotnet;
Invoke-Tests


