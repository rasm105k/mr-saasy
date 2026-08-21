targetScope = 'resourceGroup'

@description('Azure region for MOTOR resources.')
param location string = resourceGroup().location

@minLength(2)
@maxLength(20)
@description('Short workload name used in resource names.')
param workloadName string = 'motor'

@allowed([
  'dev'
  'test'
  'staging'
  'prod'
])
param environmentName string = 'dev'

@description('Mission/change reference shown in resource tags and deployment evidence.')
param changeReference string = 'unapproved-plan'

@description('All resource modules are opt-in. Defaults deploy no cloud resources.')
param enableIdentity bool = false
param enableSecurity bool = false
param enableMessaging bool = false
param enableMonitoring bool = false
param enableData bool = false

var compactName = toLower(replace('${workloadName}${environmentName}', '-', ''))
var uniqueSuffix = take(uniqueString(subscription().subscriptionId, resourceGroup().id), 6)
var anyModuleEnabled = enableIdentity || enableSecurity || enableMessaging || enableMonitoring || enableData
var hasExplicitChangeReference = !empty(changeReference) && changeReference != 'unapproved-plan'
var commonTags = {
  workload: 'MOTOR'
  environment: environmentName
  managedBy: 'bicep'
  motorChangeReference: changeReference
}

module identity 'modules/identity.bicep' = if (enableIdentity && hasExplicitChangeReference) {
  name: 'motor-identity-${environmentName}'
  params: {
    name: take('${compactName}-identity-${uniqueSuffix}', 128)
    location: location
    tags: commonTags
  }
}

module security 'modules/security.bicep' = if (enableSecurity && hasExplicitChangeReference) {
  name: 'motor-security-${environmentName}'
  params: {
    name: take('${compactName}kv${uniqueSuffix}', 24)
    location: location
    tags: commonTags
  }
}

module messaging 'modules/messaging.bicep' = if (enableMessaging && hasExplicitChangeReference) {
  name: 'motor-messaging-${environmentName}'
  params: {
    name: take('${compactName}-sb-${uniqueSuffix}', 50)
    location: location
    tags: commonTags
  }
}

module monitoring 'modules/monitoring.bicep' = if (enableMonitoring && hasExplicitChangeReference) {
  name: 'motor-monitoring-${environmentName}'
  params: {
    workspaceName: take('${compactName}-logs-${uniqueSuffix}', 63)
    applicationInsightsName: take('${compactName}-appi-${uniqueSuffix}', 260)
    location: location
    tags: commonTags
  }
}

module data 'modules/data.bicep' = if (enableData && hasExplicitChangeReference) {
  name: 'motor-data-${environmentName}'
  params: {
    name: take('${compactName}dl${uniqueSuffix}', 24)
    location: location
    tags: commonTags
  }
}

output deploymentIntent object = {
  changeReference: changeReference
  hasExplicitChangeReference: hasExplicitChangeReference
  resourcesBlocked: anyModuleEnabled && !hasExplicitChangeReference
  environment: environmentName
  modules: {
    identity: enableIdentity
    security: enableSecurity
    messaging: enableMessaging
    monitoring: enableMonitoring
    data: enableData
  }
}
