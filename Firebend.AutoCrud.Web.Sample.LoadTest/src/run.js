const { spawn, spawnSync } = require('child_process');
const parseArgs = require('minimist');
const { createWriteStream } = require("fs");
const { K6Parser } = require("k6-to-junit");
const config = require('../config.js');

const printHelpAndExit = () => {
  console.log('yarn test <test-file-name> --env [localhost|qa}');
  process.exit();
};

const sh = (cmd, childArgs = []) => spawnSync(cmd, childArgs, { stdio: 'inherit' });

const args = parseArgs(process.argv.slice(2));

if (args.help) {
  printHelpAndExit();
}

const environment = config[args.env] || config.localhost;

// maps config.js values for environment to k6 format `-e BASE_URL=... -e AUTH_ENDPOINT=...` for the spawn command
const k6Envs = Object.keys(environment).map((k) => `${k}=${environment[k]}`).join(' -e ').split(' ');

if (!args._ || args._.length !== 1) {
  console.error('Enter the name of one test file');
  printHelpAndExit();
}

const testFile = args._.shift();

const k6RunArgs = ['run', '-e', ...k6Envs, './dist/main.bundle.js'];

if (args.datadog) {
  k6RunArgs.push('--out');
  k6RunArgs.push('datadog');
}

if (args.vus && args.vus > 0) {
  k6RunArgs.push('--vus');
  k6RunArgs.push(args.vus);
}

if (args.duration && args.duration > 0) {
  k6RunArgs.push('--duration');
  k6RunArgs.push(args.duration);
}

if (args.iterations && args.iterations > 0) {
  k6RunArgs.push('--iterations');
  k6RunArgs.push(args.iterations);
}

if (args.summary) {
  k6RunArgs.push('--summary-export');
  k6RunArgs.push(`./dist/results.json`);
}

sh('webpack', ['--entry', `./src/scripts/${testFile}`]);

if (args.junit) {
  const parser = new K6Parser();
  const test = spawn('k6', k6RunArgs);
  parser.pipeFrom(test.stdout).then(() => {
    const writer = createWriteStream(`./dist/results.xml`);
    parser.toXml(writer);
    writer.once("finished", () => {
      process.exit(parser.allPassed() ? 0 : 99);
    });
  });
} else {
  sh('k6', k6RunArgs);
}
