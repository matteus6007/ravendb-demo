# RavenDB Demo

Demo for managing data in a Raven Database.

* Raven 5.4
  * [Installation](#installation-raven-54)
    * [Docker](#docker)
    * [Docker Compose](#docker-compose)
  * [Admin](#admin-raven-54)
    * [Bootstrap Cluster](#bootstrap-cluster)
    * [Create Database](#create-database)
    * [Create Collection](#create-collection)
  * [Export Data](#export-data-raven-54)
  * [Changes API](#changes-api-raven-54)
  * [Data Subscriptions](#data-subscriptions-raven-54)
* Raven 3.5
  * [Installation](#installation-raven-35)
  * [Admin](#admin-raven-35)
  * [Export Data](#export-data-raven-35)
  * [Changes API](#changes-api-raven-35)
  * [Data Subscriptions](#data-subscriptions-raven-35)

## Installation: Raven 5.4

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

## Installation: Raven 3.5

Download latest stable release from https://ravendb.net/download.

Running RavenDB version `3.5` locally running on port `8080`:

```shell
<path_to_ravendb>/Server/Raven.Server.exe
```

You can access RavenDB instance on `http://localhost:8080`.

## Admin: Raven 5.4

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

## Admin: Raven 3.5

See https://ravendb.net/docs/article-page/3.5/csharp/studio/accessing-studio.

## Export Data: Raven 5.4

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

## Export Data: Raven 3.5

### Management Studio

See https://ravendb.net/docs/article-page/3.5/csharp/file-system/studio/tasks/export-and-import-views.

Go to `Databases > Mobile > Tasks > Export Database`.

Select `Advanced > Collections > Specified collections only` if you want to export specific collections.

### CLI

Use the [Smuggler](https://ravendb.net/docs/article-page/3.5/csharp/server/administration/exporting-and-importing-data) utility.

_Note: You now need to separately download the `Tools` package._

Run command:

```shell
Raven.Smuggler out http://localhost:8080 raven.dump --operate-on-types=Documents --database="Mobile" --batch-size=1024 --excludeexpired --metadata-filter=Raven-Entity-Name="MobileDevices"
```

To view the output you can extract the `raven.dump` file to JSON:

```json
{
  "Docs": [
    {
      "name": "Galaxy S24",
      "family": "Samsung",
      "storage": "128GB",
      "colour": "Black",
      "createdOn": "2023-01-01 09:00:00",
      "updatedOn": "2023-01-01 09:00:00",
      "@metadata": {
        "Raven-Entity-Name": "MobileDevices",
        "Raven-Replication-Merged-History": true,
        "Raven-Replication-History": [
          {
            "Raven-Replication-Version": 2,
            "Raven-Replication-Source": "b95cda07-9a29-4282-94a0-3b024e727113"
          }
        ],
        "Raven-Replication-Source": "b95cda07-9a29-4282-94a0-3b024e727113",
        "Raven-Replication-Version": 3,
        "@id": "1234",
        "Last-Modified": "2023-11-01T14:43:12.5663575Z",
        "Raven-Last-Modified": "2023-11-01T14:43:12.5663575",
        "@etag": "01000000-0000-0001-0000-000000000003",
        "Non-Authoritative-Information": false
      }
    }
  ],
  "Attachments": [],
  "Indexes": [],
  "Transformers": [],
  "Identities": []
}
```

_Note: Truncated for brevity._

## Changes API

Subscribe to document changes.

_Note: the change doesn't include current state only the ID and Collection Name, you need to load the document to view the latest version._

### Changes API: Raven 5.4

https://ravendb.net/docs/article-page/5.4/csharp/client-api/changes/how-to-subscribe-to-document-changes

Returns the following information:

* `Type` - type of change such as `PUT` or `DELETE`
* `Id` - document identifier
* `CollectionName` - document's collection name
* `TypeName` - type name
* `ChangeVector` - document change vector

### Changes API: Raven 3.5

https://ravendb.net/docs/article-page/3.5/csharp/client-api/changes/how-to-subscribe-to-document-changes

Returns the following information:

* `Type` - type of change such as `PUT` or `DELETE`
* `Id` - document identifier
* `CollectionName` - document's collection name
* `TypeName` - type name  _(always seems to be null)_
* `Etag` - etag
* `Message` - notification payload _(always seems to be null)_

Run test app to listen to changes to `MobileDevices` collection:

```shell
dotnet run --project .\src\Raven35.Subscriptions\Raven35.Subscriptions.ConsoleApp\Raven35.Subscriptions.ConsoleApp.csproj Changes
```

## Data Subscriptions

Data subscriptions provide a reliable and handy way to retrieve documents from the database for processing purposes by application jobs.

Returns batches of `RavenJObject` or type `T` objects.

_Note: No way to determine type of change, it is merely the current state of the document._

### Data Subscriptions: Raven 5.4

https://ravendb.net/docs/article-page/5.4/csharp/client-api/data-subscriptions/what-are-data-subscriptions

### Data Subscriptions: Raven 3.5

https://ravendb.net/docs/article-page/3.5/csharp/client-api/data-subscriptions/what-are-data-subscriptions

Run test app to listen to changes to `MobileDevices` collection:

```shell
dotnet run --project .\src\Raven35.Subscriptions\Raven35.Subscriptions.ConsoleApp\Raven35.Subscriptions.ConsoleApp.csproj Data
```
