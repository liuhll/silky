# Changelog
All notable changes to this project will be documented in this file.

## [Unreleased]

## [2.0.0]
### Added
- Support Apollo as a service registration center

### Changed
- Optimize package dependencies
- Optimize RpcContext
- Optimize the life cycle management of services in Rpc communication
- Optimize the way to remove redundant routes
- Optimize the use of distributed locks to avoid deadlocks caused by service routing registration
- Remove IEfCoreDbContextPool interface
- Optimize heartbeat detection

### Fixed
- Fix the bug that the Cancel and Confirm phases cannot be submitted when there are multiple service instances when the memory cache is used as a transaction participant

## [1.2.0]

### Added
- Added use of Serilog as a logger
- Added identity authentication and authorization package

### Changed
- Remove unnecessary dependencies of the Silky.Rpc package
- Optimize swagger document generation

### Fixed
- Fix the bug that the Confirm phase and the Cancel phase will not automatically submit the local transaction and save the data in the TCC transaction
- Fix the exception of parameter conversion in the Cancel and Confirm phases when Json is used as the codec in a distributed transaction
- Fix the exception that the rpc call returns an empty result
- Fixed the bug that the input parameter verification failed when Json was used as the codec during the rpc call  

## [1.1.0]

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
