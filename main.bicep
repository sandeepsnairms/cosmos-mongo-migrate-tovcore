param location string
param webAppName string

/*
Uncomment the following code to reference an existing VNet and subnet

param vnetName string
param vnetResourceGroup string
param subnetName string
*/

var appServicePlanName = '${webAppName}SP'

resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: 'B1'   // Use P1V3 for large workloads
    tier: 'Basic' // Use PremiumV2 for large workloads
  }
}

resource webApp 'Microsoft.Web/sites@2022-03-01' = {
  name: webAppName
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    //virtualNetworkSubnetId: existingSubnet.id  //Uncomment this line  to reference an existing VNet and subnet
  }
}

/* 

Uncomment the following code to reference an existing VNet and subnet


// Define a reference to the virtual network
resource existingVnet 'Microsoft.Network/virtualNetworks@2023-05-01' existing = {
  name: vnetName
  scope: resourceGroup(vnetResourceGroup)
}

// Reference the existing subnet within the VNet
resource existingSubnet 'Microsoft.Network/virtualNetworks/subnets@2023-05-01' existing = {
  name: subnetName
  parent: existingVnet
}
*/
