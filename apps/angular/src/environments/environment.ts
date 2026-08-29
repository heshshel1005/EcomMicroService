import { Environment } from '@abp/ng.core';

const baseUrl = 'http://localhost:4200';

export const environment = {
  production: false,
  application: {
    baseUrl,
    name: 'EcomMicroService',
    logoUrl: '',
  },
  localization: {
    defaultResourceName: 'EcomMicroService',
  },
  oAuthConfig: {
    issuer: 'https://localhost:7600/',
    redirectUri: baseUrl,
    clientId: 'EcomMicroService_Angular',
    responseType: 'code',
    scope: 'offline_access EcomMicroServiceIdentityService EcomMicroServiceAdministration EcomMicroServiceSaaS EcomMicroServiceCatalog EcomMicroServiceBasket EcomMicroServiceOrdering EcomMicroServicePayment EcomMicroServiceCustomer EcomMicroServiceMarketing EcomMicroServiceCms EcomMicroServiceNotification',
    requireHttps: false,
  },
  apis: {
    default: {
      url: 'https://localhost:7500',
      rootNamespace: 'EcomMicroService',
    },
  },
} as Environment;
