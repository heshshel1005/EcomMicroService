const { shareAll, withModuleFederationPlugin } = require('@angular-architects/module-federation/webpack');

const config = withModuleFederationPlugin({

  remotes: {
    "catalogMfe": "http://localhost:4201/remoteEntry.js",
    "orderingMfe": "http://localhost:4202/remoteEntry.js",
  },

  shared: {
    ...shareAll({
      singleton: true,
      strictVersion: true,
      requiredVersion: 'auto',
    }),
  },

});

// publicPath: 'auto' emits import.meta.url (illegal in classic styles.js).
// Do not set environment.importMeta — this Webpack schema does not allow it.
config.output = {
  ...config.output,
  publicPath: 'http://localhost:4200/',
  environment: {
    ...(config.output && config.output.environment),
    module: false,
  },
};

module.exports = config;
