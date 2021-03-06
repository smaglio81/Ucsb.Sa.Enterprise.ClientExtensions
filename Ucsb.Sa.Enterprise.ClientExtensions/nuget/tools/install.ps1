﻿param($installPath, $toolsPath, $package, $project)

#Write-Host "installPath: $installPath"
#Write-Host "toolsPath: $toolsPath"
#Write-Host "package: $package"
#Write-Host "project: $project"

# open json.net splash page on package install
# don't open if json.net is installed as a dependency

try
{
  $url = "https://sist-sa-ucsb.visualstudio.com/Ucsb.Sa.Enterprise/_versionControl?path=%24%2FUcsb.Sa.Enterprise%2FUcsb.Sa.Enterprise.ClientExtensions%2FDev%2FUcsb.Sa.Enterprise.ClientExtensions%2Freadme.md&version=T&_a=preview"
  $dte2 = Get-Interface $dte ([EnvDTE80.DTE2])

  if ($dte2.ActiveWindow.Caption -eq "Package Manager Console")
  {
    # user is installing from VS NuGet console
    # get reference to the window, the console host and the input history
    # show webpage if "install-package newtonsoft.json" was last input

    $consoleWindow = $(Get-VSComponentModel).GetService([NuGetConsole.IPowerConsoleWindow])

    $props = $consoleWindow.GetType().GetProperties([System.Reflection.BindingFlags]::Instance -bor `
      [System.Reflection.BindingFlags]::NonPublic)

    $prop = $props | ? { $_.Name -eq "ActiveHostInfo" } | select -first 1
    if ($prop -eq $null) { return }
  
    $hostInfo = $prop.GetValue($consoleWindow)
    if ($hostInfo -eq $null) { return }

    $history = $hostInfo.WpfConsole.InputHistory.History

    $lastCommand = $history | select -last 1
    #Write-Host "lastCommand: $lastCommand"

    if ($lastCommand)
    {
      $lastCommand = $lastCommand.Trim().ToLower()

	  #Write-Host "first: $($lastCommand.StartsWith("install-package"))"
      #Write-Host "second: $($lastCommand.Contains("ucsb.sa.enterprise.clientextensions"))"
      #Write-Host "third: $($lastCommand.Contains(".redis") -eq $false)"

      if ($lastCommand.StartsWith("install-package") -and $lastCommand.Contains("ucsb.sa.enterprise.clientextensions") -and ($lastCommand.Contains(".redis") -eq $false))
      {
        $dte2.ItemOperations.Navigate($url) | Out-Null
      }
    }
  }
  else
  {
    # user is installing from VS NuGet dialog
    # get reference to the window, then smart output console provider
    # show webpage if messages in buffered console contains "installing...ucsb.sa.enterprise.clientextensions" in last operation

    $instanceField = [NuGet.Dialog.PackageManagerWindow].GetField("CurrentInstance", [System.Reflection.BindingFlags]::Static -bor `
      [System.Reflection.BindingFlags]::NonPublic)

    $consoleField = [NuGet.Dialog.PackageManagerWindow].GetField("_smartOutputConsoleProvider", [System.Reflection.BindingFlags]::Instance -bor `
      [System.Reflection.BindingFlags]::NonPublic)

    if ($instanceField -eq $null -or $consoleField -eq $null) { return }

    $instance = $instanceField.GetValue($null)

    if ($instance -eq $null) { return }

    $consoleProvider = $consoleField.GetValue($instance)
    if ($consoleProvider -eq $null) { return }

    $console = $consoleProvider.CreateOutputConsole($false)

    $messagesField = $console.GetType().GetField("_messages", [System.Reflection.BindingFlags]::Instance -bor `
      [System.Reflection.BindingFlags]::NonPublic)
    if ($messagesField -eq $null) { return }

    $messages = $messagesField.GetValue($console)
    if ($messages -eq $null) { return }

    $operations = $messages -split "=============================="

    $lastOperation = $operations | select -last 1

    if ($lastOperation)
    {
      $lastOperation = $lastOperation.ToLower()

      $lines = $lastOperation -split "`r`n"

      $installMatch = $lines |
		? {
			$_.StartsWith("------- installing...ucsb.sa.enterprise.clientextensions ")
			-or $_.StartsWith("------- installing...ucsb.sa.enterprise.clientextensions.debug ")
		} | select -first 1

      if ($installMatch)
      {
        $dte2.ItemOperations.Navigate($url) | Out-Null
      }
    }
  }
}
catch
{
  try
  {
    $pmPane = $dte2.ToolWindows.OutputWindow.OutputWindowPanes.Item("Package Manager")

    $selection = $pmPane.TextDocument.Selection
    $selection.StartOfDocument($false)
    $selection.EndOfDocument($true)

    if ($selection.Text.StartsWith("Attempting to gather dependencies information for package 'Ucsb.Sa.Enterprise.ClientExtensions." + $package.Version + "'"))
    {
      # don't show on upgrade
      if (!$selection.Text.Contains("Removed package"))
      {
        $dte2.ItemOperations.Navigate($url) | Out-Null
      }
    }
  }
  catch
  {
    # stop potential errors from bubbling up
    # worst case the splash page won't open  
  }
}

# still yolo