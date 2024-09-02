# Final OAuth .NET API

[![Codacy Badge](https://api.codacy.com/project/badge/Grade/d84025db3811465f80e72313bb9ba274)](https://app.codacy.com/gh/gary-archer/oauth.apisample.netcore?utm_source=github.com&utm_medium=referral&utm_content=gary-archer/oauth.apisample.netcore&utm_campaign=Badge_Grade)

The final OAuth secured Node.js API code sample, which returns mock `investments` data:

- The API takes finer control over claims-based authorization to enable security with good manageability.
- The API uses structured logging and log aggregation, for the best supportability.

### API Serves Frontend Clients

The API can run as part of an OAuth end-to-end setup, to serve my blog's UI code samples.\
Running the API in this manner forces it to be consumer-focused to its clients:

![SPA and API](./images/spa-and-api.png)

### API Security is Testable

The API's clients are UIs, which get user-level access tokens by running an OpenID Connect code flow.\
For productive test-driven development, the API instead mocks the authorization server:

![Test Driven Development](./images/tests.png)

### API is Load Testable

A basic load test fires batches of concurrent requests at the API.\
This enables reliability testing and error rehearsal to ensure useful client responses and API logs:

![Load Test](./images/loadtest.png)

### API is Supportable

You can aggregate API logs to Elasticsearch and run [Technical Support Queries](https://apisandclients.com/posts/api-technical-support-analysis).

![Support Queries](./images/support-queries.png)

## How to Run the API

- Install a .NET 8+ SDK.
- Also install Docker to run integration tests that use Wiremock.

Then run the API with this command:

```bash
./start.sh
```

### Configure DNS and SSL

Add these domains to your hosts file to configure DNS:

```text
127.0.0.1 localhost api.authsamples-dev.com login.authsamples-dev.com
```

Then call an endpoint over port 446:

```bash
curl -k https://api.authsamples-dev.com:446/investments/companies
```

Then configure [Operating system trust](https://apisandclients.com/posts/developer-ssl-setup#configure-operating-system-trust) for the root CA at `./certs/authsamples-dev.ca.crt`.

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

* See the [API Journey - Server Side](https://apisandclients.com/posts/api-journey-server-side) for further information on the API's behaviour.
* See the [Overview Page](https://apisandclients.com/posts/net-core-code-sample-overview) for further details on how to run the API.
* See the [OAuth Integration Page](https://apisandclients.com/posts/net-core-api-oauth-integration) for some implementation details.

## Programming Languages

* The API uses C# and .NET.

## Infrastructure

* Kestrel is the HTTP server that hosts the API endpoints.
* AWS Cognito is used as the default authorization server.
* The [jose-jwt](https://github.com/dvsekhvalnov/jose-jwt) library manages in memory validation of JWTs.
* The project includes API deployment resources for Docker and Kubernetes.
