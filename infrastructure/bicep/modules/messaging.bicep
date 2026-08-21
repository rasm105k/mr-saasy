@minLength(6)
@maxLength(50)
param name string
param location string
param tags object = {}

resource serviceBus 'Microsoft.ServiceBus/namespaces@2024-01-01' = {
  name: name
  location: location
  tags: tags
  sku: {
    name: 'Standard'
    tier: 'Standard'
    capacity: 1
  }
  properties: {
    disableLocalAuth: true
    minimumTlsVersion: '1.2'
    publicNetworkAccess: 'Disabled'
    zoneRedundant: false
  }
}

output id string = serviceBus.id
output endpoint string = serviceBus.properties.serviceBusEndpoint
