# Final OAuth .NET API

[![Codacy Badge](https://api.codacy.com/project/badge/Grade/d84025db3811465f80e72313bb9ba274)](https://app.codacy.com/gh/gary-archer/oauth.apisample.netcore?utm_source=github.com&utm_medium=referral&utm_content=gary-archer/oauth.apisample.netcore&utm_campaign=Badge_Grade)

## Behaviour

The final OAuth secured .NET API code sample:

- The API has a fictional business area of `investments`, but simply returns hard coded data
- The API takes finer control over OAuth and claims to enable the best security with good manageability
- The API uses structured logging and log aggregation, for the best supportability

### API integrates with UI Clients

The API can run as part of an OAuth end-to-end setup, to serve my blog's UI code samples.\
Running the API in this manner forces it to be consumer focused to its clients:

![SPA and API](./images/spa-and-api.png)

### API can be Productively Tested

The API's clients are UIs, which get user level access tokens by running an OpenID Connect code flow.\
For productive test driven development, the API instead mocks the Authorization Server:

![Test Driven Development](./images/tests.png)

### API can be Load Tested

A basic load test uses Tasks to fire 5 parallel requests at a time at the API.\
This ensures no concurrency problems, and error rehearsal is used, to ensure useful error responses:

![Load Test](./images/loadtest.png)

### API is Supportable

API logs can be analysed in use case based manner by running Elasticsearch SQL and Lucene queries.\
Follow the [Technical Support Queries](https://apisandclients.com/posts/api-technical-support-analysis) for some people friendly examples:

![Support Queries](./images/support-queries.png)

## Commands

### Prerequisites

- Ensure that a .NET 8+ SDK is installed
- Integration tests run Wiremock in Docker, so ensure that Docker is installed

### Run the API

Run the API with this command:

```bash
./start.sh
```

### Configure DNS and SSL

Configure DNS by adding these domains to your hosts file:

```text
127.0.0.1 localhost apilocal.authsamples-dev.com login.authsamples-dev.com
```

Then call an endpoint over port 446:

```bash
curl -k https://apilocal.authsamples-dev.com:446/investments/companies
```

Then configure [.NET SSL trust](https://apisandclients.com/posts/developer-ssl-setup) for the root CA at `./certs/authsamples-dev.ca.pem`.

### Test the API

Stop the API, then re-run it with a test configuration:

```bash
./testsetup.sh
```

Then run integration tests and a load test:

```bash
./integration_tests.sh
./load_test.sh
```

## Further Details

* See the [API Journey - Server Side](https://apisandclients.com/posts/api-journey-server-side) for further information on the API behaviour
* See the [Overview Page](https://apisandclients.com/posts/net-core-code-sample-overview) for instructions on how to run the API
* See the [OAuth Integration Page](https://apisandclients.com/posts/net-core-api-key-coding-points) for the security implementation

## Programming Languages

* C# and .NET are used to implement the REST API

## Infrastructure

* The Kestrel web server is used to host the API over SSL
* AWS Cognito is used as the default Authorization Server
* The [jose-jwt](https://github.com/dvsekhvalnov/jose-jwt) library is used to manage in memory validation of JWTs
* The project includes API deployment resources for Docker and Kubernetes
