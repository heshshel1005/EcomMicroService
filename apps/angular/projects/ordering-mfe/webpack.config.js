const { shareAll, withModuleFederationPlugin } = require('@angular-architects/module-federation/webpack');

const config = withModuleFederationPlugin({

  name: 'orderingMfe',

  exposes: {
    './Module': './projects/ordering-mfe/src/app/app.routes.ts',
  },

  shared: {
    ...shareAll({ singleton: true, strictVersion: true, requiredVersion: 'auto' }),
  },

});

config.output = {
  ...config.output,
  publicPath: 'http://localhost:4202/',
  environment: {
    ...(config.output && config.output.environment),
    module: false,
  },
};

module.exports = config;
