/**
 * Polyfills that import shared packages (zone.js, @angular/localize) must live in
 * bootstrap.ts. Webpack Module Federation cannot eagerly consume shared modules
 * from this synchronous polyfills entry.
 */
