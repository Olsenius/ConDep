﻿function Remove-ConDepWinService {
	[CmdletBinding()]
	param (
		[Parameter(Mandatory=$true)] [string] $serviceName,
		[int] $timeoutInSeconds = 0,
		[bool] $ignoreFailure = $false) 
		
	try {
		$managedService = Get-Service $serviceName -ErrorAction SilentlyContinue

		if(!$managedService) {
			return
		}

		$processId = (get-wmiobject Win32_Service -filter "name='$serviceName'").ProcessID

		Stop-ConDepWinService $serviceName $timeoutInSeconds $ignoreFailure
				
		$service = Get-WmiObject -Class Win32_Service -Filter "Name='$serviceName'"
		Write-Host "Removing Windows Service [$($service.DisplayName)]."
		$result = $service.Delete().ReturnValue
		if($result -ne 0) {
			throw 'Unable to delete service [$serviceName]. Return code [$result].'
		}
		Write-Host "Windows Service [$($service.DisplayName)] removed."

		if($processId) {
			$process = Get-Process -Id $processId -ErrorAction SilentlyContinue
			
			if($process -and !$process.HasExited) {
				Write-Host "Waiting for process with id [$processId] to exit..."
				$process.WaitForExit()
				Write-Host "Process id [$processId] has now exited."
			}
		}
	}
	catch {
		if($ignoreFailure) {
			return 0
		}
		else {
			throw
		}
	}
}

function Stop-ConDepWinService {
	[CmdletBinding()]
	param (
		[Parameter(Mandatory=$true)] [string] $serviceName,
		[int] $timeoutInSeconds = 0,
		[bool] $ignoreFailure = $false) 

	try {
	    $service = Get-Service $serviceName -ErrorAction Stop

		if ($service.Status -eq [System.ServiceProcess.ServiceControllerStatus]::Running) 
		{ 
			Write-Host "Stopping: $($service.DisplayName)"
			$service.Stop()
			$service.WaitForStatus([System.ServiceProcess.ServiceControllerStatus]::Stopped)
			Write-Host "Stopped: $($service.DisplayName)" 
		}
		else {
			Write-Host "$($service.DisplayName) is already stopped" 
		}	
	}
	catch {
		if($ignoreFailure) {
			return 0
		}
		else {
			throw
		}
	}
}

function Start-ConDepWinService {
	[CmdletBinding()]
	param (
		[Parameter(Mandatory=$true)] [string] $serviceName,
		[int] $timeoutInSeconds = 0,
		[bool] $ignoreFailure = $false) 

	try { 
	    $service = Get-Service $serviceName -ErrorAction Stop
		if ($service.Status -eq [System.ServiceProcess.ServiceControllerStatus]::Stopped) 
		{ 
			Write-Host "Starting: $($service.DisplayName)"
			$service.Start()
			$service.WaitForStatus([System.ServiceProcess.ServiceControllerStatus]::Running)
			Write-Host "Started: $($service.DisplayName)"
		} 
		else 
		{ 
			Write-Host "$($service.DisplayName) is already running" 
		} 
	}
	catch {
		if($ignoreFailure) {
			return 0
		}
		else {
			throw
		}
	}
}

function New-ConDepWinService {
	[CmdletBinding()]
	param (
		[Parameter(Mandatory=$true)] [string] $serviceName,
		[Parameter(Mandatory=$true)] [string] $pathAndParamsForExecutable,
		[string] $displayName = "",
		[string] $description = "",
		[string] $user = "",
		[string] $password = "",
		[System.ServiceProcess.ServiceStartMode] $startupType = [System.ServiceProcess.ServiceStartMode]::Automatic,
		[string[]] $dependsOn = $()) 

	$serviceParams = @{}
	$serviceParams.Name = $serviceName
	$serviceParams.BinaryPathName = $pathAndParamsForExecutable
	if($displayName) {$serviceParams.DisplayName = $displayName}
	if($description) {$serviceParams.Description = $description}
	if($startupType) {$serviceParams.StartupType = $startupType}
	if($dependsOn) {$serviceParams.DependsOn = $dependsOn}
	
	if($user) {
		if($password) {
			$secpasswd = ConvertTo-SecureString $password -AsPlainText -Force
		}
		else {
			throw "When giving credentials you need to provide password too."
		}
		
		$serviceParams.Credential = New-Object System.Management.Automation.PSCredential ($user, $secpasswd)
	}
	New-Service @serviceParams
}