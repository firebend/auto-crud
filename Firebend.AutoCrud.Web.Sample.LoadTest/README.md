Performance tests for the Control Tower Platform (official name tbd)

# Install Dependencies

```bash
brew install k6
cd path/to/ct-platform-load-tests
yarn
```

Create a new file called `config.js` at the root of the project. Copy and paste the value from "ct-platform-load-tests config.js" in Bitwarden

# Build and Test

Usage
```bash
yarn test <test-file-name> --env [localhost|qa}
```

Example
```bash
yarn test autocrud.mongo.test.js
```
prepending `./src/scripts` is unnecessary, environment defaults to `localhost`

Run against QA
```bash
yarn test autocrud.mongo.test.js --env qa
```
add environments to `config.js`, use the key as the env name
