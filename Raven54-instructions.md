# RavenDB 5.4 Instructions

Demo for managing data in a Raven `5.4` Database.

* [Installation](#installation)
  * [Docker](#docker)
  * [Docker Compose](#docker-compose)
* [Admin](#admin)
  * [Bootstrap Cluster](#bootstrap-cluster)
  * [Create Database](#create-database)
  * [Create Collection](#create-collection)
* [Export Data](#export-data)
* [Changes API](#changes-api)
* [Data Subscriptions](#data-subscriptions)
  * [Creation Options](#creation-options)
  * [Strategies](#strategies)
  * [Running the Application](#running-the-application)

## Installation

Installing RavenDB `5.4`.

### Docker

Read https://ravendb.net/docs/article-page/5.4/csharp/start/installation/running-in-docker-container for full details on how to setup Docker locally.

Install Docker image https://hub.docker.com/r/ravendb/ravendb:

```shell
docker pull ravendb/ravendb:5.4-ubuntu-latest
```

Running RavenDB version `5.4` locally running on port `8080`:

```shell
docker run --name ravendb -d -p 8080:8080 ravendb/ravendb:5.4-ubuntu-latest
```

Optional parameters:

* `-v $(pwd)/data:/opt/RavenDB/Server/RavenData` - persist data on local machine to `/data` directory
* `-e RAVEN_Security_UnsecuredAccessAllowed=[None|Local|PrivateNetwork|PublicNetwork]` - if authentication is disabled, set the address range type for which server access is unsecured
* `-e RAVEN_License_Eula_Accepted=true` - accept terms and conditions
* `-e RAVEN_Setup_Mode=[None|Unsecured]` - None: disable the setup wizard | Unsecured: run the server in unsecured mode

See https://ravendb.net/docs/article-page/5.4/csharp/server/configuration/configuration-options for full list of options.

You can access RavenDB instance on `http://localhost:8080`.

See https://ravendb.net/docs/article-page/5.4/csharp/studio/overview for details on setting up and managing your cluster.

Stopping MongoDB:

```shell
docker stop ravendb && docker rm ravendb
```

### Docker Compose

Running RavenDB version `5.4` locally running on port `8080`:

```shell
docker-compose up
```

Stopping MongoDB:

```shell
docker-compose down -v --rmi local --remove-orphans
```

You can access RavenDB instance on `http://localhost:8080`.

## Admin

Managing RavenDB `5.4` cluster.

### Bootstrap Cluster

Run `curl` command:

```shell
curl -X POST http://localhost:8080/admin/cluster/bootstrap -d ''
```

### Create Database

This will bootstrap the cluster if it has not already been done.

Create the `Mobile` database manually via the [dashboard](http://localhost:8080/studio/index.html#databases).

Or, run `curl` command:

```shell
curl -X PUT "http://localhost:8080/admin/databases" -d '{"DatabaseName": "Mobile"}'
```

Documentation on the Admin API is scarse, but have found http://live-test.ravendb.net/debug/routes which shows list of endpoints,. Although, it will require trial and error as it doesn't include example requests/responses.

### Create Collection

Create the `MobileDevices` collection.

Using the [REST API](https://ravendb.net/docs/article-page/5.4/csharp/client-api/rest-api/rest-api-intro), run `curl` command:

```shell
curl "http://localhost:8080/databases/Mobile/docs?id=1234" --upload-file "./sample-data/mobile-device.json"
```

Response:

```json
{
    "Id": "1234",
    "ChangeVector": "A:1-54C9k+Y6+EKL6sUlaBAR+w"
}
```

This will create the collection if it does not already exist, or add the document to it if it does.

## Export Data

### Management Studio

Navigate to http://localhost:8080/studio/index.html#databases/tasks/exportDatabase?&database=Mobile and click `Export Databases`.

Click `Advanced > Export all collections` if you want to export a single collection.

### Code

Use the [Smuggler](https://ravendb.net/docs/article-page/5.4/csharp/client-api/smuggler/what-is-smuggler#example) utility.

To view the output you can extract the dump file to JSON:

```json
{
    "BuildVersion": 54083,
    "DatabaseRecord": {
        "DatabaseName": "Mobile",
        "Encrypted": false,
        "UnusedDatabaseIds": [],
        "LockMode": "Unlock",
        "ConflictSolverConfig": null,
        "Settings": [],
        "Revisions": null,
        "TimeSeries": null,
        "DocumentsCompression": null,
        "Expiration": null,
        "Refresh": null,
        "Client": null,
        "Sorters": {},
        "Analyzers": {},
        "IndexesHistory": {},
        "RavenConnectionStrings": {},
        "SqlConnectionStrings": {},
        "PeriodicBackups": [],
        "ExternalReplications": [],
        "RavenEtls": [],
        "SqlEtls": [],
        "HubPullReplications": [],
        "SinkPullReplications": [],
        "OlapConnectionStrings": {},
        "OlapEtls": [],
        "ElasticSearchConnectionStrings": {},
        "ElasticSearchEtls": [],
        "QueueConnectionStrings": {},
        "QueueEtls": []
    },
    "Docs": [
        {
            "name": "Galaxy S24",
            "family": "Samsung",
            "storage": "128GB",
            "colour": "Black",
            "createdOn": "2023-01-01 09:00:00",
            "updatedOn": "2023-01-01 09:00:00",
            "@metadata": {
                "@collection": "MobileDevices",
                "@change-vector": "A:1-54C9k+Y6+EKL6sUlaBAR+w",
                "@id": "1234",
                "@last-modified": "2023-10-30T15:54:28.3113179Z"
            }
        }
    ],
    "RevisionDocuments": [],
    "Conflicts": [],
    "Indexes": [],
    "Identities": [],
    "CompareExchange": [],
    "CounterGroups": [],
    "Subscriptions": [],
    "TimeSeries": []
}
```

## Changes API

[Changes API](https://ravendb.net/docs/article-page/5.4/csharp/client-api/changes/how-to-subscribe-to-document-changes) is a Push Notification service, that allows a client to subscribe to document changes.

_Note: the change doesn't include current state only the ID and Collection Name, you need to load the document to view the latest version._

Returns the following information:

* `Type` - type of change such as `PUT` or `DELETE`
* `Id` - document identifier
* `CollectionName` - document's collection name
* `TypeName` - type name
* `ChangeVector` - document change vector

Run test app to listen to changes to `MobileDevices` collection:

```shell
dotnet run --project .\src\Raven54.Subscriptions\Raven54.Subscriptions.ConsoleApp\Raven54.Subscriptions.ConsoleApp.csproj --subscription-type Changes
```

## Data Subscriptions

[Data subscriptions](https://ravendb.net/docs/article-page/5.4/csharp/client-api/data-subscriptions/what-are-data-subscriptions) provide a reliable and handy way to retrieve documents from the database for processing purposes by application jobs.

Returns batches of `RavenJObject` or type `T` objects.

_Note: No way to determine type of change, it is merely the current state of the document, so you cannot subscribe to document deletions. However, you can use the [Changes API](#changes-api) to subscribe to document delete notifications._

### Creation Options

Define a `SubscriptionCreationOptions` with the following parameters:

* `Name` - user defined name of the subscription _(must be unique)_
* `Query` _(required)_ - [RQL](https://ravendb.net/docs/article-page/5.4/csharp/client-api/session/querying/what-is-rql) query that describes the subscription
* `ChangeVector` - 	allows to define a change vector, from which the subscription will start processing
* `MentorNode` - allows to define a specific node in the cluster that we want to treat the subscription

Or, define a `SubscriptionCreationOptions<T>` with the following parameters:

* `Name` - user defined name of the subscription _(must be unique)_
* `Filter` - lambda describing filter logic for the subscription
* `Projection` - lambda describing the projection of returned documents
* `Includes` -  define an include clause for the subscription
* `ChangeVector` - 	allows to define a change vector, from which the subscription will start processing
* `MentorNode` - allows to define a specific node in the cluster that we want to treat the subscription

_Note: `Query` defined as `from 'MobileDevices' as doc`._

### Strategies

* `OpenIfFree` _(default)_ - open a subscription only if there isn't any other currently connected client, throws `SubscriptionInUseException` if there is an open connection
* `TakeOver` - allow an incoming connection to overthrow an existing one, if existing connection has a `TakeOver` strategy the incoming connection throws `SubscriptionInUseException`
* `WaitForFree` - if there is an open connection, waits for connection to be disconnected _(active/passive)_
* `Concurrent` - multiple workers for the same subscription are allowed to connect simultaneously

### Running the Application

Run test app to listen to changes to `MobileDevices` collection using the `TakeOver` strategy:

```shell
dotnet run --project .\src\Raven54.Subscriptions\Raven54.Subscriptions.ConsoleApp\Raven54.Subscriptions.ConsoleApp.csproj --subscription-type Data
```

Get list of subscriptions for `Mobile` database:

```shell
curl http://localhost:8080/databases/Mobile/subscriptions
```

Outputs:

```json
{
    "@metadata": {
        "DateTime": "2023-11-15T09:14:24.7771315Z",
        "WebUrl": "http://29135d7df34b:8080",
        "NodeTag": "A"
    },
    "Results": [
        {
            "SubscriptionId": 1,
            "SubscriptionName": "mobiledevices_subscription",
            "ChangeVectorForNextBatchStartingPoint": "A:4-5EXQtVvs/0iAtFo6Ua9Cnw",
            "Query": "from 'MobileDevices' as doc",
            "Disabled": false,
            "LastClientConnectionTime": "2023-11-15T09:12:58.2472242Z",
            "LastBatchAckTime": "2023-11-14T16:49:46.4182762Z",
            "Connections": [],
            "RecentConnections": [],
            "RecentRejectedConnections": [],
            "CurrentPendingConnections": []
        }
    ]
}
``` 
