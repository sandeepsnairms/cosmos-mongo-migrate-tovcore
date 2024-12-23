#Clone Github repo

Clone `https://github.com/sandeepsnairms/mongomigrationui` to local

# Deploy App to Azure Web App

1. Open PowerShell.
2. Navigate to the cloned project folder.
3. Execute the below code in PowerShell.

	```Powershell

	# Variables to be replaced
	$resourceGroupName = <Replace with Existing Resource Group Name>
	$webAppName = <Replace with Web App Name>
	$projectFolderPath = <Replace with path to cloned repo on local>

	# Paths- No chnagees required
	$projectFilePath = "$projectFolderPath\MongoMigrationWebApp.csproj"
	$publishFolder = "$projectFolderPath\publish"
	$zipPath = "$publishFolder\app.zip"

	# Login to Azure
	az login

	# Set the subscription (optional)
	# az account set --subscription "your-subscription-id"


	# Deploy the Azure Web App
	
	Write-Host "Deploying the Azure Web App..."
	az deployment group create --resource-group $resourceGroupName --template-file main.bicep --parameters location=WestUs3 webAppName=$webAppName


	# Build the Blazor app
	Write-Host "Building the Blazor app..."
	dotnet publish $projectFilePath -c Release -o $publishFolder

	# Archive the published files
	Compress-Archive -Path "$publishFolder\*" -DestinationPath $zipPath -Update

	# Deploy files to Azure Web App
	Write-Host "Deploying to Azure Web App..."
	az webapp deploy --resource-group $resourceGroupName --name $webAppName --src-path $zipPath --type zip

	Write-Host "Deployment completed successfully!"
	```

4. Naviate to `http://<WebAppName>.azurewebsites.net` to use the tool.