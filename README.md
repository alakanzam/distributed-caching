## Distributed caching

### Description:
- This project is a sample of using `Mongo database` as global caching storage and `In-memory list` as local caching storage.
- Cached items will be loaded at the first time the application starts and automatically reload the value when it stales.
- Local caching storage will be accessed first, if there is no item in it, item will be loaded from `Mongo database` and pushed into `local caching storage` as available.

### Installation:
- Prerequisites:
    - [Visual studio 2019 Community](https://visualstudio.microsoft.com/) or higher.
    - [.Net Core 2.2](https://dotnet.microsoft.com/download) or higher

### Usage:
- Open `DistributedCacheExercise.sln`
- Run `DistributedCacheExercise` project.
- Use the following apis to produce or consume cache value.


### API:

| [POST] api/cache   |                   | Add an item to cache                                                                               |          |         |
|--------------------|-------------------|----------------------------------------------------------------------------------------------------|----------|---------|
|                    | key               | Key to identify item                                                                               | required | (body)  |
|                    | value             | Value of cached item                                                                               | required | (body)  |
|                    | lifeTimeInSeconds | How many seconds cached value can be fresh before stale                                            | optional | (body)  |
|                    |                   |                                                                                                    |          |         |
| [GET] api/cache    |                   | Get cached item as it is available. 204 will be returned if there is no value that has been cached |          |         |
|                    | key               | Key which used for storing cached item                                                             | required | (query) |
|                    |                   |                                                                                                    |          |         |
| [DELETE] api/cache |                   |                                                                                                    |          |         |
|                    | key               | Key of item that needs deleting                                                                    | required | (query) |


### Postman collection:
- Please download [here](https://file.io/D8fuix) to get postman collection.