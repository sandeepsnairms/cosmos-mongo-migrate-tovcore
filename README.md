# Migration Tool for Azure Cosmos DB for MongoDB (vCore-Based)

Simplify your migration journey to **Azure Cosmos DB for MongoDB (vCore-based)** with a tool designed for efficiency, reliability, and ease of use. Whether you're migrating data online or offline, this tool provides a seamless experience tailored to meet your needs.

## Key Features

- **Flexible Migration Options**  
  Supports both online and offline migrations to suit your business requirements.

- **User-Friendly**  
  Zero learning curve—just provide your connection strings and specify the collections to migrate.

- **Automatic Resume**  
  Migration automatically resumes in case of connection loss, ensuring reliability.

- **Private Deployment**  
  Deploy the tool in your private virtual network (vNet) for enhanced security.

- **Standalone Solution**  
  Operates independently, with no dependencies on other Azure resources.

- **Scalable Performance**  
  Choose your Azure Web App pricing plan based on your speed needs.  
  - Default: **B1**  
  - Recommendation for large workloads: **Premium v3 P2V3**

- **Customizable**  
  Easily adapt the provided C# code to meet your specific requirements.

---

Effortlessly migrate your MongoDB collections while maintaining control, security, and scalability. Start your migration today and unlock the full potential of Azure Cosmos DB!


## Deployment Steps

### Clone Github repo

Clone `https://github.com/sandeepsnairms/cosmos-mongo-migrate-tovcore`.

### Deploy App to Azure Web App

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

4. Naviate to `https://<WebAppName>.azurewebsites.net` to use the tool.