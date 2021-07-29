# Changelog
All notable changes to this project will be documented in this file.

## [Unreleased]

### Added
- Rename the project name and the names of some packages
- Encapsulate EFCore for data access components
- Use miniProfile for performance monitoring
- Use SkyApm to achieve link tracking
- Add module package for implementing object mapping through Mapster

## Changed
- Optimize module loading and module support for service registration through ServiceCollection
- Refactoring distributed transactions
- Use Filter to implement input parameter verification

### Fixed
- Fix bugs in distributed locks
- Fix that the client may not be able to subscribe to the routing information of the service registry
- Fix the bug that the zookeeper client session timeout can not subscribe to the routing information of the service registry